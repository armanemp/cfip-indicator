using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int ResolveCanonicalVisualDirection(
            int closedM5,
            int pendingDirection,
            bool livePlan,
            bool preTradePlanVisible,
            bool decisionReady,
            bool reactionReady,
            bool predictionReady)
        {
            if (!UseAuthoritativeSignalState)
            {
                return decisionReady &&
                       _decision != null
                    ? _decision.Direction
                    : 0;
            }

            if (livePlan &&
                _plan != null)
                return _plan.Direction;

            if (pendingDirection != 0)
                return pendingDirection;

            if (preTradePlanVisible &&
                _plan != null &&
                (_plan.Direction == 1 ||
                 _plan.Direction == -1))
                return _plan.Direction;

            if (_decision != null &&
                _decision.Direction != 0)
                return _decision.Direction;

            if (reactionReady &&
                _reaction != null)
                return _reaction.Direction;

            MarketRegimeSnapshot activeRegime =
                GetActiveM5Regime(
                    closedM5);

            if (activeRegime != null &&
                (activeRegime.Regime == "RANGE" ||
                 activeRegime.Regime == "COMPRESSION"))
            {
                if (predictionReady &&
                    _prediction != null)
                    return _prediction.Direction;

                return 0;
            }

            if (_m5Frame != null &&
                (_m5Frame.Direction == 1 ||
                 _m5Frame.Direction == -1) &&
                _m5Frame.Quality >=
                    Math.Max(
                        50,
                        LiveReactionStrongThreshold) &&
                ((_m5Frame.Direction == 1 &&
                  (_m5Frame.MssBull ||
                   _m5Frame.ChochBull)) ||
                 (_m5Frame.Direction == -1 &&
                  (_m5Frame.MssBear ||
                   _m5Frame.ChochBear))))
                return _m5Frame.Direction;

            if (predictionReady &&
                _prediction != null)
                return _prediction.Direction;

            return 0;
        }

        private bool IsRangeSignalVisualAllowed(
            int closedM5,
            int direction,
            int confidence,
            int smartQuality,
            int edge,
            int independentEvidence,
            int structuralConfirmations)
        {
            if (_m5Frame == null)
                return true;

            RangeSignalQualityResult result =
                EvaluateRangeSignalQuality(
                    closedM5,
                    direction,
                    confidence,
                    smartQuality,
                    edge,
                    independentEvidence,
                    structuralConfirmations);

            return result.Allowed;
        }

        private SignalVisualSnapshot BuildSignalVisualSnapshot(
            int closedM5)
        {
            SignalVisualSnapshot snapshot =
                new SignalVisualSnapshot
                {
                    ClosedM5 = closedM5,
                    AuthoritativeDirection = 0,
                    Stage = "WAIT",
                    CreatedM5 = -1,
                    PositionId = 0,
                    PendingOrderId = 0,
                    PendingOrderType = "",
                    EntryMode = ExecutionMode.WaitingForTrigger,
                    DecisionReason = "",
                    Regime = "UNKNOWN"
                };

            PendingOrder pending =
                GetManagedPendingOrder();

            bool pendingValid =
                pending != null &&
                IsFinitePositive(pending.TargetPrice);

            bool livePlan =
                _plan != null &&
                _plan.IsLivePosition &&
                _plan.PositionId > 0;

            bool decisionReady =
                !pendingValid &&
                !livePlan &&
                _decision != null &&
                _decision.EntryAllowed &&
                _decision.TriggerReady &&
                _decision.Direction != 0;

            bool preTradePlanVisible =
                SignalVisualLifecycleRule.IsPreTradePlanVisible(
                    new SignalVisualLifecycleInput(
                        livePlan,
                        pendingValid,
                        _plan != null,
                        _plan == null ? -1 : _plan.CreatedM5,
                        closedM5,
                        _plan == null ? 0 : _plan.Direction,
                        _decision == null ? 0 : _decision.Direction,
                        _decision != null && _decision.EntryAllowed,
                        _decision != null && _decision.ActionableNow,
                        _decision != null && _decision.TriggerReady));

            bool hasSetupPreview =
                _setupPreview != null &&
                _setupPreview.Direction != 0;

            bool setupPreviewVisible =
                SignalVisualLifecycleRule.IsSetupPreviewVisible(
                    hasSetupPreview,
                    _setupPreview == null ? -1 : _setupPreview.CreatedM5,
                    closedM5,
                    _setupPreview == null ? 0 : _setupPreview.Direction,
                    _decision == null ? 0 : _decision.Direction,
                    _decision != null && _decision.EntryAllowed);

            bool reactionReady =
                !pendingValid &&
                !livePlan &&
                EnableLiveReaction &&
                ShowReactionArrow &&
                _reaction != null &&
                _reaction.EntryAllowed &&
                _reaction.Direction != 0 &&
                _reaction.Confidence >=
                    LiveReactionStrongThreshold &&
                _reaction.IndependentEvidence >=
                    Math.Max(
                        2,
                        MinimumLiveReactionEvidence) &&
                IsRangeSignalVisualAllowed(
                    closedM5,
                    _reaction.Direction,
                    _reaction.Confidence,
                    _reaction.SmartQuality,
                    _reaction.Edge,
                    _reaction.IndependentEvidence,
                    StructuralConfirmations(
                        _reaction.Direction));

            snapshot.PlanDirection =
                _plan == null ? 0 : _plan.Direction;
            snapshot.PendingDirection =
                pendingValid
                    ? (pending.TradeType == TradeType.Buy ? 1 : -1)
                    : 0;
            snapshot.DecisionDirection =
                _decision == null ? 0 : _decision.Direction;
            snapshot.ReactionDirection =
                _reaction == null ? 0 : _reaction.Direction;

            ApplyMtfTrendStrength(snapshot);

            int strongPredictionConfidence =
                Math.Max(
                    Math.Max(
                        MinimumEarlyConfidence,
                        EarlySetupConfidence),
                    Math.Max(
                        60,
                        MinimumConfidence - 4));

            bool predictionReady =
                _prediction != null &&
                _prediction.Direction != 0 &&
                _prediction.Confidence >=
                    strongPredictionConfidence &&
                IsRangeSignalVisualAllowed(
                    closedM5,
                    _prediction.Direction,
                    _prediction.Confidence,
                    _decision == null
                        ? _prediction.Confidence
                        : _decision.SmartQuality,
                    _decision == null
                        ? 0
                        : _decision.Edge,
                    _decision == null
                        ? IndependentEvidence(
                            _prediction.Direction)
                        : _decision.IndependentEvidence,
                    _decision == null
                        ? StructuralConfirmations(
                            _prediction.Direction)
                        : _decision.StructuralConfirmations);

            bool triggerRuntimeReady =
                _triggerRuntime.Latched &&
                _decision != null &&
                _decision.Direction != 0 &&
                _triggerRuntime.Direction == _decision.Direction &&
                _triggerRuntime.DecisionM5 == closedM5 &&
                _decision.EntryAllowed &&
                _decision.ActionableNow;

            int visualDirection =
                ResolveCanonicalVisualDirection(
                    closedM5,
                    pendingValid
                        ? (pending.TradeType == TradeType.Buy ? 1 : -1)
                        : 0,
                    livePlan,
                    preTradePlanVisible,
                    decisionReady,
                    reactionReady,
                    predictionReady);

            if (livePlan)
            {
                snapshot.PlanActive = true;
                snapshot.LivePosition = true;
                snapshot.AuthoritativeDirection = _plan.Direction;
                snapshot.Stage = "ACTIVE";
                snapshot.EntryMode = _plan.EntryMode;
                snapshot.CreatedM5 = _plan.CreatedM5;
                snapshot.PositionId = _plan.PositionId;
                snapshot.Entry = _plan.Entry;
                snapshot.IdealEntry = _plan.IdealEntry;
                snapshot.Trigger = _plan.EntryTrigger;
                snapshot.Invalidation = _plan.EntryInvalidation;
                snapshot.BrokerStop = GetActiveBrokerStopPrice();
                snapshot.Stop = snapshot.BrokerStop;
                snapshot.Tp1 = _plan.Tp1;
                snapshot.Tp2 = _plan.Tp2;
                snapshot.Tp3 = _plan.Tp3;
                snapshot.Tp4 = _plan.Tp4;
                snapshot.BrokerTarget = GetActiveBrokerTargetPrice();
            }
            else if (pendingValid)
            {
                snapshot.PendingOrder = true;
                snapshot.Stage = "PENDING";
                snapshot.PendingOrderId = pending.Id;
                snapshot.PendingOrderType =
                    pending.OrderType == PendingOrderType.Stop
                        ? "STOP"
                        : pending.OrderType == PendingOrderType.Limit
                            ? "LIMIT"
                            : "PENDING";
                snapshot.PendingEntry = pending.TargetPrice;
                snapshot.PendingStop = pending.StopLoss.HasValue
                    ? pending.StopLoss.Value : 0;
                snapshot.PendingTarget = pending.TakeProfit.HasValue
                    ? pending.TakeProfit.Value : 0;
            }
            else if (preTradePlanVisible)
            {
                snapshot.PlanActive = true;
                snapshot.Stage = "PLAN";
                snapshot.EntryMode = _plan.EntryMode;
                snapshot.CreatedM5 = _plan.CreatedM5;
                snapshot.Entry = _plan.Entry;
                snapshot.IdealEntry = _plan.IdealEntry;
                snapshot.Trigger = _plan.EntryTrigger;
                snapshot.Invalidation = _plan.EntryInvalidation;
                snapshot.Stop = _plan.Stop;
                snapshot.Tp1 = _plan.Tp1;
                snapshot.Tp2 = _plan.Tp2;
                snapshot.Tp3 = _plan.Tp3;
                snapshot.Tp4 = _plan.Tp4;
            }
            else if (setupPreviewVisible)
            {
                // The setup preview is the pre-trigger structural forecast. It must
                // remain visible while the decision is directional but not yet
                // TriggerReady; otherwise the chart only shows the trigger level
                // after price has already crossed it.
                snapshot.SetupPreviewActive = true;
                snapshot.SetupEntryMode = _setupPreview.EntryMode;
                snapshot.SetupCreatedM5 = _setupPreview.CreatedM5;
                snapshot.SetupEntry = _setupPreview.Entry;
                snapshot.SetupIdealEntry = _setupPreview.IdealEntry;
                snapshot.SetupTrigger = _setupPreview.Trigger;
                snapshot.SetupInvalidation = _setupPreview.Invalidation;
                snapshot.SetupStop = _setupPreview.Stop;
                snapshot.SetupTp1 = _setupPreview.Tp1;
                snapshot.SetupTp2 = _setupPreview.Tp2;
                snapshot.SetupTp3 = _setupPreview.Tp3;
                snapshot.SetupTp4 = _setupPreview.Tp4;
                snapshot.SetupRisk = _setupPreview.Risk;
                snapshot.Stage =
                    triggerRuntimeReady
                        ? "TRIGGER READY"
                        : decisionReady
                            ? "CONFIRMED SETUP"
                            : "SETUP WATCH";
            }
            else if (decisionReady)
            {
                snapshot.Stage =
                    triggerRuntimeReady
                        ? "TRIGGER READY"
                        : "CONFIRMED";
            }
            else if (reactionReady)
            {
                snapshot.Stage = "REACTION";
            }
            else if (predictionReady)
            {
                snapshot.Stage = "PREDICTION";
            }
            else
            {
                snapshot.Stage =
                    visualDirection == 0
                        ? "WAIT"
                        : "WATCH";
            }

            snapshot.AuthoritativeDirection =
                visualDirection;

            PopulateCanonicalVisualIdentity(
                snapshot,
                closedM5,
                visualDirection);

            snapshot.HtfAnchorDirection =
                _decision == null ? 0 : _decision.HtfAnchorDirection;
            snapshot.HtfAlignment =
                _decision == null ? 0 : _decision.HtfAlignment;
            snapshot.MidframeDirection =
                _decision == null ? 0 : _decision.MidframeDirection;
            snapshot.MidframeAlignment =
                _decision == null ? 0 : _decision.MidframeAlignment;
            snapshot.EntryFrameAlignment =
                _decision == null ? 0 : _decision.EntryFrameAlignment;
            snapshot.TopDownEligible =
                _decision != null && _decision.TopDownEligible;
            snapshot.TopDownStage =
                _decision == null
                    ? "HTF SEARCH"
                    : _decision.TopDownStage ?? "HTF SEARCH";

            snapshot.DecisionReady = decisionReady;
            snapshot.DecisionEntryAllowed =
                _decision != null &&
                _decision.EntryAllowed &&
                _decision.Direction != 0;
            snapshot.ActionableNow =
                _decision != null &&
                _decision.ActionableNow;
            snapshot.ReactionReady = reactionReady;
            snapshot.ReactionIntrabar = reactionReady;
            snapshot.ReactionM5Index =
                reactionReady &&
                _m5Bars != null
                    ? _m5Bars.Count - 1
                    : -1;
            snapshot.PredictionReady = predictionReady;
            snapshot.SmartQuality =
                _decision == null ? 0 : _decision.SmartQuality;
            snapshot.TimeframeAgreement =
                _decision == null ? 0 : _decision.TimeframeAgreement;
            snapshot.IndependentEvidence =
                _decision == null ? 0 : _decision.IndependentEvidence;
            snapshot.StructuralConfirmations =
                _decision == null ? 0 : _decision.StructuralConfirmations;
            snapshot.Confidence =
                _decision == null ? 0 : _decision.Confidence;
            snapshot.EntryLocationQuality =
                _decision == null ? 0 : _decision.EntryLocationQuality;
            snapshot.EntryTimingQuality =
                _decision == null ? 0 : _decision.EntryTimingQuality;
            snapshot.EntryPositionQuality =
                _decision == null ? 0 : _decision.EntryPositionQuality;
            snapshot.EntryDistanceAtr =
                _decision == null ? 0 : _decision.EntryDistanceAtr;
            snapshot.ActionableTp1RR =
                _decision == null ? 0 : _decision.ActionableTp1RR;
            snapshot.DivergenceDirection =
                _decision == null ? 0 : _decision.DivergenceDirection;
            snapshot.DivergenceQuality =
                _decision == null ? 0 : _decision.DivergenceQuality;
            snapshot.DivergenceType =
                _decision == null
                    ? "NONE"
                    : _decision.DivergenceType ?? "NONE";
            snapshot.ActionabilityReason =
                _decision == null
                    ? "NO DECISION"
                    : _decision.ActionabilityReason ?? "NOT EVALUATED";
            snapshot.BaseConfidence =
                _decision == null ? 0 : _decision.BaseConfidence;
            snapshot.CalibratedConfidence =
                _decision == null ? 0 : _decision.CalibratedConfidence;
            snapshot.EmpiricalCalibrationAdjustment =
                _decision == null ? 0 : _decision.EmpiricalCalibrationAdjustment;
            snapshot.EmpiricalCalibrationSamples =
                _decision == null ? 0 : _decision.EmpiricalCalibrationSamples;
            snapshot.EmpiricalCalibrationWins =
                _decision == null ? 0 : _decision.EmpiricalCalibrationWins;
            snapshot.EmpiricalCalibrationBucket =
                _decision == null ? 0 : _decision.EmpiricalCalibrationBucket;
            snapshot.EmpiricalCalibrationObservedWinRate =
                _decision == null
                    ? 0.5
                    : _decision.EmpiricalCalibrationObservedWinRate;
            snapshot.EmpiricalCalibrationSource =
                _decision == null
                    ? "NONE"
                    : _decision.EmpiricalCalibrationSource;
            snapshot.DecisionReason =
                _decision == null ? "" : _decision.Reason;
            snapshot.ReactionReason =
                _reaction == null ? "" : _reaction.Reason;
            snapshot.ReactionConfidence =
                _reaction == null ? 0 : _reaction.Confidence;
            snapshot.Regime =
                _decision == null || string.IsNullOrWhiteSpace(_decision.Regime)
                    ? "UNKNOWN"
                    : _decision.Regime;

            snapshot.TriggerRuntimeReady =
                triggerRuntimeReady;

            snapshot.TriggerM1Index =
                snapshot.TriggerRuntimeReady
                    ? _triggerRuntime.ConfirmedM1
                    : -1;

            snapshot.TriggerRuntimeScore =
                _triggerRuntime.Score;

            snapshot.TriggerRuntimeRequired =
                _triggerRuntime.RequiredScore;

            snapshot.TriggerRuntimeReason =
                _triggerRuntime.Reason ?? "";

            ExecutionMode triggerMode =
                snapshot.PlanActive
                    ? snapshot.EntryMode
                    : snapshot.SetupEntryMode;

            snapshot.TriggerVisible =
                (snapshot.PlanActive ||
                 snapshot.SetupPreviewActive) &&
                !snapshot.LivePosition &&
                IsFinitePositive(
                    snapshot.PlanActive
                        ? snapshot.Trigger
                        : snapshot.SetupTrigger) &&
                (triggerMode == ExecutionMode.WaitingForTrigger ||
                 triggerMode == ExecutionMode.ContinuationStop);
            snapshot.IdealEntryVisible =
                snapshot.PlanActive &&
                IsFinitePositive(snapshot.IdealEntry) &&
                !SamePrice(snapshot.IdealEntry, snapshot.Entry);
            snapshot.ActiveBrokerTargetVisible =
                snapshot.LivePosition &&
                IsFinitePositive(snapshot.BrokerTarget);
            snapshot.ArrowM5Index =
                snapshot.ReactionIntrabar
                    ? snapshot.ReactionM5Index
                    : closedM5;

            return snapshot;
        }

    }
}