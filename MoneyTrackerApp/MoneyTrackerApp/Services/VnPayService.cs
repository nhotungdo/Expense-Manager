using MoneyTrackerApp.Helpers;

namespace MoneyTrackerApp.Services;

public class VnPayService
{
    private readonly IConfiguration _config;
    private readonly ILogger<VnPayService> _logger;

    public VnPayService(IConfiguration config, ILogger<VnPayService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string CreatePaymentUrl(string txnRef, decimal amount, string description, string ipAddress)
    {
        var vnpay = new VnPayLibrary();
        
        var tmnCode = _config["Vnpay:TmnCode"];
        var hashSecret = _config["Vnpay:HashSecret"];
        var baseUrl = _config["Vnpay:BaseUrl"];
        var returnUrl = _config["Vnpay:PaymentBackReturnUrl"];
        
        if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(returnUrl))
        {
            throw new InvalidOperationException("VnPay configuration is missing.");
        }

        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", tmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString()); 
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", _config["Vnpay:CurrCode"] ?? "VND");
        vnpay.AddRequestData("vnp_IpAddr", ipAddress);
        vnpay.AddRequestData("vnp_Locale", _config["Vnpay:Locale"] ?? "vn");
        vnpay.AddRequestData("vnp_OrderInfo", description);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        vnpay.AddRequestData("vnp_TxnRef", txnRef); 

        return vnpay.CreateRequestUrl(baseUrl, hashSecret);
    }

    public (bool Success, string Message, long Amount, string OrderId, string TransactionId) PaymentExecute(IQueryCollection collections)
    {
        var vnpay = new VnPayLibrary();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
        var hashSecret = _config["Vnpay:HashSecret"];
        
        if (string.IsNullOrEmpty(hashSecret))
             throw new InvalidOperationException("VnPay HashSecret is missing.");

        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);
        
        if (!checkSignature)
        {
            return (false, "Invalid Signature", 0, "", "");
        }

        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
        var vnp_OrderId = vnpay.GetResponseData("vnp_TxnRef");
        var vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;

        if (vnp_ResponseCode == "00")
        {
            return (true, "Success", vnp_Amount, vnp_OrderId, vnp_TransactionId);
        }

        var message = ResponseCodes.TryGetValue(vnp_ResponseCode, out var msg) 
            ? msg 
            : $"Giao dịch thất bại (Mã lỗi: {vnp_ResponseCode})";

        return (false, message, vnp_Amount, vnp_OrderId, vnp_TransactionId);
    }

    private static readonly Dictionary<string, string> ResponseCodes = new()
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
}






