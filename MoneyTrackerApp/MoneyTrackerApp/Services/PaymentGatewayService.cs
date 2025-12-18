using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Dapper;

namespace MoneyTrackerApp.Services
{
    public class PaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentGatewayService> _logger;
        private readonly string _connectionString;
        private readonly string _gatewayUrl;
        private readonly string _merchantId;
        private readonly string _secretKey;

        public PaymentGatewayService(
            IConfiguration configuration,
            ILogger<PaymentGatewayService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _connectionString = configuration.GetConnectionString("DBDefault");
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Connection string 'DBDefault' not found in configuration");
                throw new InvalidOperationException("Connection string 'DBDefault' not found. Please check appsettings.json");
            }
            
            _gatewayUrl = configuration["PaymentGateway:Url"] ?? "https://payment-gateway.example.com";
            _merchantId = configuration["PaymentGateway:MerchantId"] ?? "MERCHANT_ID";
            _secretKey = configuration["PaymentGateway:SecretKey"] ?? "SECRET_KEY";
        }

        // Generate secure session token with GUID + random salt
        public string GenerateSessionToken()
        {
            var guid = Guid.NewGuid().ToString("N");
            var salt = GenerateRandomSalt(16);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"{guid}_{salt}_{timestamp}";
        }

        // Generate random salt for security
        private string GenerateRandomSalt(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Generate HMAC signature for security
        public string GenerateHmacSignature(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        // Validate HMAC signature from gateway
        public bool ValidateHmacSignature(string data, string signature)
        {
            var expectedSignature = GenerateHmacSignature(data);
            return signature == expectedSignature;
        }

        // Create payment transaction
        public async Task<PaymentTransactionResult> CreatePaymentTransactionAsync(
            long userId, 
            int packageId, 
            string ipAddress, 
            string userAgent)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Get package details
                var package = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT Id, Name, Price FROM ServicePackages WHERE Id = @PackageId AND IsActive = 1",
                    new { PackageId = packageId });

                if (package == null)
                {
                    return new PaymentTransactionResult
                    {
                        Success = false,
                        ErrorMessage = "Package not found or inactive"
                    };
                }

                // Generate session token
                var sessionToken = GenerateSessionToken();

                // Build URLs
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
                var returnUrl = $"{baseUrl}/api/payments/return";
                var cancelUrl = $"{baseUrl}/api/payments/cancel";
                var redirectUrl = $"{_gatewayUrl}?session={sessionToken}";

                // Insert payment transaction
                var sql = @"
                    INSERT INTO PaymentTransactions 
                    (UserId, PackageId, PackageName, Amount, Currency, SessionToken, 
                     PaymentGatewayUrl, Status, RedirectUrl, ReturnUrl, CancelUrl, 
                     IpAddress, UserAgent, RequestTimestamp, CreatedAt)
                    VALUES 
                    (@UserId, @PackageId, @PackageName, @Amount, @Currency, @SessionToken,
                     @PaymentGatewayUrl, 0, @RedirectUrl, @ReturnUrl, @CancelUrl,
                     @IpAddress, @UserAgent, GETUTCDATE(), GETUTCDATE());
                    SELECT CAST(SCOPE_IDENTITY() as bigint);";

                var transactionId = await connection.ExecuteScalarAsync<long>(sql, new
                {
                    UserId = userId,
                    PackageId = packageId,
                    PackageName = package.Name,
                    Amount = package.Price,
                    Currency = "VND",
                    SessionToken = sessionToken,
                    PaymentGatewayUrl = _gatewayUrl,
                    RedirectUrl = redirectUrl,
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });

                _logger.LogInformation($"Payment transaction created: {transactionId} for user {userId}");

                return new PaymentTransactionResult
                {
                    Success = true,
                    PaymentTransactionId = transactionId,
                    PaymentUrl = redirectUrl,
                    SessionToken = sessionToken,
                    Amount = package.Price,
                    Currency = "VND"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment transaction");
                return new PaymentTransactionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Process return callback from payment gateway
        public async Task<PaymentCallbackResult> ProcessReturnCallbackAsync(
            string sessionToken,
            string gatewayTransactionId,
            string status,
            string signature,
            string ipAddress,
            string userAgent)
        {
            try
            {
                // Validate signature to prevent spoofing
                var dataToSign = $"{sessionToken}|{gatewayTransactionId}|{status}";
                if (!ValidateHmacSignature(dataToSign, signature))
                {
                    _logger.LogWarning($"Invalid signature for session {sessionToken}");
                    return new PaymentCallbackResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid signature"
                    };
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Get payment transaction
                var transaction = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT pt.*, sp.DurationDays 
                      FROM PaymentTransactions pt
                      INNER JOIN ServicePackages sp ON pt.PackageId = sp.Id
                      WHERE pt.SessionToken = @SessionToken",
                    new { SessionToken = sessionToken });

                if (transaction == null)
                {
                    return new PaymentCallbackResult
                    {
                        Success = false,
                        ErrorMessage = "Transaction not found"
                    };
                }

                // Check for duplicate callback
                if (transaction.Status == 2) // Already successful
                {
                    _logger.LogWarning($"Duplicate callback for transaction {transaction.Id}");
                    return new PaymentCallbackResult
                    {
                        Success = true,
                        AlreadyProcessed = true,
                        PaymentTransactionId = transaction.Id
                    };
                }

                // Map gateway status to our status
                int mappedStatus = MapGatewayStatus(status);

                // Update payment transaction
                var gatewayResponse = JsonSerializer.Serialize(new
                {
                    session = sessionToken,
                    transaction_id = gatewayTransactionId,
                    status = status,
                    timestamp = DateTime.UtcNow,
                    ip_address = ipAddress,
                    user_agent = userAgent
                });

                await connection.ExecuteAsync(@"
                    UPDATE PaymentTransactions 
                    SET GatewayTransactionId = @GatewayTransactionId,
                        GatewayResponse = @GatewayResponse,
                        Status = @Status,
                        ResponseTimestamp = GETUTCDATE(),
                        CompletedAt = CASE WHEN @Status = 2 THEN GETUTCDATE() ELSE NULL END,
                        FailureReason = CASE WHEN @Status = 3 THEN @FailureReason ELSE NULL END
                    WHERE Id = @Id",
                    new
                    {
                        Id = transaction.Id,
                        GatewayTransactionId = gatewayTransactionId,
                        GatewayResponse = gatewayResponse,
                        Status = mappedStatus,
                        FailureReason = status == "failed" ? "Payment failed at gateway" : null
                    });

                // If successful, create subscription and payment records
                if (mappedStatus == 2) // Success
                {
                    await CreateSubscriptionAndPaymentAsync(connection, transaction);
                }

                return new PaymentCallbackResult
                {
                    Success = true,
                    PaymentTransactionId = transaction.Id,
                    Status = mappedStatus,
                    UserId = transaction.UserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing return callback");
                return new PaymentCallbackResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Map gateway status to internal status
        private int MapGatewayStatus(string gatewayStatus)
        {
            return gatewayStatus?.ToLower() switch
            {
                "pending" => 0,      // Pending
                "processing" => 1,   // Processing
                "success" => 2,      // Success
                "completed" => 2,    // Success
                "failed" => 3,       // Failed
                "error" => 3,        // Failed
                "cancelled" => 4,    // Cancelled
                "canceled" => 4,     // Cancelled
                _ => 3               // Default to Failed
            };
        }

        // Create subscription and payment records
        private async Task CreateSubscriptionAndPaymentAsync(SqlConnection connection, dynamic transaction)
        {
            using var dbTransaction = connection.BeginTransaction();
            try
            {
                // Create subscription
                var startDate = DateTime.UtcNow;
                var endDate = startDate.AddDays((int)transaction.DurationDays);

                var subscriptionSql = @"
                    INSERT INTO Subscriptions 
                    (UserId, PackageId, Status, StartDate, EndDate, AutoRenew, CreatedAt)
                    VALUES 
                    (@UserId, @PackageId, 1, @StartDate, @EndDate, 1, GETUTCDATE());
                    SELECT CAST(SCOPE_IDENTITY() as bigint);";

                var subscriptionId = await connection.ExecuteScalarAsync<long>(
                    subscriptionSql,
                    new
                    {
                        UserId = transaction.UserId,
                        PackageId = transaction.PackageId,
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    dbTransaction);

                // Create payment record
                var paymentSql = @"
                    INSERT INTO Payments 
                    (SubscriptionId, Amount, Currency, Status, PaymentMethod, TransactionId, PaidAt, CreatedAt)
                    VALUES 
                    (@SubscriptionId, @Amount, @Currency, 2, 'payment_gateway', @TransactionId, GETUTCDATE(), GETUTCDATE());";

                await connection.ExecuteAsync(
                    paymentSql,
                    new
                    {
                        SubscriptionId = subscriptionId,
                        Amount = transaction.Amount,
                        Currency = transaction.Currency,
                        TransactionId = transaction.GatewayTransactionId
                    },
                    dbTransaction);

                dbTransaction.Commit();
                _logger.LogInformation($"Subscription {subscriptionId} created for user {transaction.UserId}");
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError(ex, "Error creating subscription and payment");
                throw;
            }
        }

        // Process cancel callback
        public async Task<bool> ProcessCancelCallbackAsync(string sessionToken)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(@"
                    UPDATE PaymentTransactions 
                    SET Status = 4, 
                        ResponseTimestamp = GETUTCDATE(),
                        FailureReason = 'User cancelled payment'
                    WHERE SessionToken = @SessionToken AND Status = 0",
                    new { SessionToken = sessionToken });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cancel callback");
                return false;
            }
        }

        // Process webhook from payment gateway
        public async Task<bool> ProcessWebhookAsync(WebhookPayload payload, string signature)
        {
            try
            {
                // Validate webhook signature
                var dataToSign = JsonSerializer.Serialize(payload);
                if (!ValidateHmacSignature(dataToSign, signature))
                {
                    _logger.LogWarning("Invalid webhook signature");
                    return false;
                }

                // Process based on event type
                switch (payload.EventType?.ToLower())
                {
                    case "payment.success":
                        await ProcessReturnCallbackAsync(
                            payload.SessionToken,
                            payload.TransactionId,
                            "success",
                            signature,
                            payload.IpAddress,
                            payload.UserAgent);
                        break;

                    case "payment.failed":
                        await ProcessReturnCallbackAsync(
                            payload.SessionToken,
                            payload.TransactionId,
                            "failed",
                            signature,
                            payload.IpAddress,
                            payload.UserAgent);
                        break;

                    case "payment.cancelled":
                        await ProcessCancelCallbackAsync(payload.SessionToken);
                        break;

                    default:
                        _logger.LogWarning($"Unknown webhook event type: {payload.EventType}");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return false;
            }
        }
    }

    // Result classes
    // Result classes
    public class PaymentTransactionResult
    {
        public bool Success { get; set; }
        public long PaymentTransactionId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? SessionToken { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentCallbackResult
    {
        public bool Success { get; set; }
        public long PaymentTransactionId { get; set; }
        public int Status { get; set; }
        public long UserId { get; set; }
        public bool AlreadyProcessed { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class WebhookPayload
    {
        public string? EventType { get; set; }
        public string? SessionToken { get; set; }
        public string? TransactionId { get; set; }
        public string? Status { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
