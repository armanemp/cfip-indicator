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
                                bool structure =
                                    BullStructure(
                                        bars,
                                        index,
                                        atr);

                                bool mss =
                                    BullMss(
                                        bars,
                                        index,
                                        atr);

                                bool choch =
                                    BullChoch(
                                        bars,
                                        index);

                                result +=
                                    StructuralEvidenceRule.CanonicalEventCount(
                                        structure,
                                        mss,
                                        choch);

                                if (BullDisplacement(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                            }
                            else
                            {
                                bool structure =
                                    BearStructure(
                                        bars,
                                        index,
                                        atr);

                                bool mss =
                                    BearMss(
                                        bars,
                                        index,
                                        atr);

                                bool choch =
                                    BearChoch(
                                        bars,
                                        index);

                                result +=
                                    StructuralEvidenceRule.CanonicalEventCount(
                                        structure,
                                        mss,
                                        choch);

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
