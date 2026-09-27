// ============================================================================
// CFIP Indicator — TargetLevelAggregation.cs
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
    }
}
