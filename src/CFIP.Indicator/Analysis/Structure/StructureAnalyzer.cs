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
            if (bars == null ||
                index < 1)
                return false;

            int plateauStart;
            int plateauEnd;
            double swing;

            if (!TryFindLatestSwingHigh(
                    bars,
                    index,
                    SwingStrength,
                    out plateauStart,
                    out plateauEnd,
                    out swing))
                return false;

            int confirmationIndex =
                plateauEnd +
                Math.Max(
                    1,
                    SwingStrength);

            return StructuralEventRule.IsFreshBreak(
                1,
                confirmationIndex,
                index,
                swing,
                atr,
                StructureBreakAtr,
                i => bars.ClosePrices[i]);
        }

        private bool BearStructure(
            Bars bars,
            int index,
            double atr)
        {
            if (bars == null ||
                index < 1)
                return false;

            int plateauStart;
            int plateauEnd;
            double swing;

            if (!TryFindLatestSwingLow(
                    bars,
                    index,
                    SwingStrength,
                    out plateauStart,
                    out plateauEnd,
                    out swing))
                return false;

            int confirmationIndex =
                plateauEnd +
                Math.Max(
                    1,
                    SwingStrength);

            return StructuralEventRule.IsFreshBreak(
                -1,
                confirmationIndex,
                index,
                swing,
                atr,
                StructureBreakAtr,
                i => bars.ClosePrices[i]);
        }

        private bool BullMss(
            Bars bars,
            int index,
            double atr)
        {
            if (!UseMssChoch ||
                bars == null ||
                index < 2)
                return false;

            int plateauStart;
            int plateauEnd;
            double previous;

            if (!TryFindLatestSwingHigh(
                    bars,
                    index - 1,
                    SwingStrength,
                    out plateauStart,
                    out plateauEnd,
                    out previous))
                return false;

            int confirmationIndex =
                plateauEnd +
                Math.Max(
                    1,
                    SwingStrength);

            return StructuralEventRule.IsFreshBreak(
                1,
                confirmationIndex,
                index,
                previous,
                atr,
                StructureBreakAtr,
                i => bars.ClosePrices[i]);
        }

        private bool BearMss(
            Bars bars,
            int index,
            double atr)
        {
            if (!UseMssChoch ||
                bars == null ||
                index < 2)
                return false;

            int plateauStart;
            int plateauEnd;
            double previous;

            if (!TryFindLatestSwingLow(
                    bars,
                    index - 1,
                    SwingStrength,
                    out plateauStart,
                    out plateauEnd,
                    out previous))
                return false;

            int confirmationIndex =
                plateauEnd +
                Math.Max(
                    1,
                    SwingStrength);

            return StructuralEventRule.IsFreshBreak(
                -1,
                confirmationIndex,
                index,
                previous,
                atr,
                StructureBreakAtr,
                i => bars.ClosePrices[i]);
        }
        
        private bool BullChoch(
                            Bars bars,
                            int index)
                        {
                            if (!UseMssChoch ||
                                bars == null ||
                                index < 12)
                                return false;

                            double atr =
                                Atr(
                                    bars,
                                    index);

                            bool freshBullBreak =
                                BullStructure(
                                    bars,
                                    index,
                                    atr);

                            double previousAtr =
                                Atr(
                                    bars,
                                    index - 1);

                            bool priorBearStructure =
                                BearStructure(
                                    bars,
                                    index - 1,
                                    previousAtr);

                            return
                                StructuralEventRule.IsChangeOfCharacter(
                                    1,
                                    priorBearStructure,
                                    freshBullBreak);
                        }
        
        private bool BearChoch(
                            Bars bars,
                            int index)
                        {
                            if (!UseMssChoch ||
                                bars == null ||
                                index < 12)
                                return false;

                            double atr =
                                Atr(
                                    bars,
                                    index);

                            bool freshBearBreak =
                                BearStructure(
                                    bars,
                                    index,
                                    atr);

                            double previousAtr =
                                Atr(
                                    bars,
                                    index - 1);

                            bool priorBullStructure =
                                BullStructure(
                                    bars,
                                    index - 1,
                                    previousAtr);

                            return
                                StructuralEventRule.IsChangeOfCharacter(
                                    -1,
                                    priorBullStructure,
                                    freshBearBreak);
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
                            if (bars == null ||
                                index < 0 ||
                                index >= bars.Count)
                                return false;

                            return RejectionRule.IsRejection(
                                bars.OpenPrices[index],
                                bars.ClosePrices[index],
                                bars.HighPrices[index],
                                bars.LowPrices[index],
                                direction,
                                Symbol.PipSize);
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
