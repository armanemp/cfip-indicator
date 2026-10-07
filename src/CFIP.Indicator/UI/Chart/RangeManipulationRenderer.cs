using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const string RangeVisualPrefix = "CFIP_RANGE_INTEL_";

        private void RenderRangeManipulation(
            RangeManipulationSnapshot snapshot,
            int closedM5)
        {
            RemoveRangeManipulationObjects();

            if (snapshot == null ||
                !snapshot.IsRange ||
                snapshot.High <= snapshot.Low ||
                _m5Bars == null)
                return;

            int start =
                ResolveRangeChartIndex(
                    snapshot.StartIndex,
                    0);

            int end =
                ResolveRangeChartIndex(
                    closedM5,
                    start + 1);

            if (start < 0 ||
                end < 0)
            {
                Print(
                    "CFIP range render skipped | unable to map M5 range to chart bars | start={0} end={1} chartBars={2} m5Bars={3}",
                    snapshot.StartIndex,
                    closedM5,
                    Bars.Count,
                    _m5Bars.Count);
                return;
            }

            end =
                Math.Max(
                    start + 1,
                    Math.Min(
                        Bars.Count - 1,
                        end));

            // Direction is semantic here: bullish=green, bearish=red, neutral=gray.
            // It is not a trend-strength scale. Strength remains owned by the
            // snapshot score/watch score and must not be encoded as a third hue.
            Color baseColor =
                snapshot.Direction > 0
                    ? Color.Green
                    : snapshot.Direction < 0
                        ? Color.Red
                        : Color.Gray;

            ChartRectangle zone =
                Chart.DrawRectangle(
                    RangeVisualPrefix + "ZONE",
                    start,
                    snapshot.High,
                    end,
                    snapshot.Low,
                    baseColor,
                    1,
                    LineStyle.Solid);

            zone.IsFilled = true;
            zone.Color = Color.FromArgb(
                snapshot.IsManipulation ? 28 : 18,
                baseColor);
            zone.IsInteractive = false;

            ChartTrendLine high =
                Chart.DrawTrendLine(
                    RangeVisualPrefix + "HIGH",
                    start,
                    snapshot.High,
                    Bars.Count - 1,
                    snapshot.High,
                    baseColor,
                    1,
                    LineStyle.Solid);

            ChartTrendLine low =
                Chart.DrawTrendLine(
                    RangeVisualPrefix + "LOW",
                    start,
                    snapshot.Low,
                    Bars.Count - 1,
                    snapshot.Low,
                    baseColor,
                    1,
                    LineStyle.Solid);

            high.ExtendToInfinity = false;
            low.ExtendToInfinity = false;
            high.IsInteractive = false;
            low.IsInteractive = false;

            int renderedEventIndex = -1;

            if (snapshot.SweepIndex >= 0)
            {
                int chartIndex =
                    ResolveRangeChartIndex(
                        snapshot.SweepIndex,
                        start);

                double price =
                    snapshot.Direction > 0
                        ? Bars.LowPrices[
                            Math.Max(
                                0,
                                Math.Min(
                                    Bars.Count - 1,
                                    chartIndex))]
                        : Bars.HighPrices[
                            Math.Max(
                                0,
                                Math.Min(
                                    Bars.Count - 1,
                                    chartIndex))];

                renderedEventIndex = chartIndex;

                Chart.DrawIcon(
                    RangeVisualPrefix + "SWEEP",
                    snapshot.Direction > 0
                        ? ChartIconType.UpArrow
                        : ChartIconType.DownArrow,
                    chartIndex,
                    price,
                    baseColor);
            }

            if (snapshot.IsConfirmedBreakout && renderedEventIndex < 0)
            {
                int chartIndex =
                    ResolveRangeChartIndex(
                        snapshot.BreakoutIndex,
                        start);

                renderedEventIndex = chartIndex;

                Chart.DrawIcon(
                    RangeVisualPrefix + "BREAKOUT",
                    snapshot.Direction > 0
                        ? ChartIconType.UpArrow
                        : ChartIconType.DownArrow,
                    chartIndex,
                    snapshot.Direction > 0
                        ? Bars.LowPrices[chartIndex]
                        : Bars.HighPrices[chartIndex],
                    baseColor);
            }

            if (snapshot.IsManipulationWatch &&
                snapshot.WatchDirection != 0 &&
                renderedEventIndex < 0)
            {
                int chartIndex =
                    ResolveRangeChartIndex(
                        closedM5,
                        start);

                double watchPrice =
                    snapshot.WatchDirection > 0
                        ? snapshot.Low
                        : snapshot.High;

                renderedEventIndex = chartIndex;

                Chart.DrawIcon(
                    RangeVisualPrefix + "WATCH",
                    snapshot.WatchDirection > 0
                        ? ChartIconType.UpArrow
                        : ChartIconType.DownArrow,
                    chartIndex,
                    watchPrice,
                    baseColor);
            }

            DateTime labelTime =
                    Bars.OpenTimes[Math.Max(0, start)];

                Chart.DrawText(
                    RangeVisualPrefix + "LABEL",
                    snapshot.IsManipulationWatch
                        ? "MANIPULATION WATCH " +
                          (snapshot.WatchDirection > 0
                              ? "BUY"
                              : "SELL") +
                          " " +
                          snapshot.WatchScore
                        : "RANGE " +
                          snapshot.State +
                          " " +
                          snapshot.Score,
                    labelTime,
                    snapshot.Direction > 0
                        ? snapshot.Low
                        : snapshot.High,
                    baseColor);
        }


        private int ResolveRangeChartIndex(
            int m5Index,
            int fallbackIndex)
        {
            if (_m5Bars == null ||
                Bars == null ||
                m5Index < 0 ||
                m5Index >= _m5Bars.Count ||
                Bars.Count < 1)
                return Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        fallbackIndex));

            DateTime target =
                _m5Bars.OpenTimes[m5Index];

            int lo = 0;
            int hi = Bars.Count - 1;
            int best = -1;

            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) / 2);
                DateTime t = Bars.OpenTimes[mid];

                if (t == target)
                    return mid;

                if (t < target)
                {
                    best = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return best >= 0
                ? best
                : Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        fallbackIndex));
        }

        private void RemoveRangeManipulationObjects()
        {
            Chart.RemoveObject(RangeVisualPrefix + "ZONE");
            Chart.RemoveObject(RangeVisualPrefix + "HIGH");
            Chart.RemoveObject(RangeVisualPrefix + "LOW");
            Chart.RemoveObject(RangeVisualPrefix + "SWEEP");
            Chart.RemoveObject(RangeVisualPrefix + "BREAKOUT");
            Chart.RemoveObject(RangeVisualPrefix + "WATCH");
            Chart.RemoveObject(RangeVisualPrefix + "LABEL");
        }
    }
}