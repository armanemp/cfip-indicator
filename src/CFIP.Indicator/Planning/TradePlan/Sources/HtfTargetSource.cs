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

                                                double sourceAgeMinutes =
                                                    TargetAgeSemanticsRule.ElapsedMinutes(
                                                        frames[i].OpenTimes[idx],
                                                        reference);
                                
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
                                                    HtfRewardBonus,
                                                    sourceAgeMinutes);
                                
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
                                                        HtfRewardBonus,
                                                        sourceAgeMinutes);
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
                                                        HtfRewardBonus,
                                                        sourceAgeMinutes);
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
                                                        HtfRewardBonus,
                                                        sourceAgeMinutes);
                                                }
                                            }
                                        }
    }
}
