// ============================================================================
// CFIP Indicator — OrderBlockAnalyzer.cs
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
        

        

    }
    }
}
