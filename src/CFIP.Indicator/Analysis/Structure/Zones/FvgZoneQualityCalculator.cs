using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int CalculateFvgQuality(
            double low,
            double high,
            double originalLow,
            double originalHigh,
            double gap,
            double atr,
            bool twoBarImbalance)
        {
            double normalizedGap =
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

            return ClampInt(
                quality,
                0,
                100);
        }
    }
}
