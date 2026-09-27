// ============================================================================
// CFIP Indicator — TargetSources.cs
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
        private void AddSupplyDemandAndLiquidityLevels(
                                    List<Level> levels,
                                    int closedM5,
                                    int direction,
                                    double entry,
                                    double atr)
                                {
                                    if (_m5Bars == null)
                                        return;
                        
                                    double swing =
                                        direction == 1
                                            ? FindSwingHighAbove(
                                                _m5Bars,
                                                closedM5,
                                                entry)
                                            : FindSwingLowBelow(
                                                _m5Bars,
                                                closedM5,
                                                entry);
                        
                                    AddLevel(
                                        levels,
                                        swing,
                                        direction == 1
                                            ? "SUPPLY_ZONE"
                                            : "DEMAND_ZONE",
                                        "M5",
                                        0,
                                        SupplyDemandWeight);
                        
                                    if (UseEqualHighLow)
                                    {
                                        double liquidity =
                                            direction == 1
                                                ? FindEqualHigh(
                                                    _m5Bars,
                                                    closedM5,
                                                    entry,
                                                    atr)
                                                : FindEqualLow(
                                                    _m5Bars,
                                                    closedM5,
                                                    entry,
                                                    atr);
                        
                                        AddLevel(
                                            levels,
                                            liquidity,
                                            "LIQUIDITY_POOL",
                                            "M5",
                                            0,
                                            EqualHighLowWeight);
                                    }
                        
                                    if (UseDailyWeeklyLiquidity)
                                    {
                                        int d1 =
                                            ClosedIndex(
                                                _d1Bars,
                                                _m5Bars.OpenTimes[
                                                    closedM5]);
                        
                                        if (d1 > 0)
                                        {
                                            AddLevel(
                                                levels,
                                                direction == 1
                                                    ? _d1Bars.HighPrices[d1 - 1]
                                                    : _d1Bars.LowPrices[d1 - 1],
                                                "LIQUIDITY_POOL",
                                                "D1",
                                                1,
                                                LiquidityPoolWeight);
                                        }
                                    }
                        
                                    if (UseSessionLiquidityTargets)
                                    {
                                        double sessionHigh;
                                        double sessionLow;
                        
                                        GetSessionRange(
                                            _m5Bars,
                                            closedM5,
                                            SessionStartUtc,
                                            SessionEndUtc,
                                            out sessionHigh,
                                            out sessionLow);
                        
                                        AddLevel(
                                            levels,
                                            direction == 1
                                                ? sessionHigh
                                                : sessionLow,
                                            "SESSION",
                                            "M5",
                                            0,
                                            Math.Max(
                                                SessionWeight,
                                                LiquidityTargetMinimumScore));
                                    }
                                }
        
        private void GetSessionRange(
                                    Bars bars,
                                    int index,
                                    int startHour,
                                    int endHour,
                                    out double high,
                                    out double low)
                                {
                                    high = 0;
                                    low = 0;
                        
                                    if (bars == null || index < 5)
                                        return;
                        
                                    DateTime anchor =
                                        bars.OpenTimes[index];
                        
                                    DateTime dayStart =
                                        new DateTime(
                                            anchor.Year,
                                            anchor.Month,
                                            anchor.Day,
                                            0,
                                            0,
                                            0);
                        
                                    bool overnight =
                                        startHour > endHour;
                        
                                    DateTime from;
                                    DateTime to;
                        
                                    if (!overnight)
                                    {
                                        from =
                                            dayStart.AddHours(startHour);
                        
                                        to =
                                            dayStart.AddHours(endHour);
                                    }
                                    else if (anchor.Hour < endHour)
                                    {
                                        from =
                                            dayStart.AddDays(-1)
                                                .AddHours(startHour);
                        
                                        to =
                                            dayStart.AddHours(endHour);
                                    }
                                    else
                                    {
                                        from =
                                            dayStart.AddHours(startHour);
                        
                                        to =
                                            dayStart.AddDays(1)
                                                .AddHours(endHour);
                                    }
                        
                                    int first = -1;
                                    int last = -1;
                        
                                    for (int i = index;
                                         i >= Math.Max(0, index - 400);
                                         i--)
                                    {
                                        DateTime t = bars.OpenTimes[i];
                        
                                        if (t < from)
                                            break;
                        
                                        if (t < to)
                                        {
                                            first = i;
                                            if (last < 0)
                                                last = i;
                                        }
                                    }
                        
                                    if (first < 0 || last < first)
                                        return;
                        
                                    high =
                                        Highest(
                                            bars,
                                            first,
                                            last);
                        
                                    low =
                                        Lowest(
                                            bars,
                                            first,
                                            last);
                                }
        
        private double FindNextLiquidityAbove(
                                    Bars bars,
                                    int index,
                                    double price)
                                {
                                    if (bars == null ||
                                        index < 10)
                                        return 0;
                        
                                    int first =
                                        Math.Max(
                                            2,
                                            index -
                                            LiquidityLookback);
                        
                                    double best = 0;
                        
                                    for (int i = first;
                                         i <= index - 2;
                                         i++)
                                    {
                                        double high =
                                            bars.HighPrices[i];
                        
                                        if (high <= price)
                                            continue;
                        
                                        if (best <= 0 ||
                                            high < best)
                                            best = high;
                                    }
                        
                                    return best;
                                }
        
        private double FindNextLiquidityBelow(
                                    Bars bars,
                                    int index,
                                    double price)
                                {
                                    if (bars == null ||
                                        index < 10)
                                        return 0;
                        
                                    int first =
                                        Math.Max(
                                            2,
                                            index -
                                            LiquidityLookback);
                        
                                    double best = 0;
                        
                                    for (int i = first;
                                         i <= index - 2;
                                         i++)
                                    {
                                        double low =
                                            bars.LowPrices[i];
                        
                                        if (low >= price)
                                            continue;
                        
                                        if (best <= 0 ||
                                            low > best)
                                            best = low;
                                    }
                        
                                    return best;
                                }
        
        private void AddSmartExtraTargetLevels(
                                    List<Level> levels,
                                    int closedM5,
                                    int direction,
                                    double entry,
                                    double atr)
                                {
                                    if (!UseExtendedLiquidityMap ||
                                        _m5Bars == null)
                                        return;
                        
                                    double forecast =
                                        direction == 1
                                            ? FindNextLiquidityAbove(
                                                _m5Bars,
                                                closedM5,
                                                entry)
                                            : FindNextLiquidityBelow(
                                                _m5Bars,
                                                closedM5,
                                                entry);
                        
                                    AddLevel(
                                        levels,
                                        forecast,
                                        "LIQUIDITY_FORECAST",
                                        "M5",
                                        0,
                                        Math.Max(
                                            LiquidityPoolWeight,
                                            LiquidityTargetMinimumScore));
                        
                                    if (UseSessionLiquidityTargets)
                                    {
                                        double high;
                                        double low;
                        
                                        GetSessionRange(
                                            _m5Bars,
                                            closedM5,
                                            SessionStartUtc,
                                            SessionEndUtc,
                                            out high,
                                            out low);
                        
                                        AddLevel(
                                            levels,
                                            direction == 1
                                                ? high
                                                : low,
                                            "SESSION_FORECAST",
                                            "M5",
                                            0,
                                            Math.Max(
                                                SessionWeight,
                                                LiquidityTargetMinimumScore));
                                    }
                                }
        
        private void AddDailyPivotLevels(
                                    List<Level> levels,
                                    int direction,
                                    double entry,
                                    double atr,
                                    DateTime reference)
                                {
                                    if (!UseDailyPivots ||
                                        _d1Bars == null ||
                                        _d1Bars.Count < 3 ||
                                        atr <= 0)
                                        return;
                        
                                    int index =
                                        ClosedIndex(
                                            _d1Bars,
                                            reference);
                        
                                    if (index <= 0)
                                        return;
                        
                                    int previous = index - 1;
                        
                                    double high = _d1Bars.HighPrices[previous];
                                    double low = _d1Bars.LowPrices[previous];
                                    double close = _d1Bars.ClosePrices[previous];
                        
                                    if (high <= low ||
                                        !IsFinitePositive(close))
                                        return;
                        
                                    double pivot = (high + low + close) / 3.0;
                                    double range = high - low;
                        
                                    double r1 = 2.0 * pivot - low;
                                    double s1 = 2.0 * pivot - high;
                                    double r2 = pivot + range;
                                    double s2 = pivot - range;
                                    double r3 = high + 2.0 * (pivot - low);
                                    double s3 = low - 2.0 * (high - pivot);
                        
                                    double weight =
                                        Math.Max(
                                            1,
                                            DailyPivotWeight);
                        
                                    AddLevel(levels, pivot, "PIVOT", "D1", 1, weight);
                        
                                    if (direction == 1)
                                    {
                                        AddLevel(levels, r1, "PIVOT_R1", "D1", 1, weight + 4);
                                        AddLevel(levels, r2, "PIVOT_R2", "D1", 1, weight + 8);
                                        AddLevel(levels, r3, "PIVOT_R3", "D1", 1, weight + 10);
                                    }
                                    else if (direction == -1)
                                    {
                                        AddLevel(levels, s1, "PIVOT_S1", "D1", 1, weight + 4);
                                        AddLevel(levels, s2, "PIVOT_S2", "D1", 1, weight + 8);
                                        AddLevel(levels, s3, "PIVOT_S3", "D1", 1, weight + 10);
                                    }
                                }
        
        private void AddHtfTargets(
                                    List<Level> levels,
                                    int direction,
                                    double entry,
                                    double atr,
                                    DateTime reference)
                                {
                                    Bars[] frames =
                                    {
                                        _m15Bars,
                                        _m30Bars,
                                        _h1Bars,
                                        _h4Bars,
                                        _d1Bars,
                                        _w1Bars
                                    };
                        
                                    string[] names =
                                    {
                                        "M15",
                                        "M30",
                                        "H1",
                                        "H4",
                                        "D1",
                                        "W1"
                                    };
                        
                                    int[] weights =
                                    {
                                        M15Weight,
                                        M30Weight,
                                        H1Weight,
                                        H4Weight,
                                        D1Weight,
                                        W1Weight
                                    };
                        
                                    for (int i = 0;
                                         i < frames.Length;
                                         i++)
                                    {
                                        if (frames[i] == null)
                                            continue;
                        
                                        int idx =
                                            ClosedIndex(
                                                frames[i],
                                                reference);
                        
                                        if (idx < 10)
                                            continue;
                        
                                        double clusterWeight =
                                            i < 2
                                                ? MtfClusterWeight
                                                : HtfStructureWeight;
                        
                                        double frameAtr =
                                            Atr(
                                                frames[i],
                                                idx);
                        
                                        if (frameAtr <= 0)
                                            frameAtr = atr;
                        
                                        double swing =
                                            direction == 1
                                                ? FindSwingHighAbove(
                                                    frames[i],
                                                    idx,
                                                    entry)
                                                : FindSwingLowBelow(
                                                    frames[i],
                                                    idx,
                                                    entry);
                        
                                        AddLevel(
                                            levels,
                                            swing,
                                            "HTF_SWING",
                                            names[i],
                                            0,
                                            weights[i] +
                                            clusterWeight / 10.0 +
                                            HtfRewardBonus);
                        
                                        int opposing =
                                            -direction;
                        
                                        Zone fvg =
                                            FindNearestFvg(
                                                frames[i],
                                                idx,
                                                opposing,
                                                frameAtr);
                        
                                        if (fvg != null)
                                        {
                                            AddLevel(
                                                levels,
                                                direction == 1
                                                    ? fvg.Low
                                                    : fvg.High,
                                                "HTF_FVG",
                                                names[i],
                                                fvg.Age,
                                                FvgWeight +
                                                clusterWeight / 10.0 +
                                                HtfRewardBonus);
                                        }
                        
                                        Zone ob =
                                            FindNearestOrderBlock(
                                                frames[i],
                                                idx,
                                                opposing,
                                                frameAtr);
                        
                                        if (ob != null)
                                        {
                                            AddLevel(
                                                levels,
                                                direction == 1
                                                    ? ob.Low
                                                    : ob.High,
                                                "HTF_ORDER_BLOCK",
                                                names[i],
                                                ob.Age,
                                                OrderBlockWeight +
                                                clusterWeight / 10.0 +
                                                HtfRewardBonus);
                                        }
                        
                                        double liquidity =
                                            direction == 1
                                                ? FindEqualHigh(
                                                    frames[i],
                                                    idx,
                                                    entry,
                                                    frameAtr)
                                                : FindEqualLow(
                                                    frames[i],
                                                    idx,
                                                    entry,
                                                    frameAtr);
                        
                                        if (UseHigherTfLiquidityTargets)
                                        {
                                            AddLevel(
                                                levels,
                                                liquidity,
                                                "HTF_LIQUIDITY",
                                                names[i],
                                                0,
                                                LiquidityPoolWeight +
                                                clusterWeight / 10.0 +
                                                HtfRewardBonus);
                                        }
                                    }
                                }
        
        private void AddPreviousPeriodLevels(
                                    List<Level> levels,
                                    int direction,
                                    double entry,
                                    double atr,
                                    DateTime reference)
                                {
                                    int d1 =
                                        ClosedIndex(
                                            _d1Bars,
                                            reference);
                        
                                    int w1 =
                                        ClosedIndex(
                                            _w1Bars,
                                            reference);
                        
                                    if (UseDailyWeeklyLiquidity &&
                                        d1 > 0)
                                    {
                                        AddLevel(
                                            levels,
                                            direction == 1
                                                ? _d1Bars.HighPrices[d1 - 1]
                                                : _d1Bars.LowPrices[d1 - 1],
                                            "PREVIOUS_DAY",
                                            "D1",
                                            1,
                                            PreviousDayWeight);
                                    }
                        
                                    if (UseDailyWeeklyLiquidity &&
                                        w1 > 0)
                                    {
                                        AddLevel(
                                            levels,
                                            direction == 1
                                                ? _w1Bars.HighPrices[w1 - 1]
                                                : _w1Bars.LowPrices[w1 - 1],
                                            "PREVIOUS_WEEK",
                                            "W1",
                                            1,
                                            PreviousWeekWeight);
                                    }
                                }
    }
}
