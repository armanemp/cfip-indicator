// ============================================================================
// CFIP Indicator — TargetAggregationEngine.cs
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
        
        private List<Level> BuildTargetLevels(
                            int closedM5,
                            int direction,
                            double entry,
                            double atr)
                        {
                            List<Level> levels =
                                new List<Level>();
                
                            if (_m5Bars == null ||
                                atr <= 0 ||
                                !IsFinitePositive(entry))
                                return levels;
                
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
                                "SWING_TARGET",
                                "M5",
                                0,
                                SwingStructureWeight);
                
                            int opposingDirection =
                                -direction;
                
                            Zone fvg =
                                FindNearestFvg(
                                    _m5Bars,
                                    closedM5,
                                    opposingDirection,
                                    atr);
                
                            if (fvg != null)
                            {
                                AddLevel(
                                    levels,
                                    direction == 1
                                        ? fvg.Low
                                        : fvg.High,
                                    "OPPOSING_FVG",
                                    "M5",
                                    fvg.Age,
                                    FvgWeight +
                                    ZoneRewardBonus);
                            }
                
                            Zone ob =
                                FindNearestOrderBlock(
                                    _m5Bars,
                                    closedM5,
                                    opposingDirection,
                                    atr);
                
                            if (ob != null)
                            {
                                AddLevel(
                                    levels,
                                    direction == 1
                                        ? ob.Low
                                        : ob.High,
                                    "OPPOSING_ORDER_BLOCK",
                                    "M5",
                                    ob.Age,
                                    OrderBlockWeight +
                                    ZoneRewardBonus);
                            }
                
                            AddSupplyDemandAndLiquidityLevels(
                                levels,
                                closedM5,
                                direction,
                                entry,
                                atr);
                
                            if (UseDailyPivots)
                            {
                                AddDailyPivotLevels(
                                    levels,
                                    direction,
                                    entry,
                                    atr,
                                    _m5Bars.OpenTimes[closedM5]);
                            }
                
                            if (UseMultiTfLevelMap)
                            {
                                AddHtfTargets(
                                    levels,
                                    direction,
                                    entry,
                                    atr,
                                    _m5Bars.OpenTimes[
                                        closedM5]);
                
                                AddPreviousPeriodLevels(
                                    levels,
                                    direction,
                                    entry,
                                    atr,
                                    _m5Bars.OpenTimes[
                                        closedM5]);
                            }
                
                            AddSmartExtraTargetLevels(
                                levels,
                                closedM5,
                                direction,
                                entry,
                                atr);
                
                            return
                                MergeLevels(
                                    levels
                                        .Where(
                                            x =>
                                                IsFinitePositive(
                                                    x.Price) &&
                                                x.Price != entry)
                                        .OrderByDescending(
                                            x =>
                                                x.Score)
                                        .Take(
                                            Math.Max(
                                                1,
                                                SmartTargetMaxCandidates))
                                        .ToList(),
                                    atr);
                        }
        
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
        
        private void AddLevel(
                            List<Level> levels,
                            double price,
                            string kind,
                            string timeframe,
                            int age,
                            double baseScore)
                        {
                            if (!IsFinitePositive(price))
                                return;
                
                            double score =
                                Math.Max(
                                    0,
                                    baseScore) * 0.60;
                
                            if (age <= 5)
                                score += 15;
                
                            if (timeframe == "H1" ||
                                timeframe == "H4" ||
                                timeframe == "D1" ||
                                timeframe == "W1")
                                score += 8;
                
                            if (kind.IndexOf(
                                    "LIQUIDITY",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                score += 5;
                
                            if (kind.IndexOf(
                                    "FVG",
                                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                                kind.IndexOf(
                                    "ORDER_BLOCK",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                score += 4;
                
                            Level level =
                                new Level
                                {
                                    Price =
                                        NormalizePrice(price),
                                    Score =
                                        Clamp(
                                            score,
                                            0,
                                            100),
                                    Kind = kind,
                                    Timeframe = timeframe,
                                    Age =
                                        Math.Max(
                                            0,
                                            age),
                                    Hits = 1
                                };
                
                            levels.Add(
                                level);
                        }
        
        private List<Level> MergeLevels(
                            List<Level> input,
                            double atr)
                        {
                            List<Level> result =
                                new List<Level>();
                
                            if (input == null ||
                                input.Count == 0)
                                return result;
                
                            double tolerance =
                                Math.Max(
                                    Symbol.PipSize * 2,
                                    atr *
                                    Math.Max(
                                        0.02,
                                        SmartLevelClusterAtr));
                
                            for (int i = 0;
                                 i < input.Count;
                                 i++)
                            {
                                Level current =
                                    input[i];
                
                                Level match =
                                    result.FirstOrDefault(
                                        x =>
                                            Math.Abs(
                                                x.Price -
                                                current.Price) <=
                                            tolerance);
                
                                if (match == null)
                                {
                                    result.Add(current);
                                    continue;
                                }
                
                                int matchHits =
                                    Math.Max(
                                        1,
                                        match.Hits);
                
                                int currentHits =
                                    Math.Max(
                                        1,
                                        current.Hits);
                
                                match.Price =
                                    NormalizePrice(
                                        (match.Price *
                                         matchHits +
                                         current.Price *
                                         currentHits) /
                                        (matchHits +
                                         currentHits));
                
                                match.Score =
                                    Clamp(
                                        Math.Max(
                                            match.Score,
                                            current.Score) +
                                        Math.Min(
                                            20,
                                            Math.Min(
                                                match.Score,
                                                current.Score) *
                                            0.25),
                                        0,
                                        100);
                
                                match.Hits =
                                    matchHits +
                                    currentHits;
                
                                match.Age =
                                    Math.Min(
                                        match.Age,
                                        current.Age);
                
                                if (current.Timeframe == "H1" ||
                                    current.Timeframe == "H4" ||
                                    current.Timeframe == "D1" ||
                                    current.Timeframe == "W1")
                                    match.Timeframe =
                                        current.Timeframe;
                
                                if (match.Kind == "SWING" &&
                                    current.Kind != "SWING")
                                    match.Kind =
                                        current.Kind;
                            }
                
                            return
                                result
                                .OrderByDescending(
                                    x => x.Score)
                                .ToList();
                        }
        
        private List<Level> SelectTargets(
                            List<Level> levels,
                            int closedM5,
                            double entry,
                            double risk,
                            int direction,
                            double atr)
                        {
                            // FIX (CFIP-BUG-STAGE-ALIGNMENT): this list always has exactly 4 slots
                            // (index 0=TP1 .. 3=TP4), and a slot stays null when no candidate meets
                            // that stage's own RR requirement. Previously the list only contained
                            // the *found* levels appended in order, so if stage 0 (TP1) found
                            // nothing but stage 1 (TP2) did, that TP2-grade level silently became
                            // "selected[0]" and got labelled/priced as TP1 by every caller
                            // (BuildPlan, SelectStructuralAutoTarget, live target refresh). That
                            // meant the reported TP could be at the wrong RR tier without any
                            // indication. Keeping fixed, possibly-null slots makes index == stage
                            // always true, so downstream RR/labels are trustworthy.
                            List<Level> selected =
                                new List<Level>
                                {
                                    null, null, null, null
                                };
                
                            if (levels == null ||
                                levels.Count == 0 ||
                                risk <= 0 ||
                                atr <= 0)
                                return selected;
                
                            double rrStep =
                                Math.Max(
                                    0.10,
                                    StructuralTpRrStep);
                
                            double adaptiveTp1RR =
                                Math.Max(
                                    Tp1MinimumRR,
                                    MinimumRequiredRR());
                
                            double maximumRR =
                                Math.Max(
                                    adaptiveTp1RR,
                                    MaximumRewardRR);
                
                            double[] requiredRR =
                            {
                                adaptiveTp1RR,
                                Math.Max(
                                    Tp2MinimumRR,
                                    adaptiveTp1RR + rrStep),
                                Math.Max(
                                    Tp3MinimumRR,
                                    Tp2MinimumRR + rrStep),
                                Math.Max(
                                    Tp4MinimumRR,
                                    Tp3MinimumRR + rrStep)
                            };
                
                            for (int stage = 0;
                                 stage < 4;
                                 stage++)
                            {
                                if (requiredRR[stage] >
                                    maximumRR)
                                    continue;
                
                                bool requireHtf =
                                    stage == 0
                                        ? RequireHtfRewardForTp1
                                        : RequireHtfRewardForTp2Plus;
                
                                Level best = null;
                                double bestScore =
                                    double.MinValue;
                
                                // Nearest already-assigned earlier stage (skipping any stage
                                // that stayed empty), falling back to entry. This keeps target
                                // spacing meaningful even when a lower stage had no candidate.
                                double previous = entry;
                
                                for (int p = stage - 1; p >= 0; p--)
                                {
                                    if (selected[p] != null)
                                    {
                                        previous = selected[p].Price;
                                        break;
                                    }
                                }
                
                                for (int i = 0;
                                     i < levels.Count;
                                     i++)
                                {
                                    Level candidate =
                                        levels[i];
                
                                    if (!IsValidTarget(
                                            direction,
                                            entry,
                                            candidate.Price))
                                        continue;
                
                                    if (candidate.Age >
                                        MaximumSetupAgeBars)
                                        continue;
                
                                    bool htf =
                                        IsHtfTimeframe(
                                            candidate.Timeframe);
                
                                    if (requireHtf &&
                                        (!htf ||
                                         candidate.Score <
                                         MinimumHtfRewardQuality))
                                        continue;
                
                                    double distance =
                                        Math.Abs(
                                            candidate.Price -
                                            entry);
                
                                    double rr =
                                        distance /
                                        Math.Max(
                                            Symbol.PipSize,
                                            risk);
                
                                    double minimumCandidateRR =
                                        requiredRR[stage];
                
                                    if (htf)
                                        minimumCandidateRR =
                                            Math.Max(
                                                minimumCandidateRR,
                                                MinimumHtfTargetRR);
                
                                    if (rr < minimumCandidateRR ||
                                        rr > maximumRR)
                                        continue;
                
                                    if (distance >
                                        atr *
                                        Math.Max(
                                            1.0,
                                            MaximumTargetExtensionAtr))
                                        continue;
                
                                    double spacing =
                                        atr *
                                        Math.Max(
                                            0.05,
                                            MinimumTpSpacingAtr);
                
                                    if (selected.Any(
                                        x =>
                                            x != null &&
                                            Math.Abs(
                                                x.Price -
                                                candidate.Price) <=
                                            spacing * 0.50))
                                        continue;
                
                                    if (stage > 0)
                                    {
                                        if (direction == 1 &&
                                            candidate.Price <=
                                            previous +
                                            spacing)
                                            continue;
                
                                        if (direction == -1 &&
                                            candidate.Price >=
                                            previous -
                                            spacing)
                                            continue;
                                    }
                
                                    if (RejectTargetObstacle &&
                                        HasTargetObstacle(
                                            _m5Bars,
                                            closedM5,
                                            direction,
                                            entry,
                                            candidate.Price,
                                            atr))
                                        continue;
                
                                    if (RejectTargetObstacle &&
                                        HasOpposingZonePathObstacle(
                                            _m5Bars,
                                            closedM5,
                                            direction,
                                            entry,
                                            candidate.Price,
                                            atr))
                                        continue;
                
                                    if (stage >= 1 &&
                                        RejectTargetObstacle &&
                                        HasHigherTfZonePathObstacle(
                                            _m5Bars.OpenTimes[
                                                closedM5],
                                            direction,
                                            entry,
                                            candidate.Price))
                                        continue;
                
                                    double normalizedDistance =
                                        distance /
                                        Math.Max(
                                            Symbol.PipSize,
                                            atr);
                
                                    double efficiency =
                                        Math.Max(
                                            0,
                                            25 -
                                            Math.Abs(
                                                rr -
                                                requiredRR[stage]) *
                                            4);
                
                                    double score =
                                        candidate.Score +
                                        efficiency;
                
                                    if (htf)
                                        score +=
                                            Math.Max(
                                                0,
                                                HtfRewardBonus);
                
                                    if (candidate.Kind.IndexOf(
                                            "LIQUIDITY",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        score +=
                                            Math.Max(
                                                0,
                                                LiquidityRewardBonus);
                
                                    if (candidate.Kind.IndexOf(
                                            "FVG",
                                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        candidate.Kind.IndexOf(
                                            "ORDER_BLOCK",
                                            StringComparison.OrdinalIgnoreCase) >= 0)
                                        score +=
                                            Math.Max(
                                                0,
                                                ZoneRewardBonus);
                
                                    score +=
                                        Math.Min(
                                            20,
                                            Math.Max(
                                                1,
                                                candidate.Hits) *
                                            2);
                
                                    score +=
                                        SmartTargetNearestBias /
                                        (1.0 +
                                         Math.Max(
                                             0,
                                             normalizedDistance)) *
                                        10.0;
                
                                    score +=
                                        stage *
                                        (htf
                                            ? HtfRewardBonus * 0.35
                                            : 2.0);
                
                                    if (score > bestScore)
                                    {
                                        bestScore =
                                            score;
                                        best =
                                            candidate;
                                    }
                                }
                
                                if (best != null)
                                    selected[stage] = best;
                            }
                
                            return selected;
                        }
        
        private double SelectTarget(
                            List<Level> selected,
                            int position,
                            double entry,
                            double risk,
                            int direction,
                            double alternateRR)
                        {
                            if (selected != null &&
                                position < selected.Count &&
                                selected[position] != null)
                                return selected[position].Price;
                
                            bool requireHtf =
                                position == 0
                                    ? RequireHtfRewardForTp1
                                    : RequireHtfRewardForTp2Plus;
                
                            double minimumRR =
                                Math.Max(
                                    0.50,
                                    alternateRR);
                
                            double maximumRR =
                                Math.Max(
                                    minimumRR,
                                    MaximumRewardRR);
                
                            if (!AllowSyntheticTargetFallback ||
                                requireHtf ||
                                minimumRR > maximumRR)
                                return 0;
                
                            return NormalizePrice(
                                direction == 1
                                    ? entry +
                                      risk *
                                      minimumRR
                                    : entry -
                                      risk *
                                      minimumRR);
                        }
        
        private void ApplyTargetMeta(
                            List<Level> candidates,
                            double target,
                            double atr,
                            out string source,
                            out int quality)
                        {
                            source =
                                target > 0
                                    ? "RR"
                                    : "";
                
                            quality =
                                target > 0
                                    ? 55
                                    : 0;
                
                            if (target <= 0)
                                return;
                
                            Level best = null;
                            double bestDistance = double.MaxValue;
                
                            for (int i = 0; i < candidates.Count; i++)
                            {
                                double distance =
                                    Math.Abs(
                                        candidates[i].Price -
                                        target);
                
                                if (distance <= atr * 0.15 &&
                                    distance < bestDistance)
                                {
                                    best =
                                        candidates[i];
                
                                    bestDistance =
                                        distance;
                                }
                            }
                
                            if (best != null)
                            {
                                source =
                                    best.Kind +
                                    "@" +
                                    best.Timeframe;
                
                                quality =
                                    ClampInt(
                                        (int)Math.Round(
                                            best.Score),
                                        0,
                                        100);
                
                                if (best.Age <= 5)
                                    quality =
                                        Math.Min(
                                            100,
                                            quality + 5);
                            }
                        }
    }
}
