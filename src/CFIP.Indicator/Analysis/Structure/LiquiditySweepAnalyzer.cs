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

            // Sweep only a causally established, still-active structural
            // low. A level already invalidated by a prior close cannot be
            // reused as fresh liquidity.
            int plateauStart;
            int plateauEnd;
            double prior;

            if (!TryFindLatestSwingLow(
                    bars,
                    index,
                    SwingStrength,
                    out plateauStart,
                    out plateauEnd,
                    out prior))
                return false;

            int confirmationIndex =
                plateauEnd +
                Math.Max(
                    1,
                    SwingStrength);

            if (!LiquiditySweepRule.IsActiveUnbrokenLevel(
                    1,
                    confirmationIndex,
                    index,
                    prior,
                    Symbol.PipSize * 2,
                    i => bars.ClosePrices[i]))
                return false;

            double minimumDepth =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr *
                    LiquiditySweepMinimumDepthAtr);

            double penetration =
                prior -
                bars.LowPrices[index];

            return
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

            int plateauStart;
            int plateauEnd;
            double prior;

            if (!TryFindLatestSwingHigh(
                    bars,
                    index,
                    SwingStrength,
                    out plateauStart,
                    out plateauEnd,
                    out prior))
                return false;

            int confirmationIndex =
                plateauEnd +
                Math.Max(
                    1,
                    SwingStrength);

            if (!LiquiditySweepRule.IsActiveUnbrokenLevel(
                    -1,
                    confirmationIndex,
                    index,
                    prior,
                    Symbol.PipSize * 2,
                    i => bars.ClosePrices[i]))
                return false;

            double minimumDepth =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr *
                    LiquiditySweepMinimumDepthAtr);

            double penetration =
                bars.HighPrices[index] -
                prior;

            return
                penetration >=
                minimumDepth &&
                bars.ClosePrices[index] <
                prior &&
                bars.ClosePrices[index] <
                bars.OpenPrices[index];
        }
    }
}
