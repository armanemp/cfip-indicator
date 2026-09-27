using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double EffectiveAutoRiskPercent()
        {
            return RiskPercentPolicy.Calculate(
                RiskPercentEquity,
                UseSmartRiskScaling,
                SuitabilityRiskMultiplier());
        }
    }
}
