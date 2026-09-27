// CFIP Indicator — HtfRewardSourcePolicy.cs
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
private bool IsHtfSourceForReward(
                                    List<Level> selected,
                                    double target)
                                {
                                    if (selected == null ||
                                        !IsFinitePositive(target))
                                        return false;
                        
                                    for (int i = 0;
                                         i < selected.Count;
                                         i++)
                                    {
                                        if (selected[i] == null)
                                            continue;
                        
                                        if (Math.Abs(
                                                selected[i].Price -
                                                target) <=
                                            Math.Max(
                                                Symbol.PipSize * 2,
                                                target * 1e-8) &&
                                            IsHtfTimeframe(
                                                selected[i].Timeframe))
                                            return true;
                                    }
                        
                                    return false;
                                }
    }
}
