// ============================================================================
// CFIP Indicator — AverageTrueRange.cs
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
        private double Atr(Bars bars, int index)
                        {
                            if (bars == null || index < 0 || index >= bars.Count)
                                return 0;
                
                            Native set = GetNative(bars);
                
                            if (set == null ||
                                set.Atr == null ||
                                index >= set.Atr.Result.Count)
                                return 0;
                
                            return SafePositive(set.Atr.Result[index]);
                        }
    }
}
