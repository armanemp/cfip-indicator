using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // One candle of actual chart-space clearance is measured from the
        // canonical signal-line left endpoint. The distance is derived from
        // cTrader's real X projection, not from a guessed pixel or price offset.
        private const int CompactPlanLabelGapBars = 1;

        private DateTime GetCompactPlanLabelAnchorTime()
        {
            return GetCompactPlanLabelAnchorTime(
                GetPlanLineLeftBar());
        }

        private DateTime GetCompactPlanLabelAnchorTime(
            int lineLeftBar)
        {
            if (Bars == null ||
                Bars.Count < 2)
                return DateTime.MinValue;

            int canonicalLineLeftBar =
                Math.Max(
                    1,
                    Math.Min(
                        Bars.Count - 1,
                        lineLeftBar));

            double lineX =
                Chart.BarIndexToX(
                    canonicalLineLeftBar);

            double previousBarX =
                Chart.BarIndexToX(
                    canonicalLineLeftBar - 1);

            double barWidth =
                Math.Abs(
                    lineX -
                    previousBarX);

            if (double.IsNaN(lineX) ||
                double.IsInfinity(lineX) ||
                double.IsNaN(barWidth) ||
                double.IsInfinity(barWidth) ||
                barWidth <= 0)
            {
                return Bars.OpenTimes[
                    Math.Max(
                        0,
                        canonicalLineLeftBar -
                        CompactPlanLabelGapBars)];
            }

            double targetX =
                lineX -
                (barWidth *
                 CompactPlanLabelGapBars);

            return Chart.XToTime(
                targetX);
        }
    }
}
