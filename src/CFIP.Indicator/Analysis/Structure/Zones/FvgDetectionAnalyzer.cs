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
                    Zone candidate =
                        BuildManagedFvgZone(
                            bars,
                            i,
                            index,
                            direction,
                            low,
                            high,
                            gap,
                            false,
                            creationAtr);

                    if (!requireCurrentRetest ||
                        PassesCurrentFvgRetest(
                            bars,
                            index,
                            candidate,
                            atr))
                    {
                        best =
                            SelectNearestFvg(
                                nearestPrice,
                                best,
                                candidate);
                    }
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
                    Zone candidate =
                        BuildManagedFvgZone(
                            bars,
                            i,
                            index,
                            direction,
                            low,
                            high,
                            gap,
                            true,
                            creationAtr);

                    if (!requireCurrentRetest ||
                        PassesCurrentFvgRetest(
                            bars,
                            index,
                            candidate,
                            atr))
                    {
                        best =
                            SelectNearestFvg(
                                nearestPrice,
                                best,
                                candidate);
                    }
                }
            }

            if (best == null ||
                best.Age >
                MaximumZoneAgeBars)
                return null;

            if (requireCurrentRetest &&
                RequireFvgRetest &&
                !PassesCurrentFvgRetest(
                    bars,
                    index,
                    best,
                    atr))
                return null;

            return best;
        }

        private bool PassesCurrentFvgRetest(
            Bars bars,
            int index,
            Zone zone,
            double atr)
        {
            if (bars == null ||
                zone == null ||
                zone.CreatedIndex < 0 ||
                index <= zone.CreatedIndex ||
                index >= bars.Count ||
                atr <= 0)
                return false;

            double tolerance =
                atr *
                (RequireRetestQuality
                    ? RetestZoneToleranceAtr
                    : ZoneProximityAtr);

            return FvgRule.IsOverlapInclusive(
                bars.LowPrices[index] - tolerance,
                bars.HighPrices[index] + tolerance,
                zone.Low,
                zone.High);
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
