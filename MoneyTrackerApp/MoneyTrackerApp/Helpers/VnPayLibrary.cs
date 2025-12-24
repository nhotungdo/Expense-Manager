using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace MoneyTrackerApp.Helpers;

public class VnPayLibrary
{
    public const string VERSION = "2.1.0";
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        var data = new StringBuilder();
        foreach (var (key, value) in _requestData)
        {
            if (data.Length > 0)
            {
                data.Append('&');
            }
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value));
        }

        var queryString = data.ToString();
        var vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, queryString);
        
        return $"{baseUrl}?{queryString}&vnp_SecureHash={vnp_SecureHash}";
    }

    public bool ValidateSignature(string inputHash, string vnp_HashSecret)
    {
        var data = new StringBuilder();
        foreach (var (key, value) in _responseData)
        {
            if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
            {
                if (data.Length > 0)
                {
                    data.Append('&');
                }
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value));
            }
        }

        var checkSum = Utils.HmacSHA512(vnp_HashSecret, data.ToString());
        return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return string.Compare(x, y, StringComparison.Ordinal);
    }
}

public static class Utils
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }
}
