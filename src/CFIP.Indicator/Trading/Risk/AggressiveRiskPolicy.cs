using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double EffectiveAggressiveRiskPercent()
        {
            return RiskPercentPolicy.Calculate(
                AggressiveRiskPercentEquity,
                UseSmartRiskScaling,
                SuitabilityRiskMultiplier());
        }
    }
}
