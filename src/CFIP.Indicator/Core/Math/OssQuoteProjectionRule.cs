using System;

namespace cAlgo
{
    internal static class OssQuoteProjectionRule
    {
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
