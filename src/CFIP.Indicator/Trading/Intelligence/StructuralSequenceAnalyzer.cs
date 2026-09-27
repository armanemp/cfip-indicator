// CFIP Indicator — StructuralSequenceAnalyzer.cs
// Single-responsibility intelligence module.

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
        private int StructuralSequence(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null ||
                                index < 8)
                                return 0;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            int result = 0;
                
                            if (direction == 1)
                            {
                                if (BullStructure(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BullMss(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BullDisplacement(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                            }
                            else
                            {
                                if (BearStructure(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BearMss(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BearDisplacement(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                            }
                
                            return result;
                        }
    }
}
