using System;

namespace cAlgo
{
    internal static class HealthyVolatilityRule
    {
        public static bool IsHealthy(
            double atr,
            double baselineAtr,
            double minimumRatio,
            double maximumRatio)
        {
            if (double.IsNaN(atr) ||
                double.IsInfinity(atr) ||
                double.IsNaN(baselineAtr) ||
                double.IsInfinity(baselineAtr) ||
                atr <= 0 ||
                baselineAtr <= 0)
                return false;

            double minimum =
                Math.Max(
                    0.50,
                    minimumRatio);

            double maximum =
                Math.Max(
                    minimum,
                    maximumRatio);

            double ratio =
                atr /
                baselineAtr;

            return ratio >= minimum &&
                   ratio <= maximum;
        }
    }
}
