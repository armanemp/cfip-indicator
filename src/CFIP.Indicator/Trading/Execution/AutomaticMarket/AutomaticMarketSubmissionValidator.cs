using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryValidateAutomaticMarketSubmission(
            int closedM5,
            TradeType type,
            double entry,
            double target,
            double volume,
            out ExecutionIntent marketIntent,
            out string reason)
        {
            reason = "";
            marketIntent = null;

            if (!CanRunAutomaticEntry())
            {
                ApplyRuntimeEntryGate();
                reason = "RUNTIME ENTRY BLOCKED";
                return false;
            }

            // Final quote-sensitive recheck immediately before broker mutation.
            // This is intentionally later than plan construction so the current
            // executable price cannot be authorized by stale ActionableNow state.
            RefreshLiveDecisionActionability(
                closedM5);

            if (_decision == null ||
                !_decision.ActionableNow)
            {
                reason =
                    _decision == null
                        ? "NO DECISION"
                        : string.IsNullOrWhiteSpace(
                            _decision.ActionabilityReason)
                            ? "ENTRY NOT ACTIONABLE"
                            : _decision.ActionabilityReason;
                return false;
            }

            if (!EnsureTradingPermission())
            {
                reason =
                    "TRADING PERMISSION NOT GRANTED";
                return false;
            }

            if (!PassesAutoTradeSafetyGuards(
                    type,
                    volume,
                    out reason))
                return false;

            string suitabilityReason;
            if (!PassesMarketSuitability(
                    closedM5,
                    type == TradeType.Buy ? 1 : -1,
                    out suitabilityReason,
                    true))
            {
                reason =
                    "SUITABILITY • " +
                    suitabilityReason;
                return false;
            }

            if (_plan != null)
            {
                double atr =
                    Atr(
                        _m5Bars,
                        closedM5);

                PlanRewardRiskQualityResult rewardRisk =
                    PlanRewardRiskQualityRule.Evaluate(
                        type == TradeType.Buy ? 1 : -1,
                        entry,
                        _plan.Stop,
                        target,
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
                        "REWARD/RISK • " +
                        rewardRisk.Reason;
                    return false;
                }
            }

            marketIntent =
                BuildExecutionIntent(
                    _plan.Direction,
                    ConfirmedSignalsOnly
                        ? DecisionPolicyMode.Confirmed
                        : DecisionPolicyMode.Soft,
                    ExecutionIntentKind.Market,
                    entry,
                    _plan.EntryTrigger,
                    _plan.EntryZoneLow,
                    _plan.EntryZoneHigh,
                    _plan.Stop,
                    target,
                    volume,
                    closedM5,
                    "NORMAL MARKET");

            if (!ValidateExecutionIntent(
                    marketIntent,
                    entry,
                    out reason))
            {
                marketIntent = null;
                return false;
            }

            return true;
        }
    }
}
