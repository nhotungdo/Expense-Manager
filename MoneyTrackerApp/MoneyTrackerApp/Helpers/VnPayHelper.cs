using System.Security.Cryptography;
using System.Text;

namespace MoneyTrackerApp.Helpers;

public static class VnPayHelper
{
    public static string BuildPaymentUrl(
        string baseUrl,
        string tmnCode,
        string hashSecret,
        string returnUrl,
        string orderInfo,
        string ipAddress,
        string txnRef,
        long amount,
        string locale = "vn",
        string currency = "VND",
        string version = "2.1.0")
    {
        var data = new SortedDictionary<string, string>
        {
            { "vnp_Version", version },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", tmnCode },
            { "vnp_Amount", amount.ToString() },
            { "vnp_CurrCode", currency },
            { "vnp_TxnRef", txnRef },
            { "vnp_OrderInfo", orderInfo },
            { "vnp_OrderType", "other" },
            { "vnp_Locale", locale },
            { "vnp_ReturnUrl", returnUrl },
            { "vnp_IpAddr", ipAddress },
            { "vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") }
        };

        var query = string.Join("&", data.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        var secureHash = CreateHmacSha512(hashSecret, query);

        return $"{baseUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public static bool ValidateSignature(IDictionary<string, string> vnpayData, string receivedSignature, string hashSecret)
    {
        // Exclude secure hash params, sort by key, then compute HMAC SHA512
        var filtered = vnpayData
            .Where(kvp => !string.Equals(kvp.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                       && !string.Equals(kvp.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase)
                       && kvp.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .ToDictionary(k => k.Key, v => v.Value);

        var data = string.Join("&", filtered.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}"));
        var computed = CreateHmacSha512(hashSecret, data);

        return string.Equals(computed, receivedSignature, StringComparison.OrdinalIgnoreCase);
    }

    public static string ComputeSignature(string key, string data) => CreateHmacSha512(key, data);

    private static string CreateHmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}

