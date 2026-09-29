using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double EffectiveAutoRiskPercent()
        {
            double suitability =
                SuitabilityRiskMultiplier();

            double outcome =
                EnableOutcomeTelemetry
                    ? AdaptiveOutcomeRiskPolicy.Calculate(
                        _outcomeHistory,
                        suitability)
                    : suitability;

            return RiskPercentPolicy.Calculate(
                RiskPercentEquity,
                UseSmartRiskScaling,
                outcome);
        }
        private double OutcomeRiskMultiplier()
        {
            return EnableOutcomeTelemetry
                ? AdaptiveOutcomeRiskPolicy.Calculate(
                    _outcomeHistory,
                    1.0)
                : 1.0;
        }
    }
}
