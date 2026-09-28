using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int CalculateOrderBlockQuality(
            double range,
            double body,
            double strongestBody,
            double remainingRatio,
            bool displacement,
            bool structureBreak,
            bool liquiditySweep,
            bool fvgConfluence,
            bool partiallyMitigated,
            int createdIndex,
            int currentIndex)
        {
            double bodyRatio =
                body /
                Math.Max(
                    Symbol.TickSize,
                    range);

            double impulseRatio =
                strongestBody /
                Math.Max(
                    Symbol.PipSize,
                    Atr(
                        _m5Bars,
                        currentIndex));

            int quality = 54;

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

            return ClampInt(
                quality,
                0,
                100);
        }
    }
}
