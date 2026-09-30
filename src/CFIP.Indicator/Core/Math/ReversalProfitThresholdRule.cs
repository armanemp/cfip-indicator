// CFIP Indicator — ReversalProfitThresholdRule.cs
// Platform-neutral minimum-profit invariant for protective reversal exits.

namespace cAlgo
{
    internal static class ReversalProfitThresholdRule
    {
        public static bool MeetsMinimumNetProfit(
            double netProfit,
            double minimumNetProfit)
        {
            return
                NumericGuards.IsFiniteValue(netProfit) &&
                NumericGuards.IsFiniteValue(minimumNetProfit) &&
                minimumNetProfit >= 0 &&
                netProfit > minimumNetProfit;
        }
    }
}
