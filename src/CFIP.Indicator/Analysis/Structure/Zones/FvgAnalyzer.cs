// ============================================================================
// CFIP Indicator — FvgAnalyzer.cs
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
        private bool IsZoneFullyMitigated(
                                    Bars bars,
                                    int createdIndex,
                                    int currentIndex,
                                    int direction,
                                    double low,
                                    double high)
                                {
                                    if (!UseZoneMitigationGuard ||
                                        bars == null ||
                                        createdIndex < 0 ||
                                        currentIndex <= createdIndex)
                                        return false;
                        
                                    int start =
                                        Math.Max(
                                            0,
                                            createdIndex + 1);
                        
                                    int end =
                                        Math.Min(
                                            bars.Count - 1,
                                            currentIndex);
                        
                                    for (int i = start;
                                         i <= end;
                                         i++)
                                    {
                                        if (direction == 1)
                                        {
                                            if (bars.LowPrices[i] <= low)
                                                return true;
                                        }
                                        else if (direction == -1)
                                        {
                                            if (bars.HighPrices[i] >= high)
                                                return true;
                                        }
                                    }
                        
                                    return false;
                                }
        
        private bool HasZoneRetest(
                                    Bars bars,
                                    int createdIndex,
                                    int currentIndex,
                                    double low,
                                    double high)
                                {
                                    if (bars == null ||
                                        createdIndex < 0 ||
                                        currentIndex <= createdIndex)
                                        return false;
                        
                                    int start =
                                        Math.Max(
                                            0,
                                            createdIndex + 1);
                        
                                    int end =
                                        Math.Min(
                                            bars.Count - 1,
                                            currentIndex);
                        
                                    for (int i = start;
                                         i <= end;
                                         i++)
                                    {
                                        bool overlaps =
                                            bars.HighPrices[i] >= low &&
                                            bars.LowPrices[i] <= high;
                        
                                        if (overlaps)
                                            return true;
                                    }
                        
                                    return false;
                                }
        
        private Zone FindNearestFvg(
                                    Bars bars,
                                    int index,
                                    int direction,
                                    double atr,
                                    bool requireCurrentRetest = true,
                                    double selectionPrice = double.NaN)
                                {
                                    if (!UseFvg ||
                                        bars == null ||
                                        index < 2 ||
                                        atr <= 0)
                                        return null;
                        
                                    int first =
                                        Math.Max(
                                            1,
                                            index -
                                            FvgLookback);
                        
                                    double nearestPrice =
                                        IsFinitePositive(selectionPrice)
                                            ? selectionPrice
                                            : bars.ClosePrices[index];
                        
                                    Zone best = null;
                        
                                    for (int i = index;
                                         i >= first;
                                         i--)
                                    {
                                        if (direction == 1)
                                        {
                                            if (i >= 2)
                                            {
                                                double gap =
                                                    bars.LowPrices[i] -
                                                    bars.HighPrices[i - 2];
                        
                                                if (gap >=
                                                    atr *
                                                    MinimumFvgAtr)
                                                {
                                                    best =
                                                        SelectNearestFvg(
                                                            nearestPrice,
                                                            best,
                                                            BuildManagedFvgZone(
                                                                bars,
                                                                i,
                                                                index,
                                                                direction,
                                                                bars.HighPrices[i - 2],
                                                                bars.LowPrices[i],
                                                                gap,
                                                                false,
                                                                atr));
                                                }
                                            }
                        
                                            if (UseTwoBarImbalanceFvg)
                                            {
                                                double gap =
                                                    bars.LowPrices[i] -
                                                    bars.HighPrices[i - 1];
                        
                                                if (gap >=
                                                    atr *
                                                    MinimumFvgAtr)
                                                {
                                                    best =
                                                        SelectNearestFvg(
                                                            nearestPrice,
                                                            best,
                                                            BuildManagedFvgZone(
                                                                bars,
                                                                i,
                                                                index,
                                                                direction,
                                                                bars.HighPrices[i - 1],
                                                                bars.LowPrices[i],
                                                                gap,
                                                                true,
                                                                atr));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (i >= 2)
                                            {
                                                double gap =
                                                    bars.LowPrices[i - 2] -
                                                    bars.HighPrices[i];
                        
                                                if (gap >=
                                                    atr *
                                                    MinimumFvgAtr)
                                                {
                                                    best =
                                                        SelectNearestFvg(
                                                            nearestPrice,
                                                            best,
                                                            BuildManagedFvgZone(
                                                                bars,
                                                                i,
                                                                index,
                                                                direction,
                                                                bars.HighPrices[i],
                                                                bars.LowPrices[i - 2],
                                                                gap,
                                                                false,
                                                                atr));
                                                }
                                            }
                        
                                            if (UseTwoBarImbalanceFvg)
                                            {
                                                double gap =
                                                    bars.LowPrices[i - 1] -
                                                    bars.HighPrices[i];
                        
                                                if (gap >=
                                                    atr *
                                                    MinimumFvgAtr)
                                                {
                                                    best =
                                                        SelectNearestFvg(
                                                            nearestPrice,
                                                            best,
                                                            BuildManagedFvgZone(
                                                                bars,
                                                                i,
                                                                index,
                                                                direction,
                                                                bars.HighPrices[i],
                                                                bars.LowPrices[i - 1],
                                                                gap,
                                                                true,
                                                                atr));
                                                }
                                            }
                                        }
                                    }
                        
                                    if (best == null ||
                                        best.Age >
                                        MaximumZoneAgeBars)
                                        return null;
                        
                                    if (requireCurrentRetest &&
                                        RequireFvgRetest)
                                    {
                                        double price =
                                            bars.ClosePrices[index];
                        
                                        double tolerance =
                                            atr *
                                            (RequireRetestQuality
                                                ? RetestZoneToleranceAtr
                                                : ZoneProximityAtr);
                        
                                        if (price <
                                            best.Low -
                                            tolerance ||
                                            price >
                                            best.High +
                                            tolerance)
                                            return null;
                                    }
                        
                                    return best;
                                }
        
        private Zone BuildManagedFvgZone(
                                    Bars bars,
                                    int createdIndex,
                                    int currentIndex,
                                    int direction,
                                    double low,
                                    double high,
                                    double gap,
                                    bool twoBarImbalance,
                                    double atr)
                                {
                                    if (bars == null ||
                                        createdIndex < 0 ||
                                        currentIndex < createdIndex ||
                                        low >= high)
                                        return null;
                        
                                    double managedLow = low;
                                    double managedHigh = high;
                        
                                    if (EnableFvgPartialMitigation &&
                                        UseZoneMitigationGuard &&
                                        currentIndex > createdIndex)
                                    {
                                        for (int i = createdIndex + 1;
                                             i <= currentIndex;
                                             i++)
                                        {
                                            double breaker =
                                                direction == 1
                                                    ? (FvgBreakByWicks
                                                        ? bars.LowPrices[i]
                                                        : Math.Min(
                                                            bars.OpenPrices[i],
                                                            bars.ClosePrices[i]))
                                                    : (FvgBreakByWicks
                                                        ? bars.HighPrices[i]
                                                        : Math.Max(
                                                            bars.OpenPrices[i],
                                                            bars.ClosePrices[i]));
                        
                                            // FIX (CFIP-BUG-FVG-RESET): a full breach used to reset
                                            // managedLow/managedHigh back to the ZONE'S ORIGINAL,
                                            // pristine size and stop scanning ("soft" mode), which
                                            // made a completely swept FVG look full-size and fresh
                                            // again to every caller (including live target/stop
                                            // selection). A zone price has traded straight through
                                            // has no valid remaining range in either mode — collapse
                                            // it to zero width instead, so the standard
                                            // "managedLow >= managedHigh" guard below excludes it
                                            // exactly like the strict (FvgInvalidateOnFullFill=true)
                                            // path already does. The toggle's only remaining effect
                                            // is whether the scan exits immediately on first breach
                                            // (true) or keeps evaluating for reporting purposes
                                            // before arriving at the same exclusion (false).
                                            if (direction == 1)
                                            {
                                                if (breaker <= managedLow)
                                                {
                                                    if (FvgInvalidateOnFullFill)
                                                        return null;
                        
                                                    managedHigh = managedLow;
                                                    break;
                                                }
                        
                                                if (breaker < managedHigh)
                                                    managedHigh =
                                                        Math.Max(
                                                            managedLow,
                                                            breaker);
                                            }
                                            else
                                            {
                                                if (breaker >= managedHigh)
                                                {
                                                    if (FvgInvalidateOnFullFill)
                                                        return null;
                        
                                                    managedLow = managedHigh;
                                                    break;
                                                }
                        
                                                if (breaker > managedLow)
                                                    managedLow =
                                                        Math.Min(
                                                            managedHigh,
                                                            breaker);
                                            }
                        
                                            if (managedHigh -
                                                managedLow <=
                                                Symbol.TickSize)
                                            {
                                                if (FvgInvalidateOnFullFill)
                                                    return null;
                        
                                                managedHigh = managedLow;
                                                break;
                                            }
                                        }
                                    }
                                    else if (FvgInvalidateOnFullFill &&
                                             IsZoneFullyMitigated(
                                                 bars,
                                                 createdIndex,
                                                 currentIndex,
                                                 direction,
                                                 low,
                                                 high))
                                    {
                                        return null;
                                    }
                        
                                    if (managedLow >= managedHigh)
                                        return null;
                        
                                    double normalizedGap =
                                        gap /
                                        Math.Max(
                                            Symbol.PipSize,
                                            atr);
                        
                                    double remainingRatio =
                                        (managedHigh - managedLow) /
                                        Math.Max(
                                            Symbol.TickSize,
                                            high - low);
                        
                                    int quality =
                                        70 +
                                        (int)Math.Round(
                                            15 *
                                            Math.Max(
                                                0,
                                                normalizedGap));
                        
                                    if (EnableFvgPartialMitigation &&
                                        UseZoneMitigationGuard)
                                    {
                                        quality +=
                                            (int)Math.Round(
                                                10 *
                                                ClampDouble(
                                                    remainingRatio,
                                                    0,
                                                    1));
                                    }
                        
                                    if (twoBarImbalance)
                                        quality -= 3;
                        
                                    Zone z = new Zone();
                        
                                    z.Low = managedLow;
                                    z.High = managedHigh;
                                    z.Direction = direction;
                                    z.Kind =
                                        twoBarImbalance
                                            ? "FVG_2BAR"
                                            : "FVG";
                                    z.Age =
                                        currentIndex -
                                        createdIndex;
                                    z.Quality =
                                        ClampInt(
                                            quality,
                                            0,
                                            100);
                        
                                    return z;
                                }
        
        private Zone SelectNearestFvg(
                                    double price,
                                    Zone best,
                                    Zone candidate)
                                {
                                    if (candidate == null)
                                        return best;
                        
                                    if (best == null)
                                        return candidate;
                        
                                    double candidateDistance =
                                        DistanceToZone(
                                            price,
                                            candidate);
                        
                                    double bestDistance =
                                        DistanceToZone(
                                            price,
                                            best);
                        
                                    if (candidateDistance <
                                        bestDistance -
                                        Symbol.TickSize)
                                        return candidate;
                        
                                    if (Math.Abs(
                                            candidateDistance -
                                            bestDistance) <=
                                        Symbol.TickSize &&
                                        candidate.Quality >
                                        best.Quality)
                                        return candidate;
                        
                                    if (Math.Abs(
                                            candidateDistance -
                                            bestDistance) <=
                                        Symbol.TickSize &&
                                        candidate.Quality ==
                                        best.Quality &&
                                        candidate.Age <
                                        best.Age)
                                        return candidate;
                        
                                    return best;
                                }
    }
}
