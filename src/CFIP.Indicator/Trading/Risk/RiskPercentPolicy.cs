namespace cAlgo
{
    internal static class RiskPercentPolicy
    {
        public static double Calculate(
            double baseRiskPercent,
            bool smartScalingEnabled,
            double suitabilityMultiplier)
        {
            double baseRisk =
                NumericGuards.Clamp(
                    baseRiskPercent,
                    0.05,
                    100.0);

            if (!smartScalingEnabled)
                return baseRisk;

            double multiplier =
                NumericGuards.Clamp(
                    suitabilityMultiplier,
                    0.25,
                    1.0);

            return NumericGuards.Clamp(
                baseRisk * multiplier,
                0.05,
                baseRisk);
        }
    }
}
