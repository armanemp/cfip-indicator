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
                        Zone zone =
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
                        Zone zone =
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
