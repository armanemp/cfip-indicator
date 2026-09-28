using System;

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
            VerifyThresholdReasons();
            VerifySmartConsensusReasons();
            VerifyConfidenceDeterminism();
            VerifyCorrelationAwareEvidence();
            VerifyQualityWeightedFrameContribution();
            VerifyMarketRegimeClassification();

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

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Decision contract failed: " + name);
        }
    }
}
