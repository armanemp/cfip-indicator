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
    }
}
