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
            Console.WriteLine("Decision contracts OK");
        }

        private static void VerifyConfidenceDeterminism()
        {
            DecisionConfidenceCalculator calculator =
                new DecisionConfidenceCalculator();

            int first =
                calculator.Calculate(82, 74, 78, 3, 4);

            int second =
                calculator.Calculate(82, 74, 78, 3, 4);

            Assert(
                first == second,
                "confidence deterministic repeatability");
        }



        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Decision contract failed: " + name);
        }
    }
}
