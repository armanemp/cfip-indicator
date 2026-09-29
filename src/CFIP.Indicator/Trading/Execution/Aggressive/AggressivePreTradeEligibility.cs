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
                !_reaction.EntryAllowed ||
                _reaction.Direction == 0)
            {
                _aggressiveEntryPolicy.ResetQualification();
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

            if (!ValidateConfiguredPositionCapacity(
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

            if (DailyLossLimitHit(
                    TimeInUtc))
                return false;

            if (OneOrderPerSignal &&
                _lastAutoM5 ==
                closedM5)
                return false;

            if (_reaction.Confidence <
                    AggressiveMinimumConfidence ||
                _reaction.IndependentEvidence <
                    AggressiveMinimumEvidence)
                return false;

            if (AggressiveRequireSmartAgreement &&
                (_decision == null ||
                 _decision.Direction !=
                 _reaction.Direction ||
                 _decision.SmartQuality <
                 AggressiveMinimumSmartQuality))
                return false;

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    closedM5,
                    _reaction.Direction,
                    out suitabilityReason))
            {
                SetAutoTradingState(
                    "BLOCKED",
                    "AGGRESSIVE • " +
                    suitabilityReason);
                return false;
            }

            if (ManagedPositionCount() >=
                Math.Max(
                    1,
                    MaximumOpenPositions))
                return false;

            return true;
        }
    }
}
