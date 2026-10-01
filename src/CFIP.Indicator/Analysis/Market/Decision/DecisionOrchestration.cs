using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly DecisionEvaluator _decisionEvaluator =
            new DecisionEvaluator();

        private readonly DecisionInputSnapshotFactory
            _decisionInputSnapshotFactory =
                new DecisionInputSnapshotFactory();

        private readonly DecisionReasonBuilder
            _decisionReasonBuilder =
                new DecisionReasonBuilder();

        private Decision BuildDecision(
            int closedM5,
            DateTime reference,
            MtfClosedContext closedContext)
        {
            if (closedContext == null ||
                closedContext.Reference != reference ||
                closedContext.M5 != closedM5 ||
                !closedContext.HasPrimaryDecisionHistory)
            {
                throw new InvalidOperationException(
                    "Decision closed-bar context is missing or inconsistent.");
            }

            if (_marketStateSnapshot == null ||
                !_marketStateSnapshot.MatchesReference(reference) ||
                _marketStateSnapshot.M5.ClosedIndex != closedM5)
                throw new InvalidOperationException(
                    "Decision market-state snapshot is missing or inconsistent.");

            string regime =
                _marketStateSnapshot.M5.Regime;

            DecisionInputBuildRequest request =
                new DecisionInputBuildRequest
                {
                    M1Frame = _m1Frame,
                    M5Frame = _m5Frame,
                    M15Frame = _m15Frame,
                    M30Frame = _m30Frame,
                    H1Frame = _h1Frame,
                    H4Frame = _h4Frame,
                    D1Frame = _d1Frame,
                    W1Frame = _w1Frame,

                    M5Weight = M5Weight,
                    M15Weight = M15Weight,
                    M30Weight = M30Weight,
                    H1Weight = H1Weight,
                    H4Weight = H4Weight,
                    D1Weight = D1Weight,
                    W1Weight = W1Weight,

                    SmartWeeklyContext = SmartWeeklyContext,
                    UseAdvancedConfluence = UseAdvancedConfluence,
                    AdvancedConfluenceBuy =
                        UseAdvancedConfluence
                            ? LiveBias(
                                closedM5,
                                reference,
                                1)
                            : 0,
                    AdvancedConfluenceSell =
                        UseAdvancedConfluence
                            ? LiveBias(
                                closedM5,
                                reference,
                                -1)
                            : 0,
                    UsePremiumDiscount =
                        UsePremiumDiscount,
                    PremiumDiscountBias =
                        _marketStateSnapshot.PremiumDiscountBias,
                    UseM1Trigger = UseM1Trigger,

                    Regime = regime,
                    MarketStateSnapshot =
                        _marketStateSnapshot,
                    AdaptiveRegimeWeighting =
                        AdaptiveRegimeWeighting,
                    UseHistoricalChoppinessGuard =
                        UseHistoricalChoppinessGuard,
                    SmartScoreTemperature =
                        SmartScoreTemperature,
                    MinimumSmartDirectionShare =
                        MinimumSmartDirectionShare,

                    Reference = reference,
                    ClosedM5 = closedM5,
                    ClosedContext = closedContext,

                    Evidence =
                        new DecisionEvidenceSnapshot(
                            TimeframeAgreement(1, closedContext),
                            TimeframeAgreement(-1, closedContext),
                            IndependentEvidence(1),
                            IndependentEvidence(-1),
                            StructuralConfirmations(1),
                            StructuralConfirmations(-1),
                            _marketStateSnapshot.M5.RegimeQuality,
                            RetestQuality(
                                _m5Bars,
                                closedM5,
                                1),
                            RetestQuality(
                                _m5Bars,
                                closedM5,
                                -1),
                            ClosedBarTriggerReady(
                                _m5Bars,
                                closedM5,
                                1),
                            ClosedBarTriggerReady(
                                _m5Bars,
                                closedM5,
                                -1),
                            UseM1Trigger
                                ? M1TriggerReady(
                                    _m1Bars,
                                    _m5Bars,
                                    closedContext.M1,
                                    closedM5,
                                    reference,
                                    1)
                                : false,
                            UseM1Trigger
                                ? M1TriggerReady(
                                    _m1Bars,
                                    _m5Bars,
                                    closedContext.M1,
                                    closedM5,
                                    reference,
                                    -1)
                                : false,
                            0,
                            0,
                            HigherTimeframeConfidencePenalty(1),
                            HigherTimeframeConfidencePenalty(-1))
                };

            Decision decision =
                _decisionEvaluator.Evaluate(
                    _decisionInputSnapshotFactory.Create(
                        request));

            decision.IndependentEvidenceGroupCount =
                decision.Direction == 0
                    ? 0
                    : IndependentEvidenceGroupCount(
                        decision.Direction);

            decision.IndicatorIndependentEvidenceGroupCount =
                _m5Frame == null ? 0 : _m5Frame.IndicatorIndependentEvidenceGroupCount;

            TopDownCalibrationSnapshot topDown =
                EvaluateTopDownCalibration(
                    decision);

            decision.HtfAnchorDirection =
                topDown.HtfDirection;
            decision.HtfAlignment =
                topDown.HtfAlignment;
            decision.HtfAbsoluteStrength =
                topDown.HtfAbsoluteStrength;
            decision.MidframeDirection =
                topDown.MidDirection;
            decision.MidframeAlignment =
                topDown.MidAlignment;
            decision.MidframeAbsoluteStrength =
                topDown.MidAbsoluteStrength;
            decision.EntryFrameAlignment =
                topDown.EntryAlignment;
            decision.EntryFrameAbsoluteStrength =
                topDown.EntryAbsoluteStrength;
            decision.TopDownEligible =
                topDown.Eligible;
            decision.TopDownStage =
                topDown.Stage;

            TacticalOpportunityResult tactical =
                EvaluateTacticalOpportunity(
                    decision,
                    closedM5);

            decision.TacticalOpportunityAllowed =
                tactical.Allowed;
            decision.TacticalOpportunityLane =
                tactical.Lane;
            decision.TacticalOpportunityQuality =
                tactical.Quality;
            decision.TacticalOpportunityRR =
                tactical.RiskReward;

            OpportunityLane decisionLane =
                ResolveSignalTraceLane(
                    decision,
                    tactical.Lane);

            ApplyEmpiricalCalibration(
                decision,
                decisionLane);

            decision.EntryAllowed =
                PassesDecisionFilters(
                    closedM5,
                    reference,
                    decision,
                    out decision.BlockReason);

            if (_m5Frame != null)
            {
                decision.DivergenceDirection =
                    _m5Frame.DivergenceDirection;
                decision.DivergenceQuality =
                    _m5Frame.DivergenceQuality;
                decision.DivergenceType =
                    _m5Frame.DivergenceType ?? "NONE";
            }

            decision.ActionableNow = false;
            decision.EntryLocationQuality = 0;
            decision.EntryTimingQuality = 0;
            decision.EntryPositionQuality = 0;
            decision.EntryDistanceAtr = 0;
            decision.ActionableTp1RR = 0;
            decision.ActionabilityReason =
                decision.EntryAllowed
                    ? "NOT EVALUATED"
                    : string.IsNullOrWhiteSpace(
                        decision.BlockReason)
                        ? "DECISION FILTER"
                        : decision.BlockReason;

            if (decision.EntryAllowed &&
                decision.Direction != 0)
            {
                OpportunityLane lane =
                    decisionLane;

                if (lane == OpportunityLane.Strategic ||
                    lane == OpportunityLane.Tactical ||
                    lane == OpportunityLane.CounterHtfTactical ||
                    lane == OpportunityLane.MicroReaction)
                {
                    ExecutionModel actionExecution =
                        BuildExecutionModel(
                            closedM5,
                            decision.Direction);

                    TradeSetupPreview actionPreview =
                        BuildTradeSetupPreview(
                            closedM5,
                            actionExecution,
                            lane);

                    TradeActionabilityResult actionability =
                        EvaluateTradeActionability(
                            closedM5,
                            decision.Direction,
                            lane,
                            decision.Regime,
                            actionExecution,
                            actionPreview);

                    decision.ActionableNow =
                        actionability.Actionable;
                    decision.EntryLocationQuality =
                        actionability.LocationQuality;
                    decision.EntryTimingQuality =
                        actionability.TimingQuality;
                    decision.EntryPositionQuality =
                        actionability.PricePositionQuality;
                    decision.EntryDistanceAtr =
                        actionability.EntryDistanceAtr;
                    decision.ActionableTp1RR =
                        actionability.Tp1RR;
                    decision.DivergenceQuality =
                        Math.Max(
                            decision.DivergenceQuality,
                            actionability.DivergenceQuality);
                    decision.DivergenceDirection =
                        actionability.DivergenceDirection;
                    decision.DivergenceType =
                        actionability.DivergenceType;
                    decision.ActionabilityReason =
                        actionability.Reason;

                    if (decision.ActionableNow)
                    {
                        ActionableSignalQualityResult qualityGate =
                            EvaluateFinalActionableSignalQuality(
                                decision,
                                actionability);

                        if (!qualityGate.Allowed)
                        {
                            decision.ActionableNow = false;
                            decision.ActionabilityReason =
                                qualityGate.Reason;
                        }
                    }
                }
            }

            decision.Reason =
                _decisionReasonBuilder.Build(
                    decision);

            return decision;
        }
    }
}
