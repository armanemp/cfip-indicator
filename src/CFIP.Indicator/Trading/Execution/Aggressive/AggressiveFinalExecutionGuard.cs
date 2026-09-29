using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryValidateAggressiveFinalExecution(
            int closedM5,
            TradeType tradeType,
            double entry,
            double volume,
            out string reason)
        {
            reason = string.Empty;

            if (!CanRunAutomaticEntry())
            {
                ApplyRuntimeEntryGate();
                reason = "AGGRESSIVE • RUNTIME ENTRY BLOCKED";
                return false;
            }

            RefreshLiveDecisionActionability(
                closedM5);

            if (_reaction == null ||
                _reaction.Direction == 0)
            {
                reason = "AGGRESSIVE • NO REACTION";
                return false;
            }

            if (_decision == null ||
                !_decision.ActionableNow)
            {
                reason =
                    "AGGRESSIVE • " +
                    (_decision == null
                        ? "NO DECISION"
                        : string.IsNullOrWhiteSpace(
                            _decision.ActionabilityReason)
                            ? "ENTRY NOT ACTIONABLE"
                            : _decision.ActionabilityReason);
                return false;
            }

            if (!EnsureTradingPermission())
            {
                reason =
                    "AGGRESSIVE • TRADING PERMISSION NOT GRANTED";
                return false;
            }

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    closedM5,
                    _reaction.Direction,
                    out suitabilityReason,
                    true))
            {
                reason =
                    "AGGRESSIVE • SUITABILITY • " +
                    suitabilityReason;
                return false;
            }

            if (!IsFinitePositive(entry))
            {
                reason =
                    "AGGRESSIVE • INVALID LIVE ENTRY";
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
                        tradeType == TradeType.Buy ? 1 : -1,
                        entry,
                        _plan.Stop,
                        _plan.Tp1,
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
                        Math.Min(
                            Math.Max(
                                MinimumSlAtr,
                                MaximumSlAtr),
                            Math.Max(
                                MinimumSlAtr,
                                MaximumStructuralStopAtr)));

                if (!rewardRisk.Allowed)
                {
                    reason =
                        "AGGRESSIVE • REWARD/RISK • " +
                        rewardRisk.Reason;
                    return false;
                }
            }

            return true;
        }
    }
}
