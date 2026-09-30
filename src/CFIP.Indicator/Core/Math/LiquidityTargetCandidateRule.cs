using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Platform-neutral rules for structural liquidity target candidates.
    /// Candidate extraction remains owned by the indicator analysis boundary;
    /// this type owns only direction, spacing and deterministic ordering.
    /// </summary>
    internal static class LiquidityTargetCandidateRule
    {
        public static double MinimumSeparationPrice(
            double atr,
            double minimumSpacingAtr)
        {
            if (atr <= 0 ||
                double.IsNaN(atr) ||
                double.IsInfinity(atr))
                return 0;

            double spacingAtr =
                Math.Max(
                    0.05,
                    minimumSpacingAtr);

            return atr * spacingAtr;
        }

        public static bool IsDirectionallyValid(
            int direction,
            double referencePrice,
            double candidatePrice)
        {
            if ((direction != 1 && direction != -1) ||
                referencePrice <= 0 ||
                candidatePrice <= 0 ||
                double.IsNaN(referencePrice) ||
                double.IsInfinity(referencePrice) ||
                double.IsNaN(candidatePrice) ||
                double.IsInfinity(candidatePrice))
                return false;

            return direction == 1
                ? candidatePrice > referencePrice
                : candidatePrice < referencePrice;
        }

        public static bool IsDistinct(
            double candidatePrice,
            List<double> acceptedPrices,
            double atr,
            double minimumSpacingAtr)
        {
            if (candidatePrice <= 0 ||
                double.IsNaN(candidatePrice) ||
                double.IsInfinity(candidatePrice) ||
                acceptedPrices == null)
                return false;

            double minimumSeparation =
                MinimumSeparationPrice(
                    atr,
                    minimumSpacingAtr);

            if (minimumSeparation <= 0)
                return false;

            for (int i = 0;
                 i < acceptedPrices.Count;
                 i++)
            {
                double accepted = acceptedPrices[i];

                if (Math.Abs(
                        accepted -
                        candidatePrice) <
                    minimumSeparation)
                    return false;
            }

            return true;
        }

        public static List<double> OrderByDistance(
            int direction,
            double referencePrice,
            List<double> candidates)
        {
            List<double> ordered =
                candidates == null
                    ? new List<double>()
                    : new List<double>(candidates);

            if ((direction != 1 && direction != -1) ||
                referencePrice <= 0)
                return ordered;

            ordered.Sort(
                (left, right) =>
                {
                    double leftDistance =
                        Math.Abs(
                            left -
                            referencePrice);

                    double rightDistance =
                        Math.Abs(
                            right -
                            referencePrice);

                    int comparison =
                        leftDistance.CompareTo(
                            rightDistance);

                    if (comparison != 0)
                        return comparison;

                    return left.CompareTo(right);
                });

            return ordered;
        }
    }
}
