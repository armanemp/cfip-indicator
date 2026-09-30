using System;
using cAlgo.API;

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
            int currentIndex,
            double atr)
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
                    atr);

            return
                OrderBlockQualityRule.Calculate(
                    bodyRatio,
                    impulseRatio,
                    remainingRatio,
                    currentIndex - createdIndex,
                    displacement,
                    structureBreak,
                    liquiditySweep,
                    fvgConfluence,
                    partiallyMitigated);
        }
    }
}
