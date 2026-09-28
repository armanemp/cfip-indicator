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
                                    if (bars == null ||
                                        index < 5 ||
                                        atr <= 0)
                                        return false;
                        
                                    int start =
                                        Math.Max(
                                            1,
                                            index -
                                            Math.Max(
                                                5,
                                                Math.Min(
                                                    LiquidityLookback,
                                                    20)));
                        
                                    double minimumDepth =
                                        Math.Max(
                                            Symbol.PipSize * 2,
                                            atr *
                                            LiquiditySweepMinimumDepthAtr);
                        
                                    if (direction == 1)
                                    {
                                        double priorLow =
                                            Lowest(
                                                bars,
                                                start,
                                                index - 1);
                        
                                        return
                                            priorLow > 0 &&
                                            priorLow -
                                            bars.LowPrices[index] >=
                                            minimumDepth;
                                    }
                        
                                    double priorHigh =
                                        Highest(
                                            bars,
                                            start,
                                            index - 1);
                        
                                    return
                                        priorHigh > 0 &&
                                        bars.HighPrices[index] -
                                        priorHigh >=
                                        minimumDepth;
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
                                        double gap =
                                            direction == 1
                                                ? bars.LowPrices[i] -
                                                  bars.HighPrices[i - 2]
                                                : bars.LowPrices[i - 2] -
                                                  bars.HighPrices[i];
                        
                                        if (gap <
                                            atr *
                                            MinimumFvgAtr)
                                            continue;
                        
                                        double low =
                                            direction == 1
                                                ? bars.HighPrices[i - 2]
                                                : bars.HighPrices[i];
                        
                                        double high =
                                            direction == 1
                                                ? bars.LowPrices[i]
                                                : bars.LowPrices[i - 2];
                        
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
                                                atr);
                        
                                        if (managed == null)
                                            continue;
                        
                                        if (managed.High >=
                                                zoneLow -
                                                atr * 0.05 &&
                                            managed.Low <=
                                                zoneHigh +
                                                atr * 0.05)
                                            return true;
                        
                                        if (UseTwoBarImbalanceFvg &&
                                            i >= startIndex + 2)
                                        {
                                            double twoBarGap =
                                                direction == 1
                                                    ? bars.LowPrices[i] -
                                                      bars.HighPrices[i - 1]
                                                    : bars.LowPrices[i - 1] -
                                                      bars.HighPrices[i];
                        
                                            if (twoBarGap >=
                                                atr *
                                                MinimumFvgAtr)
                                            {
                                                double twoLow =
                                                    direction == 1
                                                        ? bars.HighPrices[i - 1]
                                                        : bars.HighPrices[i];
                        
                                                double twoHigh =
                                                    direction == 1
                                                        ? bars.LowPrices[i]
                                                        : bars.LowPrices[i - 1];
                        
                                                Zone managedTwoBar =
                                                    BuildManagedFvgZone(
                                                        bars,
                                                        i,
                                                        endIndex,
                                                        direction,
                                                        twoLow,
                                                        twoHigh,
                                                        twoBarGap,
                                                        true,
                                                        atr);
                        
                                                if (managedTwoBar != null &&
                                                    managedTwoBar.High >=
                                                        zoneLow -
                                                        atr * 0.05 &&
                                                    managedTwoBar.Low <=
                                                        zoneHigh +
                                                        atr * 0.05)
                                                    return true;
                                            }
                                        }
                                    }
                        
                                    return false;
                                }
    }
}
