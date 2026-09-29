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
            int chartIndex,
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

            int closedChartIndex =
                MapM5ToClosedChart(
                    closedM5,
                    chartIndex);

            string regime =
                DetectRegime(
                    _m5Bars,
                    closedM5);

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
                                closedChartIndex,
                                1)
                            : 0,
                    AdvancedConfluenceSell =
                        UseAdvancedConfluence
                            ? LiveBias(
                                closedChartIndex,
                                -1)
                            : 0,
                    UsePremiumDiscount =
                        UsePremiumDiscount,
                    PremiumDiscountBias =
                        UsePremiumDiscount
                            ? PremiumDiscountBias(
                                _m5Bars,
                                closedM5)
                            : 0,
                    UseM1Trigger = UseM1Trigger,

                    Regime = regime,
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
                            RegimeQuality(
                                regime,
                                _m5Bars,
                                closedM5),
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

            TopDownCalibrationSnapshot topDown =
                EvaluateTopDownCalibration(
                    decision);

            decision.HtfAnchorDirection =
                topDown.HtfDirection;
            decision.HtfAlignment =
                topDown.HtfAlignment;
            decision.MidframeDirection =
                topDown.MidDirection;
            decision.MidframeAlignment =
                topDown.MidAlignment;
            decision.EntryFrameAlignment =
                topDown.EntryAlignment;
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

            ApplyEmpiricalCalibration(
                decision,
                tactical.Lane);

            DivergenceResult divergence =
                AnalyzeDivergence(
                    _m5Bars,
                    closedM5);

            decision.DivergenceDirection =
                divergence.Direction;
            decision.DivergenceQuality =
                divergence.Quality;
            decision.DivergenceType =
                divergence.Type;
            decision.DivergenceAgeBars =
                divergence.AgeBars;
            decision.DivergenceRegular =
                divergence.IsRegular;
            decision.DivergenceHidden =
                divergence.IsHidden;

            if (divergence.Direction == -decision.Direction &&
                divergence.IsRegular &&
                divergence.Quality >= 70)
            {
                decision.Confidence =
                    Math.Max(
                        0,
                        decision.Confidence -
                        Math.Min(
                            18,
                            8 +
                            (divergence.Quality - 70) / 4));
            }
            else if (divergence.Direction == decision.Direction &&
                     divergence.Quality >= 78)
            {
                decision.Confidence =
                    Math.Min(
                        99,
                        decision.Confidence +
                        Math.Min(
                            6,
                            2 +
                            (divergence.Quality - 78) / 5));
            }

            decision.EntryAllowed =
                PassesDecisionFilters(
                    closedM5,
                    reference,
                    decision,
                    out decision.BlockReason);

            decision.Reason =
                _decisionReasonBuilder.Build(
                    decision);

            return decision;
        }
    }
}
