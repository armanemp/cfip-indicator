using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidatePendingSubmission(
            ExecutionIntent intent,
            TradeType tradeType,
            double marketReference,
            double volume,
            string prefix,
            out string reason)
        {
            reason = "";

            if (!CanRunAutomaticEntry())
            {
                ApplyRuntimeEntryGate();
                reason =
                    prefix +
                    "RUNTIME ENTRY BLOCKED";
                return false;
            }

            if (!EnsureTradingPermission())
            {
                reason =
                    prefix +
                    "TRADING PERMISSION NOT GRANTED";
                return false;
            }

            string intentReason;

            if (!ValidateExecutionIntent(
                    intent,
                    marketReference,
                    out intentReason))
            {
                reason =
                    prefix +
                    intentReason;
                return false;
            }

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    intent.CreatedM5,
                    intent.Direction,
                    out suitabilityReason,
                    true))
            {
                reason =
                    prefix +
                    "SUITABILITY " +
                    suitabilityReason;
                return false;
            }

            if (_m5Bars != null &&
                intent != null)
            {
                double atr =
                    Atr(
                        _m5Bars,
                        Math.Max(
                            0,
                            Math.Min(
                                intent.CreatedM5,
                                _m5Bars.Count - 1)));

                PlanRewardRiskQualityResult rewardRisk =
                    PlanRewardRiskQualityRule.Evaluate(
                        intent.Direction,
                        intent.RequestedEntry,
                        intent.Stop,
                        intent.Target,
                        atr,
                        Math.Max(
                            0,
                            Symbol.Ask - Symbol.Bid),
                        Math.Max(
                            Tp1MinimumRR,
                            MinimumRequiredRRForRegime(
                                _decision == null
                                    ? "UNKNOWN"
                                    : _decision.Regime)),
                        PreferredStopRiskAtr,
                        StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(
                        MinimumSlAtr,
                        MaximumSlAtr,
                        MaximumStructuralStopAtr),
                        Math.Max(0, MaximumRewardRR),
                        Symbol.PipSize);

                if (!rewardRisk.Allowed)
                {
                    reason =
                        prefix +
                        "REWARD/RISK " +
                        rewardRisk.Reason;
                    return false;
                }
            }

            if (_m5Frame != null)
            {
                IndicatorQualityGateResult indicatorGate =
                    IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                        IndicatorQualityGateStage.PendingSubmission,
                        _m5Frame.IndicatorConfluenceQuality,
                        _m5Frame.IndicatorConflict);

                if (!indicatorGate.Allowed)
                {
                    reason =
                        prefix +
                        indicatorGate.Reason;
                    return false;
                }
            }

            string safetyReason;

            if (!PassesAutoTradeSafetyGuards(
                    tradeType,
                    volume,
                    out safetyReason))
            {
                reason =
                    prefix +
                    safetyReason;
                return false;
            }

            return true;
        }

        private DateTime PendingExpiration()
        {
            return
                TimeInUtc.AddMinutes(
                    Math.Max(
                        15,
                        PendingOrderExpiryMinutes));
        }
    }
}
