using MoneyTrackerApp.Helpers;

namespace MoneyTrackerApp.Services;

public class VnPayService
{
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayService> _logger;

    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _baseUrl;
    private readonly string _returnUrl;
    private readonly string _locale;
    private readonly string _currency;

    public VnPayService(IConfiguration config, ILogger<VnPayService> logger)
    {
        _config = config;
        _logger = logger;

        _tmnCode = _config["Vnpay:TmnCode"] ?? throw new InvalidOperationException("Vnpay:TmnCode is missing");
        _hashSecret = _config["Vnpay:HashSecret"] ?? throw new InvalidOperationException("Vnpay:HashSecret is missing");
        _baseUrl = _config["Vnpay:BaseUrl"] ?? throw new InvalidOperationException("Vnpay:BaseUrl is missing");
        _returnUrl = _config["Vnpay:PaymentBackReturnUrl"] ?? throw new InvalidOperationException("Vnpay:PaymentBackReturnUrl is missing");
        _locale = _config["Vnpay:Locale"] ?? "vn";
        _currency = _config["Vnpay:CurrCode"] ?? "VND";
    }

    public string CreatePaymentUrl(long userId, int packageId, decimal amount, string packageName, string ipAddress)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        var orderInfo = $"Thanh toan goi {packageName}";
        var txnRef = $"{userId}_{packageId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var amountInSmallestUnit = (long)(amount * 100); // VNPay expects x100 for VND

        _logger.LogInformation("Creating VNPay URL for user {UserId}, package {PackageId}, amount {Amount}", userId, packageId, amount);

        return VnPayHelper.BuildPaymentUrl(
            _baseUrl,
            _tmnCode,
            _hashSecret,
            _returnUrl,
            orderInfo,
            ipAddress,
            txnRef,
            amountInSmallestUnit,
            _locale,
            _currency);
    }
}

