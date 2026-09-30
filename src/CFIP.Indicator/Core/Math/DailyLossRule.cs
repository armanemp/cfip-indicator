using System;

namespace cAlgo
{
    internal static class DailyLossRule
    {
        internal static DailyLossEvaluation Evaluate(
            double baselineEquity,
            double baselineUnrealizedNetProfit,
            double currentEquity,
            double currentUnrealizedNetProfit,
            double realizedNetProfit,
            double netCashFlow,
            double maximumDailyLossPercent,
            bool historyAvailable,
            bool previouslyLocked)
        {
            if (!IsFinitePositive(baselineEquity) ||
                !IsFinitePositive(currentEquity) ||
                double.IsNaN(baselineUnrealizedNetProfit) ||
                double.IsInfinity(baselineUnrealizedNetProfit))
            {
                return DailyLossEvaluation.Unavailable(
                    "DAILY LOSS BASELINE UNAVAILABLE");
            }

            double dailyNetPnl;
            bool usedEquityFallback = false;

            if (historyAvailable)
            {
                dailyNetPnl =
                    realizedNetProfit +
                    (currentUnrealizedNetProfit -
                     baselineUnrealizedNetProfit);
            }
            else
            {
                dailyNetPnl =
                    currentEquity -
                    baselineEquity -
                    netCashFlow;

                usedEquityFallback = true;
            }

            if (previouslyLocked)
            {
                return new DailyLossEvaluation(
                    true,
                    true,
                    true,
                    Math.Max(
                        0,
                        -dailyNetPnl),
                    CalculatePercent(
                        Math.Max(0, -dailyNetPnl),
                        baselineEquity),
                    dailyNetPnl,
                    realizedNetProfit,
                    currentUnrealizedNetProfit -
                        baselineUnrealizedNetProfit,
                    netCashFlow,
                    usedEquityFallback,
                    "DAILY LOSS LIMIT ALREADY LOCKED");
            }

            double lossAmount =
                Math.Max(
                    0,
                    -dailyNetPnl);

            double lossPercent =
                CalculatePercent(
                    lossAmount,
                    baselineEquity);

            double threshold =
                Math.Max(
                    0,
                    maximumDailyLossPercent);

            bool limitHit =
                lossPercent >= threshold &&
                threshold > 0;

            return new DailyLossEvaluation(
                true,
                limitHit,
                limitHit,
                lossAmount,
                lossPercent,
                dailyNetPnl,
                realizedNetProfit,
                currentUnrealizedNetProfit -
                    baselineUnrealizedNetProfit,
                netCashFlow,
                usedEquityFallback,
                limitHit
                    ? "DAILY LOSS LIMIT REACHED"
                    : "DAILY LOSS WITHIN LIMIT");
        }

        private static double CalculatePercent(
            double amount,
            double baseline)
        {
            if (!IsFinitePositive(baseline) ||
                double.IsNaN(amount) ||
                double.IsInfinity(amount))
                return 0;

            return amount /
                   baseline *
                   100.0;
        }

        private static bool IsFinitePositive(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
