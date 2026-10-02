// CFIP Indicator — FvgDetectionAnalyzer.cs
// Single-responsibility zone detection module.

using System;
using System.Collections.Generic;
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
            double selectionPrice = double.NaN,
            bool preferQualityForSelection = false)
        {
            if (!UseFvg ||
                bars == null ||
                index < 2 ||
                atr <= 0)
                return null;

            bool cacheable =
                index >= 0 &&
                index <
                bars.Count - 1;

            if (cacheable)
            {
                ResetZoneLookupCacheIfNeeded(
                    bars,
                    index);
            }

            string cacheKey =
                direction.ToString() +
                "|" +
                (requireCurrentRetest ? "1" : "0");

            Zone[] candidates;

            if (!cacheable ||
                !TryGetCachedFvgCandidates(
                    cacheKey,
                    out candidates))
            {
                List<Zone> built =
                    new List<Zone>();

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

                for (int i = index;
                     i >= first;
                     i--)
                {
                    double creationAtr =
                        Atr(
                            bars,
                            i);

                    double low;
                    double high;
                    double gap;

                    if (i >= 2 &&
                        FvgRule.TryGetThreeBarGap(
                            direction,
                            bars.HighPrices[i - 2],
                            bars.LowPrices[i - 2],
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
                                false,
                                creationAtr);

                        if (candidate != null &&
                            (!requireCurrentRetest ||
                             PassesCurrentFvgRetest(
                                 bars,
                                 index,
                                 candidate,
                                 atr)))
                        {
                            built.Add(
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

                        if (candidate != null &&
                            (!requireCurrentRetest ||
                             PassesCurrentFvgRetest(
                                 bars,
                                 index,
                                 candidate,
                                 atr)))
                        {
                            built.Add(
                                candidate);
                        }
                    }
                }

                if (cacheable)
                {
                    StoreCachedFvgCandidates(
                        cacheKey,
                        built);
                }

                candidates =
                    built.ToArray();
            }

            double nearestPrice =
                IsFinitePositive(selectionPrice)
                    ? selectionPrice
                    : bars.ClosePrices[index];

            Zone best = null;

            for (int i = 0;
                 i < candidates.Length;
                 i++)
            {
                Zone candidate =
                    candidates[i];

                if (candidate == null ||
                    candidate.Age >
                    MaximumZoneAgeBars)
                    continue;

                if (requireCurrentRetest &&
                    RequireFvgRetest &&
                    !PassesCurrentFvgRetest(
                        bars,
                        index,
                        candidate,
                        atr))
                    continue;

                best =
                    preferQualityForSelection
                        ? SelectBestFvgForExecution(
                            nearestPrice,
                            atr,
                            best,
                            candidate)
                        : SelectNearestFvg(
                            nearestPrice,
                            best,
                            candidate);
            }

            return best;
        }

        private Zone SelectBestFvgForExecution(
            double price,
            double atr,
            Zone best,
            Zone candidate)
        {
            if (candidate == null)
                return best;

            if (best == null)
                return candidate;

            double candidateDistanceAtr =
                DistanceToZone(
                    price,
                    candidate) /
                Math.Max(
                    Symbol.TickSize,
                    atr);

            double bestDistanceAtr =
                DistanceToZone(
                    price,
                    best) /
                Math.Max(
                    Symbol.TickSize,
                    atr);

            double candidateScore =
                candidate.Quality -
                18.0 *
                Math.Min(
                    1.0,
                    candidateDistanceAtr / 3.0) -
                10.0 *
                Math.Min(
                    1.0,
                    candidate.Age /
                    (double)Math.Max(
                        1,
                        MaximumZoneAgeBars));

            double bestScore =
                best.Quality -
                18.0 *
                Math.Min(
                    1.0,
                    bestDistanceAtr / 3.0) -
                10.0 *
                Math.Min(
                    1.0,
                    best.Age /
                    (double)Math.Max(
                        1,
                        MaximumZoneAgeBars));

            if (candidateScore >
                bestScore +
                0.0001)
                return candidate;

            if (Math.Abs(
                    candidateScore -
                    bestScore) <=
                0.0001 &&
                candidate.Quality >
                best.Quality)
                return candidate;

            if (Math.Abs(
                    candidateScore -
                    bestScore) <=
                0.0001 &&
                candidate.Quality ==
                best.Quality &&
                candidate.Age <
                best.Age)
                return candidate;

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
