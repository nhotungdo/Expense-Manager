using System.Globalization;

namespace MoneyTrackerApp.Helpers
{
    public static class CurrencyHelper
    {
        private static readonly CultureInfo UsCulture = new CultureInfo("en-US");

        public static string FormatCurrency(decimal? amount)
        {
            if (!amount.HasValue) return "0";
            return amount.Value.ToString("#,##0", UsCulture);
        }

        public static string FormatCurrency(double? amount)
        {
            if (!amount.HasValue) return "0";
            return amount.Value.ToString("#,##0", UsCulture);
        }

        public static string FormatCurrency(long? amount)
        {
            if (!amount.HasValue) return "0";
            return amount.Value.ToString("#,##0", UsCulture);
        }

        public static string FormatCurrencyVND(decimal? amount)
        {
            return FormatCurrency(amount) + " VNĐ";
        }

        public static string FormatCurrencyVND(double? amount)
        {
            return FormatCurrency(amount) + " VNĐ";
        }

        public static string FormatCurrencyVND(long? amount)
        {
            return FormatCurrency(amount) + " VNĐ";
        }
    }
}
