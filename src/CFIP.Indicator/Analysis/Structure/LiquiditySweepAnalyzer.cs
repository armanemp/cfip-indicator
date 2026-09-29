// CFIP Indicator — LiquiditySweepAnalyzer.cs
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
        private bool BullLiquiditySweep(
            Bars bars,
            int index,
            double atr)
        {
            if (!UseLiquiditySweep ||
                bars == null ||
                index < 5 ||
                atr <= 0)
                return false;

            // Sweep only a causally established structural low. Rolling raw
            // extremes are not treated as structural liquidity identities.
            double prior =
                FindSwingLow(
                    bars,
                    index,
                    SwingStrength,
                    1);

            double minimumDepth =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr *
                    LiquiditySweepMinimumDepthAtr);

            double penetration =
                prior -
                bars.LowPrices[index];

            return
                prior > 0 &&
                penetration >=
                minimumDepth &&
                bars.ClosePrices[index] >
                prior &&
                bars.ClosePrices[index] >
                bars.OpenPrices[index];
        }

        private bool BearLiquiditySweep(
            Bars bars,
            int index,
            double atr)
        {
            if (!UseLiquiditySweep ||
                bars == null ||
                index < 5 ||
                atr <= 0)
                return false;

            double prior =
                FindSwingHigh(
                    bars,
                    index,
                    SwingStrength,
                    1);

            double minimumDepth =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr *
                    LiquiditySweepMinimumDepthAtr);

            double penetration =
                bars.HighPrices[index] -
                prior;

            return
                prior > 0 &&
                penetration >=
                minimumDepth &&
                bars.ClosePrices[index] <
                prior &&
                bars.ClosePrices[index] <
                bars.OpenPrices[index];
        }
    }
}
