using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int CalculateFvgQuality(
            Bars bars,
            int currentIndex,
            int direction,
            int ageBars,
            double low,
            double high,
            double originalLow,
            double originalHigh,
            double gap,
            double atr,
            bool twoBarImbalance)
        {
            double gapAtrRatio =
                gap /
                Math.Max(
                    Symbol.PipSize,
                    atr);

            double remainingRatio =
                (high - low) /
                Math.Max(
                    Symbol.TickSize,
                    originalHigh -
                    originalLow);

            double displacementAtrRatio = 0;
            bool structuralAlignment = false;
            bool higherTimeframeAlignment = false;

            if (bars != null &&
                atr > 0 &&
                currentIndex >= 0 &&
                currentIndex < bars.Count)
            {
                double body =
                    Math.Abs(
                        bars.ClosePrices[currentIndex] -
                        bars.OpenPrices[currentIndex]);

                bool directionAlignedDisplacement =
                    direction == 1
                        ? bars.ClosePrices[currentIndex] >
                          bars.OpenPrices[currentIndex]
                        : direction == -1 &&
                          bars.ClosePrices[currentIndex] <
                          bars.OpenPrices[currentIndex];

                if (directionAlignedDisplacement)
                {
                    displacementAtrRatio =
                        body /
                        atr;
                }

                if (UseInternalStructure)
                {
                    structuralAlignment =
                        direction == 1
                            ? BullStructure(
                                bars,
                                currentIndex,
                                atr)
                            : direction == -1 &&
                              BearStructure(
                                bars,
                                currentIndex,
                                atr);
                }

                higherTimeframeAlignment =
                    IsFvgHigherTimeframeAligned(
                        bars,
                        direction);
            }

            return FvgQualityRule.Calculate(
                gapAtrRatio,
                remainingRatio,
                ageBars,
                MaximumZoneAgeBars,
                displacementAtrRatio,
                structuralAlignment,
                higherTimeframeAlignment,
                twoBarImbalance);
        }

        private bool IsFvgHigherTimeframeAligned(
            Bars bars,
            int direction)
        {
            Frame higherFrame = null;

            if (ReferenceEquals(bars, _m5Bars))
                higherFrame = _m15Frame;
            else if (ReferenceEquals(bars, _m15Bars))
                higherFrame = _m30Frame;
            else if (ReferenceEquals(bars, _m30Bars))
                higherFrame = _h1Frame;
            else if (ReferenceEquals(bars, _h1Bars))
                higherFrame = _h4Frame;
            else if (ReferenceEquals(bars, _h4Bars))
                higherFrame = _d1Frame;
            else if (ReferenceEquals(bars, _d1Bars))
                higherFrame = _w1Frame;

            return higherFrame != null &&
                   higherFrame.Direction == direction;
        }
    }
}
