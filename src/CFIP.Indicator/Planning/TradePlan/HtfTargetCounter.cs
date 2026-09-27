// CFIP Indicator — HtfTargetCounter.cs
// Single-responsibility planning module.

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
private int CountHtfTargetsInPlan(
                                    Plan plan)
                                {
                                    if (plan == null)
                                        return 0;
                        
                                    int count = 0;
                        
                                    if (IsHtfSource(plan.Tp1Source))
                                        count++;
                        
                                    if (IsHtfSource(plan.Tp2Source))
                                        count++;
                        
                                    if (IsHtfSource(plan.Tp3Source))
                                        count++;
                        
                                    if (IsHtfSource(plan.Tp4Source))
                                        count++;
                        
                                    return count;
                                }
    }
}
