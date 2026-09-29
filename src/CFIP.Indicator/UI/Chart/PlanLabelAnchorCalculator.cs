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
        private const int CompactPlanLabelWidthBars = 7;

        private int GetCompactPlanLineLeftBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            return Math.Max(
                0,
                Bars.Count -
                1 -
                CompactPlanLineLengthBars);
        }

        private int GetCompactPlanLabelAnchorBar(
            int lineLeft)
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            int offset =
                Math.Max(
                    1,
                    LabelLeftOffsetBars);

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

            int configuredWidth =
                Math.Max(
                    CompactPlanLabelWidthBars,
                    Math.Max(
                        1,
                        LabelLeftOffsetBars) + 4);

            return Math.Max(
                lineLeft,
                Math.Min(
                    Bars.Count - 1,
                    lineLeft +
                    configuredWidth));
        }
    }
}
