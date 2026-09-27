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

                    TimeframeAgreement =
                        TimeframeAgreement,
                    IndependentEvidence =
                        IndependentEvidence,
                    StructuralConfirmations =
                        StructuralConfirmations,
                    RegimeQuality =
                        value =>
                            RegimeQuality(
                                value,
                                _m5Bars,
                                closedM5),
                    RetestQuality =
                        (bar, direction) =>
                            RetestQuality(
                                _m5Bars,
                                bar,
                                direction),
                    ClosedBarTriggerReady =
                        (bar, direction) =>
                            ClosedBarTriggerReady(
                                _m5Bars,
                                bar,
                                direction),
                    CalibrateConfidence =
                        CalibratedConfidence
                };

            Decision decision =
                _decisionEngine.Evaluate(
                    _decisionInputSnapshotFactory.Create(
                        request));

            decision.EntryAllowed =
                PassesDecisionFilters(
                    chartIndex,
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
