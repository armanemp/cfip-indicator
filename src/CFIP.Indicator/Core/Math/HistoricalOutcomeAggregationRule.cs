using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal readonly struct HistoricalOutcomeAggregate
    {
        public bool Available { get; }
        public int TradeCount { get; }
        public double NetProfit { get; }
        public double GrossProfit { get; }
        public double Swap { get; }
        public double Commissions { get; }
        public double Pips { get; }
        public DateTime ClosingTime { get; }

        public HistoricalOutcomeAggregate(
            bool available,
            int tradeCount,
            double netProfit,
            double grossProfit,
            double swap,
            double commissions,
            double pips,
            DateTime closingTime)
        {
            Available = available;
            TradeCount = Math.Max(0, tradeCount);
            NetProfit = netProfit;
            GrossProfit = grossProfit;
            Swap = swap;
            Commissions = commissions;
            Pips = pips;
            ClosingTime = closingTime;
        }
    }

    internal sealed class HistoricalOutcomeRecord
    {
        public double NetProfit;
        public double GrossProfit;
        public double Swap;
        public double Commissions;
        public double Pips;
        public DateTime ClosingTime;
    }

    internal static class HistoricalOutcomeAggregationRule
    {
        public static HistoricalOutcomeAggregate Aggregate(
            IEnumerable<HistoricalOutcomeRecord> trades)
        {
            if (trades == null)
                return Empty();

            int count = 0;
            double netProfit = 0;
            double grossProfit = 0;
            double swap = 0;
            double commissions = 0;
            double pips = 0;
            DateTime closingTime = DateTime.MinValue;

            foreach (HistoricalOutcomeRecord trade in trades)
            {
                if (trade == null ||
                    !IsFiniteOutcomeValue(trade.NetProfit) ||
                    !IsFiniteOutcomeValue(trade.GrossProfit) ||
                    !IsFiniteOutcomeValue(trade.Swap) ||
                    !IsFiniteOutcomeValue(trade.Commissions) ||
                    !IsFiniteOutcomeValue(trade.Pips))
                    continue;

                count++;
                netProfit += trade.NetProfit;
                grossProfit += trade.GrossProfit;
                swap += trade.Swap;
                commissions += trade.Commissions;
                pips += trade.Pips;

                if (trade.ClosingTime > closingTime)
                    closingTime = trade.ClosingTime;
            }

            return new HistoricalOutcomeAggregate(
                count > 0,
                count,
                netProfit,
                grossProfit,
                swap,
                commissions,
                pips,
                closingTime);
        }

        public static HistoricalOutcomeAggregate FromPositionFallback(
            double netProfit,
            double pips)
        {
            if (!IsFiniteOutcomeValue(netProfit) ||
                !IsFiniteOutcomeValue(pips))
                return Empty();

            return new HistoricalOutcomeAggregate(
                true,
                1,
                netProfit,
                netProfit,
                0,
                0,
                pips,
                DateTime.MinValue);
        }

        private static HistoricalOutcomeAggregate Empty()
        {
            return new HistoricalOutcomeAggregate(
                false,
                0,
                0,
                0,
                0,
                0,
                0,
                DateTime.MinValue);
        }

        private static bool IsFiniteOutcomeValue(
            double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}
