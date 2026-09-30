using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PassAggressivePreTradeEligibility(
            int closedM5)
        {
            if (!AutoTradingEnabled ||
                !EnableAggressiveAutoEntry ||
                _lifecycleState ==
                    LifecycleState.ExitRequested ||
                _plan != null ||
                _reaction == null ||
                _reaction.Direction == 0)
            {
                _aggressiveEntryPolicy.ResetQualification();
                _autoExecutionBlockReason =
                    "AGGRESSIVE • SETUP NOT ELIGIBLE";
                return false;
            }

            bool intrabarReactionQualified =
                ReactionQualificationRule.IsQualified(
                    _reaction.Direction,
                    _reaction.ReactionIntrabarQuality,
                    _reaction.ReactionIntrabarEvidence,
                    _reaction.ReactionHasContext,
                    Math.Max(
                        50,
                        Math.Max(
                            FastReversalMinimumQuality,
                            LiveReactionThreshold)),
                    Math.Max(
                        2,
                        LiveReversalMinimumEvidence),
                    false,
                    false);

            if (!intrabarReactionQualified)
            {
                _aggressiveEntryPolicy.ResetQualification();
                _autoExecutionBlockReason =
                    "AGGRESSIVE • REACTION CONTEXT";
                return false;
            }

            int reactionM5 =
                _m5Bars == null
                    ? -1
                    : _m5Bars.Count - 1;

            bool intrabarQualified =
                _aggressiveEntryPolicy.ObserveReactionSample(
                    reactionM5,
                    _reaction.Direction,
                    _reaction.EntryAllowed,
                    _lastReactionCalcUtc);

            if (!intrabarQualified)
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • " +
                    _aggressiveEntryPolicy.GetQualificationStateText();
                return false;
            }

            _autoExecutionBlockReason =
                "AGGRESSIVE • INTRABAR ARMED";

            string capacityReason;

            if (!ValidateSingleExecutionCapacity(
                    out capacityReason))
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • " +
                    capacityReason;
                SetAutoTradingState(
                    "BLOCKED",
                    "AGGRESSIVE • " +
                    capacityReason);
                return false;
            }

            if (GetManagedPendingOrder() != null)
            {
                _autoExecutionBlockReason =
                    "PENDING ORDER EXISTS";
                return false;
            }

            string dailyLossReason;

            if (DailyLossLimitHit(
                    TimeInUtc,
                    out dailyLossReason))
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • " +
                    dailyLossReason;
                return false;
            }

            if (OneOrderPerSignal &&
                _lastAutoM5 ==
                closedM5)
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • ALREADY TRADED THIS M5";
                return false;
            }

            if (_reaction.Confidence <
                    AggressiveMinimumConfidence ||
                _reaction.IndependentEvidence <
                    AggressiveMinimumEvidence)
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • CONF " +
                    _reaction.Confidence +
                    " / EVID " +
                    _reaction.IndependentEvidence;
                return false;
            }

            if (AggressiveRequireSmartAgreement &&
                (_decision == null ||
                 _decision.Direction !=
                 _reaction.Direction ||
                 _decision.SmartQuality <
                 AggressiveMinimumSmartQuality))
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • SMART AGREEMENT";
                return false;
            }

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    closedM5,
                    _reaction.Direction,
                    out suitabilityReason))
            {
                _autoExecutionBlockReason =
                    "AGGRESSIVE • " +
                    suitabilityReason;
                SetAutoTradingState(
                    "BLOCKED",
                    _autoExecutionBlockReason);
                return false;
            }

            return true;
        }
    }
}
