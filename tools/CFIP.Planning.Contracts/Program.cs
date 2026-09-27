using System;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyTargetSymmetry();
            VerifyTargetInvalidDirections();
            VerifyRiskPercentBounds();
            VerifyRiskAmount();
            VerifyMarginUsage();

            Console.WriteLine("Planning contracts OK");
        }

        private static void VerifyTargetSymmetry()
        {
            Assert(
                TargetProgressionRule.IsValid(1, 100, 110),
                "BUY progression");

            Assert(
                TargetProgressionRule.IsValid(-1, 100, 90),
                "SELL progression");

            Assert(
                TargetProgressionRule.IsValid(1, 90, 100) ==
                TargetProgressionRule.IsValid(-1, 90, 80),
                "directional progression symmetry");
        }

        private static void VerifyTargetInvalidDirections()
        {
            Assert(
                !TargetProgressionRule.IsValid(0, 100, 110),
                "neutral progression blocked");

            Assert(
                !TargetProgressionRule.IsValid(1, 100, 100),
                "equal target blocked");
        }

        private static void VerifyRiskPercentBounds()
        {
            Assert(
                RiskPercentPolicy.Calculate(2, false, 0.5) == 2,
                "base risk");

            Assert(
                RiskPercentPolicy.Calculate(10, true, 0.5) == 5,
                "smart risk scaling");

            Assert(
                RiskPercentPolicy.Calculate(0, true, 1) >= 0.05,
                "minimum risk");

            Assert(
                RiskPercentPolicy.Calculate(10, true, 2) == 10,
                "maximum multiplier clamp");

            Assert(
                RiskPercentPolicy.Calculate(10, true, 0.1) == 2.5,
                "minimum multiplier clamp");
        }

        private static void VerifyRiskAmount()
        {
            Assert(
                RiskAmountCalculator.Calculate(10000, 1.5) == 150,
                "risk amount");

            Assert(
                RiskAmountCalculator.Calculate(0, 1) == 0,
                "zero equity");

            Assert(
                RiskAmountCalculator.Calculate(10000, 0) == 0,
                "zero risk");
        }

        private static void VerifyMarginUsage()
        {
            Assert(
                MarginUsagePolicy.CalculateAllowedPercent(80, 20) == 60,
                "margin buffer");

            Assert(
                MarginUsagePolicy.CalculateAllowedPercent(5, 0) == 10,
                "margin floor");

            Assert(
                MarginUsagePolicy.CalculateAllowedPercent(120, 0) == 100,
                "margin ceiling");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Planning contract failed: " + name);
        }
    }
}
