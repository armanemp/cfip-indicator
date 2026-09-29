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

        private int GetLabelAnchorBar(
            string name,
            int referenceBar)
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            int left =
                GetCompactPlanLineLeftBar();

            int offset =
                1;

            return Math.Max(
                0,
                Math.Min(
                    Bars.Count - 1,
                    left + offset));
        }

        private int GetLabelBoxRightBar()
        {
            if (Bars == null ||
                Bars.Count < 2)
                return 0;

            int left =
                GetCompactPlanLineLeftBar();

            return Math.Max(
                left,
                Math.Min(
                    Bars.Count - 1,
                    left + CompactPlanLabelWidthBars));
        }
    }
}
