using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyConsensusSymmetry();
            VerifyDecisionScoreBoundariesAndTraceability();
            VerifyNeutralQualityIsolation();
            VerifyQualityBoundaries();
            VerifyTopDownCalibrationAbsoluteStrength();
            VerifyConfidenceCalibrationKeyEquality();
            VerifyConfidenceCalibration();
            VerifyContextualConfidenceCalibration();
            VerifyRecentOutcomeCalibration();
            VerifyAdaptiveOutcomeRisk();
            VerifyThresholdReasons();
            VerifySmartConsensusReasons();
            VerifyConfidenceDeterminism();
            VerifyCorrelationAwareEvidence();
            VerifyQualityWeightedFrameContribution();
            VerifyLocationEvidenceHierarchy();
            VerifyExecutionPlanGeometry();
            VerifyLiveExitGeometry();
            VerifyMarketRegimeClassification();
            VerifyEntryTrapRisk();
            VerifyActionableSignalQuality();
            VerifySignalVisualLifecycle();
            VerifyRangeSignalQuality();
            VerifyIndicatorEvidenceFusion();
            VerifyIndicatorActionability();
            VerifySmartBreakEven();
            VerifyEntryGeometry();
            VerifyEntrySignalTiming();

            Console.WriteLine("Decision contracts OK");
        }

        private static void VerifyEntryGeometry()
        {
            EntryGeometrySnapshot retest =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,
                    100.08,
                    99.00,
                    100.00,
                    0.10,
                    100.00,
                    101.00,
                    100.00,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    false,
                    0.75,
                    0.50);

            Assert(
                retest.Mode == ExecutionMode.RetestMarket &&
                retest.InsideZone &&
                !retest.TriggerReached &&
                !retest.IsLate &&
                Math.Abs(retest.ActualEntry - retest.Market) < 1e-12,
                "BUY retest geometry uses the canonical tolerant zone");

            EntryGeometrySnapshot continuationRetest =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,
                    99.85,
                    99.00,
                    100.00,
                    0.10,
                    99.80,
                    101.00,
                    99.80,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    true,
                    0.75,
                    0.50);

            Assert(
                continuationRetest.Mode == ExecutionMode.RetestMarket &&
                continuationRetest.InsideZone &&
                !continuationRetest.TriggerReached &&
                Math.Abs(continuationRetest.ActualEntry - continuationRetest.Market) < 1e-12,
                "continuation context must not hide an in-zone BUY retest");

            EntryGeometrySnapshot breakout =
                EntryGeometryRule.Evaluate(
                    -1,
                    ExecutionMode.None,
                    98.995,
                    99.00,
                    100.00,
                    0.02,
                    99.50,
                    99.00,
                    99.60,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    false,
                    0.75,
                    0.50);

            Assert(
                breakout.Mode == ExecutionMode.BreakoutMarket &&
                breakout.TriggerReached &&
                breakout.Anchor == 99.00 &&
                !breakout.IsLate,
                "SELL breakout geometry is directionally symmetric");

            EntryGeometrySnapshot lateBreakout =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,
                    102.00,
                    99.00,
                    100.00,
                    0.10,
                    99.50,
                    100.00,
                    99.50,
                    1.00,
                    0.01,
                    0.10,
                    true,
                    false,
                    0.75,
                    0.50);

            Assert(
                lateBreakout.Mode == ExecutionMode.BreakoutMarket &&
                Math.Abs(lateBreakout.TriggerExtensionAtr - 2.0) < 1e-12 &&
                lateBreakout.IsLate,
                "breakout late state is derived from trigger extension");

            EntryGeometrySnapshot lateRetest =
                EntryGeometryRule.Evaluate(
                    -1,
                    ExecutionMode.None,
                    99.00,
                    98.00,
                    99.20,
                    0.05,
                    98.20,
                    97.50,
                    98.20,
                    1.00,
                    0.01,
                    0.10,
                    false,
                    false,
                    0.75,
                    0.50);

            Assert(
                lateRetest.Mode == ExecutionMode.RetestMarket &&
                Math.Abs(lateRetest.EntryDistanceAtr - 0.8) < 1e-12 &&
                lateRetest.IsLate,
                "retest late state is derived from ideal-entry distance");

            EntryGeometrySnapshot waiting =
                EntryGeometryRule.Evaluate(
                    1,
                    ExecutionMode.None,
                    102.00,
                    99.00,
                    100.00,
                    0.10,
                    99.50,
                    101.00,
                    99.50,
                    1.00,
                    0.01,
                    0.10,
                    false,
                    true,
                    0.75,
                    0.50);

            Assert(
                waiting.Mode == ExecutionMode.WaitingForTrigger &&
                waiting.ActualEntry == 99.50 &&
                !waiting.IsLate,
                "continuation state waits for a trigger without inheriting retest-late logic");
        }

        private static void VerifyEntrySignalTiming()
        {
            DateTime causal =
                new DateTime(
                    2026,
                    10,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            DateTime actionable =
                causal.AddMilliseconds(1250);

            EntrySignalTiming measured =
                EntrySignalTimingRule.Measure(
                    causal,
                    actionable);

            Assert(
                measured.Measured &&
                measured.LatencyMilliseconds == 1250 &&
                measured.CausalEventUtcTicks == causal.Ticks &&
                measured.ActionableUtcTicks == actionable.Ticks,
                "signal latency is measured from the causal event");

            EntrySignalTiming reversed =
                EntrySignalTimingRule.Measure(
                    actionable,
                    causal);

            Assert(
                !reversed.Measured,
                "negative signal latency fails closed");

            EntrySignalTiming missing =
                EntrySignalTimingRule.Measure(
                    DateTime.MinValue,
                    actionable);

            Assert(
                !missing.Measured,
                "missing causal timestamps are not treated as zero-latency");
        }

        private static void VerifyTopDownCalibrationAbsoluteStrength()
        {
            TopDownCalibrationSnapshot weakAligned =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1 },
                    new[] { 40, 35, 45 },
                    new[] { 1.0, 1.0, 1.0 },
                    new[] { 1, 1 },
                    new[] { 80, 80 },
                    new[] { 1.0, 1.0 },
                    1, 80, 75, 1);
            Assert(
                weakAligned.HtfAlignment == 100 &&
                weakAligned.HtfAbsoluteStrength == 40 &&
                !weakAligned.HtfStrong &&
                weakAligned.Stage == "HTF MIXED",
                "perfect HTF alignment does not imply strong absolute strength");

            TopDownCalibrationSnapshot strongAligned =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1 },
                    new[] { 80, 75, 85 },
                    new[] { 1.0, 1.0, 1.0 },
                    new[] { 1, 1 },
                    new[] { 80, 80 },
                    new[] { 1.0, 1.0 },
                    1, 80, 75, 1);
            Assert(
                strongAligned.HtfAlignment == 100 &&
                strongAligned.HtfAbsoluteStrength == 80 &&
                strongAligned.HtfStrong &&
                strongAligned.Eligible &&
                strongAligned.Stage == "ENTRY CALIBRATED",
                "strong HTF alignment requires absolute strength");

            TopDownCalibrationSnapshot weakCounterFrame =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1 },
                    new[] { 80, 80, 80 },
                    new[] { 1.0, 1.0, 1.0 },
                    new[] { -1, -1 },
                    new[] { 40, 45 },
                    new[] { 1.0, 1.0 },
                    1, 80, 75, 1);
            Assert(
                weakCounterFrame.MidAlignment == 100 &&
                weakCounterFrame.MidAbsoluteStrength == 43 &&
                weakCounterFrame.Eligible &&
                weakCounterFrame.Stage == "MIDFRAME CALIBRATION",
                "weak counter-frame alignment cannot override a strong anchor");

            TopDownCalibrationSnapshot strongCounterFrame =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1 },
                    new[] { 80, 80, 80 },
                    new[] { 1.0, 1.0, 1.0 },
                    new[] { -1, -1 },
                    new[] { 80, 75 },
                    new[] { 1.0, 1.0 },
                    1, 80, 75, 1);
            Assert(
                strongCounterFrame.MidAlignment == 100 &&
                strongCounterFrame.MidAbsoluteStrength == 78 &&
                !strongCounterFrame.Eligible &&
                strongCounterFrame.Stage == "MIDFRAME CONFLICT",
                "strong counter-frame alignment plus strength blocks");

            TopDownCalibrationSnapshot weakEntryConflict =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1 },
                    new[] { 80, 80, 80 },
                    new[] { 1.0, 1.0, 1.0 },
                    new[] { 1, 1 },
                    new[] { 80, 80 },
                    new[] { 1.0, 1.0 },
                    -1, 40, 75, 1);
            Assert(
                weakEntryConflict.EntryAlignment == 0 &&
                weakEntryConflict.EntryAbsoluteStrength == 40 &&
                weakEntryConflict.Eligible,
                "weak entry conflict does not override a strong anchor");

            TopDownCalibrationSnapshot strongEntryConflict =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1 },
                    new[] { 80, 80, 80 },
                    new[] { 1.0, 1.0, 1.0 },
                    new[] { 1, 1 },
                    new[] { 80, 80 },
                    new[] { 1.0, 1.0 },
                    -1, 80, 75, 1);
            Assert(
                strongEntryConflict.EntryAlignment == 0 &&
                strongEntryConflict.EntryAbsoluteStrength == 80 &&
                !strongEntryConflict.Eligible &&
                strongEntryConflict.Stage == "ENTRY CONFLICT",
                "strong entry conflict blocks a strong anchor");

            TopDownCalibrationSnapshot repeat =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1 },
                    new[] { 80, 75, 85 },
                    new[] { 1.0, 1.0, 1.0 },
                    new[] { 1, 1 },
                    new[] { 80, 80 },
                    new[] { 1.0, 1.0 },
                    1, 80, 75, 1);
            Assert(
                repeat.HtfAlignment == strongAligned.HtfAlignment &&
                repeat.HtfAbsoluteStrength == strongAligned.HtfAbsoluteStrength &&
                repeat.MidAlignment == strongAligned.MidAlignment &&
                repeat.MidAbsoluteStrength == strongAligned.MidAbsoluteStrength &&
                repeat.EntryAbsoluteStrength == strongAligned.EntryAbsoluteStrength &&
                repeat.Stage == strongAligned.Stage,
                "top-down absolute-strength evaluation is deterministic");
        }
        private static void VerifyConsensusSymmetry()
        {
            DecisionConsensusCalculator calculator =
                new DecisionConsensusCalculator();

            DecisionConsensusSnapshot buy =
                calculator.Calculate(20, 10, 3, 60);

            DecisionConsensusSnapshot sell =
                calculator.Calculate(10, 20, 3, 60);

            Assert(buy.Direction == 1, "BUY consensus direction");
            Assert(sell.Direction == -1, "SELL consensus direction");
            Assert(buy.BuyShare == sell.SellShare, "BUY/SELL share symmetry");
            Assert(buy.SellShare == sell.BuyShare, "SELL/BUY share symmetry");
            Assert(buy.Edge == sell.Edge, "BUY/SELL edge symmetry");
        }

        private static void VerifyDecisionScoreBoundariesAndTraceability()
        {
            DecisionScoreInput input =
                new DecisionScoreInput(
                    new DecisionFrameContribution(1, 2, 0),
                    new DecisionFrameContribution(3, 4, 0),
                    new DecisionFrameContribution(5, 6, 0),
                    new DecisionFrameContribution(7, 8, 0),
                    new DecisionFrameContribution(9, 10, 0),
                    new DecisionFrameContribution(11, 12, 0),
                    new DecisionFrameContribution(13, 14, 0),
                    true,
                    true,
                    2,
                    3,
                    true,
                    1,
                    false,
                    false,
                    0,
                    0,
                    0,
                    false,
                    false);

            DecisionScoreSnapshot score =
                new DecisionScoreCalculator().Calculate(input);

            double expectedBuy =
                1 + 3 + 5 + 7 + 9 + 11 + 13 + 2 + 6;

            double expectedSell =
                2 + 4 + 6 + 8 + 10 + 12 + 14 + 3;

            Assert(
                Math.Abs(score.M5BullContribution - 1) < 1e-12 &&
                Math.Abs(score.M15BullContribution - 3) < 1e-12 &&
                Math.Abs(score.H4BearContribution - 10) < 1e-12 &&
                Math.Abs(score.AdvancedConfluenceBuy - 2) < 1e-12 &&
                Math.Abs(score.PremiumDiscountBuy - 6) < 1e-12,
                "score snapshot exposes frame/confluence component provenance");

            Assert(
                Math.Abs(score.Buy - expectedBuy) < 1e-12 &&
                Math.Abs(score.Sell - expectedSell) < 1e-12 &&
                Math.Abs(score.AdaptiveRegimeBuy) < 1e-12 &&
                Math.Abs(score.ConflictPenaltyBuy) < 1e-12 &&
                Math.Abs(score.ChoppinessFactor - 1.0) < 1e-12,
                "score trace reconstructs final totals without hidden adjustments");

            DecisionScoreInput invalid =
                new DecisionScoreInput(
                    new DecisionFrameContribution(
                        double.NaN,
                        double.PositiveInfinity,
                        0),
                    new DecisionFrameContribution(0, 0, 0),
                    new DecisionFrameContribution(0, 0, 0),
                    new DecisionFrameContribution(0, 0, 0),
                    new DecisionFrameContribution(0, 0, 0),
                    new DecisionFrameContribution(0, 0, 0),
                    new DecisionFrameContribution(0, 0, 0),
                    false,
                    true,
                    double.NaN,
                    double.PositiveInfinity,
                    false,
                    0,
                    false,
                    false,
                    0,
                    0,
                    0,
                    false,
                    false);

            DecisionScoreSnapshot sanitized =
                new DecisionScoreCalculator().Calculate(invalid);

            Assert(
                sanitized.Buy == 0 &&
                sanitized.Sell == 0,
                "non-finite score inputs fail closed");

            double choppyFactor =
                DecisionScoreCalculator.ResolveChoppinessFactor(
                    true,
                    true,
                    true);

            double neutralFactor =
                DecisionScoreCalculator.ResolveChoppinessFactor(
                    true,
                    false,
                    true);

            Assert(
                Math.Abs(choppyFactor - 0.90) < 1e-12 &&
                Math.Abs(neutralFactor - 1.0) < 1e-12,
                "choppiness factor rule is explicit and directional-state neutral");

            Console.WriteLine(
                "CI-09 score boundary and provenance contracts PASS");
        }

        private static void VerifyNeutralQualityIsolation()
        {
            DecisionQualityCalculator calculator =
                new DecisionQualityCalculator();

            DecisionEvidenceSnapshot directional =
                new DecisionEvidenceSnapshot(
                    90, 10,
                    8, 1,
                    6, 1,
                    70,
                    90, 10,
                    true, false,
                    true, false,
                    0, 0,
                    0, 10);

            int neutralQuality =
                calculator.Calculate(
                    50,
                    0,
                    0,
                    0,
                    directional.RegimeQuality,
                    50);

            int expected =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        50 * 0.25 +
                        70 * 0.10 +
                        50 * 0.10),
                    0,
                    100);

            Assert(
                neutralQuality == expected,
                "neutral quality ignores directional evidence");
        }

        private static void VerifyQualityBoundaries()
        {
            DecisionQualityCalculator calculator =
                new DecisionQualityCalculator();

            DecisionEvidenceSnapshot evidence =
                new DecisionEvidenceSnapshot(
                    80, 70,
                    6, 5,
                    4, 3,
                    75,
                    65, 55,
                    true, false,
                    true, false,
                    4, -4,
                    0, 5);

            int quality =
                calculator.Calculate(
                    90,
                    evidence.BullTimeframeAgreement,
                    evidence.BullIndependentEvidence,
                    evidence.BullStructuralConfirmations,
                    evidence.RegimeQuality,
                    evidence.BullRetestQuality);

            Assert(quality >= 0 && quality <= 100, "quality clamp");

            int neutralQuality =
                calculator.Calculate(50, 0, 0, 0, 0, 50);

            Assert(neutralQuality >= 0 && neutralQuality <= 100, "zero-input quality");
        }

        private static void VerifyConfidenceCalibrationKeyEquality()
        {
            ConfidenceCalibrationKey a =
                new ConfidenceCalibrationKey(
                    1,
                    OpportunityLane.Strategic,
                    " expansion ",
                    85);

            ConfidenceCalibrationKey b =
                new ConfidenceCalibrationKey(
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    85);

            ConfidenceCalibrationKey c =
                new ConfidenceCalibrationKey(
                    -1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    85);

            Assert(
                a.Equals(b) &&
                a.GetHashCode() == b.GetHashCode(),
                "calibration key structural equality");

            Assert(
                !a.Equals(c),
                "calibration key direction inequality");
        }

        private static void VerifyConfidenceCalibration()
        {
            EmpiricalConfidenceCalibrator calibrator =
                new EmpiricalConfidenceCalibrator();

            int positive =
                calibrator.CalculateAdjustment(
                    true, true,
                    30, 20, 15,
                    10, 5, 12);

            int negative =
                calibrator.CalculateAdjustment(
                    true, true,
                    30, 20, 5,
                    10, 5, 12);

            Assert(positive > 0, "positive calibration");
            Assert(negative < 0, "negative calibration");
            Assert(
                positive == -negative,
                "calibration symmetry");

            int blocked =
                calibrator.CalculateAdjustment(
                    true, true,
                    4, 3, 3,
                    10, 5, 12);

            Assert(blocked == 0, "minimum sample gate");

            DecisionConfidenceCalculator confidence =
                new DecisionConfidenceCalculator();

            int up =
                confidence.Calculate(
                    80, 70, 75, 5, 0);

            int down =
                confidence.Calculate(
                    80, 70, 75, -5, 0);

            Assert(up > down, "calibration affects confidence");
        }

        private static void VerifyContextualConfidenceCalibration()
        {
            EmpiricalConfidenceCalibrator calibrator =
                new EmpiricalConfidenceCalibrator();

            Dictionary<ConfidenceCalibrationKey, int> samples =
                new Dictionary<ConfidenceCalibrationKey, int>();

            Dictionary<ConfidenceCalibrationKey, int> wins =
                new Dictionary<ConfidenceCalibrationKey, int>();

            ConfidenceCalibrationKey exactKey =
                new ConfidenceCalibrationKey(
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    EmpiricalConfidenceCalibrator.ConfidenceBucket(85));

            samples[exactKey] = 20;
            wins[exactKey] = 17;

            EmpiricalCalibrationSnapshot exact =
                calibrator.CalculateContextual(
                    true,
                    true,
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    85,
                    samples,
                    wins,
                    5,
                    6,
                    8);

            Assert(
                exact.Available &&
                exact.Source == "EXACT" &&
                exact.Samples == 20 &&
                exact.Wins == 17 &&
                exact.ConfidenceBucket == 3 &&
                exact.ObservedWinRate > 0.84 &&
                exact.ObservedWinRate < 0.86 &&
                exact.Adjustment > 0 &&
                exact.Adjustment <= 8,
                "exact contextual calibration");

            ConfidenceCalibrationKey adjacent =
                new ConfidenceCalibrationKey(
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    EmpiricalConfidenceCalibrator.ConfidenceBucket(75));

            samples[adjacent] = 10;
            wins[adjacent] = 4;

            samples[exactKey] = 4;
            wins[exactKey] = 1;

            EmpiricalCalibrationSnapshot context =
                calibrator.CalculateContextual(
                    true,
                    true,
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    85,
                    samples,
                    wins,
                    5,
                    6,
                    8);

            Assert(
                context.Available &&
                context.Source == "LANE+REGIME" &&
                context.Samples == 14 &&
                context.Wins == 5,
                "low-sample exact bucket falls back to lane/regime context");

            Dictionary<ConfidenceCalibrationKey, int> directionSamples =
                new Dictionary<ConfidenceCalibrationKey, int>();

            Dictionary<ConfidenceCalibrationKey, int> directionWins =
                new Dictionary<ConfidenceCalibrationKey, int>();

            ConfidenceCalibrationKey tacticalA =
                new ConfidenceCalibrationKey(
                    1,
                    OpportunityLane.Tactical,
                    "TREND",
                    EmpiricalConfidenceCalibrator.ConfidenceBucket(75));

            ConfidenceCalibrationKey tacticalB =
                new ConfidenceCalibrationKey(
                    1,
                    OpportunityLane.Tactical,
                    "RANGE",
                    EmpiricalConfidenceCalibrator.ConfidenceBucket(75));

            directionSamples[tacticalA] = 4;
            directionWins[tacticalA] = 3;
            directionSamples[tacticalB] = 4;
            directionWins[tacticalB] = 1;

            ConfidenceCalibrationKey directionalExtra =
                new ConfidenceCalibrationKey(
                    1,
                    OpportunityLane.Tactical,
                    "RANGE",
                    EmpiricalConfidenceCalibrator.ConfidenceBucket(85));

            directionSamples[directionalExtra] = 4;
            directionWins[directionalExtra] = 2;

            EmpiricalCalibrationSnapshot direction =
                calibrator.CalculateContextual(
                    true,
                    true,
                    1,
                    OpportunityLane.CounterHtfTactical,
                    "UNKNOWN",
                    75,
                    directionSamples,
                    directionWins,
                    5,
                    6,
                    8);

            Assert(
                direction.Available &&
                direction.Source == "DIRECTION" &&
                direction.Samples == 12 &&
                direction.Wins == 6,
                "insufficient context falls back to directional history");

            EmpiricalCalibrationSnapshot sparse =
                calibrator.CalculateContextual(
                    true,
                    true,
                    -1,
                    OpportunityLane.MicroReaction,
                    "COMPRESSION",
                    55,
                    samples,
                    wins,
                    5,
                    6,
                    8);

            Assert(
                !sparse.Available &&
                sparse.Adjustment == 0,
                "insufficient observations cannot calibrate");

            Assert(
                direction.Samples == 12 &&
                direction.Wins == 6 &&
                direction.ObservedWinRate >= 0.49 &&
                direction.ObservedWinRate <= 0.51,
                "directional fallback uses observed outcomes without directional asymmetry bias");
        }

        private static void VerifyRecentOutcomeCalibration()
        {
            EmpiricalConfidenceCalibrator calibrator =
                new EmpiricalConfidenceCalibrator();

            List<OutcomeObservation> outcomes =
                new List<OutcomeObservation>();

            for (int i = 0; i < 8; i++)
            {
                outcomes.Add(
                    new OutcomeObservation
                    {
                        PositionId = i + 1,
                        Direction = 1,
                        Lane = OpportunityLane.Strategic,
                        EntryMode = ExecutionMode.BreakoutMarket,
                        Regime = "EXPANSION",
                        Confidence = 85,
                        Profitable = false,
                        RealizedR = -0.50,
                        CalibrationEligible = true
                    });
            }

            for (int i = 0; i < 4; i++)
            {
                outcomes.Add(
                    new OutcomeObservation
                    {
                        PositionId = 100 + i,
                        Direction = 1,
                        Lane = OpportunityLane.Strategic,
                        EntryMode =
                            i == 0
                                ? ExecutionMode.RetestMarket
                                : i == 1
                                    ? ExecutionMode.BreakoutMarket
                                    : i == 2
                                        ? ExecutionMode.ContinuationStop
                                        : ExecutionMode.ReversalLimit,
                        Regime = "EXPANSION",
                        Confidence = 85,
                        Profitable = true,
                        RealizedR = 1.50,
                        CalibrationEligible = true
                    });
            }

            EmpiricalCalibrationSnapshot recent =
                calibrator.CalculateRecentContextual(
                    true,
                    true,
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    85,
                    outcomes,
                    4,
                    4,
                    4,
                    8);

            Assert(
                recent.Available &&
                recent.Source == "EXACT-RECENT" &&
                recent.Samples == 4 &&
                recent.Wins == 4 &&
                recent.ObservedWinRate == 1.0 &&
                recent.AverageRealizedR == 1.5 &&
                recent.Adjustment > 0,
                "recent exact calibration prefers current lifecycle window");

            outcomes[11].Profitable = false;
            outcomes[11].RealizedR = -0.50;

            EmpiricalCalibrationSnapshot changed =
                calibrator.CalculateRecentContextual(
                    true,
                    true,
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    85,
                    outcomes,
                    4,
                    4,
                    4,
                    8);

            Assert(
                changed.Samples == 4 &&
                changed.Wins == 3 &&
                changed.Adjustment < recent.Adjustment,
                "recent calibration reacts to newest outcome only");

            EmpiricalCalibrationSnapshot repeat =
                calibrator.CalculateRecentContextual(
                    true,
                    true,
                    1,
                    OpportunityLane.Strategic,
                    "EXPANSION",
                    85,
                    outcomes,
                    4,
                    4,
                    4,
                    8);

            Assert(
                repeat.Available &&
                repeat.Source == changed.Source &&
                repeat.Samples == changed.Samples &&
                repeat.Wins == changed.Wins &&
                repeat.Adjustment == changed.Adjustment,
                "recent calibration deterministic");
        }

        private static void VerifyAdaptiveOutcomeRisk()
        {
            List<OutcomeObservation> weak =
                new List<OutcomeObservation>();

            for (int i = 0; i < 8; i++)
            {
                weak.Add(
                    new OutcomeObservation
                    {
                        PositionId = i + 1,
                        Direction = i % 2 == 0 ? 1 : -1,
                        Lane = OpportunityLane.Strategic,
                        EntryMode = ExecutionMode.RetestMarket,
                        Regime = "TREND",
                        Profitable = false,
                        RealizedR = -0.75,
                        CalibrationEligible = true
                    });
            }

            double reduced =
                AdaptiveOutcomeRiskPolicy.Calculate(
                    weak,
                    1.0);

            Assert(
                reduced < 1.0 &&
                reduced >= 0.25,
                "weak recent outcomes reduce risk without breaching floor");

            List<OutcomeObservation> mixed =
                new List<OutcomeObservation>();

            for (int i = 0; i < 8; i++)
            {
                mixed.Add(
                    new OutcomeObservation
                    {
                        PositionId = 100 + i,
                        Direction = 1,
                        Lane = OpportunityLane.Strategic,
                        EntryMode = ExecutionMode.RetestMarket,
                        Regime = "TREND",
                        Profitable = i < 6,
                        RealizedR = i < 6 ? 0.8 : -0.25,
                        CalibrationEligible = true
                    });
            }

            double mixedRisk =
                AdaptiveOutcomeRiskPolicy.Calculate(
                    mixed,
                    0.75);

            Assert(
                mixedRisk == 0.75,
                "mixed stable outcomes do not add an unnecessary penalty");

            Assert(
                AdaptiveOutcomeRiskPolicy.Calculate(
                    weak,
                    0.25) == 0.25,
                "adaptive risk never goes below hard suitability floor");

            double repeat =
                AdaptiveOutcomeRiskPolicy.Calculate(
                    weak,
                    1.0);

            Assert(
                repeat == reduced,
                "adaptive outcome risk deterministic");

            List<OutcomeObservation> tooShort =
                new List<OutcomeObservation>();

            for (int i = 0; i < 7; i++)
                tooShort.Add(
                    new OutcomeObservation
                    {
                        PositionId = 200 + i,
                        Direction = 1,
                        Lane = OpportunityLane.Strategic,
                        EntryMode = ExecutionMode.RetestMarket,
                        Regime = "TREND",
                        Profitable = false,
                        RealizedR = -1.0
                    });

            Assert(
                AdaptiveOutcomeRiskPolicy.Calculate(
                    tooShort,
                    1.0) == 1.0,
                "insufficient outcome history is neutral");
        }

        private static void VerifyThresholdReasons()
        {
            DecisionThresholdFilterEvaluator evaluator =
                new DecisionThresholdFilterEvaluator();

            Decision decision =
                new Decision
                {
                    Direction = 1,
                    Confidence = 85,
                    Edge = 30,
                    SmartQuality = 80,
                    TimeframeAgreement = 75,
                    IndependentEvidence = 6,
                    StructuralConfirmations = 4,
                    RetestQuality = 75,
                    BuyShare = 85,
                    SellShare = 15,
                    Regime = "EXPANSION",
                    RegimeQuality = 80,
                    TriggerReady = true,
                    EntryAllowed = true,
                    BlockReason = "",
                    Reason = "CONTRACT"
                };

            DecisionFilterResult result =
                evaluator.Evaluate(
                    decision,
                    new DecisionThresholdFilterInput(
                        60, 25, 70, 80, 5, 3,
                        70, 20, 65,
                        true, 75,
                        3, 4, true,
                        true, 2));

            Assert(!result.Allowed, "threshold rejection");
            Assert(
                result.Reason == "CONFIDENCE",
                "threshold reason");

            DecisionFilterResult accepted =
                evaluator.Evaluate(
                    decision,
                    new DecisionThresholdFilterInput(
                        85, 30, 80, 90, 6, 4,
                        70, 20, 65,
                        true, 75,
                        3, 4, true,
                        true, 2));

            Assert(accepted.Allowed, "threshold acceptance");
        }

        private static void VerifySmartConsensusReasons()
        {
            DecisionSmartConsensusFilterEvaluator evaluator =
                new DecisionSmartConsensusFilterEvaluator();

            DecisionFilterResult rejected =
                evaluator.Evaluate(
                    new DecisionSmartConsensusFilterInput(
                        true, true,
                        65, 75, 75,
                        false,
                        60, 80,
                        10, 20,
                        3, 4,
                        70, 75));

            Assert(!rejected.Allowed, "smart consensus rejection");
            Assert(
                rejected.Reason == "SMART CONSENSUS",
                "smart consensus reason");

            DecisionFilterResult accepted =
                evaluator.Evaluate(
                    new DecisionSmartConsensusFilterInput(
                        true, true,
                        82, 75, 78,
                        false,
                        85, 80,
                        30, 20,
                        6, 4,
                        90, 75));

            Assert(accepted.Allowed, "smart consensus acceptance");
        }

        private static void VerifyConfidenceDeterminism()
        {
            DecisionConfidenceCalculator calculator =
                new DecisionConfidenceCalculator();

            int first =
                calculator.Calculate(
                    82, 74, 78, 3, 4);

            int second =
                calculator.Calculate(
                    82, 74, 78, 3, 4);

            Assert(
                first == second,
                "confidence deterministic repeatability");
        }

        
        private static void VerifyCorrelationAwareEvidence()
        {
            int structural =
                IndependentEvidenceFusionRule.CalculateScore(
                    new IndependentEvidenceFusionInput(
                        true,
                        true,
                        true,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false));

            Assert(
                structural == 2,
                "correlated structural evidence retains the established score cap");

            Assert(
                IndependentEvidenceFusionRule.CountGroups(
                    new IndependentEvidenceFusionInput(
                        true, true, true,
                        false, false, false,
                        false, false, false, false,
                        false, false, false, false)) == 1,
                "correlated structural evidence resolves to one independent group");

            int bull =
                IndependentEvidenceFusionRule.CalculateScore(
                    new IndependentEvidenceFusionInput(
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true));

            int bear =
                IndependentEvidenceFusionRule.CalculateScore(
                    new IndependentEvidenceFusionInput(
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true));

            Assert(bull == 8, "maximum independent evidence score remains 8");
            Assert(bear == bull, "BUY/SELL evidence score symmetry");
            Assert(
                IndependentEvidenceFusionRule.CountGroups(
                    new IndependentEvidenceFusionInput(
                        true, true, true,
                        true, true, true,
                        true, true, true, true,
                        true, true, true, true)) == 4,
                "maximum evidence spans exactly four independent groups");
        }

        private static void VerifyQualityWeightedFrameContribution()
        {
            DecisionFrameContributionCalculator calculator =
                new DecisionFrameContributionCalculator();

            DecisionFrameContribution contribution =
                calculator.Calculate(
                    80,
                    20,
                    80,
                    10);

            Assert(
                Math.Abs(contribution.Bull - 64) < 0.0001,
                "quality-weighted bull contribution");

            Assert(
                Math.Abs(contribution.Bear - 16) < 0.0001,
                "quality-weighted bear contribution");

            DecisionFrameContribution mirrored =
                calculator.Calculate(
                    20,
                    80,
                    80,
                    10);

            Assert(
                Math.Abs(contribution.Bull - mirrored.Bear) < 0.0001 &&
                Math.Abs(contribution.Bear - mirrored.Bull) < 0.0001,
                "BUY/SELL contribution symmetry");

            DecisionFrameContribution weakConcentrated =
                calculator.Calculate(
                    35,
                    0,
                    80,
                    10,
                    1);

            Assert(
                weakConcentrated.Bull < contribution.Bull &&
                weakConcentrated.Bull > weakConcentrated.Bear,
                "weak concentrated frame cannot dominate as a strong frame");

            DecisionFrameContribution weakBalanced =
                calculator.Calculate(
                    20,
                    20,
                    80,
                    10,
                    1);

            Assert(
                weakBalanced.Bull > 0 &&
                weakBalanced.Bear > 0 &&
                Math.Abs(
                    weakBalanced.Bull -
                    weakBalanced.Bear) <
                Math.Abs(
                    weakConcentrated.Bull -
                    weakConcentrated.Bear),
                "weak balanced frame stays near neutral");

            DecisionFrameContribution strong =
                calculator.Calculate(
                    70,
                    10,
                    80,
                    10,
                    3);

            Assert(
                strong.Bull > weakConcentrated.Bull &&
                strong.Bull > strong.Bear,
                "absolute evidence strength restores influence for strong frame");

            DecisionFrameContribution mirroredWeak =
                calculator.Calculate(
                    0,
                    35,
                    80,
                    10,
                    1);

            Assert(
                Math.Abs(weakConcentrated.Bull - mirroredWeak.Bear) < 0.0001 &&
                Math.Abs(weakConcentrated.Bear - mirroredWeak.Bull) < 0.0001,
                "weak BUY/SELL contribution symmetry");

            DecisionFrameContribution legacy =
                calculator.Calculate(
                    35,
                    0,
                    80,
                    10);

            Assert(
                legacy.Bull > weakConcentrated.Bull,
                "compatibility overload treats isolated inputs as strong evidence");
        }

        private static void VerifyLocationEvidenceHierarchy()
        {
            LocationEvidenceScore none =
                LocationEvidenceRule.Evaluate(
                    false,
                    0,
                    false,
                    0,
                    false);

            Assert(
                none.Score == 0 &&
                none.Evidence == 0 &&
                !none.Confluence,
                "no location evidence");

            LocationEvidenceScore fvg =
                LocationEvidenceRule.Evaluate(
                    true,
                    85,
                    false,
                    0,
                    false);

            LocationEvidenceScore ob =
                LocationEvidenceRule.Evaluate(
                    false,
                    0,
                    true,
                    85,
                    false);

            LocationEvidenceScore combo =
                LocationEvidenceRule.Evaluate(
                    true,
                    85,
                    true,
                    85,
                    true);

            Assert(
                combo.Score > ob.Score &&
                combo.Score > fvg.Score &&
                combo.Confluence &&
                combo.Evidence == 1,
                "OB+FVG is strongest bounded location feature");

            LocationEvidenceScore separate =
                LocationEvidenceRule.Evaluate(
                    true,
                    70,
                    true,
                    70,
                    false);

            Assert(
                separate.Evidence == 1 &&
                separate.Score < combo.Score,
                "correlated location evidence stays one evidence unit");

            LocationEvidenceScore mirrored =
                LocationEvidenceRule.Evaluate(
                    true,
                    85,
                    false,
                    0,
                    false);

            LocationEvidenceScore mirroredOpposite =
                LocationEvidenceRule.Evaluate(
                    false,
                    0,
                    true,
                    85,
                    false);

            Assert(
                mirrored.Score != mirroredOpposite.Score,
                "OB/FVG individual weights remain intentionally distinct");

            LocationEvidenceScore buyCombo =
                LocationEvidenceRule.Evaluate(
                    true,
                    80,
                    true,
                    80,
                    true);

            LocationEvidenceScore sellCombo =
                LocationEvidenceRule.Evaluate(
                    true,
                    80,
                    true,
                    80,
                    true);

            Assert(
                buyCombo.Score == sellCombo.Score &&
                buyCombo.Evidence == sellCombo.Evidence,
                "OB/FVG combo symmetry");
        }

        private static void VerifyExecutionPlanGeometry()
        {
            ExecutionPlanGeometryResult buy =
                ExecutionPlanGeometryRule.Evaluate(
                    1,
                    100,
                    95,
                    110,
                    0,
                    1.5,
                    12.0,
                    0.01);

            Assert(
                buy.Allowed &&
                buy.Risk > 0 &&
                buy.Reward > 0 &&
                buy.RiskReward > 1.5,
                "valid BUY execution geometry");

            ExecutionPlanGeometryResult sell =
                ExecutionPlanGeometryRule.Evaluate(
                    -1,
                    100,
                    105,
                    90,
                    0,
                    1.5,
                    12.0,
                    0.01);

            Assert(
                sell.Allowed &&
                sell.RiskReward > 1.5,
                "valid SELL execution geometry symmetry");

            ExecutionPlanGeometryResult wrongBuyStop =
                ExecutionPlanGeometryRule.Evaluate(
                    1,
                    100,
                    105,
                    120,
                    0,
                    1.5,
                    12.0,
                    0.01);

            Assert(
                !wrongBuyStop.Allowed &&
                wrongBuyStop.Reason == "STOP WRONG SIDE",
                "BUY protective stop validation");

            ExecutionPlanGeometryResult wrongSellStop =
                ExecutionPlanGeometryRule.Evaluate(
                    -1,
                    100,
                    95,
                    80,
                    0,
                    1.5,
                    12.0,
                    0.01);

            Assert(
                !wrongSellStop.Allowed &&
                wrongSellStop.Reason == "STOP WRONG SIDE",
                "SELL protective stop validation");

            ExecutionPlanGeometryResult weakRR =
                ExecutionPlanGeometryRule.Evaluate(
                    1,
                    100,
                    90,
                    105,
                    0,
                    2.0,
                    12.0,
                    0.01);

            Assert(
                !weakRR.Allowed &&
                weakRR.Reason == "RR BELOW EXECUTION FLOOR",
                "execution reward path floor");

            ExecutionPlanGeometryResult mirrored =
                ExecutionPlanGeometryRule.Evaluate(
                    -1,
                    100,
                    105,
                    90,
                    0,
                    2.0,
                    12.0,
                    0.01);

            Assert(
                mirrored.Allowed &&
                Math.Abs(
                    buy.RiskReward -
                    mirrored.RiskReward) < 0.0001,
                "BUY/SELL reward geometry symmetry");
        }


        private static void VerifyLiveExitGeometry()
        {
            LiveExitGeometryResult buyForward =
                LiveExitGeometryRule.ValidateLiveTarget(
                    1,
                    100,
                    115,
                    130,
                    10,
                    1);

            Assert(
                buyForward.Allowed &&
                buyForward.Distance > 0 &&
                Math.Abs(buyForward.RiskReward - 3.0) < 0.0001,
                "BUY forward target geometry");

            LiveExitGeometryResult buyBehind =
                LiveExitGeometryRule.ValidateLiveTarget(
                    1,
                    100,
                    115,
                    110,
                    10,
                    1);

            Assert(
                !buyBehind.Allowed &&
                buyBehind.Reason == "TARGET BEHIND MARKET",
                "BUY target behind market rejected");

            LiveExitGeometryResult buyAtBoundary =
                LiveExitGeometryRule.ValidateLiveTarget(
                    1,
                    100,
                    115,
                    116,
                    10,
                    1);

            Assert(
                !buyAtBoundary.Allowed &&
                buyAtBoundary.Reason == "TARGET BEHIND MARKET",
                "BUY target at minimum-forward boundary rejected");

            LiveExitGeometryResult sellForward =
                LiveExitGeometryRule.ValidateLiveTarget(
                    -1,
                    100,
                    85,
                    70,
                    10,
                    1);

            Assert(
                sellForward.Allowed &&
                Math.Abs(sellForward.RiskReward - 3.0) < 0.0001,
                "SELL forward target geometry");

            LiveExitGeometryResult sellBehind =
                LiveExitGeometryRule.ValidateLiveTarget(
                    -1,
                    100,
                    85,
                    90,
                    10,
                    1);

            Assert(
                !sellBehind.Allowed &&
                sellBehind.Reason == "TARGET BEHIND MARKET",
                "SELL target behind market rejected");

            LiveExitGeometryResult wrongSide =
                LiveExitGeometryRule.ValidateLiveTarget(
                    1,
                    100,
                    90,
                    95,
                    10,
                    1);

            Assert(
                !wrongSide.Allowed &&
                wrongSide.Reason == "TARGET WRONG SIDE",
                "BUY target below entry rejected even when forward of market");

            LiveExitGeometryResult invalidNumeric =
                LiveExitGeometryRule.ValidateLiveTarget(
                    1,
                    100,
                    115,
                    double.NaN,
                    10,
                    1);

            Assert(
                !invalidNumeric.Allowed &&
                invalidNumeric.Reason == "TARGET GEOMETRY INVALID",
                "non-finite live target rejected");

            Assert(
                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    1,
                    120,
                    132,
                    118,
                    1),
                "BUY target advances only forward");

            Assert(
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    1,
                    120,
                    119,
                    118,
                    1),
                "BUY target backward move rejected");

            Assert(
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    1,
                    120,
                    122,
                    121,
                    1),
                "BUY target touching forward boundary rejected");

            Assert(
                LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    -1,
                    80,
                    68,
                    82,
                    1),
                "SELL target advances only forward");

            Assert(
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    -1,
                    80,
                    81,
                    82,
                    1),
                "SELL target backward move rejected");

            Assert(
                !LiveExitGeometryRule.ShouldAdvanceLiveTarget(
                    -1,
                    80,
                    78,
                    77,
                    1),
                "SELL target touching forward boundary rejected");

            Assert(
                LiveExitGeometryRule.IsProgressiveTargetLadder(
                    1,
                    100,
                    115,
                    130,
                    150,
                    175),
                "BUY progressive TP ladder");

            Assert(
                LiveExitGeometryRule.IsProgressiveTargetLadder(
                    -1,
                    100,
                    85,
                    70,
                    50,
                    30),
                "SELL progressive TP ladder");

            Assert(
                LiveExitGeometryRule.IsProgressiveTargetLadder(
                    1,
                    100,
                    115,
                    0,
                    150,
                    175),
                "BUY progressive TP ladder tolerates unavailable middle stage");

            Assert(
                !LiveExitGeometryRule.IsProgressiveTargetLadder(
                    1,
                    100,
                    115,
                    110,
                    150,
                    175),
                "BUY regressive TP ladder rejected");

            Assert(
                !LiveExitGeometryRule.IsProgressiveTargetLadder(
                    1,
                    100,
                    115,
                    double.NaN,
                    150,
                    175),
                "non-finite TP ladder stage ignored safely");

            Assert(
                !LiveExitGeometryRule.IsProtectiveStop(
                    1,
                    100,
                    110,
                    120,
                    1),
                "BUY stop wrong side rejected");

            Assert(
                !LiveExitGeometryRule.IsProtectiveStop(
                    -1,
                    100,
                    90,
                    80,
                    1),
                "SELL stop wrong side rejected");

            Assert(
                LiveExitGeometryRule.IsProtectiveStop(
                    1,
                    100,
                    115,
                    108,
                    1) &&
                LiveExitGeometryRule.IsProtectiveStop(
                    -1,
                    100,
                    85,
                    92,
                    1),
                "BUY/SELL protective stop symmetry");

            Assert(
                !LiveExitGeometryRule.IsProtectiveStop(
                    1,
                    100,
                    110,
                    double.NaN,
                    1),
                "non-finite protective stop rejected");
        }

        private static void VerifyMarketRegimeClassification()
        {
            MarketRegimeClassificationInput trend =
                new MarketRegimeClassificationInput
                {
                    AtrRatio = 1.00,
                    Choppiness = 35,
                    RangeEfficiency = 0.65,
                    RangeWidthAtr = 8.0,
                    ReturnAtr = 2.5,
                    Adx = 28,
                    EmaSpreadAtr = 0.60
                };

            string trendRegime =
                MarketRegimeClassifier.Classify(
                    trend, 0.78, 1.30, 1.65,
                    58, 0.30, 3.50, 0.80,
                    20, 18, 0.32, 58, 0.30);

            Assert(
                trendRegime == "TREND",
                "trend regime classification");

            MarketRegimeClassificationInput range =
                new MarketRegimeClassificationInput
                {
                    AtrRatio = 0.95,
                    Choppiness = 70,
                    RangeEfficiency = 0.15,
                    RangeWidthAtr = 2.5,
                    ReturnAtr = 0.35,
                    Adx = 12,
                    EmaSpreadAtr = 0.10
                };

            string rangeRegime =
                MarketRegimeClassifier.Classify(
                    range, 0.78, 1.30, 1.65,
                    58, 0.30, 3.50, 0.80,
                    20, 18, 0.32, 58, 0.30);

            Assert(
                rangeRegime == "COMPRESSION",
                "micro-range classification");

            MarketRegimeClassificationInput expansion =
                new MarketRegimeClassificationInput
                {
                    AtrRatio = 1.45,
                    Choppiness = 45,
                    RangeEfficiency = 0.50,
                    RangeWidthAtr = 6.0,
                    ReturnAtr = 2.0,
                    Adx = 23,
                    EmaSpreadAtr = 0.35
                };

            string expansionRegime =
                MarketRegimeClassifier.Classify(
                    expansion, 0.78, 1.30, 1.65,
                    58, 0.30, 3.50, 0.80,
                    20, 18, 0.32, 58, 0.30);

            Assert(
                expansionRegime == "EXPANSION",
                "expansion regime classification");

            MarketRegimeClassificationInput highVol =
                new MarketRegimeClassificationInput
                {
                    AtrRatio = 2.00,
                    Choppiness = 63,
                    RangeEfficiency = 0.20,
                    RangeWidthAtr = 9.0,
                    ReturnAtr = 3.0,
                    Adx = 16,
                    EmaSpreadAtr = 0.25
                };

            string highVolRegime =
                MarketRegimeClassifier.Classify(
                    highVol, 0.78, 1.30, 1.65,
                    58, 0.30, 3.50, 0.80,
                    20, 18, 0.32, 58, 0.30);

            Assert(
                highVolRegime == "HIGH_VOLATILITY",
                "high volatility classification");

            Assert(
                MarketRegimeClassifier.Quality(
                    "RANGE",
                    range) <
                MarketRegimeClassifier.Quality(
                    "TREND",
                    trend),
                "regime quality ordering");

            Assert(
                MarketRegimeClassifier.Quality(
                    "COMPRESSION",
                    range) <
                50,
                "compression quality is low");
        }

        private static void VerifyEntryTrapRisk()
        {
            EntryTrapRiskResult safe =
                EntryTrapRiskRule.Evaluate(
                    1,
                    0.50,
                    0.05,
                    0.05,
                    0,
                    false);

            Assert(
                !safe.Block &&
                safe.Risk < 40,
                "safe entry trap risk");

            EntryTrapRiskResult buyTop =
                EntryTrapRiskRule.Evaluate(
                    1,
                    0.92,
                    0.20,
                    0.10,
                    0,
                    false);

            Assert(
                buyTop.Block &&
                buyTop.Reason == "EXTREME ENTRY LOCATION",
                "buy at range top blocked");

            EntryTrapRiskResult sellBottom =
                EntryTrapRiskRule.Evaluate(
                    -1,
                    0.08,
                    0.20,
                    0.10,
                    0,
                    false);

            Assert(
                sellBottom.Block &&
                sellBottom.Reason == "EXTREME ENTRY LOCATION",
                "sell at range bottom blocked");

            EntryTrapRiskResult fallingBuy =
                EntryTrapRiskRule.Evaluate(
                    1,
                    0.55,
                    0.55,
                    0.45,
                    0,
                    false);

            Assert(
                fallingBuy.Block &&
                fallingBuy.Reason == "ADVERSE MOMENTUM",
                "falling BUY blocked");

            EntryTrapRiskResult opposingDiv =
                EntryTrapRiskRule.Evaluate(
                    1,
                    0.55,
                    0.10,
                    0.05,
                    82,
                    false);

            Assert(
                opposingDiv.Block &&
                opposingDiv.Reason ==
                    "OPPOSING REGULAR DIVERGENCE",
                "opposing divergence blocks entry");

            EntryTrapRiskResult hiddenSupport =
                EntryTrapRiskRule.Evaluate(
                    1,
                    0.55,
                    0.05,
                    0.05,
                    0,
                    true);

            Assert(
                !hiddenSupport.Block &&
                hiddenSupport.Risk < 20,
                "supportive hidden divergence reduces trap risk");
        }

        private static void VerifyActionableSignalQuality()
        {
            ActionableSignalQualityInput strong =
                new ActionableSignalQualityInput(
                    85, 80, 85, 6, 6,
                    80, 82, 80, 2.10,
                    76, 73, 75, 5, 5,
                    70, 75, 70, 1.50);

            ActionableSignalQualityResult accepted =
                ActionableSignalQualityRule.Evaluate(
                    strong);

            Assert(
                accepted.Allowed &&
                string.IsNullOrEmpty(accepted.Reason),
                "strong actionable signal accepted");

            ActionableSignalQualityResult weakTiming =
                ActionableSignalQualityRule.Evaluate(
                    new ActionableSignalQualityInput(
                        85, 80, 85, 6, 6,
                        80, 74, 80, 2.10,
                        76, 73, 75, 5, 5,
                        70, 75, 70, 1.50));

            Assert(
                weakTiming.Allowed &&
                weakTiming.Reason ==
                    "ACTIONABLE • QUALITY RECOVERY",
                "strong one-dimension timing recovery accepted");

            ActionableSignalQualityResult veryWeakTiming =
                ActionableSignalQualityRule.Evaluate(
                    new ActionableSignalQualityInput(
                        85, 80, 85, 6, 6,
                        80, 60, 80, 2.10,
                        76, 73, 75, 5, 5,
                        70, 75, 70, 1.50));

            Assert(
                !veryWeakTiming.Allowed &&
                veryWeakTiming.Reason ==
                    "SIGNAL QUALITY • TIMING",
                "weak timing remains rejected");

            ActionableSignalQualityResult recoveredPosition =
                ActionableSignalQualityRule.Evaluate(
                    new ActionableSignalQualityInput(
                        85, 80, 85, 6, 6,
                        80, 82, 69, 2.10,
                        76, 73, 75, 5, 5,
                        70, 75, 70, 1.50));

            Assert(
                recoveredPosition.Allowed &&
                recoveredPosition.Reason ==
                    "ACTIONABLE • QUALITY RECOVERY",
                "strong one-dimension price-position recovery accepted");

            ActionableSignalQualityResult weakEvidence =
                ActionableSignalQualityRule.Evaluate(
                    new ActionableSignalQualityInput(
                        85, 80, 85, 4, 6,
                        80, 82, 80, 2.10,
                        76, 73, 75, 5, 5,
                        70, 75, 70, 1.50));

            Assert(
                !weakEvidence.Allowed &&
                weakEvidence.Reason ==
                    "SIGNAL QUALITY • INDEPENDENT EVIDENCE",
                "weak independent evidence rejected");

            ActionableSignalQualityResult weakRiskReward =
                ActionableSignalQualityRule.Evaluate(
                    new ActionableSignalQualityInput(
                        85, 80, 85, 6, 6,
                        80, 82, 80, 1.40,
                        76, 73, 75, 5, 5,
                        70, 75, 70, 1.50));

            Assert(
                !weakRiskReward.Allowed &&
                weakRiskReward.Reason ==
                    "SIGNAL QUALITY • RR",
                "weak RR rejected");

            ActionableSignalQualityResult repeat =
                ActionableSignalQualityRule.Evaluate(
                    strong);

            Assert(
                repeat.Allowed == accepted.Allowed &&
                repeat.Reason == accepted.Reason,
                "actionable quality deterministic");
        }

        private static void VerifySignalVisualLifecycle()
        {
            SignalVisualLifecycleInput fresh =
                new SignalVisualLifecycleInput(
                    false,
                    false,
                    true,
                    100,
                    101,
                    1,
                    1,
                    true,
                    true,
                    true);

            Assert(
                SignalVisualLifecycleRule.IsPreTradePlanVisible(fresh),
                "fresh actionable plan visual is visible");

            SignalVisualLifecycleInput expired =
                new SignalVisualLifecycleInput(
                    false,
                    false,
                    true,
                    100,
                    103,
                    1,
                    1,
                    true,
                    true,
                    true);

            Assert(
                !SignalVisualLifecycleRule.IsPreTradePlanVisible(expired),
                "expired pre-trade visual is hidden");

            SignalVisualLifecycleInput movedDirection =
                new SignalVisualLifecycleInput(
                    false,
                    false,
                    true,
                    100,
                    101,
                    1,
                    -1,
                    true,
                    true,
                    true);

            Assert(
                !SignalVisualLifecycleRule.IsPreTradePlanVisible(movedDirection),
                "direction-changed plan visual is hidden");

            SignalVisualLifecycleInput blocked =
                new SignalVisualLifecycleInput(
                    false,
                    false,
                    true,
                    100,
                    101,
                    1,
                    1,
                    true,
                    false,
                    true);

            Assert(
                !SignalVisualLifecycleRule.IsPreTradePlanVisible(blocked),
                "non-actionable plan visual is hidden");

            SignalVisualLifecycleInput live =
                new SignalVisualLifecycleInput(
                    true,
                    false,
                    true,
                    100,
                    101,
                    1,
                    1,
                    true,
                    true,
                    true);

            Assert(
                !SignalVisualLifecycleRule.IsPreTradePlanVisible(live),
                "live position does not use pre-trade visual state");

            Assert(
                SignalVisualLifecycleRule.IsSetupPreviewVisible(
                    true,
                    100,
                    102,
                    1,
                    1,
                    true),
                "fresh setup preview is visible");

            Assert(
                !SignalVisualLifecycleRule.IsSetupPreviewVisible(
                    true,
                    100,
                    103,
                    1,
                    1,
                    true),
                "expired setup preview is hidden");
        }

        private static void VerifyRangeSignalQuality()
        {
            RangeSignalQualityInput weakMiddle =
                new RangeSignalQualityInput(
                    1, 0.52, 0.20, 72, 14,
                    true, true, true,
                    true, true, false,
                    70, 4, 2,
                    85, 82, 0);

            RangeSignalQualityResult middleResult =
                RangeSignalQualityRule.Evaluate(
                    "RANGE",
                    weakMiddle);

            Assert(
                !middleResult.Allowed &&
                middleResult.Reason ==
                    "RANGE NO-TRADE • MID-RANGE",
                "range mid-zone rejection");

            RangeSignalQualityInput weakLiquidity =
                new RangeSignalQualityInput(
                    1, 0.22, 0.20, 72, 14,
                    false, true, true,
                    true, true, false,
                    70, 5, 2,
                    85, 82, 0);

            RangeSignalQualityResult liquidityResult =
                RangeSignalQualityRule.Evaluate(
                    "RANGE",
                    weakLiquidity);

            Assert(
                !liquidityResult.Allowed &&
                liquidityResult.Reason ==
                    "RANGE NO-TRADE • NO LIQUIDITY EVENT",
                "range liquidity requirement");

            RangeSignalQualityInput strongReversal =
                new RangeSignalQualityInput(
                    1, 0.18, 0.20, 70, 14,
                    true, true, true,
                    true, true, false,
                    72, 6, 3,
                    89, 84, 8);

            RangeSignalQualityResult reversalResult =
                RangeSignalQualityRule.Evaluate(
                    "RANGE",
                    strongReversal);

            Assert(
                reversalResult.Allowed &&
                reversalResult.Reason ==
                    "RANGE REVERSAL QUALIFIED",
                "strong range reversal");

            RangeSignalQualityInput strongBreakout =
                new RangeSignalQualityInput(
                    1, 1.02, 0.42, 60, 22,
                    false, true, true,
                    true, false, true,
                    68, 6, 3,
                    88, 83, 5);

            RangeSignalQualityResult breakoutResult =
                RangeSignalQualityRule.Evaluate(
                    "RANGE",
                    strongBreakout);

            Assert(
                breakoutResult.Allowed &&
                breakoutResult.Reason ==
                    "RANGE BREAKOUT QUALIFIED",
                "strong range breakout");

            RangeSignalQualityResult compression =
                RangeSignalQualityRule.Evaluate(
                    "COMPRESSION",
                    strongBreakout);

            Assert(
                !compression.Allowed &&
                compression.Reason ==
                    "COMPRESSION NO-TRADE",
                "compression hard no-trade");
        }

        private static void VerifyIndicatorEvidenceFusion()
        {
            IndicatorEvidenceFusionResult trendAligned =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "TREND",
                        true, false,
                        true, false,
                        true, false,
                        true, false,
                        true, false,
                        true, false,
                        true, true, true, true,
                        6, 25, 20,
                        61, 1.25, 0.20,
                        1, 74,

                            58,
                        0, 0,
                        true, false,
                        false, false,
                        5, 1, 8, 4, 3));

            Assert(
                trendAligned.BullBonus >
                    trendAligned.BearBonus &&
                trendAligned.Quality >= 60 &&
                trendAligned.Conflict < 35,
                "trend indicator fusion");

            IndicatorEvidenceFusionResult conflicted =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "TRANSITION",
                        true, true,
                        true, true,
                        true, true,
                        true, true,
                        true, true,
                        true, true,
                        true, true, true, true,
                        6, 25, 20,
                        50, 0, 0,
                        0, 0,

                            58,
                        0, 0,
                        false, false,
                        false, false,
                        4, 4, 8, 4, 3));

            Assert(
                conflicted.Conflict >= 45 &&
                conflicted.Quality < 65,
                "indicator conflict penalty");

            IndicatorEvidenceFusionResult rangeReversal =
                IndicatorEvidenceFusionRule.Evaluate(
                    new IndicatorEvidenceFusionInput(
                        "RANGE",
                        true, false,
                        true, false,
                        false, false,
                        false, false,
                        false, false,
                        false, false,
                        true, true, true, true,
                        6, 25, 20,
                        36, -0.30, -0.05,
                        1, 76,

                            58,
                        0, 0,
                        true, false,
                        true, false,
                        6, 1, 8, 4, 3));

            Assert(
                rangeReversal.BullBonus >
                    rangeReversal.BearBonus &&
                rangeReversal.Quality >= 60,
                "range reversal indicator fusion");
        }

        private static void VerifyIndicatorActionability()
        {
            IndicatorActionabilityResult trend =
                IndicatorActionabilityRule.Evaluate(
                    "TREND",
                    72,
                    30);

            Assert(
                trend.Allowed,
                "strong trend indicator actionability");

            IndicatorActionabilityResult weakQuality =
                IndicatorActionabilityRule.Evaluate(
                    "TREND",
                    59,
                    30);

            Assert(
                !weakQuality.Allowed &&
                weakQuality.Reason ==
                    "INDICATOR FUSION • QUALITY",
                "weak trend indicator quality blocks");

            IndicatorActionabilityResult unavailable =
                IndicatorActionabilityRule.Evaluate(
                    "TREND",
                    0,
                    0);

            Assert(
                !unavailable.Allowed &&
                unavailable.Reason ==
                    "INDICATOR FUSION • QUALITY",
                "unavailable indicator fusion fails closed");

            IndicatorActionabilityResult conflicted =
                IndicatorActionabilityRule.Evaluate(
                    "TREND",
                    75,
                    53);

            Assert(
                !conflicted.Allowed &&
                conflicted.Reason ==
                    "INDICATOR FUSION • CONFLICT",
                "high indicator conflict blocks");

            IndicatorActionabilityResult range =
                IndicatorActionabilityRule.Evaluate(
                    "RANGE",
                    58,
                    55);

            Assert(
                range.Allowed,
                "range uses bounded indicator floor");

            IndicatorActionabilityResult compression =
                IndicatorActionabilityRule.Evaluate(
                    "COMPRESSION",
                    95,
                    0);

            Assert(
                !compression.Allowed &&
                compression.Reason ==
                    "INDICATOR FUSION • COMPRESSION",
                "compression indicator gate");

            IndicatorActionabilityResult mirrored =
                IndicatorActionabilityRule.Evaluate(
                    "TREND",
                    72,
                    30);

            Assert(
                mirrored.Allowed &&
                mirrored.Reason == trend.Reason,
                "indicator actionability deterministic");
        }



        private static void VerifySmartBreakEven()
        {
            SmartBreakEvenResult strong =
                SmartBreakEvenRule.Evaluate(
                    20,
                    40,
                    1.5,
                    0.90,
                    0.5,
                    0.5,
                    true);

            Assert(
                strong.Allowed &&
                strong.TriggerPips >= 18 &&
                strong.TriggerPips < 30 &&
                strong.OffsetPips >= 2 &&
                strong.OffsetPips <= 10,
                "smart break-even uses risk, TP1 and spread-aware protection");

            SmartBreakEvenResult tooTight =
                SmartBreakEvenRule.Evaluate(
                    20,
                    10,
                    1,
                    0.90,
                    0.5,
                    0.5,
                    true);

            Assert(
                !tooTight.Allowed &&
                tooTight.Reason == "TP1 TOO CLOSE FOR SMART BE",
                "smart break-even avoids TP1 collision");

            SmartBreakEvenResult mirrored =
                SmartBreakEvenRule.Evaluate(
                    20,
                    40,
                    1.5,
                    0.90,
                    0.5,
                    0.5,
                    true);

            Assert(
                mirrored.Allowed &&
                Math.Abs(
                    mirrored.TriggerPips -
                    strong.TriggerPips) < 0.0001 &&
                Math.Abs(
                    mirrored.OffsetPips -
                    strong.OffsetPips) < 0.0001,
                "smart break-even deterministic");

            SmartBreakEvenResult invalidRisk =
                SmartBreakEvenRule.Evaluate(
                    double.NaN,
                    40,
                    1.5,
                    0.90,
                    0.5,
                    0.5,
                    true);

            Assert(
                !invalidRisk.Allowed &&
                invalidRisk.Reason == "RISK/TP NUMERIC INVALID",
                "smart break-even rejects non-finite risk");

            SmartBreakEvenResult invalidTp =
                SmartBreakEvenRule.Evaluate(
                    20,
                    double.PositiveInfinity,
                    1.5,
                    0.90,
                    0.5,
                    0.5,
                    true);

            Assert(
                !invalidTp.Allowed &&
                invalidTp.Reason == "RISK/TP NUMERIC INVALID",
                "smart break-even rejects non-finite TP1");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Decision contract failed: " + name);
        }
    }
}