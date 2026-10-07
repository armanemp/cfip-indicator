using System;

namespace cAlgo
{
    internal static class SignalQualityPrecisionContracts
    {
        internal static void VerifyTimeframeAgreementPrecision()
        {
            int[] directions = { 1, 1, 1, 1, 1, 1, 1 };
            int[] qualities = { 100, 100, 100, 100, 100, 100, 100 };
            int[] actualIndices = { 1, 1, 1, 1, 1, 1, 1 };
            int[] expectedIndices = { 1, 1, 1, 1, 1, 1, 1 };
            double[] weights = { 7, 8, 5, 3, 2, 2, 3 };
            bool[] enabled = { true, true, true, true, true, true, true };

            Assert(
                TimeframeAgreementRule.Calculate(
                    1, directions, qualities, actualIndices,
                    expectedIndices, weights, enabled) == 100,
                "fully aligned high-quality frames must produce 100% MTF agreement");

            qualities[1] = 25;

            Assert(
                TimeframeAgreementRule.Calculate(
                    1, directions, qualities, actualIndices,
                    expectedIndices, weights, enabled) == 80,
                "weak aligned M15 quality must reduce MTF agreement");

            directions[1] = -1;
            qualities[1] = 100;

            int opposing =
                TimeframeAgreementRule.Calculate(
                    1, directions, qualities, actualIndices,
                    expectedIndices, weights, enabled);

            Assert(
                opposing == 73,
                "a strong opposing M15 frame must materially reduce MTF agreement");

            int mirrored =
                TimeframeAgreementRule.Calculate(
                    -1,
                    new[] { -1, 1, -1, -1, -1, -1, -1 },
                    qualities, actualIndices, expectedIndices,
                    weights, enabled);

            Assert(
                mirrored == opposing,
                "BUY/SELL quality-aware MTF agreement must be symmetric");

            Assert(
                TimeframeAgreementRule.Calculate(
                    1, directions, qualities, actualIndices, expectedIndices,
                    weights, new[] { true, false, true, true, true, true, true }) == 100,
                "disabled timeframes must not distort the agreement denominator");

            Assert(
                TimeframeAgreementRule.Calculate(
                    1,
                    new[] { 1, 1, 1 },
                    new[] { 100, 100, 100 },
                    new[] { 5, 4, 5 },
                    new[] { 5, 5, 5 },
                    new[] { 1.0, 8.0, 1.0 },
                    new[] { true, true, true }) == 100,
                "stale frames must be excluded");

            Assert(
                TimeframeAgreementRule.Calculate(
                    0, directions, qualities, actualIndices,
                    expectedIndices, weights, enabled) == 0,
                "invalid target direction must fail closed");
        }

        internal static void VerifyStructuralConfirmationDirectionality()
        {
            Assert(
                StructuralConfirmationRule.CountDirectionalStructureContribution(1, 1, true) == 1,
                "BUY structure on a BUY frame is supporting confirmation");

            Assert(
                StructuralConfirmationRule.CountDirectionalStructureContribution(1, 0, true) == 1,
                "BUY structure on a neutral frame remains transition evidence");

            Assert(
                StructuralConfirmationRule.CountDirectionalStructureContribution(1, -1, true) == 0,
                "BUY must not count structure from an opposing frame direction");

            Assert(
                StructuralConfirmationRule.CountDirectionalStructureContribution(-1, -1, true) == 1,
                "SELL structure on a SELL frame is supporting confirmation");

            Assert(
                StructuralConfirmationRule.CountDirectionalStructureContribution(-1, 1, true) == 0,
                "SELL must not count structure from an opposing frame direction");

            Assert(
                StructuralConfirmationRule.CountDirectionalStructureContribution(1, 1, false) == 0,
                "missing structure must not create confirmation");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
