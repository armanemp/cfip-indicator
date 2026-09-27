// ============================================================================
// CFIP Indicator — TargetClassifier.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

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
        private bool IsHtfTimeframe(
                                    string timeframe)
                                {
                                    if (string.IsNullOrWhiteSpace(
                                            timeframe))
                                        return false;
                        
                                    return
                                        timeframe == "M15" ||
                                        timeframe == "M30" ||
                                        timeframe == "H1" ||
                                        timeframe == "H4" ||
                                        timeframe == "D1" ||
                                        timeframe == "W1";
                                }
        
        private bool IsHtfSource(
                                    string source)
                                {
                                    if (string.IsNullOrWhiteSpace(
                                            source))
                                        return false;
                        
                                    return
                                        source.IndexOf(
                                            "@M15",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@M30",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@H1",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@H4",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@D1",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        source.IndexOf(
                                            "@W1",
                                            StringComparison.OrdinalIgnoreCase) >= 0;
                                }
        
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
