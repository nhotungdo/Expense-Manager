using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MoneyTrackerApp.Helpers;

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

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
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        var data = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (data.Length > 0)
            {
                data.Append('&');
            }
            data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value));
        }
        var queryString = data.ToString();
        var baseUrlWithQuery = baseUrl + "?" + queryString;
        var signData = queryString;
        if (signData.Length > 0)
        {
            var vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            baseUrlWithQuery += "&vnp_SecureHash=" + vnp_SecureHash;
        }
        return baseUrlWithQuery;
    }

    public bool ValidateSignature(string inputHash, string vnp_HashSecret)
    {
        var rspRaw = GetResponseData();
        var myChecksum = HmacSHA512(vnp_HashSecret, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }
        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }
        foreach (var kv in _responseData)
        {
            if (data.Length > 0)
            {
                data.Append('&');
            }
            data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value));
        }
        return data.ToString();
    }

    private string HmacSHA512(string key, string inputData)
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

public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
