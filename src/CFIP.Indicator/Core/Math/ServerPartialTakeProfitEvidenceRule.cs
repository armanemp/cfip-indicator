using System;

namespace cAlgo
{
    internal static class ServerPartialTakeProfitEvidenceRule
    {
        public static bool IsMatchingClosingDeal(
            long positionId,
            int direction,
            long dealPositionId,
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
                !NumericGuards.IsFinitePositive(executionPrice) ||
                !NumericGuards.IsFinitePositive(expectedPrice) ||
                !NumericGuards.IsFinitePositive(volumeInUnits) ||
                !NumericGuards.IsFinitePositive(expectedVolumeInUnits) ||
                !NumericGuards.IsFiniteValue(priceTolerance) ||
                priceTolerance < 0 ||
                !NumericGuards.IsFiniteValue(volumeTolerance) ||
                volumeTolerance < 0)
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

    }
}
