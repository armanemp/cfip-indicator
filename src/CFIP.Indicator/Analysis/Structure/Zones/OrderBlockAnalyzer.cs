// ============================================================================
// CFIP Indicator — OrderBlockAnalyzer.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Zone FindNearestOrderBlock(
            Bars bars,
            int index,
            int direction,
            double atr,
            double selectionPrice = double.NaN,
            bool requireHistoricalRetest = true)
        {
            if (!UseOrderBlock ||
                bars == null ||
                index < 8 ||
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
                (requireHistoricalRetest ? "1" : "0");

            Zone[] candidates;

            if (!cacheable ||
                !TryGetCachedObCandidates(
                    cacheKey,
                    out candidates))
            {
                List<Zone> built =
                    new List<Zone>();

                int effectiveLookback =
                    Math.Min(
                        ObLookback,
                        Math.Max(
                            1,
                            MaximumZoneAgeBars));

                int first =
                    Math.Max(
                        2,
                        index -
                        effectiveLookback);

                for (int i = index - 1;
                     i >= first;
                     i--)
                {
                    Zone candidate =
                        BuildOrderBlockCandidate(
                            bars,
                            i,
                            index,
                            direction,
                            atr);

                    if (candidate == null ||
                        candidate.Quality <
                        Math.Max(
                            50,
                            ObMinimumQuality))
                        continue;

                    if (requireHistoricalRetest &&
                        RequireObRetest &&
                        !HasZoneRetest(
                            bars,
                            i,
                            index,
                            candidate.Low,
                            candidate.High))
                        continue;

                    built.Add(
                        candidate);
                }

                if (cacheable)
                {
                    StoreCachedObCandidates(
                        cacheKey,
                        built);
                }

                candidates =
                    built.ToArray();
            }

            double market =
                IsFinitePositive(selectionPrice)
                    ? selectionPrice
                    : bars.ClosePrices[index];

            if (!IsFinitePositive(market))
                return null;

            Zone best = null;
            double bestScore =
                double.MinValue;

            for (int i = 0;
                 i < candidates.Length;
                 i++)
            {
                Zone candidate =
                    candidates[i];

                if (candidate == null ||
                    candidate.OrderBlockLifecycle ==
                    OrderBlockLifecycleState.Broken ||
                    candidate.OrderBlockLifecycle == null)
                    continue;

                if (!OrderBlockRule.IsOnCorrectMarketSide(
                        direction,
                        market,
                        candidate.Low,
                        candidate.High,
                        Symbol.TickSize))
                    continue;

                if (candidate.Age >
                    MaximumZoneAgeBars)
                    continue;

                double distance =
                    DistanceToZone(
                        market,
                        candidate);

                double distancePenalty =
                    14.0 *
                    Math.Min(
                        1.0,
                        distance /
                        Math.Max(
                            Symbol.TickSize,
                            atr * 3.0));

                double agePenalty =
                    Math.Min(
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
                {
                    selectionScore += 2.0;
                }

                if (best == null ||
                    selectionScore >
                    bestScore)
                {
                    best =
                        candidate;
                    bestScore =
                        selectionScore;
                }
            }

            return best;
        }

    }
}
