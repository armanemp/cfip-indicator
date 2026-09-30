using System;

namespace cAlgo
{
    internal static class DailyLossRule
    {
        internal static Evaluation Evaluate(
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
            if (!HasFinitePositiveValue(baselineEquity) ||
                !HasFinitePositiveValue(currentEquity) ||
                double.IsNaN(baselineUnrealizedNetProfit) ||
                double.IsInfinity(baselineUnrealizedNetProfit))
            {
                return Evaluation.Unavailable(
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
                return new Evaluation(
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

            return new Evaluation(
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

        internal sealed class Evaluation
        {
            internal bool DataReady { get; }
            internal bool LimitHit { get; }
            internal bool Locked { get; }
            internal double LossAmount { get; }
            internal double LossPercent { get; }
            internal double DailyNetPnl { get; }
            internal double RealizedNetProfit { get; }
            internal double FloatingPnlChange { get; }
            internal double NetCashFlow { get; }
            internal bool UsedEquityFallback { get; }
            internal string Reason { get; }

            internal Evaluation(
                bool dataReady,
                bool limitHit,
                bool locked,
                double lossAmount,
                double lossPercent,
                double dailyNetPnl,
                double realizedNetProfit,
                double floatingPnlChange,
                double netCashFlow,
                bool usedEquityFallback,
                string reason)
            {
                DataReady = dataReady;
                LimitHit = limitHit;
                Locked = locked;
                LossAmount = lossAmount;
                LossPercent = lossPercent;
                DailyNetPnl = dailyNetPnl;
                RealizedNetProfit = realizedNetProfit;
                FloatingPnlChange = floatingPnlChange;
                NetCashFlow = netCashFlow;
                UsedEquityFallback = usedEquityFallback;
                Reason = reason ?? "";
            }

            internal static Evaluation Unavailable(
                string reason)
            {
                return new Evaluation(
                    false,
                    false,
                    false,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    false,
                    reason);
            }
        }

        private static double CalculatePercent(
            double amount,
            double baseline)
        {
            if (!HasFinitePositiveValue(baseline) ||
                double.IsNaN(amount) ||
                double.IsInfinity(amount))
                return 0;

            return amount /
                   baseline *
                   100.0;
        }

        private static bool HasFinitePositiveValue(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
