using System.Text.Json;
using MoneyTrackerApp.Helpers;

namespace MoneyTrackerApp.Services
{
    /// <summary>
    /// Comprehensive error handler for VNPay payment integration
    /// </summary>
    public class VnPayErrorHandler
    {
        private readonly ILogger<VnPayErrorHandler> _logger;
        private readonly IConfiguration _configuration;

        public VnPayErrorHandler(
            ILogger<VnPayErrorHandler> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// VNPay response codes and their meanings
        /// </summary>
        public static readonly Dictionary<string, string> ResponseCodes = new()
        {
            { "00", "Giao dịch thành công" },
            { "07", "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)" },
            { "09", "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng" },
            { "10", "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần" },
            { "11", "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch" },
            { "12", "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa" },
            { "13", "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP)" },
            { "24", "Giao dịch không thành công do: Khách hàng hủy giao dịch" },
            { "51", "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch" },
            { "65", "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày" },
            { "75", "Ngân hàng thanh toán đang bảo trì" },
            { "79", "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định" },
            { "99", "Các lỗi khác (lỗi còn lại, không có trong danh sách mã lỗi đã liệt kê)" }
        };

        /// <summary>
        /// Error categories for better handling
        /// </summary>
        public enum ErrorCategory
        {
            NetworkError,
            AuthenticationError,
            ValidationError,
            TransactionError,
            ConfigurationError,
            DatabaseError,
            UnknownError
        }

        /// <summary>
        /// Analyze and categorize error
        /// </summary>
        public (ErrorCategory category, string message, string details) AnalyzeError(Exception ex)
        {
            _logger.LogError(ex, "Analyzing VNPay error");

            return ex switch
            {
                HttpRequestException httpEx => (
                    ErrorCategory.NetworkError,
                    "Không thể kết nối đến VNPay",
                    $"Network error: {httpEx.Message}"
                ),
                UnauthorizedAccessException => (
                    ErrorCategory.AuthenticationError,
                    "Xác thực VNPay thất bại",
                    "Invalid merchant credentials"
                ),
                ArgumentException argEx => (
                    ErrorCategory.ValidationError,
                    "Dữ liệu không hợp lệ",
                    argEx.Message
                ),
                InvalidOperationException opEx => (
                    ErrorCategory.ConfigurationError,
                    "Lỗi cấu hình hệ thống",
                    opEx.Message
                ),
                Microsoft.Data.SqlClient.SqlException sqlEx => (
                    ErrorCategory.DatabaseError,
                    "Lỗi cơ sở dữ liệu",
                    $"SQL Error: {sqlEx.Message}"
                ),
                _ => (
                    ErrorCategory.UnknownError,
                    "Lỗi không xác định",
                    ex.Message
                )
            };
        }

        /// <summary>
        /// Get user-friendly error message based on VNPay response code
        /// </summary>
        public string GetUserFriendlyMessage(string responseCode)
        {
            if (ResponseCodes.TryGetValue(responseCode, out var message))
            {
                return message;
            }
            return "Giao dịch không thành công. Vui lòng thử lại sau.";
        }

        /// <summary>
        /// Validate VNPay configuration
        /// </summary>
        public (bool isValid, List<string> errors) ValidateConfiguration()
        {
            var errors = new List<string>();

            var tmnCode = _configuration["Vnpay:TmnCode"];
            if (string.IsNullOrEmpty(tmnCode))
                errors.Add("VNPay TmnCode is not configured");

            var hashSecret = _configuration["Vnpay:HashSecret"];
            if (string.IsNullOrEmpty(hashSecret))
                errors.Add("VNPay HashSecret is not configured");

            var baseUrl = _configuration["Vnpay:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
                errors.Add("VNPay BaseUrl is not configured");
            else if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
                errors.Add("VNPay BaseUrl is not a valid URL");

            var returnUrl = _configuration["Vnpay:PaymentBackReturnUrl"];
            if (string.IsNullOrEmpty(returnUrl))
                errors.Add("VNPay ReturnUrl is not configured");

            var version = _configuration["Vnpay:Version"];
            if (string.IsNullOrEmpty(version))
                errors.Add("VNPay Version is not configured");

            var command = _configuration["Vnpay:Command"];
            if (string.IsNullOrEmpty(command))
                errors.Add("VNPay Command is not configured");

            var currCode = _configuration["Vnpay:CurrCode"];
            if (string.IsNullOrEmpty(currCode))
                errors.Add("VNPay CurrCode is not configured");

            var locale = _configuration["Vnpay:Locale"];
            if (string.IsNullOrEmpty(locale))
                errors.Add("VNPay Locale is not configured");

            if (errors.Any())
            {
                _logger.LogError("VNPay configuration validation failed: {Errors}", string.Join(", ", errors));
            }

            return (!errors.Any(), errors);
        }

        /// <summary>
        /// Log error with full context
        /// </summary>
        public void LogError(
            string operation,
            Exception exception,
            Dictionary<string, object>? context = null)
        {
            var errorLog = new
            {
                Timestamp = DateTime.UtcNow,
                Operation = operation,
                ErrorType = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message,
                Context = context ?? new Dictionary<string, object>()
            };

            var json = JsonSerializer.Serialize(errorLog, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogError(exception, "VNPay Error - {Operation}: {ErrorLog}", operation, json);
        }

        /// <summary>
        /// Determine if error is retryable
        /// </summary>
        public bool IsRetryable(Exception ex)
        {
            return ex switch
            {
                HttpRequestException => true,
                TimeoutException => true,
                Microsoft.Data.SqlClient.SqlException sqlEx when IsTransientSqlError(sqlEx) => true,
                _ => false
            };
        }

        /// <summary>
        /// Check if SQL error is transient (temporary)
        /// </summary>
        private bool IsTransientSqlError(Microsoft.Data.SqlClient.SqlException ex)
        {
            // Common transient SQL error codes
            int[] transientErrorCodes = { -2, -1, 2, 20, 64, 233, 10053, 10054, 10060, 40197, 40501, 40613 };
            return transientErrorCodes.Contains(ex.Number);
        }

        /// <summary>
        /// Create error response for API
        /// </summary>
        public object CreateErrorResponse(
            ErrorCategory category,
            string message,
            string? details = null,
            string? errorCode = null)
        {
            var isDevelopment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";

            var response = new Dictionary<string, object>
            {
                { "success", false },
                { "error", message },
                { "category", category.ToString() },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            if (!string.IsNullOrEmpty(errorCode))
            {
                response["errorCode"] = errorCode;
            }

            if (isDevelopment && !string.IsNullOrEmpty(details))
            {
                response["details"] = details;
            }

            return response;
        }

        /// <summary>
        /// Validate signature from VNPay callback
        /// </summary>
        public bool ValidateSignature(
            Dictionary<string, string> vnpayData,
            string receivedSignature,
            string hashSecret)
        {
            try
            {
                var isValid = VnPayHelper.ValidateSignature(vnpayData, receivedSignature, hashSecret);

                if (!isValid)
                {
                    _logger.LogWarning("VNPay signature validation failed. Received: {Signature}", receivedSignature);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating VNPay signature");
                return false;
            }
        }

        /// <summary>
        /// Check if transaction amount matches
        /// </summary>
        public bool ValidateAmount(long expectedAmount, long receivedAmount)
        {
            bool isValid = expectedAmount == receivedAmount;
            
            if (!isValid)
            {
                _logger.LogWarning(
                    "Amount mismatch - Expected: {Expected}, Received: {Received}",
                    expectedAmount,
                    receivedAmount);
            }

            return isValid;
        }
    }
}
