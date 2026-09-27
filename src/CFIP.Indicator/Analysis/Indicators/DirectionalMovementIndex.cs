// ============================================================================
// CFIP Indicator — DirectionalMovementIndex.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private double DmiBias(Bars bars, int index)
                        {
                            if (bars == null || index < 0 || index >= bars.Count)
                                return 0;
                
                            Native set = GetNative(bars);
                
                            if (set == null ||
                                set.Dms == null ||
                                index >= set.Dms.DIPlus.Count ||
                                index >= set.Dms.DIMinus.Count)
                                return 0;
                
                            double plus = set.Dms.DIPlus[index];
                            double minus = set.Dms.DIMinus[index];
                
                            if (double.IsNaN(plus) ||
                                double.IsInfinity(plus) ||
                                double.IsNaN(minus) ||
                                double.IsInfinity(minus))
                                return 0;
                
                            double total = plus + minus;
                
                            return
                                total <= 0
                                    ? 0
                                    : Clamp(
                                        (plus - minus) / total,
                                        -1,
                                        1);
                        }
    }
}
