// CFIP Indicator — TargetLevelBuilder.cs
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
private List<Level> BuildTargetLevels(
                                            int closedM5,
                                            int direction,
                                            double entry,
                                            double atr)
                                        {
                                            if (_targetLevelCache != null &&
                                                _targetLevelCacheM5 == closedM5 &&
                                                _targetLevelCacheDirection == direction &&
                                                Math.Abs(
                                                    _targetLevelCacheEntry -
                                                    entry) <=
                                                Math.Max(
                                                    Symbol.TickSize,
                                                    Symbol.PipSize * 0.25) &&
                                                Math.Abs(
                                                    _targetLevelCacheAtr -
                                                    atr) <=
                                                Math.Max(
                                                    Symbol.TickSize,
                                                    atr * 0.0001))
                                            {
                                                return CloneLevels(
                                                    _targetLevelCache);
                                            }

                                            List<Level> levels =
                                                new List<Level>();

                                            AddM1MicroTargetContext(
                                                levels,
                                                closedM5,
                                                direction,
                                                entry,
                                                atr);
                                
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
                                                    atr,
                                                    false,
                                                    entry,
                                                    true);
                                
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
                                
                                            DateTime closedM5Boundary =
                                                ClosedBarBoundaryReference(
                                                    _m5Bars,
                                                    closedM5,
                                                    CanonicalTimeRule.EnsureUtc(
                                                        Server.TimeInUtc));

                                            if (UseDailyPivots)
                                            {
                                                AddDailyPivotLevels(
                                                    levels,
                                                    direction,
                                                    entry,
                                                    atr,
                                                    closedM5Boundary);
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
                                
                                            List<Level> preparedLevels =
                                                levels
                                                    .Where(
                                                        x =>
                                                            IsFinitePositive(
                                                                x.Price) &&
                                                            x.Price != entry)
                                                    .OrderByDescending(
                                                        x => x.Score)
                                                    .ToList();

                                            List<Level> mergedLevels =
                                                MergeLevels(
                                                    preparedLevels,
                                                    atr);

                                            mergedLevels =
                                                mergedLevels
                                                    .OrderByDescending(
                                                        x => x.Score)
                                                    .Take(
                                                        Math.Max(
                                                            1,
                                                            SmartTargetMaxCandidates))
                                                    .ToList();

                                            _targetLevelCacheM5 = closedM5;
                                            _targetLevelCacheDirection = direction;
                                            _targetLevelCacheEntry = entry;
                                            _targetLevelCacheAtr = atr;
                                            _targetLevelCache =
                                                CloneLevels(
                                                    mergedLevels);

                                            return CloneLevels(
                                                mergedLevels);
                                        }

private static List<Level> CloneLevels(
                                            IEnumerable<Level> source)
                                        {
                                            if (source == null)
                                                return
                                                    new List<Level>();

                                            return source
                                                .Where(x => x != null)
                                                .Select(
                                                    x =>
                                                        new Level
                                                        {
                                                            Price = x.Price,
                                                            Score = x.Score,
                                                            Kind = x.Kind,
                                                            Timeframe =
                                                                x.Timeframe,
                                                            Age = x.Age,
                                                            SourceAgeMinutes =
                                                                x.SourceAgeMinutes,
                                                            Hits = x.Hits
                                                        })
                                                .ToList();
                                        }
    }
}