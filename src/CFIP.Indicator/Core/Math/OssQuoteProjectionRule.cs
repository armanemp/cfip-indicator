using System;

namespace cAlgo
{
    internal static class OssQuoteProjectionRule
    {
        internal static bool TryNormalizePrice(
            double price,
            out decimal normalized)
        {
            normalized = 0m;

            if (double.IsNaN(price) ||
                double.IsInfinity(price) ||
                price <= 0 ||
                price > (double)decimal.MaxValue)
                return false;

            normalized = (decimal)price;
            return normalized > 0m;
        }

        internal static decimal NormalizeVolume(
            double volume)
        {
            if (double.IsNaN(volume) ||
                double.IsInfinity(volume) ||
                volume <= 0)
                return 0m;

            double decimalMaximum =
                (double)decimal.MaxValue;

            return volume >= decimalMaximum
                ? decimal.MaxValue
                : (decimal)volume;
        }
    }
}
