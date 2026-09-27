// ============================================================================
// CFIP Indicator — StructureAnalyzer.cs
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
        // ============================================================
                
                        private bool BullStructure(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            double swing =
                                FindSwingHigh(
                                    bars,
                                    index,
                                    SwingStrength,
                                    1);
                
                            return
                                IsFinitePositive(swing) &&
                                bars.ClosePrices[index] >
                                swing +
                                atr *
                                StructureBreakAtr;
                        }
        
        private bool BearStructure(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            double swing =
                                FindSwingLow(
                                    bars,
                                    index,
                                    SwingStrength,
                                    1);
                
                            return
                                IsFinitePositive(swing) &&
                                bars.ClosePrices[index] <
                                swing -
                                atr *
                                StructureBreakAtr;
                        }
        
        private bool BullMss(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            double previous =
                                FindSwingHigh(
                                    bars,
                                    index - 1,
                                    SwingStrength,
                                    1);
                
                            return
                                UseMssChoch &&
                                IsFinitePositive(previous) &&
                                bars.ClosePrices[index] >
                                previous +
                                atr *
                                StructureBreakAtr;
                        }
        
        private bool BearMss(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            double previous =
                                FindSwingLow(
                                    bars,
                                    index - 1,
                                    SwingStrength,
                                    1);
                
                            return
                                UseMssChoch &&
                                IsFinitePositive(previous) &&
                                bars.ClosePrices[index] <
                                previous -
                                atr *
                                StructureBreakAtr;
                        }
        
        private bool BullChoch(
                            Bars bars,
                            int index)
                        {
                            if (!UseMssChoch ||
                                index < 12)
                                return false;
                
                            double level =
                                Highest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - 8),
                                    index - 1);
                
                            return
                                bars.ClosePrices[index] >
                                level &&
                                bars.ClosePrices[index - 1] <=
                                level;
                        }
        
        private bool BearChoch(
                            Bars bars,
                            int index)
                        {
                            if (!UseMssChoch ||
                                index < 12)
                                return false;
                
                            double level =
                                Lowest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - 8),
                                    index - 1);
                
                            return
                                bars.ClosePrices[index] <
                                level &&
                                bars.ClosePrices[index - 1] >=
                                level;
                        }
        
        private bool BullDisplacement(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            return
                                UseDisplacement &&
                                bars.ClosePrices[index] >
                                bars.OpenPrices[index] &&
                                body >=
                                atr *
                                DisplacementAtr;
                        }
        
        private bool BearDisplacement(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            return
                                UseDisplacement &&
                                bars.ClosePrices[index] <
                                bars.OpenPrices[index] &&
                                body >=
                                atr *
                                DisplacementAtr;
                        }
        
        private bool Momentum(
                            Bars bars,
                            int index,
                            int direction,
                            double atr)
                        {
                            if (index < 3)
                                return false;
                
                            double move =
                                bars.ClosePrices[index] -
                                bars.ClosePrices[index - 2];
                
                            return direction == 1
                                ? move > atr * 0.15
                                : move < -atr * 0.15;
                        }
        
        private bool Rejection(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            double range =
                                Math.Max(
                                    Symbol.PipSize,
                                    bars.HighPrices[index] -
                                    bars.LowPrices[index]);
                
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            if (direction == 1)
                            {
                                double wick =
                                    Math.Min(
                                        bars.OpenPrices[index],
                                        bars.ClosePrices[index]) -
                                    bars.LowPrices[index];
                
                                return wick > body * 1.25 &&
                                       wick / range > 0.20;
                            }
                
                            double upper =
                                bars.HighPrices[index] -
                                Math.Max(
                                    bars.OpenPrices[index],
                                    bars.ClosePrices[index]);
                
                            return upper > body * 1.25 &&
                                   upper / range > 0.20;
                        }
        
        private bool StableDirection(
                            Bars bars,
                            int index,
                            int direction,
                            int count)
                        {
                            for (int k = 0;
                                 k < Math.Max(
                                     1,
                                     count);
                                 k++)
                            {
                                int i =
                                    index -
                                    k;
                
                                if (i < 15)
                                    return false;
                
                                double fast =
                                    Ema(
                                        bars,
                                        i,
                                        true);
                
                                double slow =
                                    Ema(
                                        bars,
                                        i,
                                        false);
                
                                double rsi =
                                    Rsi(
                                        bars,
                                        i);
                
                                double dmi =
                                    DmiBias(
                                        bars,
                                        i);
                
                                bool bull =
                                    bars.ClosePrices[i] >
                                    fast &&
                                    fast >= slow &&
                                    rsi >= 50 &&
                                    dmi >= 0;
                
                                bool bear =
                                    bars.ClosePrices[i] <
                                    fast &&
                                    fast <= slow &&
                                    rsi <= 50 &&
                                    dmi <= 0;
                
                                if (direction == 1 &&
                                    !bull)
                                    return false;
                
                                if (direction == -1 &&
                                    !bear)
                                    return false;
                            }
                
                            return true;
                        }
    }
}
