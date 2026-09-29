using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void CollectPredictiveZoneCandidates(
            Bars bars,
            int index,
            int direction,
            double atr,
            double market,
            double minimumFutureDistance,
            double maximumFutureDistance,
            double timeframeWeight,
            string timeframe,
            List<PredictivePendingCandidate> candidates)
        {
            if (bars == null ||
                candidates == null ||
                index < 8 ||
                atr <= 0)
                return;

            int lookback =
                Math.Min(
                    Math.Max(
                        8,
                        timeframe == "M5"
                            ? FvgLookback
                            : ObLookback),
                    Math.Max(
                        8,
                        MaximumZoneAgeBars));

            int first =
                Math.Max(
                    2,
                    index - lookback);

            for (int i = index - 1;
                 i >= first;
                 i--)
            {
                if (UseFvg)
                {
                    if (direction == 1 &&
                        i >= 2)
                    {
                        double gap =
                            bars.LowPrices[i] -
                            bars.HighPrices[i - 2];

                        if (gap >=
                            atr * MinimumFvgAtr)
                        {
                            Zone zone =
                                BuildManagedFvgZone(
                                    bars,
                                    i,
                                    index,
                                    direction,
                                    bars.HighPrices[i - 2],
                                    bars.LowPrices[i],
                                    gap,
                                    false,
                                    atr);

                            AddPredictiveZoneCandidate(
                                candidates,
                                zone,
                                atr,
                                market,
                                direction,
                                minimumFutureDistance,
                                maximumFutureDistance,
                                timeframeWeight,
                                timeframe,
                                "FVG");
                        }
                    }
                    else if (direction == -1 &&
                             i >= 2)
                    {
                        double gap =
                            bars.LowPrices[i - 2] -
                            bars.HighPrices[i];

                        if (gap >=
                            atr * MinimumFvgAtr)
                        {
                            Zone zone =
                                BuildManagedFvgZone(
                                    bars,
                                    i,
                                    index,
                                    direction,
                                    bars.HighPrices[i],
                                    bars.LowPrices[i - 2],
                                    gap,
                                    false,
                                    atr);

                            AddPredictiveZoneCandidate(
                                candidates,
                                zone,
                                atr,
                                market,
                                direction,
                                minimumFutureDistance,
                                maximumFutureDistance,
                                timeframeWeight,
                                timeframe,
                                "FVG");
                        }
                    }

                    if (UseTwoBarImbalanceFvg)
                    {
                        if (direction == 1 &&
                            i >= 1)
                        {
                            double gap =
                                bars.LowPrices[i] -
                                bars.HighPrices[i - 1];

                            if (gap >=
                                atr * MinimumFvgAtr)
                            {
                                Zone zone =
                                    BuildManagedFvgZone(
                                        bars,
                                        i,
                                        index,
                                        direction,
                                        bars.HighPrices[i - 1],
                                        bars.LowPrices[i],
                                        gap,
                                        true,
                                        atr);

                                AddPredictiveZoneCandidate(
                                    candidates,
                                    zone,
                                    atr,
                                    market,
                                    direction,
                                    minimumFutureDistance,
                                    maximumFutureDistance,
                                    timeframeWeight,
                                    timeframe,
                                    "2BAR FVG");
                            }
                        }
                        else if (direction == -1 &&
                                 i >= 1)
                        {
                            double gap =
                                bars.LowPrices[i - 1] -
                                bars.HighPrices[i];

                            if (gap >=
                                atr * MinimumFvgAtr)
                            {
                                Zone zone =
                                    BuildManagedFvgZone(
                                        bars,
                                        i,
                                        index,
                                        direction,
                                        bars.HighPrices[i],
                                        bars.LowPrices[i - 1],
                                        gap,
                                        true,
                                        atr);

                                AddPredictiveZoneCandidate(
                                    candidates,
                                    zone,
                                    atr,
                                    market,
                                    direction,
                                    minimumFutureDistance,
                                    maximumFutureDistance,
                                    timeframeWeight,
                                    timeframe,
                                    "2BAR FVG");
                            }
                        }
                    }
                }

                if (UseOrderBlock)
                {
                    Zone ob =
                        BuildOrderBlockCandidate(
                            bars,
                            i,
                            index,
                            direction,
                            atr);

                    if (ob != null &&
                        (!RequireObRetest ||
                         HasZoneRetest(
                             bars,
                             i,
                             index,
                             ob.Low,
                             ob.High)))
                    {
                        AddPredictiveZoneCandidate(
                            candidates,
                            ob,
                            atr,
                            market,
                            direction,
                            minimumFutureDistance,
                            maximumFutureDistance,
                            timeframeWeight,
                            timeframe,
                            "ORDER BLOCK");
                    }
                }
            }
        }

        private void AddPredictiveZoneCandidate(
            List<PredictivePendingCandidate> candidates,
            Zone zone,
            double atr,
            double market,
            int direction,
            double minimumFutureDistance,
            double maximumFutureDistance,
            double timeframeWeight,
            string timeframe,
            string sourceType)
        {
            if (zone == null ||
                zone.Age >
                MaximumZoneAgeBars ||
                !IsFinitePositive(zone.Low) ||
                !IsFinitePositive(zone.High) ||
                zone.High <= zone.Low)
                return;

            double level =
                (zone.Low + zone.High) *
                0.50;

            double distance =
                Math.Abs(
                    level - market);

            bool future =
                direction == 1
                    ? zone.High <=
                      market - minimumFutureDistance
                    : zone.Low >=
                      market + minimumFutureDistance;

            if (!future ||
                distance >
                maximumFutureDistance)
                return;

            int contextQuality =
                PredictivePendingContextQuality(
                    direction,
                    timeframe);

            double distancePenalty =
                10.0 *
                Math.Min(
                    1.0,
                    distance /
                    Math.Max(
                        Symbol.TickSize,
                        maximumFutureDistance));

            double score =
                zone.Quality *
                0.65 *
                timeframeWeight +
                contextQuality -
                distancePenalty;

            candidates.Add(
                new PredictivePendingCandidate
                {
                    Price = NormalizePrice(level),
                    Score = Math.Max(0, score),
                    Quality = zone.Quality,
                    DistanceAtr =
                        distance /
                        Math.Max(
                            Symbol.TickSize,
                            atr),
                    Source =
                        timeframe +
                        " " +
                        sourceType +
                        (zone.Quality >= 88
                            ? " HIGH"
                            : ""),
                    ConfluenceCount = 1
                });
        }

    }
}
