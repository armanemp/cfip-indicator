// ============================================================================
// CFIP Indicator — RewardPathValidation.cs
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
        private bool HasOpposingZonePathObstacle(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double entry,
                                    double target,
                                    double atr)
                                {
                                    if (bars == null ||
                                        index < 8 ||
                                        atr <= 0 ||
                                        !IsFinitePositive(entry) ||
                                        !IsFinitePositive(target))
                                        return false;
                        
                                    int first =
                                        Math.Max(
                                            2,
                                            index -
                                            Math.Max(
                                                5,
                                                TargetObstacleLookbackBars));
                        
                                    double clearance =
                                        atr *
                                        Math.Max(
                                            TargetClearanceAtr,
                                            TargetObstacleBufferAtr);
                        
                                    for (int i = first;
                                         i <= index - 1;
                                         i++)
                                    {
                                        if (direction == 1)
                                        {
                                            double gap =
                                                bars.LowPrices[i] -
                                                bars.HighPrices[i - 2];
                        
                                            if (gap >=
                                                atr *
                                                MinimumFvgAtr)
                                            {
                                                double low =
                                                    bars.HighPrices[i - 2];
                        
                                                double high =
                                                    bars.LowPrices[i];
                        
                                                if (ZoneBlocksRewardPath(
                                                        low,
                                                        high,
                                                        entry,
                                                        target,
                                                        clearance))
                                                    return true;
                                            }
                                        }
                                        else
                                        {
                                            double gap =
                                                bars.LowPrices[i - 2] -
                                                bars.HighPrices[i];
                        
                                            if (gap >=
                                                atr *
                                                MinimumFvgAtr)
                                            {
                                                double low =
                                                    bars.HighPrices[i];
                        
                                                double high =
                                                    bars.LowPrices[i - 2];
                        
                                                if (ZoneBlocksRewardPath(
                                                        low,
                                                        high,
                                                        entry,
                                                        target,
                                                        clearance))
                                                    return true;
                                            }
                                        }
                        
                                        if (UseOrderBlock)
                                        {
                                            Zone oppositeOb =
                                                BuildOrderBlockCandidate(
                                                    bars,
                                                    i,
                                                    index,
                                                    -direction,
                                                    atr);
                        
                                            if (oppositeOb != null &&
                                                ZoneBlocksRewardPath(
                                                    oppositeOb.Low,
                                                    oppositeOb.High,
                                                    entry,
                                                    target,
                                                    clearance))
                                                return true;
                                        }
                                    }
                        
                                    return false;
                                }
        
        private bool HasHigherTfZonePathObstacle(
                                    DateTime reference,
                                    int direction,
                                    double entry,
                                    double target)
                                {
                                    Bars[] frames =
                                    {
                                        _m15Bars,
                                        _m30Bars,
                                        _h1Bars,
                                        _h4Bars
                                    };
                        
                                    for (int i = 0;
                                         i < frames.Length;
                                         i++)
                                    {
                                        Bars frame =
                                            frames[i];
                        
                                        if (frame == null)
                                            continue;
                        
                                        int index =
                                            ClosedIndex(
                                                frame,
                                                reference);
                        
                                        if (index < 8)
                                            continue;
                        
                                        double frameAtr =
                                            Atr(
                                                frame,
                                                index);
                        
                                        if (frameAtr <= 0)
                                            continue;
                        
                                        if (HasOpposingZonePathObstacle(
                                                frame,
                                                index,
                                                direction,
                                                entry,
                                                target,
                                                frameAtr))
                                            return true;
                                    }
                        
                                    return false;
                                }
        
        private bool ZoneBlocksRewardPath(
                                    double low,
                                    double high,
                                    double entry,
                                    double target,
                                    double clearance)
                                {
                                    if (low >= high)
                                        return false;
                        
                                    double pathLow =
                                        Math.Min(
                                            entry,
                                            target);
                        
                                    double pathHigh =
                                        Math.Max(
                                            entry,
                                            target);
                        
                                    if (high <=
                                        pathLow +
                                        clearance ||
                                        low >=
                                        pathHigh -
                                        clearance)
                                        return false;
                        
                                    bool containsTarget =
                                        target >=
                                            low - clearance &&
                                        target <=
                                            high + clearance;
                        
                                    if (containsTarget)
                                        return false;
                        
                                    bool containsEntry =
                                        entry >=
                                            low - clearance &&
                                        entry <=
                                            high + clearance;
                        
                                    if (containsEntry)
                                        return false;
                        
                                    return
                                        high >
                                        pathLow + clearance &&
                                        low <
                                        pathHigh - clearance;
                                }
        
        private bool HasTargetObstacle(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double entry,
                                    double target,
                                    double atr)
                                {
                                    if (bars == null ||
                                        index < 8 ||
                                        atr <= 0 ||
                                        !IsFinitePositive(entry) ||
                                        !IsFinitePositive(target))
                                        return false;
                        
                                    double clearance =
                                        atr *
                                        Math.Max(
                                            TargetClearanceAtr,
                                            TargetObstacleBufferAtr);
                        
                                    int strength =
                                        Math.Max(
                                            1,
                                            Math.Min(
                                                SwingStrength,
                                                3));
                        
                                    int start =
                                        Math.Max(
                                            strength + 1,
                                            index -
                                            Math.Max(
                                                5,
                                                TargetObstacleLookbackBars));
                        
                                    int last =
                                        Math.Max(
                                            start,
                                            index -
                                            strength -
                                            1);
                        
                                    for (int i = start;
                                         i <= last;
                                         i++)
                                    {
                                        bool swing = true;
                        
                                        for (int j = 1;
                                             j <= strength;
                                             j++)
                                        {
                                            if (direction == 1)
                                            {
                                                if (bars.HighPrices[i] <=
                                                    bars.HighPrices[i - j] ||
                                                    bars.HighPrices[i] <=
                                                    bars.HighPrices[i + j])
                                                {
                                                    swing = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (bars.LowPrices[i] >=
                                                    bars.LowPrices[i - j] ||
                                                    bars.LowPrices[i] >=
                                                    bars.LowPrices[i + j])
                                                {
                                                    swing = false;
                                                    break;
                                                }
                                            }
                                        }
                        
                                        if (!swing)
                                            continue;
                        
                                        if (direction == 1)
                                        {
                                            double level =
                                                bars.HighPrices[i];
                        
                                            if (level > entry &&
                                                level < target - clearance)
                                                return true;
                                        }
                                        else
                                        {
                                            double level =
                                                bars.LowPrices[i];
                        
                                            if (level < entry &&
                                                level > target + clearance)
                                                return true;
                                        }
                                    }
                        
                                    if (UseEqualHighLow)
                                    {
                                        double liquidityLevel =
                                            direction == 1
                                                ? FindEqualHigh(
                                                    bars,
                                                    index,
                                                    entry,
                                                    atr)
                                                : FindEqualLow(
                                                    bars,
                                                    index,
                                                    entry,
                                                    atr);
                        
                                        if (direction == 1 &&
                                            liquidityLevel > entry &&
                                            liquidityLevel <
                                            target - clearance)
                                            return true;
                        
                                        if (direction == -1 &&
                                            liquidityLevel < entry &&
                                            liquidityLevel >
                                            target + clearance)
                                            return true;
                                    }
                        
                                    return false;
                                }
        
        private bool HasAnyHtfTargetLevel(
                                    List<Level> candidates)
                                {
                                    if (candidates == null)
                                        return false;
                        
                                    for (int i = 0;
                                         i < candidates.Count;
                                         i++)
                                    {
                                        string tf =
                                            candidates[i].Timeframe;
                        
                                        if (tf == "H1" ||
                                            tf == "H4" ||
                                            tf == "D1" ||
                                            tf == "W1")
                                            return true;
                                    }
                        
                                    return false;
                                }
    }
}
