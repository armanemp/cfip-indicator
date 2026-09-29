using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLabelAnchorCalculator.cs
// Single-responsibility plan-label renderer.

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int CompactPlanLabelWidthBars = 8;
        private const int CompactPlanLabelMinimumGapBars = 3;

        private int GetCompactPlanLabelAnchorBar(
            int lineLeft)
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            int offset =
                Math.Max(
                    CompactPlanLabelMinimumGapBars,
                    Math.Min(
                        6,
                        LabelLeftOffsetBars));

            // Compact labels stay attached to the line geometry. Their visible
            // separation from the level is handled in price space by the label
            // renderer; the line endpoint itself remains unchanged.
            return Math.Max(
                0,
                Math.Min(
                    Bars.Count - 1,
                    lineLeft + offset));
        }

        private int GetLabelBoxRightBar(
            int lineLeft)
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            int labelBar =
                GetCompactPlanLabelAnchorBar(
                    lineLeft);

            return Math.Max(
                labelBar,
                Math.Min(
                    Bars.Count - 1,
                    labelBar +
                    CompactPlanLabelWidthBars));
        }
    }
}
