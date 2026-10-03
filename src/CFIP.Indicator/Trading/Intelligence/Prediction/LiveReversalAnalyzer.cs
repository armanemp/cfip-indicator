// ============================================================================
// CFIP Indicator — LiveReversalAnalyzer.cs
// ============================================================================

using System;
using CFIP.Contracts;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool CheckLiveReversalAgainstPlan(
            int closedM5)
        {
            if (_plan == null ||
                !_plan.IsLivePosition ||
                _m5Frame == null ||
                !EnableLiveStructuralReversal ||
                !EnableFastReversalIntelligence)
            {
                ResetReversalEpisode();
                return false;
            }

            int opposite =
                LiveReversalDecisionRule.OppositeDirection(
                    _plan.Direction);

            if (opposite == 0 ||
                _plan.PositionId <= 0)
            {
                ResetReversalEpisode();
                return false;
            }

            Position livePosition =
                GetManagedLivePositionForPlan();

            if (livePosition == null)
            {
                MarkBrokerStateDirty();

                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "ACTIVE PLAN REVERSAL • POSITION STATE RECONCILING");

                SetAutoTradingState(
                    "WAIT",
                    "REVERSAL DETECTED • POSITION STATE RECONCILING");

                return true;
            }

            ResetReversalEpisodeIfPositionChanged(
                livePosition.Id);

            bool structural =
                opposite == 1
                    ? (_m5Frame.MssBull ||
                       _m5Frame.ChochBull)
                    : (_m5Frame.MssBear ||
                       _m5Frame.ChochBear);

            bool force =
                opposite == 1
                    ? (_m5Frame.DisplacementBull &&
                       _m5Frame.LiquidityBull)
                    : (_m5Frame.DisplacementBear &&
                       _m5Frame.LiquidityBear);

            int reactionDirection =
                _reaction != null
                    ? _reaction.Direction
                    : 0;

            int reactionConfidence =
                _reaction != null
                    ? _reaction.Confidence
                    : 0;

            int reversalConfidence =
                LiveReversalDecisionRule.ResolveDirectionalConfidence(
                    _plan.Direction,
                    reactionDirection,
                    reactionConfidence,
                    _m5Frame.Direction,
                    _m5Frame.Quality);

            int minimumConfidence =
                ExecutionThresholdPolicy.NormalizeLiveReversalConfidence(
                    LiveReversalMinimumConfidence);

            int minimumEvidence =
                ExecutionThresholdPolicy.NormalizeReversalEvidence(
                    LiveReversalMinimumEvidence);

            int structuralScore =
                ExecutionThresholdPolicy.NormalizeLiveReversalStructuralScore(
                    LiveReversalStructuralScore);

            if (reversalConfidence < minimumConfidence ||
                !structural ||
                _m5Frame.Quality < structuralScore ||
                _m5Frame.Evidence < minimumEvidence)
                return false;

            if (RequireReversalForce &&
                !force)
                return false;

            if (!AllowReversalAgainstStaleHtf &&
                _m15Frame != null &&
                _m15Frame.Direction ==
                    _plan.Direction)
                return false;

            if (RequireM15ReversalForOpposite &&
                _m15Frame != null &&
                _m15Frame.Direction !=
                    opposite)
                return false;

            if (StructuralSequence(
                    _m5Bars,
                    closedM5,
                    opposite) <
                MinimumOppositeM5Structure)
                return false;

            if (PreventRapidDirectionFlip &&
                _lastSignalM5 >= 0 &&
                closedM5 -
                    _lastSignalM5 <
                    OppositeSignalCooldownM5)
                return false;

            int flipBars =
                Math.Max(
                    1,
                    SmartFlipConfirmationBars);

            if (flipBars > 1 &&
                !StableDirection(
                    _m5Bars,
                    closedM5,
                    opposite,
                    flipBars))
                return false;

            if (TryMarkReversalAlertEmitted(
                    livePosition.Id,
                    opposite))
            {
                SendUnifiedAlert(
                    "REVERSAL|" +
                    closedM5 +
                    "|" +
                    livePosition.Id +
                    "|" +
                    opposite,
                    "CFIP ACTIVE PLAN REVERSAL DETECTED | " +
                    (opposite == 1
                        ? "BUY"
                        : "SELL") +
                    " REVERSAL | Q " +
                    reversalConfidence +
                    " | EVID " +
                    _m5Frame.Evidence,
                    opposite,
                    true);
            }

            LiveReversalAction action =
                LiveReversalDecisionRule.ResolveAction(
                    true,
                    EnableReversalProtectionClose,
                    livePosition.NetProfit > 0,
                    _lifecycleState ==
                        LifecycleState.ExitRequested);

            if (action ==
                LiveReversalAction.AwaitBrokerConfirmation)
            {
                SetAutoTradingState(
                    "WAIT",
                    "REVERSAL EXIT AWAITING BROKER CONFIRMATION");

                return true;
            }

            if (action ==
                LiveReversalAction.DetectedRetain)
            {
                SetAutoTradingState(
                    "WAIT",
                    "REVERSAL DETECTED • POSITION RETAINED");

                return false;
            }

            if (action !=
                LiveReversalAction.ExitRequested)
                return true;

            double protectedProfit =
                livePosition.NetProfit;

            SetLifecycleState(
                LifecycleState.ExitRequested,
                "ACTIVE PLAN REVERSAL");

            ManagementCommandRequestStatus closeStatus =
                RequestClosePosition(
                    livePosition,
                    "ACTIVE PLAN REVERSAL");
            if (!closeStatus.IsAccepted())
            {
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "ACTIVE PLAN REVERSAL • EXIT REJECTED");

                SetAutoTradingState(
                    "ERROR",
                    "REVERSAL CLOSE REJECTED");

                return false;
            }

            _lastExitM5 =
                Math.Max(
                    _lastExitM5,
                    closedM5);

            SetAutoTradingState(
                "EXIT_REQUESTED",
                "REVERSAL EXIT REQUESTED #" +
                livePosition.Id);

            SendUnifiedAlert(
                "REVERSAL-CLOSE|" +
                livePosition.Id,
                "CFIP ACTIVE PLAN REVERSAL • EXIT REQUESTED | #" +
                livePosition.Id +
                " | protected +" +
                protectedProfit.ToString("F2"),
                _plan.Direction,
                true);

            return true;
        }

        private Position GetManagedLivePositionForPlan()
        {
            if (_plan == null ||
                !_plan.IsLivePosition ||
                _plan.PositionId <= 0)
                return null;

            foreach (Position position in Positions)
            {
                if (position != null &&
                    position.SymbolName == SymbolName &&
                    position.Id == _plan.PositionId)
                    return position;
            }

            return null;
        }
    }
}
