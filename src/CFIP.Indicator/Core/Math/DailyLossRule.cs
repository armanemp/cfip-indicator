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
            bool transactionsAvailable,
            bool previouslyLocked)
        {
            if (!HasFinitePositiveValue(baselineEquity) ||
                !HasFinitePositiveValue(currentEquity) ||
                !IsFiniteDailyLossValue(baselineUnrealizedNetProfit) ||
                !IsFiniteDailyLossValue(currentUnrealizedNetProfit) ||
                !IsFiniteDailyLossValue(realizedNetProfit) ||
                !IsFiniteDailyLossValue(netCashFlow) ||
                !IsFiniteDailyLossValue(maximumDailyLossPercent) ||
                maximumDailyLossPercent < 0)
            {
                return DailyLossEvaluation.Unavailable(
                    "DAILY LOSS DATA UNAVAILABLE");
            }

            if (!transactionsAvailable)
            {
                return DailyLossEvaluation.Unavailable(
                    "DAILY LOSS TRANSACTION DATA UNAVAILABLE");
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

            if (!IsFiniteDailyLossValue(dailyNetPnl))
            {
                return DailyLossEvaluation.Unavailable(
                    "DAILY LOSS PNL UNAVAILABLE");
            }

            if (previouslyLocked)
            {
                return new DailyLossEvaluation(
                    true,
                    true,
                    true,
                    Math.Max(0, -dailyNetPnl),
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
                Math.Max(0, -dailyNetPnl);

            double lossPercent =
                CalculatePercent(
                    lossAmount,
                    baselineEquity);

            bool limitHit =
                maximumDailyLossPercent > 0 &&
                lossPercent >= maximumDailyLossPercent;

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
            if (!HasFinitePositiveValue(baseline) ||
                !IsFiniteDailyLossValue(amount))
                return 0;

            return amount /
                   baseline *
                   100.0;
        }

        private static bool HasFinitePositiveValue(double value)
        {
            return value > 0 &&
                   IsFiniteDailyLossValue(value);
        }

        private static bool IsFiniteDailyLossValue(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}