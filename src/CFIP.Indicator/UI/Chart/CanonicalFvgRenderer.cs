using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const string FvgVisualPrefix = "CFIP_FVG_";

        private void RenderCanonicalFvgZones(int closedM5)
        {
            RemoveCanonicalFvgObjects();

            if (_m15Bars == null || closedM5 < 10 || Bars == null || Bars.Count < 2)
                return;

            int m15Index = ClosedIndex(
                _m15Bars,
                ClosedBarBoundaryReference(_m5Bars, closedM5, Server.TimeInUtc));

            if (m15Index < 10)
                return;

            double market = Symbol.Bid > 0 ? Symbol.Bid : Bars.ClosePrices[Bars.Count - 1];
            double m15Atr = Atr(_m15Bars, m15Index);

            if (m15Atr <= 0)
                return;

            RenderNearestFvg(
                _m15Bars,
                m15Index,
                1,
                m15Atr,
                market,
                "M15_BULL",
                Color.Green);

            RenderNearestFvg(
                _m15Bars,
                m15Index,
                -1,
                m15Atr,
                market,
                "M15_BEAR",
                Color.Red);

            if (_h1Bars != null)
            {
                int h1Index = ClosedIndex(
                    _h1Bars,
                    ClosedBarBoundaryReference(_m5Bars, closedM5, Server.TimeInUtc));

                if (h1Index >= 10)
                {
                    double h1Atr = Atr(_h1Bars, h1Index);

                    if (h1Atr > 0)
                    {
                        RenderNearestFvg(
                            _h1Bars,
                            h1Index,
                            1,
                            h1Atr,
                            market,
                            "H1_BULL",
                            Color.DarkGreen);

                        RenderNearestFvg(
                            _h1Bars,
                            h1Index,
                            -1,
                            h1Atr,
                            market,
                            "H1_BEAR",
                            Color.DarkRed);
                    }
                }
            }
        }

        private void RenderNearestFvg(
            Bars sourceBars,
            int sourceIndex,
            int direction,
            double atr,
            double market,
            string key,
            Color color)
        {
            Zone zone = FindNearestFvgForExecution(
                sourceBars,
                sourceIndex,
                direction,
                atr,
                market);

            if (zone == null ||
                zone.CreatedIndex < 0 ||
                zone.Low >= zone.High)
                return;

            int start = ResolveFvgChartIndex(
                sourceBars.OpenTimes[zone.CreatedIndex],
                0);

            int end = Math.Max(
                start + 1,
                Math.Min(
                    Bars.Count - 1,
                    ResolveFvgChartIndex(
                        sourceBars.OpenTimes[sourceIndex],
                        start + 1)));

            if (start < 0 || end < 0 || start >= Bars.Count)
                return;

            ChartRectangle rectangle = Chart.DrawRectangle(
                FvgVisualPrefix + key,
                start,
                zone.High,
                end,
                zone.Low,
                color,
                1,
                LineStyle.Solid);

            rectangle.IsFilled = true;
            rectangle.Color = Color.FromArgb(
                16,
                color);
            rectangle.IsInteractive = false;

            Chart.DrawText(
                FvgVisualPrefix + key + "_LABEL",
                key.Replace("_", " ") + " FVG " + zone.Quality,
                Bars.OpenTimes[start],
                direction > 0 ? zone.Low : zone.High,
                color);
        }

        private int ResolveFvgChartIndex(DateTime target, int fallback)
        {
            if (Bars == null || Bars.Count == 0)
                return -1;

            int lo = 0;
            int hi = Bars.Count - 1;
            int best = -1;

            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) / 2);
                DateTime time = Bars.OpenTimes[mid];

                if (time == target)
                    return mid;

                if (time < target)
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
                        fallback));
        }

        private void RemoveCanonicalFvgObjects()
        {
            Chart.RemoveObject(FvgVisualPrefix + "M15_BULL");
            Chart.RemoveObject(FvgVisualPrefix + "M15_BULL_LABEL");
            Chart.RemoveObject(FvgVisualPrefix + "M15_BEAR");
            Chart.RemoveObject(FvgVisualPrefix + "M15_BEAR_LABEL");
            Chart.RemoveObject(FvgVisualPrefix + "H1_BULL");
            Chart.RemoveObject(FvgVisualPrefix + "H1_BULL_LABEL");
            Chart.RemoveObject(FvgVisualPrefix + "H1_BEAR");
            Chart.RemoveObject(FvgVisualPrefix + "H1_BEAR_LABEL");
        }
    }
}