// CFIP Indicator — PlanLabelAnchorCalculator.cs
Single-responsibility plan-label renderer.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int GetLabelAnchorBar(
                                    string name,
                                    int referenceBar)
                                {
                                    if (Bars == null ||
                                        Bars.Count < 2)
                                        return 0;
                        
                                    bool prediction =
                                        name != null &&
                                        name.IndexOf(
                                            "PRED_",
                                            StringComparison.OrdinalIgnoreCase) >= 0;
                        
                                    int left;
                        
                                    if (!prediction &&
                                        FullWidthLevelLines)
                                    {
                                        try
                                        {
                                            left =
                                                Chart.FirstVisibleBarIndex;
                                        }
                                        catch
                                        {
                                            left =
                                                referenceBar -
                                                Math.Max(
                                                    1,
                                                    LineLengthBars);
                                        }
                                    }
                                    else
                                    {
                                        left =
                                            referenceBar -
                                            Math.Max(
                                                1,
                                                LineLengthBars);
                                    }
                        
                                    left =
                                        Math.Max(
                                            0,
                                            Math.Min(
                                                Bars.Count - 1,
                                                left));
                        
                                    int offset =
                                        Math.Max(
                                            1,
                                            LabelLeftOffsetBars);
                        
                                    return
                                        Math.Max(
                                            0,
                                            Math.Min(
                                                Bars.Count - 1,
                                                left + offset));
                                }
    }
}
