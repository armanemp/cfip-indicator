using System;

namespace cAlgo
{
    internal static class ServerPartialTakeProfitEvidenceRule
    {
        public static bool IsMatchingClosingDeal(
            int positionId,
            int direction,
            int dealPositionId,
            int dealDirection,
            bool isClosing,
            double executionPrice,
            double expectedPrice,
            double volumeInUnits,
            double expectedVolumeInUnits,
            double priceTolerance,
            double volumeTolerance)
        {
            if (positionId <= 0 ||
                dealPositionId != positionId ||
                direction != 1 &&
                direction != -1 ||
                dealDirection != -direction ||
                !isClosing ||
                !IsFinitePositive(executionPrice) ||
                !IsFinitePositive(expectedPrice) ||
                !IsFinitePositive(volumeInUnits) ||
                !IsFinitePositive(expectedVolumeInUnits) ||
                !IsFiniteNonNegative(priceTolerance) ||
                !IsFiniteNonNegative(volumeTolerance))
                return false;

            return
                Math.Abs(
                    executionPrice -
                    expectedPrice) <=
                priceTolerance &&
                Math.Abs(
                    volumeInUnits -
                    expectedVolumeInUnits) <=
                volumeTolerance;
        }

        private static bool IsFinitePositive(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegative(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
