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

            AddHigherTimeframeStructureCandidates(
                candidates,
                market,
                direction,
                m15Index,
                m15Atr,
                "M15",
                _m15Frame == null ? 0 : _m15Frame.Quality);

            int h1Index =
                _h1Bars == null
                    ? -1
                    : ClosedIndex(
                        _h1Bars,
                        _m5Bars.OpenTimes[closedM5]);

            double h1Atr =
                h1Index >= 10
                    ? Atr(_h1Bars, h1Index)
                    : 0;

            AddHigherTimeframeStructureCandidates(
                candidates,
                market,
                direction,
                h1Index,
                h1Atr,
                "H1",
                _h1Frame == null ? 0 : _h1Frame.Quality);

            ApplyRewardPathPreferences(
                candidates,
                closedM5,
                direction,
                atr);

            ExecutionZoneSelectionCandidate best = null;

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                ExecutionZoneSelectionCandidate candidate =
                    candidates[i];

                if (candidate == null ||
                    candidate.RewardPathRR <= 0)
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
















    }
}
