namespace cAlgo
{
    internal static class RiskAmountCalculator
    {
        public static double Calculate(
            double equity,
            double riskPercent)
        {
            if (!NumericGuards.IsFinitePositive(equity) ||
                !NumericGuards.IsFinitePositive(riskPercent))
                return 0;

            double amount =
                equity * riskPercent / 100.0;

            return NumericGuards.IsFinitePositive(amount)
                ? amount
                : 0;
        }
    }
}
