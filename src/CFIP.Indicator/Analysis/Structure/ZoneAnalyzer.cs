// ============================================================================
// CFIP Indicator — ZoneAnalyzer.cs
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
        
        private Zone FindNearestOrderBlock(
                            Bars bars,
                            int index,
                            int direction,
                            double atr,
                            double selectionPrice = double.NaN,
                            bool requireHistoricalRetest = true)
                        {
                            if (!UseOrderBlock ||
                                bars == null ||
                                index < 8 ||
                                atr <= 0)
                                return null;
                
                            int first =
                                Math.Max(
                                    2,
                                    index -
                                    ObLookback);
                
                            Zone best = null;
                            double bestScore =
                                double.MinValue;
                
                            double market =
                                IsFinitePositive(selectionPrice)
                                    ? selectionPrice
                                    : bars.ClosePrices[index];
                
                            for (int i = index - 1;
                                 i >= first;
                                 i--)
                            {
                                bool opposite =
                                    direction == 1
                                        ? bars.ClosePrices[i] <
                                          bars.OpenPrices[i]
                                        : bars.ClosePrices[i] >
                                          bars.OpenPrices[i];
                
                                if (!opposite)
                                    continue;
                
                                Zone candidate =
                                    BuildOrderBlockCandidate(
                                        bars,
                                        i,
                                        index,
                                        direction,
                                        atr);
                
                                if (candidate == null ||
                                    candidate.Quality <
                                    Math.Max(
                                        50,
                                        ObMinimumQuality))
                                    continue;
                
                                if (requireHistoricalRetest &&
                                    RequireObRetest &&
                                    !HasZoneRetest(
                                        bars,
                                        i,
                                        index,
                                        candidate.Low,
                                        candidate.High))
                                    continue;
                
                                double distance =
                                    DistanceToZone(
                                        market,
                                        candidate);
                
                                double distancePenalty =
                                    14.0 *
                                    Math.Min(
                                        1.0,
                                        distance /
                                        Math.Max(
                                            Symbol.TickSize,
                                            atr * 3.0));
                
                                double agePenalty =
                                    Math.Min(
                                        12.0,
                                        candidate.Age * 0.35);
                
                                double selectionScore =
                                    candidate.Quality -
                                    distancePenalty -
                                    agePenalty;
                
                                if (candidate.Quality >
                                    (best == null
                                        ? 0
                                        : best.Quality))
                                {
                                    selectionScore += 2.0;
                                }
                
                                if (best == null ||
                                    selectionScore >
                                    bestScore)
                                {
                                    best =
                                        candidate;
                                    bestScore =
                                        selectionScore;
                                }
                            }
                
                            return best;
                        }
        
        private Zone BuildOrderBlockCandidate(
                            Bars bars,
                            int createdIndex,
                            int currentIndex,
                            int direction,
                            double atr)
                        {
                            if (bars == null ||
                                createdIndex < 2 ||
                                currentIndex <= createdIndex ||
                                atr <= 0)
                                return null;
                
                            double open =
                                bars.OpenPrices[
                                    createdIndex];
                
                            double close =
                                bars.ClosePrices[
                                    createdIndex];
                
                            double high =
                                bars.HighPrices[
                                    createdIndex];
                
                            double low =
                                bars.LowPrices[
                                    createdIndex];
                
                            double bodyLow =
                                Math.Min(
                                    open,
                                    close);
                
                            double bodyHigh =
                                Math.Max(
                                    open,
                                    close);
                
                            double zoneLow =
                                ObUseBodyForZone
                                    ? bodyLow
                                    : low;
                
                            double zoneHigh =
                                ObUseBodyForZone
                                    ? bodyHigh
                                    : high;
                
                            if (zoneLow >=
                                zoneHigh)
                                return null;
                
                            double range =
                                high -
                                low;
                
                            double body =
                                Math.Abs(
                                    close -
                                    open);
                
                            if (range <= 0 ||
                                body <= 0)
                                return null;
                
                            bool displacement =
                                false;
                
                            double strongestBody =
                                0;
                
                            int impulseEnd =
                                Math.Min(
                                    currentIndex,
                                    createdIndex +
                                    Math.Max(
                                        2,
                                        ObImpulseBars));
                
                            for (int j =
                                     createdIndex + 1;
                                 j <= impulseEnd;
                                 j++)
                            {
                                double nextBody =
                                    Math.Abs(
                                        bars.ClosePrices[j] -
                                        bars.OpenPrices[j]);
                
                                bool directional =
                                    direction == 1
                                        ? bars.ClosePrices[j] >
                                          bars.OpenPrices[j]
                                        : bars.ClosePrices[j] <
                                          bars.OpenPrices[j];
                
                                if (directional &&
                                    nextBody >=
                                    atr *
                                    ObDisplacementAtr)
                                {
                                    displacement =
                                        true;
                
                                    strongestBody =
                                        Math.Max(
                                            strongestBody,
                                            nextBody);
                                }
                            }
                
                            if (RequireObDisplacement &&
                                !displacement)
                                return null;
                
                            bool structureBreak =
                                false;
                
                            int structureStart =
                                Math.Max(
                                    1,
                                    createdIndex -
                                    Math.Max(
                                        3,
                                        ObStructureLookback));
                
                            if (direction == 1)
                            {
                                double priorHigh =
                                    Highest(
                                        bars,
                                        structureStart,
                                        createdIndex - 1);
                
                                for (int j =
                                         createdIndex + 1;
                                     j <= impulseEnd;
                                     j++)
                                {
                                    if (bars.ClosePrices[j] >
                                        priorHigh +
                                        atr *
                                        StructureBreakAtr)
                                    {
                                        structureBreak =
                                            true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                double priorLow =
                                    Lowest(
                                        bars,
                                        structureStart,
                                        createdIndex - 1);
                
                                for (int j =
                                         createdIndex + 1;
                                     j <= impulseEnd;
                                     j++)
                                {
                                    if (bars.ClosePrices[j] <
                                        priorLow -
                                        atr *
                                        StructureBreakAtr)
                                    {
                                        structureBreak =
                                            true;
                                        break;
                                    }
                                }
                            }
                
                            double managedLow =
                                zoneLow;
                
                            double managedHigh =
                                zoneHigh;
                
                            bool partiallyMitigated =
                                false;
                
                            double originalWidth =
                                Math.Max(
                                    Symbol.TickSize,
                                    zoneHigh -
                                    zoneLow);
                
                            if (UseZoneMitigationGuard)
                            {
                                for (int j =
                                         createdIndex + 1;
                                     j <= currentIndex;
                                     j++)
                                {
                                    double probe =
                                        direction == 1
                                            ? (ObBreakByWicks
                                                ? bars.LowPrices[j]
                                                : Math.Min(
                                                    bars.OpenPrices[j],
                                                    bars.ClosePrices[j]))
                                            : (ObBreakByWicks
                                                ? bars.HighPrices[j]
                                                : Math.Max(
                                                    bars.OpenPrices[j],
                                                    bars.ClosePrices[j]));
                
                                    if (direction == 1)
                                    {
                                        if (probe <=
                                            zoneLow)
                                            return null;
                
                                        if (probe <
                                            managedHigh)
                                        {
                                            managedHigh =
                                                Math.Max(
                                                    managedLow,
                                                    probe);
                                            partiallyMitigated =
                                                true;
                                        }
                                    }
                                    else
                                    {
                                        if (probe >=
                                            zoneHigh)
                                            return null;
                
                                        if (probe >
                                            managedLow)
                                        {
                                            managedLow =
                                                Math.Min(
                                                    managedHigh,
                                                    probe);
                                            partiallyMitigated =
                                                true;
                                        }
                                    }
                
                                    if (managedHigh -
                                        managedLow <=
                                        Symbol.TickSize)
                                        return null;
                                }
                            }
                
                            double remainingRatio =
                                (managedHigh -
                                 managedLow) /
                                originalWidth;
                
                            if (remainingRatio <=
                                0.05)
                                return null;
                
                            bool liquiditySweep =
                                HasOrderBlockLiquiditySweep(
                                    bars,
                                    createdIndex,
                                    direction,
                                    atr);
                
                            bool fvgConfluence =
                                HasOrderBlockFvgConfluence(
                                    bars,
                                    createdIndex,
                                    impulseEnd,
                                    direction,
                                    atr,
                                    managedLow,
                                    managedHigh);
                
                            double bodyRatio =
                                body /
                                Math.Max(
                                    Symbol.TickSize,
                                    range);
                
                            double impulseRatio =
                                strongestBody /
                                Math.Max(
                                    Symbol.PipSize,
                                    atr);
                
                            int quality =
                                54;
                
                            if (displacement)
                                quality += 14;
                
                            if (structureBreak)
                                quality += 13;
                
                            if (liquiditySweep)
                                quality += 8;
                
                            if (fvgConfluence)
                                quality += 8;
                
                            quality +=
                                (int)Math.Round(
                                    8 *
                                    Math.Min(
                                        1.0,
                                        Math.Max(
                                            0,
                                            remainingRatio)));
                
                            if (bodyRatio <=
                                0.25)
                                quality -= 4;
                            else if (bodyRatio >=
                                     0.65)
                                quality += 3;
                
                            if (impulseRatio >=
                                1.50)
                                quality += 4;
                            else if (impulseRatio >=
                                     1.00)
                                quality += 2;
                
                            quality -=
                                Math.Min(
                                    10,
                                    (currentIndex -
                                     createdIndex) /
                                    6);
                
                            if (partiallyMitigated)
                            {
                                quality -=
                                    (int)Math.Round(
                                        10 *
                                        (1.0 -
                                         Math.Min(
                                             1.0,
                                             Math.Max(
                                                 0,
                                                 remainingRatio))));
                            }
                
                            return new Zone
                            {
                                Low = managedLow,
                                High = managedHigh,
                                Direction = direction,
                                Kind = "ORDER_BLOCK",
                                Age = currentIndex -
                                    createdIndex,
                                Quality = ClampInt(
                                    quality,
                                    0,
                                    100)
                            };
                        }
        
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
        
        // Internal FVG engine: standard 3-candle FVG plus optional 2-bar imbalance, with body/wick-aware partial mitigation. No chart objects are created here.
                        private Zone FindNearestFvgForExecution(
                            Bars bars,
                            int index,
                            int direction,
                            double atr,
                            double market)
                        {
                            return FindNearestFvg(
                                bars,
                                index,
                                direction,
                                atr,
                                false,
                                market);
                        }
        
        private Zone FindNearestOrderBlockForExecution(
                            Bars bars,
                            int index,
                            int direction,
                            double atr,
                            double market)
                        {
                            return FindNearestOrderBlock(
                                bars,
                                index,
                                direction,
                                atr,
                                market,
                                false);
                        }
        
        private Zone FindNearestOpposingZone(
                            Bars bars,
                            int index,
                            int direction,
                            double atr)
                        {
                            Zone fvg =
                                FindNearestFvg(
                                    bars,
                                    index,
                                    direction,
                                    atr);
                
                            Zone ob =
                                FindNearestOrderBlock(
                                    bars,
                                    index,
                                    direction,
                                    atr);
                
                            if (fvg == null)
                                return ob;
                
                            if (ob == null)
                                return fvg;
                
                            return
                                ob.Quality >=
                                fvg.Quality
                                    ? ob
                                    : fvg;
                        }
        
        private double DistanceToZone(
                            double price,
                            Zone zone)
                        {
                            if (zone == null)
                                return double.MaxValue;
                
                            if (price < zone.Low)
                                return zone.Low - price;
                
                            if (price > zone.High)
                                return price - zone.High;
                
                            return 0;
                        }
    }
}
