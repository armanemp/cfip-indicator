// CFIP Indicator — OrderBlockConfluenceAnalyzer.cs
// Single-responsibility structure module.

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
        private bool HasOrderBlockLiquiditySweep(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double atr)
                                {
                                    if (!UseLiquiditySweep ||
                                        bars == null ||
                                        index < 6 ||
                                        atr <= 0 ||
                                        (direction != 1 &&
                                         direction != -1))
                                        return false;

                                    // An OB source candle is not itself the sweep
                                    // event: for a directional OB it is normally the
                                    // opposing source candle. Search only earlier
                                    // fully-closed bars and consume the same canonical
                                    // swing penetration + reclaim definition used by
                                    // the main structural engine.
                                    int start =
                                        Math.Max(
                                            5,
                                            index -
                                            Math.Max(
                                                5,
                                                Math.Min(
                                                    LiquidityLookback,
                                                    20)));

                                    for (int i = start;
                                         i < index;
                                         i++)
                                    {
                                        bool swept =
                                            direction == 1
                                                ? BullLiquiditySweep(
                                                    bars,
                                                    i,
                                                    atr)
                                                : BearLiquiditySweep(
                                                    bars,
                                                    i,
                                                    atr);

                                        if (swept)
                                            return true;
                                    }

                                    return false;
                                }

        private bool HasOrderBlockFvgConfluence(
                                    Bars bars,
                                    int startIndex,
                                    int endIndex,
                                    int direction,
                                    double atr,
                                    double zoneLow,
                                    double zoneHigh)
                                {
                                    if (!UseFvg ||
                                        bars == null ||
                                        atr <= 0 ||
                                        startIndex >= endIndex)
                                        return false;
                        
                                    int first =
                                        Math.Max(
                                            2,
                                            startIndex + 1);
                        
                                    for (int i = first;
                                         i <= endIndex;
                                         i++)
                                    {
                                        double creationAtr =
                                            Atr(
                                                bars,
                                                i);

                                        if (creationAtr <= 0)
                                            continue;

                                        if (FvgRule.TryGetThreeBarGap(
                                                direction,
                                                bars.HighPrices[i - 2],
                                                bars.LowPrices[i - 2],
                                                bars.HighPrices[i],
                                                bars.LowPrices[i],
                                                out double low,
                                                out double high,
                                                out double gap) &&
                                            FvgRule.MeetsMinimumGap(
                                                gap,
                                                creationAtr,
                                                MinimumFvgAtr))
                                        {
                                            Zone managed =
                                                BuildManagedFvgZone(
                                                    bars,
                                                    i,
                                                    endIndex,
                                                    direction,
                                                    low,
                                                    high,
                                                    gap,
                                                    false,
                                                    creationAtr);

                                            if (managed != null &&
                                                FvgRule.IsOverlapInclusive(
                                                    managed.Low,
                                                    managed.High,
                                                    zoneLow - atr * 0.05,
                                                    zoneHigh + atr * 0.05))
                                                return true;
                                        }

                                        if (UseTwoBarImbalanceFvg &&
                                            i >= startIndex + 1 &&
                                            FvgRule.TryGetTwoBarGap(
                                                direction,
                                                bars.HighPrices[i - 1],
                                                bars.LowPrices[i - 1],
                                                bars.HighPrices[i],
                                                bars.LowPrices[i],
                                                out low,
                                                out high,
                                                out gap) &&
                                            FvgRule.MeetsMinimumGap(
                                                gap,
                                                creationAtr,
                                                MinimumFvgAtr))
                                        {
                                            Zone managedTwoBar =
                                                BuildManagedFvgZone(
                                                    bars,
                                                    i,
                                                    endIndex,
                                                    direction,
                                                    low,
                                                    high,
                                                    gap,
                                                    true,
                                                    creationAtr);

                                            if (managedTwoBar != null &&
                                                FvgRule.IsOverlapInclusive(
                                                    managedTwoBar.Low,
                                                    managedTwoBar.High,
                                                    zoneLow - atr * 0.05,
                                                    zoneHigh + atr * 0.05))
                                                return true;
                                        }
                                    }
                                    return false;
                                }
    }
}
