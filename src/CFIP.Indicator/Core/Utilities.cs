using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // ============================================================
        
                        private int ClosedIndex(
                    Bars bars,
                    DateTime reference)
                {
                    if (bars == null ||
                        bars.Count < 2 ||
                        reference < bars.OpenTimes[0])
                        return -1;
        
                    int probe =
                        bars.OpenTimes.GetIndexByTime(
                            reference);
        
                    if (probe < 0)
                        probe =
                            bars.Count - 1;
        
                    probe =
                        Math.Max(
                            0,
                            Math.Min(
                                probe,
                                bars.Count - 1));
        
                    for (int i = probe;
                         i >= 0;
                         i--)
                    {
                        TimeSpan span;
        
                        if (i + 1 < bars.Count)
                            span =
                                bars.OpenTimes[i + 1] -
                                bars.OpenTimes[i];
                        else if (i > 0)
                            span =
                                bars.OpenTimes[i] -
                                bars.OpenTimes[i - 1];
                        else
                            span =
                                TimeSpan.FromMinutes(1);
        
                        if (span <= TimeSpan.Zero)
                            span =
                                TimeSpan.FromMinutes(1);
        
                        if (bars.OpenTimes[i] +
                            span <=
                            reference)
                            return i;
                    }
        
                    return -1;
                }
        
                private double Highest(
                    Bars bars,
                    int start,
                    int end)
                {
                    if (bars == null ||
                        bars.Count == 0)
                        return 0;
        
                    start =
                        Math.Max(
                            0,
                            start);
        
                    end =
                        Math.Min(
                            bars.Count - 1,
                            end);
        
                    if (end < start)
                        return
                            bars.HighPrices[
                                Math.Max(
                                    0,
                                    Math.Min(
                                        bars.Count - 1,
                                        start))];
        
                    double value =
                        double.MinValue;
        
                    for (int i = start;
                         i <= end;
                         i++)
                        value =
                            Math.Max(
                                value,
                                bars.HighPrices[i]);
        
                    return
                        value ==
                        double.MinValue
                            ? 0
                            : value;
                }
        
                private double Lowest(
                    Bars bars,
                    int start,
                    int end)
                {
                    if (bars == null ||
                        bars.Count == 0)
                        return 0;
        
                    start =
                        Math.Max(
                            0,
                            start);
        
                    end =
                        Math.Min(
                            bars.Count - 1,
                            end);
        
                    if (end < start)
                        return
                            bars.LowPrices[
                                Math.Max(
                                    0,
                                    Math.Min(
                                        bars.Count - 1,
                                        start))];
        
                    double value =
                        double.MaxValue;
        
                    for (int i = start;
                         i <= end;
                         i++)
                        value =
                            Math.Min(
                                value,
                                bars.LowPrices[i]);
        
                    return
                        value ==
                        double.MaxValue
                            ? 0
                            : value;
                }
        
                private bool IsFinitePositive(
                    double value)
                {
                    return
                        !double.IsNaN(value) &&
                        !double.IsInfinity(value) &&
                        value > 0;
                }
        
                private double SafePositive(
                    double value)
                {
                    return
                        IsFinitePositive(value)
                            ? value
                            : 0;
                }
        
                private double NormalizePrice(
                    double price)
                {
                    if (!IsFinitePositive(price))
                        return 0;
        
                    if (Symbol.TickSize > 0)
                    {
                        price =
                            Math.Round(
                                price /
                                Symbol.TickSize,
                                MidpointRounding.AwayFromZero) *
                            Symbol.TickSize;
                    }
        
                    return Math.Round(
                        price,
                        Symbol.Digits);
                }
        
                private string Price(
                    double value)
                {
                    return
                        NormalizePrice(
                            value)
                        .ToString(
                            "F" +
                            Symbol.Digits);
                }
        
                private string CompactText(
                    string value,
                    int maxLength)
                {
                    if (string.IsNullOrEmpty(value))
                        return string.Empty;
        
                    int limit =
                        Math.Max(
                            1,
                            maxLength);
        
                    if (value.Length <=
                        limit)
                        return value;
        
                    if (limit <= 3)
                        return value.Substring(
                            0,
                            limit);
        
                    return
                        value.Substring(
                            0,
                            limit - 3) +
                        "...";
                }
        
                private double ClampDouble(
                    double value,
                    double min,
                    double max)
                {
                    if (double.IsNaN(value) ||
                        double.IsInfinity(value))
                        return min;
        
                    if (value < min)
                        return min;
        
                    if (value > max)
                        return max;
        
                    return value;
                }
        
                private double Clamp(
                    double value,
                    double min,
                    double max)
                {
                    if (value < min)
                        return min;
        
                    if (value > max)
                        return max;
        
                    return value;
                }
        
                private int ClampInt(
                    int value,
                    int min,
                    int max)
                {
                    if (value < min)
                        return min;
        
                    if (value > max)
                        return max;
        
                    return value;
                }
        
                private void RemoveAllChartObjects()
                {
                    RemovePlanObjects();
                    RemoveHistoricalObjects();
                    RemoveManagedPendingOrderObjects();
        
                    foreach (string name in _outcomeDrawn)
                        Chart.RemoveObject(name);
        
                    _outcomeDrawn.Clear();
                }
        
                // ============================================================
        
                // ============================================================
    }
}
