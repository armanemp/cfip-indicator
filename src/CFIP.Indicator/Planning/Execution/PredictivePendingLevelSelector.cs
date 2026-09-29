using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TrySelectPredictivePendingLevel(
            int closedM5,
            int direction,
            double atr,
            double market,
            out PredictivePendingCandidate selected)
        {
            selected = null;

            if (_m5Bars == null ||
                _m5Bars.Count < 20 ||
                atr <= 0 ||
                (direction != 1 && direction != -1) ||
                !IsFinitePositive(market))
                return false;

            double minimumFutureDistance =
                Math.Max(
                    Symbol.PipSize * 4,
                    atr * 0.18);

            double maximumFutureDistance =
                atr * 2.50;

            List<PredictivePendingCandidate> candidates =
                new List<PredictivePendingCandidate>();

            CollectPredictiveZoneCandidates(
                _m5Bars,
                closedM5,
                direction,
                atr,
                market,
                minimumFutureDistance,
                maximumFutureDistance,
                1.00,
                "M5",
                candidates);

            if (_m15Bars != null)
            {
                int m15Index =
                    ClosedIndex(
                        _m15Bars,
                        _m5Bars.OpenTimes[closedM5]);

                double m15Atr =
                    m15Index >= 20
                        ? Atr(_m15Bars, m15Index)
                        : 0;

                if (m15Index >= 20 &&
                    m15Atr > 0)
                {
                    CollectPredictiveZoneCandidates(
                        _m15Bars,
                        m15Index,
                        direction,
                        m15Atr,
                        market,
                        minimumFutureDistance,
                        maximumFutureDistance,
                        1.20,
                        "M15",
                        candidates);
                }
            }

            double structuralLevel =
                direction == 1
                    ? FindSwingLowBelow(
                        _m5Bars,
                        closedM5,
                        market)
                    : FindSwingHighAbove(
                        _m5Bars,
                        closedM5,
                        market);

            AddPredictivePointCandidate(
                candidates,
                structuralLevel,
                70,
                direction == 1
                    ? "M5 SWING LOW"
                    : "M5 SWING HIGH",
                direction,
                market,
                atr,
                minimumFutureDistance,
                maximumFutureDistance);

            double equalLevel =
                direction == 1
                    ? FindEqualLow(
                        _m5Bars,
                        closedM5,
                        market,
                        atr)
                    : FindEqualHigh(
                        _m5Bars,
                        closedM5,
                        market,
                        atr);

            AddPredictivePointCandidate(
                candidates,
                equalLevel,
                82,
                direction == 1
                    ? "M5 EQUAL LOW / LIQUIDITY"
                    : "M5 EQUAL HIGH / LIQUIDITY",
                direction,
                market,
                atr,
                minimumFutureDistance,
                maximumFutureDistance);

            if (candidates.Count == 0)
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • NO FUTURE STRUCTURAL LEVEL";
                return false;
            }

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                PredictivePendingCandidate candidate =
                    candidates[i];

                int confluence = 1;
                List<string> sources =
                    new List<string>
                    {
                        candidate.Source
                    };

                for (int j = 0;
                     j < candidates.Count;
                     j++)
                {
                    if (i == j)
                        continue;

                    PredictivePendingCandidate other =
                        candidates[j];

                    if (Math.Abs(
                            other.Price -
                            candidate.Price) >
                        atr * 0.12)
                        continue;

                    confluence++;
                    if (sources.Count < 3 &&
                        other.Source != candidate.Source &&
                        !sources.Contains(other.Source))
                    {
                        sources.Add(other.Source);
                    }
                }

                candidate.ConfluenceCount =
                    confluence;

                candidate.Score =
                    Math.Min(
                        100,
                        candidate.Score +
                        Math.Min(
                            16,
                            Math.Max(
                                0,
                                confluence - 1) * 7));

                candidate.Source =
                    string.Join(
                        "+",
                        sources.ToArray());

                candidate.Quality =
                    ClampInt(
                        (int)Math.Round(
                            candidate.Score),
                        0,
                        100);
            }

            candidates =
                candidates
                    .OrderByDescending(c => c.Score)
                    .ThenBy(c => c.DistanceAtr)
                    .ToList();

            selected =
                candidates.FirstOrDefault(
                    c =>
                        c.Score >=
                        Math.Max(
                            65,
                            PendingMinimumSmartQuality));

            return selected != null;
        }

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

        private void AddPredictivePointCandidate(
            List<PredictivePendingCandidate> candidates,
            double level,
            int baseQuality,
            string source,
            int direction,
            double market,
            double atr,
            double minimumFutureDistance,
            double maximumFutureDistance)
        {
            if (!IsFinitePositive(level) ||
                atr <= 0)
                return;

            double distance =
                Math.Abs(
                    level - market);

            bool future =
                direction == 1
                    ? level <=
                      market - minimumFutureDistance
                    : level >=
                      market + minimumFutureDistance;

            if (!future ||
                distance >
                maximumFutureDistance)
                return;

            int contextQuality =
                PredictivePendingContextQuality(
                    direction,
                    "M5");

            double score =
                baseQuality *
                0.65 +
                contextQuality -
                10.0 *
                Math.Min(
                    1.0,
                    distance /
                    Math.Max(
                        Symbol.TickSize,
                        maximumFutureDistance));

            candidates.Add(
                new PredictivePendingCandidate
                {
                    Price = NormalizePrice(level),
                    Score = Math.Max(0, score),
                    Quality = baseQuality,
                    DistanceAtr =
                        distance /
                        Math.Max(
                            Symbol.TickSize,
                            atr),
                    Source = source,
                    ConfluenceCount = 1
                });
        }

        private int PredictivePendingContextQuality(
            int direction,
            string timeframe)
        {
            int score = 0;

            Frame frame =
                timeframe == "M15"
                    ? _m15Frame
                    : _m5Frame;

            if (frame != null)
            {
                if (frame.Direction == direction)
                    score += 6;

                if (frame.StructureBull && direction == 1 ||
                    frame.StructureBear && direction == -1)
                    score += 5;

                if (direction == 1)
                {
                    if (frame.LiquidityBull) score += 5;
                    if (frame.FvgBull) score += 4;
                    if (frame.ObBull) score += 4;
                    if (frame.FvgObBullConfluence) score += 5;
                    if (frame.VolumeBull) score += 2;
                    if (frame.MacdBull) score += 2;
                    if (frame.VwapBull) score += 2;
                    if (frame.VolatilityBull) score += 1;
                    if (frame.EqualLow) score += 3;
                    if (frame.MssBull || frame.ChochBull) score += 3;
                }
                else
                {
                    if (frame.LiquidityBear) score += 5;
                    if (frame.FvgBear) score += 4;
                    if (frame.ObBear) score += 4;
                    if (frame.FvgObBearConfluence) score += 5;
                    if (frame.VolumeBear) score += 2;
                    if (frame.MacdBear) score += 2;
                    if (frame.VwapBear) score += 2;
                    if (frame.VolatilityBear) score += 1;
                    if (frame.EqualHigh) score += 3;
                    if (frame.MssBear || frame.ChochBear) score += 3;
                }

                if (frame.OssBull && direction == 1)
                    score += Math.Min(
                        5,
                        frame.OssBullVotes);

                if (frame.OssBear && direction == -1)
                    score += Math.Min(
                        5,
                        frame.OssBearVotes);
            }

            if (_m15Frame != null &&
                _m15Frame.Direction == direction &&
                timeframe != "M15")
                score += 5;

            if (_m30Frame != null &&
                _m30Frame.Direction == direction)
                score += 3;

            if (_decision != null &&
                _decision.Direction == direction)
                score += Math.Min(
                    8,
                    (int)Math.Round(
                        _decision.Confidence * 0.08));

            if (_reaction != null &&
                _reaction.Direction == direction)
                score += Math.Min(
                    8,
                    (int)Math.Round(
                        _reaction.Confidence * 0.08));

            return Math.Min(
                35,
                score);
        }
    }
}
