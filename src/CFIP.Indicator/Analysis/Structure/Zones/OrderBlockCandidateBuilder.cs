using System;
using cAlgo.API;
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

            if (!TryBuildOrderBlockImpulseEvidence(
                    bars,
                    createdIndex,
                    currentIndex,
                    direction,
                    atr,
                    out int impulseEnd,
                    out bool displacement,
                    out double strongestBody,
                    out bool structureBreak))
                return null;

            if (!TryApplyOrderBlockMitigation(
                    bars,
                    createdIndex,
                    currentIndex,
                    direction,
                    zoneLow,
                    zoneHigh,
                    out double managedLow,
                    out double managedHigh,
                    out bool partiallyMitigated,
                    out double originalWidth,
                    out double remainingRatio))
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

            int quality =
                CalculateOrderBlockQuality(
                    range,
                    body,
                    strongestBody,
                    remainingRatio,
                    displacement,
                    structureBreak,
                    liquiditySweep,
                    fvgConfluence,
                    partiallyMitigated,
                    createdIndex,
                    currentIndex);

            return new Zone
            {
                Low = managedLow,
                High = managedHigh,
                Direction = direction,
                Kind = "ORDER_BLOCK",
                Age = currentIndex -
                    createdIndex,
                Quality = quality
            };
        }
    }
}
