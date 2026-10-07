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
                Math.Max(
                    0,
                    Math.Min(
                        Bars.Count - 1,
                        MapM5ToChart(
                            snapshot.StartIndex,
                            Bars.Count - 1)));

            int end =
                Math.Max(
                    start + 1,
                    Math.Min(
                        Bars.Count - 1,
                        MapM5ToChart(
                            closedM5,
                            Bars.Count - 1)));

            Color baseColor =
                snapshot.Direction > 0
                    ? Color.Lime
                    : snapshot.Direction < 0
                        ? Color.Red
                        : Color.Gold;

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

            if (snapshot.SweepIndex >= 0)
            {
                int chartIndex =
                    MapM5ToChart(
                        snapshot.SweepIndex,
                        Bars.Count - 1);

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

                Chart.DrawIcon(
                    RangeVisualPrefix + "SWEEP",
                    snapshot.Direction > 0
                        ? ChartIconType.UpArrow
                        : ChartIconType.DownArrow,
                    chartIndex,
                    price,
                    baseColor);
            }

            if (snapshot.IsConfirmedBreakout)
            {
                int chartIndex =
                    MapM5ToChart(
                        snapshot.BreakoutIndex,
                        Bars.Count - 1);

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
                snapshot.WatchDirection != 0)
            {
                int chartIndex =
                    Math.Max(
                        0,
                        Math.Min(
                            Bars.Count - 1,
                            MapM5ToChart(
                                closedM5,
                                Bars.Count - 1)));

                double watchPrice =
                    snapshot.WatchDirection > 0
                        ? snapshot.Low
                        : snapshot.High;

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