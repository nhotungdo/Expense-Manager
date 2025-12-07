using System.Text;

namespace MoneyTrackerApp.Helpers;

public class EmvQrLibrary
{
    private const string PayloadFormatIndicator = "00";
    private const string PointOfInitiationMethod = "01";
    private const string MerchantAccountInformation = "38";
    private const string TransactionCurrency = "53";
    private const string TransactionAmount = "54";
    private const string CountryCode = "58";
    private const string AdditionalDataField = "62";
    private const string CRC = "63";

    public static string GenerateVietQr(string bankBin, string accountNumber, string amount, string content)
    {
        var sb = new StringBuilder();

        // 00. Payload Format Indicator: 01
        AppendTlv(sb, PayloadFormatIndicator, "01");

        // 01. Point of Initiation Method: 12 (Dynamic - amount included) or 11 (Static)
        // Since we are including amount, usually 12 is preferred, but 11 works too. Let's use 12.
        AppendTlv(sb, PointOfInitiationMethod, "12");

        // 38. Merchant Account Information
        // GUID: A000000727 (VietQR)
        // Service Code: QRIBFT (000201)
        var merchantInfo = new StringBuilder();
        AppendTlv(merchantInfo, "00", "A000000727");
        
        var bankInfo = new StringBuilder();
        AppendTlv(bankInfo, "00", bankBin);
        AppendTlv(bankInfo, "01", accountNumber);
        
        AppendTlv(merchantInfo, "01", bankInfo.ToString());
        AppendTlv(merchantInfo, "02", "QRIBFT"); // Service code for 24/7 transfer

        AppendTlv(sb, MerchantAccountInformation, merchantInfo.ToString());

        // 53. Transaction Currency: 704 (VND)
        AppendTlv(sb, TransactionCurrency, "704");

        // 54. Transaction Amount
        if (!string.IsNullOrEmpty(amount))
        {
            AppendTlv(sb, TransactionAmount, amount);
        }

        // 58. Country Code: VN
        AppendTlv(sb, CountryCode, "VN");

        // 59. Merchant Name
        AppendTlv(sb, "59", "MoneyTracker");

        // 60. Merchant City
        AppendTlv(sb, "60", "Vietnam");

        // 62. Additional Data Field
        if (!string.IsNullOrEmpty(content))
        {
            var additionalData = new StringBuilder();
            AppendTlv(additionalData, "08", content); // 08 is for Bill Number / Reference
            AppendTlv(sb, AdditionalDataField, additionalData.ToString());
        }

        // 63. CRC
        var dataWithoutCrc = sb.ToString() + CRC + "04";
        var crcValue = Crc16Ccitt(dataWithoutCrc);
        sb.Append(CRC).Append("04").Append(crcValue);

        return sb.ToString();
    }

    private static void AppendTlv(StringBuilder sb, string id, string value)
    {
        sb.Append(id);
        sb.Append(value.Length.ToString("D2"));
        sb.Append(value);
    }

    private static string Crc16Ccitt(string data)
    {
        ushort crc = 0xFFFF;
        byte[] bytes = Encoding.ASCII.GetBytes(data);

        foreach (byte b in bytes)
        {
            for (int i = 0; i < 8; i++)
            {
                bool bit = ((b >> (7 - i) & 1) == 1);
                bool c15 = ((crc >> 15 & 1) == 1);
                crc <<= 1;
                if (c15 ^ bit) crc ^= 0x1021;
            }
        }

        return crc.ToString("X4");
    }
}
