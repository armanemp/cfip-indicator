using System;

namespace cAlgo
{
    internal static class OssIndicatorParameters
    {
        internal const int RollingQuoteWindowSize = 161;

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
