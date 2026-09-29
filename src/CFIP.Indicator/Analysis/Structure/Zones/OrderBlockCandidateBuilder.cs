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
            double creationAtr =
                Atr(
                    bars,
                    createdIndex);
            if (creationAtr <= 0)
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
            if (!OrderBlockRule.TryGetZone(
                    direction,
                    ObUseBodyForZone,
                    open,
                    close,
                    high,
                    low,
                    out double zoneLow,
                    out double zoneHigh))
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
                    creationAtr,
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
                    out double remainingRatio))
                return null;
            bool liquiditySweep =
                HasOrderBlockLiquiditySweep(
                    bars,
                    createdIndex,
                    direction,
                    creationAtr);
            bool fvgConfluence =
                HasOrderBlockFvgConfluence(
                    bars,
                    createdIndex,
                    impulseEnd,
                    direction,
                    creationAtr,
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
                    currentIndex,
                    creationAtr);
            return new Zone
            {
                Low = managedLow,
                High = managedHigh,
                Direction = direction,
                Kind = "ORDER_BLOCK",
                Id =
                    OrderBlockRule.OrderBlockIdentity(
                        direction,
                        createdIndex,
                        ObUseBodyForZone),
                CreatedIndex =
                    createdIndex,
                Age = currentIndex -
                    createdIndex,
                Quality = quality
            };
        }
    }
}
