using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double MinimumRequiredRR()
        {
            double atr =
                _m5Frame != null && _m5Frame.Atr > 0
                    ? _m5Frame.Atr
                    : (_m5Bars != null && _m5Bars.Count > 20
                        ? Atr(_m5Bars, _m5Bars.Count - 2)
                        : 0);

            double risk =
                _plan != null && _plan.Risk > 0
                    ? _plan.Risk
                    : Math.Max(
                        Symbol.PipSize,
                        Math.Max(0.20, MinimumSlAtr) * Math.Max(Symbol.PipSize, atr));

            return ResolveAdaptiveRequiredRR(
                risk,
                atr,
                ResolvePlanTargetSelectionLane());
        }

        private double MinimumRequiredRRForRegime(
            string regime)
        {
            double atr =
                _m5Frame != null && _m5Frame.Atr > 0
                    ? _m5Frame.Atr
                    : (_m5Bars != null && _m5Bars.Count > 20
                        ? Atr(_m5Bars, _m5Bars.Count - 2)
                        : 0);

            double risk =
                _plan != null && _plan.Risk > 0
                    ? _plan.Risk
                    : Math.Max(
                        Symbol.PipSize,
                        Math.Max(0.20, MinimumSlAtr) * Math.Max(Symbol.PipSize, atr));

            return ResolveAdaptiveRequiredRR(
                risk,
                atr,
                ResolvePlanTargetSelectionLane(),
                regime);
        }

        private double ResolveAdaptiveRequiredRR(
            double risk,
            double atr,
            OpportunityLane lane,
            string regimeOverride = null)
        {
            double riskAtr =
                risk > 0 && atr > 0
                    ? risk / Math.Max(Symbol.PipSize, atr)
                    : Math.Max(0.20, MinimumSlAtr);

            string regime =
                string.IsNullOrWhiteSpace(regimeOverride)
                    ? (_decision == null ? "UNKNOWN" : _decision.Regime)
                    : regimeOverride;

            return AdaptiveRewardRiskProfileRule.ResolveRequiredTp1RR(
                regime,
                lane,
                riskAtr,
                _decision == null ? 0 : _decision.Confidence,
                _decision == null ? 0 : _decision.SmartQuality,
                Math.Max(0, SmartTargetQuality),
                _plan == null ? 60 : Math.Max(0, _plan.StopQuality),
                Math.Max(1.0, MaximumTargetExtensionAtr));
        }
    }
}
