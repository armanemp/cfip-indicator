using System;
using System.Collections.Generic;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyConsensusSymmetry();
            VerifyNeutralQualityIsolation();
            VerifyQualityBoundaries();
            VerifyConfidenceCalibration();
            VerifyContextualConfidenceCalibration();
            VerifyThresholdReasons();
            VerifySmartConsensusReasons();
            VerifyConfidenceDeterminism();
            VerifyCorrelationAwareEvidence();
            VerifyQualityWeightedFrameContribution();
            VerifyMarketRegimeClassification();
            VerifyEntryTrapRisk();
            VerifyActionableSignalQuality();
            VerifyRangeSignalQuality();
            VerifyIndicatorEvidenceFusion();
            VerifyIndicatorActionability();

            Console.WriteLine("Decision contracts OK");
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
            IndependentEvidenceFusionCalculator calculator =
                new IndependentEvidenceFusionCalculator();

            int structural =
                calculator.Calculate(
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
                "correlated structural evidence is capped");

            int bull =
                calculator.Calculate(
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
                calculator.Calculate(
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

            Assert(bull == 8, "maximum independent evidence");
            Assert(bear == bull, "BUY/SELL evidence symmetry");
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
                !weakTiming.Allowed &&
                weakTiming.Reason ==
                    "SIGNAL QUALITY • TIMING",
                "weak timing rejected");

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

        private static void VerifyRangeSignalQuality()
        {
            RangeSignalQualityInput weakMiddle =
                new RangeSignalQualityInput(
                    1, 0.52, 0.20, 72, 14,
                    true, true, true,
                    true, true, false,
                    70, 4, 2,
                    85, 82, 0, 2.0);

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
                    85, 82, 0, 2.0);

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
                    89, 84, 8, 2.0);

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
                    88, 83, 5, 2.1);

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

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Decision contract failed: " + name);
        }
    }
}
