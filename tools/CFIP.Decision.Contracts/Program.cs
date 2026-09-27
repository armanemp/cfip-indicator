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
                        50 * 0.28 +
                        70 * 0.12),
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
                    evidence.RegimeQuality);

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

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Decision contract failed: " + name);
        }
    }
}
