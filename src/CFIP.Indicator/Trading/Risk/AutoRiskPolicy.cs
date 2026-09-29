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
                AdaptiveOutcomeRiskPolicy.Calculate(
                    _outcomeHistory,
                    suitability);

            return RiskPercentPolicy.Calculate(
                RiskPercentEquity,
                UseSmartRiskScaling,
                outcome);
        }
        private double OutcomeRiskMultiplier()
        {
            return AdaptiveOutcomeRiskPolicy.Calculate(
                _outcomeHistory,
                1.0);
        }
    }
}
