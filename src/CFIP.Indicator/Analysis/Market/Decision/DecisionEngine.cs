using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly DecisionEvaluator _decisionEvaluator =
            new DecisionEvaluator();

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

            DecisionInputSnapshot input =
                new DecisionInputSnapshot(
                    _m5Frame,
                    _m15Frame,
                    _m30Frame,
                    _h1Frame,
                    _h4Frame,
                    _d1Frame,
                    _w1Frame,
                    BuildDecisionFrameContribution(
                        _m5Frame,
                        M5Weight),
                    BuildDecisionFrameContribution(
                        _m15Frame,
                        M15Weight),
                    BuildDecisionFrameContribution(
                        _m30Frame,
                        M30Weight),
                    BuildDecisionFrameContribution(
                        _h1Frame,
                        H1Weight),
                    BuildDecisionFrameContribution(
                        _h4Frame,
                        H4Weight),
                    BuildDecisionFrameContribution(
                        _d1Frame,
                        D1Weight),
                    BuildDecisionFrameContribution(
                        _w1Frame,
                        W1Weight),
                    SmartWeeklyContext,
                    UseAdvancedConfluence,
                    UseAdvancedConfluence
                        ? LiveBias(
                            closedChartIndex,
                            1)
                        : 0,
                    UseAdvancedConfluence
                        ? LiveBias(
                            closedChartIndex,
                            -1)
                        : 0,
                    UsePremiumDiscount,
                    UsePremiumDiscount
                        ? PremiumDiscountBias(
                            _m5Bars,
                            closedM5)
                        : 0,
                    UseM1Trigger,
                    regime,
                    AdaptiveRegimeWeighting,
                    UseHistoricalChoppinessGuard,
                    SmartScoreTemperature,
                    MinimumSmartDirectionShare,
                    HigherTfPenalty,
                    reference,
                    closedM5,
                    TimeframeAgreement,
                    IndependentEvidence,
                    StructuralConfirmations,
                    value =>
                        RegimeQuality(
                            value,
                            _m5Bars,
                            closedM5),
                    (bar, direction) =>
                        RetestQuality(
                            _m5Bars,
                            bar,
                            direction),
                    (bar, direction) =>
                        ClosedBarTriggerReady(
                            _m5Bars,
                            bar,
                            direction),
                    CalibratedConfidence);

            Decision decision =
                _decisionEvaluator.Evaluate(
                    input);

            decision.EntryAllowed =
                PassesDecisionFilters(
                    chartIndex,
                    closedM5,
                    reference,
                    decision,
                    out decision.BlockReason);

            decision.Reason =
                BuildReason(
                    decision,
                    decision.BuyShare,
                    decision.SellShare);

            return decision;
        }

        private DecisionFrameContribution BuildDecisionFrameContribution(
            Frame frame,
            double weight)
        {
            double buy = 0;
            double sell = 0;
            int evidence = 0;

            AddFrame(
                frame,
                weight,
                ref buy,
                ref sell,
                ref evidence);

            return new DecisionFrameContribution(
                buy,
                sell,
                evidence);
        }
    }
}