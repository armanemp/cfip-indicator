using System;

namespace cAlgo
{
    internal static class DailyLossBaselineRule
    {
        internal static bool TryReconstruct(
            double currentEquity,
            double currentUnrealizedNetProfit,
            double realizedNetProfit,
            double netCashFlow,
            bool positionsOpen,
            out double startEquity,
            out double baselineUnrealizedNetProfit)
        {
            startEquity = 0;
            baselineUnrealizedNetProfit = 0;

            if (positionsOpen ||
                double.IsNaN(currentEquity) ||
                double.IsInfinity(currentEquity) ||
                currentEquity <= 0 ||
                double.IsNaN(currentUnrealizedNetProfit) ||
                double.IsInfinity(currentUnrealizedNetProfit) ||
                double.IsNaN(realizedNetProfit) ||
                double.IsInfinity(realizedNetProfit) ||
                double.IsNaN(netCashFlow) ||
                double.IsInfinity(netCashFlow))
                return false;

            // With no open positions, current unrealized P/L is expected to be
            // zero. The exact start-of-day equity can therefore be reconstructed
            // from today's current equity, realized net P/L and cash flow.
            startEquity =
                currentEquity -
                realizedNetProfit -
                netCashFlow;

            if (startEquity <= 0 ||
                double.IsNaN(startEquity) ||
                double.IsInfinity(startEquity))
            {
                startEquity = 0;
                return false;
            }

            return true;
        }
    }
}
