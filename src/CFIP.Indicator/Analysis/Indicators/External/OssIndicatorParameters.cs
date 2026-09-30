using System;

namespace cAlgo
{
    internal static class OssIndicatorParameters
    {
        internal const int RollingQuoteWindowSize = 161;

        internal const int MacdSignalPeriod = 9;

        internal const int BollingerPeriod = 20;
        internal const double BollingerStandardDeviations = 2.0;

        internal const int MfiPeriod = 14;

        internal const int StochLookbackPeriod = 14;
        internal const int StochSignalPeriod = 3;
        internal const int StochSmoothPeriod = 3;

        internal const int SuperTrendPeriod = 10;
        internal const double SuperTrendMultiplier = 3.0;

        internal const int AroonPeriod = 25;
        internal const int CciPeriod = 20;

        internal const double ParabolicSarAccelerationFactor = 0.02;
        internal const double ParabolicSarMaximumAccelerationFactor = 0.20;

        internal const int RsiMinimumHistory = 20;
        internal const int BollingerMinimumHistory = 40;
        internal const int MfiMinimumHistory = 40;
        internal const int StochMinimumHistory = 40;
        internal const int SuperTrendMinimumHistory = 60;
        internal const int AroonMinimumHistory = 40;
        internal const int CciMinimumHistory = 40;
        internal const int ObvMinimumHistory = 3;
        internal const int ParabolicSarMinimumHistory = 60;
        internal const int MacdWarmupMargin = 20;

        internal static int SafeRsiPeriod(int configuredPeriod)
        {
            return Math.Max(2, configuredPeriod);
        }

        internal static int SafeMacdFastPeriod(int configuredPeriod)
        {
            return Math.Max(2, configuredPeriod);
        }

        internal static int SafeMacdSlowPeriod(
            int configuredFastPeriod,
            int configuredSlowPeriod)
        {
            return Math.Max(
                SafeMacdFastPeriod(configuredFastPeriod) + 1,
                configuredSlowPeriod);
        }

        internal static int RsiHistoryRequired(int configuredPeriod)
        {
            return Math.Max(
                RsiMinimumHistory,
                SafeRsiPeriod(configuredPeriod) + 5);
        }

        internal static int MacdHistoryRequired(
            int configuredFastPeriod,
            int configuredSlowPeriod)
        {
            return SafeMacdSlowPeriod(
                       configuredFastPeriod,
                       configuredSlowPeriod) +
                   MacdWarmupMargin;
        }
    }
}
