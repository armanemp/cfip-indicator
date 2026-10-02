// CFIP Indicator — StructuralStopPlanner.cs
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
        private List<Level> CollectStructuralStopCandidates(
                                    int closedM5,
                                    int direction,
                                    double entry,
                                    double atr)
                                {
                                    List<Level> candidates =
                                        new List<Level>();

                            if (UseM5StructureForStop)
                            {
                                double swing =
                                    direction == 1
                                        ? FindSwingLowBelow(
                                            _m5Bars,
                                            closedM5,
                                            entry)
                                        : FindSwingHighAbove(
                                            _m5Bars,
                                            closedM5,
                                            entry);
                
                                AddLevel(
                                    candidates,
                                    swing,
                                    "M5_SWING_STOP",
                                    "M5",
                                    0,
                                    SwingStructureWeight);
                            }
                
                            Zone supportFvg =
                                FindNearestFvg(
                                    _m5Bars,
                                    closedM5,
                                    direction,
                                    atr,
                                    false,
                                    entry,
                                    true);
                
                            if (supportFvg != null)
                            {
                                AddLevel(
                                    candidates,
                                    direction == 1
                                        ? supportFvg.Low
                                        : supportFvg.High,
                                    "FVG_STOP",
                                    "M5",
                                    supportFvg.Age,
                                    FvgWeight +
                                    SmartStopZoneBonus);
                            }
                
                            Zone supportOb =
                                FindNearestOrderBlock(
                                    _m5Bars,
                                    closedM5,
                                    direction,
                                    atr);
                
                            if (supportOb != null)
                            {
                                AddLevel(
                                    candidates,
                                    direction == 1
                                        ? supportOb.Low
                                        : supportOb.High,
                                    "ORDER_BLOCK_STOP",
                                    "M5",
                                    supportOb.Age,
                                    OrderBlockWeight +
                                    SmartStopZoneBonus);
                            }
                
                            if (UseHtfStructureForStop)
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
                
                                DateTime reference =
                                    _m5Bars.OpenTimes[
                                        closedM5];
                
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
                
                                    double frameAtr =
                                        Atr(
                                            frames[i],
                                            idx);
                
                                    if (frameAtr <= 0)
                                        frameAtr = atr;
                
                                    double swing =
                                        direction == 1
                                            ? FindSwingLowBelow(
                                                frames[i],
                                                idx,
                                                entry)
                                            : FindSwingHighAbove(
                                                frames[i],
                                                idx,
                                                entry);
                
                                    AddLevel(
                                        candidates,
                                        swing,
                                        "HTF_STRUCTURE_STOP",
                                        names[i],
                                        0,
                                        weights[i] +
                                        HtfRewardBonus / 2);
                
                                    Zone fvg =
                                        FindNearestFvg(
                                            frames[i],
                                            idx,
                                            direction,
                                            frameAtr);
                
                                    if (fvg != null)
                                    {
                                        AddLevel(
                                            candidates,
                                            direction == 1
                                                ? fvg.Low
                                                : fvg.High,
                                            "HTF_FVG_STOP",
                                            names[i],
                                            fvg.Age,
                                            FvgWeight +
                                            HtfRewardBonus / 2);
                                    }
                
                                    Zone ob =
                                        FindNearestOrderBlock(
                                            frames[i],
                                            idx,
                                            direction,
                                            frameAtr);
                
                                    if (ob != null)
                                    {
                                        AddLevel(
                                            candidates,
                                            direction == 1
                                                ? ob.Low
                                                : ob.High,
                                            "HTF_ORDER_BLOCK_STOP",
                                            names[i],
                                            ob.Age,
                                            OrderBlockWeight +
                                            HtfRewardBonus / 2);
                                    }
                                }
                            }
                                    return candidates;
                                }
    }
}
