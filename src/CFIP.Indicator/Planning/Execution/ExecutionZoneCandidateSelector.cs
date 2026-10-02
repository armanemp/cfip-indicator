using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TrySelectExecutionZoneCandidate(
            int closedM5,
            int direction,
            double atr,
            double market,
            out double low,
            out double high,
            out string source,
            out int quality)
        {
            low = 0;
            high = 0;
            source = "NONE";
            quality = 0;

            Zone m5Fvg =
                FindNearestFvgForExecution(
                    _m5Bars,
                    closedM5,
                    direction,
                    atr,
                    market);

            Zone m5Ob =
                FindNearestOrderBlockForExecution(
                    _m5Bars,
                    closedM5,
                    direction,
                    atr,
                    market);

            Zone m15Fvg = null;
            Zone m15Ob = null;

            int m15Index =
                ClosedIndex(
                    _m15Bars,
                    _m5Bars.OpenTimes[closedM5]);

            double m15Atr =
                m15Index >= 10
                    ? Atr(
                        _m15Bars,
                        m15Index)
                    : 0;

            if (m15Index >= 10 &&
                m15Atr > 0)
            {
                m15Fvg =
                    FindNearestFvgForExecution(
                        _m15Bars,
                        m15Index,
                        direction,
                        m15Atr,
                        market);

                m15Ob =
                    FindNearestOrderBlockForExecution(
                        _m15Bars,
                        m15Index,
                        direction,
                        m15Atr,
                        market);
            }

            var candidates =
                new List<ExecutionZoneSelectionCandidate>();

            AddZoneCandidate(
                candidates,
                market,
                atr,
                m5Fvg,
                "M5 FVG",
                false,
                true,
                false,
                false);

            if (m5Ob != null &&
                m5Ob.High > m5Ob.Low)
            {
                AddZoneCandidate(
                    candidates,
                    market,
                    atr,
                    m5Ob,
                    "M5 ORDER_BLOCK",
                    false,
                    false,
                    true,
                    false);
            }

            AddZoneCandidate(
                candidates,
                market,
                Math.Max(
                    Symbol.TickSize,
                    m15Atr),
                m15Fvg,
                "M15 FVG",
                true,
                true,
                false,
                false);

            AddZoneCandidate(
                candidates,
                market,
                Math.Max(
                    Symbol.TickSize,
                    m15Atr),
                m15Ob,
                "M15 ORDER_BLOCK",
                true,
                false,
                true,
                false);

            AddOverlapCandidate(
                candidates,
                market,
                atr,
                m5Fvg,
                m5Ob,
                "M5 FVG+OB",
                false);

            AddOverlapCandidate(
                candidates,
                market,
                Math.Max(
                    Symbol.TickSize,
                    m15Atr),
                m15Fvg,
                m15Ob,
                "M15 FVG+OB",
                true);

            AddMtfOverlapCandidate(
                candidates,
                market,
                atr,
                m5Fvg,
                m5Ob,
                m15Fvg,
                m15Ob);

            ExecutionZoneSelectionCandidate best = null;

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                ExecutionZoneSelectionCandidate candidate =
                    candidates[i];

                if (candidate == null)
                    continue;

                if (best == null ||
                    candidate.Score >
                    best.Score + 0.0001 ||
                    (Math.Abs(
                        candidate.Score -
                        best.Score) <= 0.0001 &&
                     candidate.Quality >
                        best.Quality))
                    best = candidate;
            }

            if (best == null)
            {
                double swing =
                    direction == 1
                        ? FindSwingLowBelow(
                            _m5Bars,
                            closedM5,
                            market)
                        : FindSwingHighAbove(
                            _m5Bars,
                            closedM5,
                            market);

                if (IsFinitePositive(swing))
                {
                    if (direction == 1)
                    {
                        low = swing;
                        high =
                            swing +
                            atr *
                            Math.Max(
                                0.10,
                                ExecutionZoneAtr);
                    }
                    else
                    {
                        low =
                            swing -
                            atr *
                            Math.Max(
                                0.10,
                                ExecutionZoneAtr);
                        high = swing;
                    }

                    source = "M5 SWING";
                    quality = 70;
                }

                return
                    IsFinitePositive(low) &&
                    IsFinitePositive(high) &&
                    high > low;
            }

            low = best.Low;
            high = best.High;
            source = best.Source;
            quality = best.Quality;

            return
                IsFinitePositive(low) &&
                IsFinitePositive(high) &&
                high > low;
        }

        private void AddZoneCandidate(
            List<ExecutionZoneSelectionCandidate> candidates,
            double market,
            double atr,
            Zone zone,
            string source,
            bool primary,
            bool isFvg,
            bool isOrderBlock,
            bool confluence)
        {
            if (zone == null ||
                candidates == null ||
                !IsFinitePositive(atr) ||
                zone.High <= zone.Low ||
                zone.Age > MaximumZoneAgeBars)
                return;

            double distance =
                DistanceToZone(
                    market,
                    zone);

            ExecutionZoneSelectionResult score =
                ExecutionZoneSelectionRule.Evaluate(
                    new ExecutionZoneSelectionInput(
                        zone.Quality,
                        distance /
                        Math.Max(
                            Symbol.TickSize,
                            atr),
                        zone.Age,
                        primary,
                        isFvg,
                        isOrderBlock,
                        confluence,
                        false),
                    Math.Max(
                        1,
                        MaximumZoneAgeBars),
                    3.0);

            if (!score.Valid)
                return;

            candidates.Add(
                new ExecutionZoneSelectionCandidate
                {
                    Low = zone.Low,
                    High = zone.High,
                    Source = source,
                    Quality = zone.Quality,
                    Score = score.Score,
                    Primary = primary,
                    Fvg = isFvg,
                    OrderBlock = isOrderBlock,
                    Confluence = confluence
                });
        }

        private void AddOverlapCandidate(
            List<ExecutionZoneSelectionCandidate> candidates,
            double market,
            double atr,
            Zone fvg,
            Zone ob,
            string source,
            bool primary)
        {
            if (fvg == null ||
                ob == null ||
                atr <= 0 ||
                !ZoneConfluenceRule.HasOverlap(
                    fvg.Low,
                    fvg.High,
                    ob.Low,
                    ob.High,
                    0))
                return;

            double low =
                Math.Max(
                    fvg.Low,
                    ob.Low);

            double high =
                Math.Min(
                    fvg.High,
                    ob.High);

            if (high <= low)
                return;

            int quality =
                (int)Math.Round(
                    Math.Min(
                        100,
                        Math.Max(
                            fvg.Quality,
                            ob.Quality) +
                        0.60 *
                        Math.Min(
                            fvg.Quality,
                            ob.Quality)));

            AddSyntheticCandidate(
                candidates,
                market,
                atr,
                low,
                high,
                source,
                primary,
                quality,
                true,
                fvg,
                ob);
        }

        private void AddMtfOverlapCandidate(
            List<ExecutionZoneSelectionCandidate> candidates,
            double market,
            double atr,
            Zone m5Fvg,
            Zone m5Ob,
            Zone m15Fvg,
            Zone m15Ob)
        {
            Zone[] m5 =
            {
                m5Fvg,
                m5Ob
            };

            Zone[] m15 =
            {
                m15Fvg,
                m15Ob
            };

            string[] m5Names =
            {
                "FVG",
                "OB"
            };

            string[] m15Names =
            {
                "FVG",
                "OB"
            };

            for (int i = 0; i < m5.Length; i++)
            {
                for (int j = 0; j < m15.Length; j++)
                {
                    Zone first = m5[i];
                    Zone second = m15[j];

                    if (first == null ||
                        second == null ||
                        !ZoneConfluenceRule.HasOverlap(
                            first.Low,
                            first.High,
                            second.Low,
                            second.High,
                            0))
                        continue;

                    double low =
                        Math.Max(
                            first.Low,
                            second.Low);

                    double high =
                        Math.Min(
                            first.High,
                            second.High);

                    if (high <= low)
                        continue;

                    int quality =
                        (int)Math.Round(
                            Math.Min(
                                100,
                                Math.Max(
                                    first.Quality,
                                    second.Quality) +
                                0.45 *
                                Math.Min(
                                    first.Quality,
                                    second.Quality)));

                    AddSyntheticCandidate(
                        candidates,
                        market,
                        atr,
                        low,
                        high,
                        "M5 " +
                        m5Names[i] +
                        "+M15 " +
                        m15Names[j],
                        false,
                        quality,
                        false,
                        first,
                        second);
                }
            }
        }

        private void AddSyntheticCandidate(
            List<ExecutionZoneSelectionCandidate> candidates,
            double market,
            double atr,
            double low,
            double high,
            string source,
            bool primary,
            int quality,
            bool obFvgConfluence,
            Zone first,
            Zone second)
        {
            if (candidates == null ||
                atr <= 0 ||
                high <= low ||
                !IsFinitePositive(low) ||
                !IsFinitePositive(high))
                return;

            double distance =
                market < low
                    ? low - market
                    : market > high
                        ? market - high
                        : 0;

            int age =
                Math.Max(
                    first == null ? 0 : first.Age,
                    second == null ? 0 : second.Age);

            ExecutionZoneSelectionResult score =
                ExecutionZoneSelectionRule.Evaluate(
                    new ExecutionZoneSelectionInput(
                        quality,
                        distance /
                        Math.Max(
                            Symbol.TickSize,
                            atr),
                        age,
                        primary,
                        first != null &&
                        string.Equals(
                            first.Kind,
                            "FVG",
                            StringComparison.OrdinalIgnoreCase),
                        first != null &&
                        string.Equals(
                            first.Kind,
                            "ORDER_BLOCK",
                            StringComparison.OrdinalIgnoreCase) ||
                        second != null &&
                        string.Equals(
                            second.Kind,
                            "ORDER_BLOCK",
                            StringComparison.OrdinalIgnoreCase),
                        obFvgConfluence,
                        !primary),
                    Math.Max(
                        1,
                        MaximumZoneAgeBars),
                    3.0);

            if (!score.Valid)
                return;

            candidates.Add(
                new ExecutionZoneSelectionCandidate
                {
                    Low = low,
                    High = high,
                    Source = source,
                    Quality = Math.Max(
                        0,
                        Math.Min(
                            100,
                            quality)),
                    Score = score.Score,
                    Primary = primary,
                    Fvg = first != null,
                    OrderBlock = second != null,
                    Confluence = obFvgConfluence
                });
        }
    }
}
