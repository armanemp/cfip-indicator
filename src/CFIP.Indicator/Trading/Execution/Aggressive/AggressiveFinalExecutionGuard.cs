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
            double atr,
            double stop,
            double target,
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

            int expectedDirection =
                tradeType == TradeType.Buy
                    ? 1
                    : -1;

            if (_reaction.Direction != expectedDirection)
            {
                reason =
                    "AGGRESSIVE • REACTION/TRADE DIRECTION MISMATCH";
                return false;
            }

            if (AggressiveRequireSmartAgreement &&
                (_decision == null ||
                 _decision.Direction != _reaction.Direction ||
                 _decision.SmartQuality < AggressiveMinimumSmartQuality))
            {
                reason =
                    "AGGRESSIVE • SMART AGREEMENT DIRECTION";
                return false;
            }

            if (!IsFinitePositive(entry) ||
                !IsFinitePositive(atr) ||
                !IsFinitePositive(stop) ||
                !IsFinitePositive(target))
            {
                reason =
                    "AGGRESSIVE • INVALID EXECUTION GEOMETRY";
                return false;
            }

            PlanRewardRiskQualityResult rewardRisk =
                PlanRewardRiskQualityRule.Evaluate(
                    expectedDirection,
                    entry,
                    stop,
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
                    "AGGRESSIVE • REWARD/RISK • " +
                    rewardRisk.Reason;
                return false;
            }

            return true;
        }
    }
}
