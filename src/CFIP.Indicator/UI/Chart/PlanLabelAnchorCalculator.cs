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

            int lineRight =
                GetPlanLineRightBar();

            int offset =
                Math.Max(
                    1,
                    CompactPlanLabelMinimumGapBars);

            // Labels are anchored immediately after the reserved line endpoint.
            // The label renderer uses left alignment so its text grows away from
            // the line rather than back across it.
            return Math.Max(
                0,
                Math.Min(
                    Bars.Count - 1,
                    lineRight + offset));
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
