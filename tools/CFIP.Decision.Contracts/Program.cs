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
                    directional.RegimeQuality);

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
                calculator.Calculate(50, 0, 0, 0, 0);

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
                new Decision { Direction = 1 };

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

            Frame structuralOnly =
                new Frame
                {
                    StructureBull = true,
                    MssBull = true,
                    ChochBull = true,
                    DisplacementBull = true
                };

            int structural =
                calculator.Calculate(
                    new IndependentEvidenceFusionInput(
                        structuralOnly.StructureBull,
                        structuralOnly.MssBull || structuralOnly.ChochBull,
                        structuralOnly.DisplacementBull,
                        structuralOnly.LiquidityBull,
                        structuralOnly.FvgBull,
                        structuralOnly.ObBull,
                        structuralOnly.TrendBull,
                        structuralOnly.MomentumBull,
                        structuralOnly.MacdBull,
                        structuralOnly.VwapBull,
                        structuralOnly.VolumeBull,
                        structuralOnly.VolatilityBull,
                        structuralOnly.RejectionBull,
                        structuralOnly.EqualLow));

            Assert(
                structural == 2,
                "correlated structural evidence is capped");

            Frame fullBull =
                new Frame
                {
                    StructureBull = true,
                    MssBull = true,
                    ChochBull = true,
                    DisplacementBull = true,
                    LiquidityBull = true,
                    FvgBull = true,
                    ObBull = true,
                    TrendBull = true,
                    MomentumBull = true,
                    MacdBull = true,
                    VwapBull = true,
                    VolumeBull = true,
                    VolatilityBull = true,
                    RejectionBull = true,
                    EqualLow = true
                };

            int bull =
                calculator.Calculate(
                    new IndependentEvidenceFusionInput(
                        fullBull.StructureBull,
                        fullBull.MssBull || fullBull.ChochBull,
                        fullBull.DisplacementBull,
                        fullBull.LiquidityBull,
                        fullBull.FvgBull,
                        fullBull.ObBull,
                        fullBull.TrendBull,
                        fullBull.MomentumBull,
                        fullBull.MacdBull,
                        fullBull.VwapBull,
                        fullBull.VolumeBull,
                        fullBull.VolatilityBull,
                        fullBull.RejectionBull,
                        fullBull.EqualLow));

            Frame fullBear =
                new Frame
                {
                    StructureBear = true,
                    MssBear = true,
                    ChochBear = true,
                    DisplacementBear = true,
                    LiquidityBear = true,
                    FvgBear = true,
                    ObBear = true,
                    TrendBear = true,
                    MomentumBear = true,
                    MacdBear = true,
                    VwapBear = true,
                    VolumeBear = true,
                    VolatilityBear = true,
                    RejectionBear = true,
                    EqualHigh = true
                };

            int bear =
                calculator.Calculate(
                    new IndependentEvidenceFusionInput(
                        fullBear.StructureBear,
                        fullBear.MssBear || fullBear.ChochBear,
                        fullBear.DisplacementBear,
                        fullBear.LiquidityBear,
                        fullBear.FvgBear,
                        fullBear.ObBear,
                        fullBear.TrendBear,
                        fullBear.MomentumBear,
                        fullBear.MacdBear,
                        fullBear.VwapBear,
                        fullBear.VolumeBear,
                        fullBear.VolatilityBear,
                        fullBear.RejectionBear,
                        fullBear.EqualHigh));

            Assert(bull == 8, "maximum independent evidence");
            Assert(bear == bull, "BUY/SELL evidence symmetry");
        }

        private static void VerifyQualityWeightedFrameContribution()
        {
            DecisionFrameContributionCalculator calculator =
                new DecisionFrameContributionCalculator();

            Frame strong =
                new Frame
                {
                    BullScore = 80,
                    BearScore = 20,
                    Quality = 80
                };

            DecisionFrameContribution contribution =
                calculator.Calculate(
                    strong,
                    10);

            Assert(
                Math.Abs(contribution.Bull - 64) < 0.0001,
                "quality-weighted bull contribution");

            Assert(
                Math.Abs(contribution.Bear - 16) < 0.0001,
                "quality-weighted bear contribution");

            Frame mirror =
                new Frame
                {
                    BullScore = 20,
                    BearScore = 80,
                    Quality = 80
                };

            DecisionFrameContribution mirrored =
                calculator.Calculate(
                    mirror,
                    10);

            Assert(
                Math.Abs(contribution.Bull - mirrored.Bear) < 0.0001 &&
                Math.Abs(contribution.Bear - mirrored.Bull) < 0.0001,
                "BUY/SELL contribution symmetry");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Decision contract failed: " + name);
        }
    }
}
