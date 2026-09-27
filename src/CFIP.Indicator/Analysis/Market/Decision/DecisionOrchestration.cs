using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly DecisionEngine _decisionEngine =
            new DecisionEngine();

        private readonly DecisionInputSnapshotFactory
            _decisionInputSnapshotFactory =
                new DecisionInputSnapshotFactory();

        private readonly DecisionReasonBuilder
            _decisionReasonBuilder =
                new DecisionReasonBuilder();

        private Decision BuildDecision(
            int chartIndex,
            int closedM5,
            DateTime reference)
        {
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
                    HigherTfPenalty =
                        HigherTfPenalty,

                    Reference = reference,
                    ClosedM5 = closedM5,

                    Evidence =
                        new DecisionEvidenceSnapshot(
                            TimeframeAgreement(1, reference),
                            TimeframeAgreement(-1, reference),
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
                            ConfidenceCalibrationAdjustment(1),
                            ConfidenceCalibrationAdjustment(-1),
                            HigherTimeframeConfidencePenalty(1),
                            HigherTimeframeConfidencePenalty(-1))
                };

            Decision decision =
                _decisionEngine.Evaluate(
                    _decisionInputSnapshotFactory.Create(
                        request));

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
