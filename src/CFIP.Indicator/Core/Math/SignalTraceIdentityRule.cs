using System;
using System.Text;

namespace cAlgo
{
    internal static class SignalTraceIdentityRule
    {
        public const string CurrentPrefix = "CFIP-ST1";

        public static string Build(
            string symbol,
            string timeframe,
            string accountScope,
            string configurationFingerprint,
            long barOpenTimeUtcTicks)
        {
            if (barOpenTimeUtcTicks <= 0)
                return string.Empty;

            return
                CurrentPrefix +
                "|" +
                NormalizePart(symbol) +
                "|" +
                NormalizePart(timeframe) +
                "|" +
                NormalizePart(accountScope) +
                "|" +
                NormalizePart(configurationFingerprint) +
                "|" +
                barOpenTimeUtcTicks.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string NormalizePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "UNKNOWN";

            StringBuilder result = new StringBuilder(value.Length);

            foreach (char ch in value.Trim())
            {
                result.Append(
                    ch == '|' || char.IsControl(ch)
                        ? '_'
                        : ch);
            }

            return
                result.Length == 0
                    ? "UNKNOWN"
                    : result.ToString();
        }
    }
}
