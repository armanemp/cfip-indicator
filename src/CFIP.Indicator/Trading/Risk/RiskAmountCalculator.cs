namespace cAlgo
{
    internal static class RiskAmountCalculator
    {
        public static double Calculate(
            double equity,
            double riskPercent)
        {
            if (equity <= 0 ||
                riskPercent <= 0)
                return 0;

            return equity * riskPercent / 100.0;
        }
    }
}
