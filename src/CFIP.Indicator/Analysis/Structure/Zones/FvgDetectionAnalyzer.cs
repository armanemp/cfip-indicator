// CFIP Indicator — FvgDetectionAnalyzer.cs
// Single-responsibility zone detection module.

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Zone FindNearestFvg(
            Bars bars,
            int index,
            int direction,
            double atr,
            bool requireCurrentRetest = true,
            double selectionPrice = double.NaN)
        {
            if (!UseFvg ||
                bars == null ||
                index < 2 ||
                atr <= 0)
                return null;

            int effectiveLookback =
                Math.Min(
                    FvgLookback,
                    Math.Max(
                        1,
                        MaximumZoneAgeBars));

            int first =
                Math.Max(
                    1,
                    index -
                    effectiveLookback);

            double nearestPrice =
                IsFinitePositive(selectionPrice)
                    ? selectionPrice
                    : bars.ClosePrices[index];

            Zone best = null;

            for (int i = index;
                 i >= first;
                 i--)
            {
                double creationAtr =
                    Atr(
                        bars,
                        i);

                if (i >= 2 &&
                    FvgRule.TryGetThreeBarGap(
                        direction,
                        bars.HighPrices[i - 2],
                        bars.LowPrices[i - 2],
                        bars.HighPrices[i],
                        bars.LowPrices[i],
                        out double low,
                        out double high,
                        out double gap) &&
                    FvgRule.MeetsMinimumGap(
                        gap,
                        creationAtr,
                        MinimumFvgAtr))
                {
                    best =
                        SelectNearestFvg(
                            nearestPrice,
                            best,
                            BuildManagedFvgZone(
                                bars,
                                i,
                                index,
                                direction,
                                low,
                                high,
                                gap,
                                false,
                                creationAtr));
                }

                if (UseTwoBarImbalanceFvg &&
                    FvgRule.TryGetTwoBarGap(
                        direction,
                        bars.HighPrices[i - 1],
                        bars.LowPrices[i - 1],
                        bars.HighPrices[i],
                        bars.LowPrices[i],
                        out low,
                        out high,
                        out gap) &&
                    FvgRule.MeetsMinimumGap(
                        gap,
                        creationAtr,
                        MinimumFvgAtr))
                {
                    best =
                        SelectNearestFvg(
                            nearestPrice,
                            best,
                            BuildManagedFvgZone(
                                bars,
                                i,
                                index,
                                direction,
                                low,
                                high,
                                gap,
                                true,
                                creationAtr));
                }
            }

            if (best == null ||
                best.Age >
                MaximumZoneAgeBars)
                return null;

            if (requireCurrentRetest &&
                RequireFvgRetest)
            {
                double price =
                    bars.ClosePrices[index];

                double tolerance =
                    atr *
                    (RequireRetestQuality
                        ? RetestZoneToleranceAtr
                        : ZoneProximityAtr);

                if (price <
                    best.Low -
                    tolerance ||
                    price >
                    best.High +
                    tolerance)
                    return null;
            }

            return best;
        }

        private Zone SelectNearestFvg(
            double price,
            Zone best,
            Zone candidate)
        {
            if (candidate == null)
                return best;

            if (best == null)
                return candidate;

            double candidateDistance =
                DistanceToZone(
                    price,
                    candidate);

            double bestDistance =
                DistanceToZone(
                    price,
                    best);

            if (candidateDistance <
                bestDistance -
                Symbol.TickSize)
                return candidate;

            if (Math.Abs(
                    candidateDistance -
                    bestDistance) <=
                Symbol.TickSize &&
                candidate.Quality >
                best.Quality)
                return candidate;

            if (Math.Abs(
                    candidateDistance -
                    bestDistance) <=
                Symbol.TickSize &&
                candidate.Quality ==
                best.Quality &&
                candidate.Age <
                best.Age)
                return candidate;

            return best;
        }
    }
}
