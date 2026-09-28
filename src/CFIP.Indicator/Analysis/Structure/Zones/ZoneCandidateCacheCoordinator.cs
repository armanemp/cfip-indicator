using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Zone FindEfficientFvg(
            Bars bars,
            int index,
            int direction,
            double atr)
        {
            Zone cached;

            if (TryFindCachedNearestFvg(
                    bars,
                    index,
                    direction,
                    atr,
                    true,
                    double.NaN,
                    out cached))
                return cached;

            return FindNearestFvg(
                bars,
                index,
                direction,
                atr);
        }

        private Zone FindEfficientOrderBlock(
            Bars bars,
            int index,
            int direction,
            double atr)
        {
            Zone cached;

            if (TryFindCachedNearestOrderBlock(
                    bars,
                    index,
                    direction,
                    atr,
                    true,
                    double.NaN,
                    out cached))
                return cached;

            return FindNearestOrderBlock(
                bars,
                index,
                direction,
                atr);
        }

        private bool TryFindCachedNearestFvg(
            Bars bars,
            int index,
            int direction,
            double atr,
            bool requireCurrentRetest,
            double selectionPrice,
            out Zone result)
        {
            result = null;

            List<Zone> candidates =
                GetCachedFvgCandidates(
                    bars,
                    index,
                    direction,
                    atr);

            if (candidates == null)
                return false;

            double nearestPrice =
                IsFinitePositive(selectionPrice)
                    ? selectionPrice
                    : bars.ClosePrices[index];

            Zone best = null;

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                Zone candidate =
                    candidates[i];

                if (candidate == null ||
                    candidate.Age >
                    MaximumZoneAgeBars)
                    continue;

                best =
                    SelectNearestFvg(
                        nearestPrice,
                        best,
                        candidate);
            }

            if (best == null)
                return true;

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
                        best.Low - tolerance ||
                    price >
                        best.High + tolerance)
                    return true;
            }

            result = best;
            return true;
        }

        private bool TryFindCachedNearestOrderBlock(
            Bars bars,
            int index,
            int direction,
            double atr,
            bool requireHistoricalRetest,
            double selectionPrice,
            out Zone result)
        {
            result = null;

            List<Zone> candidates =
                GetCachedOrderBlockCandidates(
                    bars,
                    index,
                    direction,
                    atr);

            if (candidates == null)
                return false;

            double market =
                IsFinitePositive(selectionPrice)
                    ? selectionPrice
                    : bars.ClosePrices[index];

            Zone best = null;
            double bestScore =
                double.MinValue;

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                Zone candidate =
                    candidates[i];

                if (candidate == null ||
                    candidate.Quality <
                    System.Math.Max(
                        50,
                        ObMinimumQuality))
                    continue;

                if (requireHistoricalRetest &&
                    RequireObRetest &&
                    !HasZoneRetest(
                        bars,
                        index - candidate.Age,
                        index,
                        candidate.Low,
                        candidate.High))
                    continue;

                double distance =
                    DistanceToZone(
                        market,
                        candidate);

                double distancePenalty =
                    14.0 *
                    System.Math.Min(
                        1.0,
                        distance /
                        System.Math.Max(
                            Symbol.TickSize,
                            atr * 3.0));

                double agePenalty =
                    System.Math.Min(
                        12.0,
                        candidate.Age * 0.35);

                double selectionScore =
                    candidate.Quality -
                    distancePenalty -
                    agePenalty;

                if (candidate.Quality >
                    (best == null
                        ? 0
                        : best.Quality))
                    selectionScore += 2.0;

                if (best == null ||
                    selectionScore > bestScore)
                {
                    best = candidate;
                    bestScore = selectionScore;
                }
            }

            result = best;
            return true;
        }

        private List<Zone> GetCachedFvgCandidates(
            Bars bars,
            int index,
            int direction,
            double atr)
        {
            ZoneCandidateCache cache =
                GetOrBuildZoneCandidateCache(
                    bars,
                    index,
                    atr);

            if (cache == null)
                return null;

            return direction == 1
                ? cache.BullFvgs
                : direction == -1
                    ? cache.BearFvgs
                    : null;
        }

        private List<Zone> GetCachedOrderBlockCandidates(
            Bars bars,
            int index,
            int direction,
            double atr)
        {
            ZoneCandidateCache cache =
                GetOrBuildZoneCandidateCache(
                    bars,
                    index,
                    atr);

            if (cache == null)
                return null;

            return direction == 1
                ? cache.BullOrderBlocks
                : direction == -1
                    ? cache.BearOrderBlocks
                    : null;
        }

        private ZoneCandidateCache GetOrBuildZoneCandidateCache(
            Bars bars,
            int index,
            double atr)
        {
            if (bars == null ||
                index < 0 ||
                index >= bars.Count - 1 ||
                atr <= 0)
                return null;

            if (_zoneCacheBuildDepth > 0)
            {
                if (_activeZoneCandidateCache != null &&
                    ReferenceEquals(
                        _activeZoneCandidateCache.Bars,
                        bars) &&
                    _activeZoneCandidateCache.Index == index)
                    return _activeZoneCandidateCache;

                return null;
            }

            for (int i = 0;
                 i < _zoneCandidateCaches.Count;
                 i++)
            {
                ZoneCandidateCache cached =
                    _zoneCandidateCaches[i];

                if (cached != null &&
                    ReferenceEquals(
                        cached.Bars,
                        bars) &&
                    cached.Index == index)
                    return cached;
            }

            ZoneCandidateCache cache =
                new ZoneCandidateCache
                {
                    Bars = bars,
                    Index = index,
                    Atr = atr
                };

            _zoneCandidateCaches.Add(cache);

            while (_zoneCandidateCaches.Count > 32)
                _zoneCandidateCaches.RemoveAt(0);

            BuildFvgCandidates(cache);

            _activeZoneCandidateCache =
                cache;

            _zoneCacheBuildDepth++;

            try
            {
                BuildOrderBlockCandidates(cache);
            }
            finally
            {
                _zoneCacheBuildDepth--;

                _activeZoneCandidateCache =
                    null;
            }

            return cache;
        }

        private void BuildFvgCandidates(
            ZoneCandidateCache cache)
        {
            if (cache == null ||
                !UseFvg)
                return;

            Bars bars =
                cache.Bars;

            int first =
                System.Math.Max(
                    1,
                    cache.Index - FvgLookback);

            for (int i = cache.Index;
                 i >= first;
                 i--)
            {
                if (i < 2)
                    continue;

                double bullGap =
                    bars.LowPrices[i] -
                    bars.HighPrices[i - 2];

                if (bullGap >=
                    cache.Atr * MinimumFvgAtr)
                {
                    Zone bull =
                        BuildManagedFvgZone(
                            bars,
                            i,
                            cache.Index,
                            1,
                            bars.HighPrices[i - 2],
                            bars.LowPrices[i],
                            bullGap,
                            false,
                            cache.Atr);

                    if (bull != null &&
                        bull.Age <= MaximumZoneAgeBars)
                        cache.BullFvgs.Add(bull);
                }

                double bearGap =
                    bars.LowPrices[i - 2] -
                    bars.HighPrices[i];

                if (bearGap >=
                    cache.Atr * MinimumFvgAtr)
                {
                    Zone bear =
                        BuildManagedFvgZone(
                            bars,
                            i,
                            cache.Index,
                            -1,
                            bars.HighPrices[i],
                            bars.LowPrices[i - 2],
                            bearGap,
                            false,
                            cache.Atr);

                    if (bear != null &&
                        bear.Age <= MaximumZoneAgeBars)
                        cache.BearFvgs.Add(bear);
                }

                if (!UseTwoBarImbalanceFvg)
                    continue;

                double bullTwoBarGap =
                    bars.LowPrices[i] -
                    bars.HighPrices[i - 1];

                if (bullTwoBarGap >=
                    cache.Atr * MinimumFvgAtr)
                {
                    Zone bullTwoBar =
                        BuildManagedFvgZone(
                            bars,
                            i,
                            cache.Index,
                            1,
                            bars.HighPrices[i - 1],
                            bars.LowPrices[i],
                            bullTwoBarGap,
                            true,
                            cache.Atr);

                    if (bullTwoBar != null &&
                        bullTwoBar.Age <= MaximumZoneAgeBars)
                        cache.BullFvgs.Add(bullTwoBar);
                }

                double bearTwoBarGap =
                    bars.LowPrices[i - 1] -
                    bars.HighPrices[i];

                if (bearTwoBarGap >=
                    cache.Atr * MinimumFvgAtr)
                {
                    Zone bearTwoBar =
                        BuildManagedFvgZone(
                            bars,
                            i,
                            cache.Index,
                            -1,
                            bars.HighPrices[i],
                            bars.LowPrices[i - 1],
                            bearTwoBarGap,
                            true,
                            cache.Atr);

                    if (bearTwoBar != null &&
                        bearTwoBar.Age <= MaximumZoneAgeBars)
                        cache.BearFvgs.Add(bearTwoBar);
                }
            }
        }

        private void BuildOrderBlockCandidates(
            ZoneCandidateCache cache)
        {
            if (cache == null ||
                !UseOrderBlock ||
                cache.Index < 8)
                return;

            Bars bars =
                cache.Bars;

            int first =
                System.Math.Max(
                    2,
                    cache.Index - ObLookback);

            for (int i = cache.Index - 1;
                 i >= first;
                 i--)
            {
                if (bars.ClosePrices[i] <
                    bars.OpenPrices[i])
                {
                    Zone bull =
                        BuildOrderBlockCandidate(
                            bars,
                            i,
                            cache.Index,
                            1,
                            cache.Atr);

                    if (bull != null &&
                        bull.Quality >=
                        System.Math.Max(
                            50,
                            ObMinimumQuality))
                        cache.BullOrderBlocks.Add(bull);
                }

                if (bars.ClosePrices[i] >
                    bars.OpenPrices[i])
                {
                    Zone bear =
                        BuildOrderBlockCandidate(
                            bars,
                            i,
                            cache.Index,
                            -1,
                            cache.Atr);

                    if (bear != null &&
                        bear.Quality >=
                        System.Math.Max(
                            50,
                            ObMinimumQuality))
                        cache.BearOrderBlocks.Add(bear);
                }
            }
        }
    }
}
