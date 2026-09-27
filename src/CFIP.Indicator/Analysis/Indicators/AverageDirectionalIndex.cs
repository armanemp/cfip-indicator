// ============================================================================
// CFIP Indicator — AverageDirectionalIndex.cs
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
        private double Adx(Bars bars, int index)
                        {
                            if (bars == null || index < 0 || index >= bars.Count)
                                return 0;
                
                            Native set = GetNative(bars);
                
                            if (set == null ||
                                set.Dms == null ||
                                index >= set.Dms.ADX.Count)
                                return 0;
                
                            double value = set.Dms.ADX[index];
                
                            return
                                double.IsNaN(value) ||
                                double.IsInfinity(value)
                                    ? 0
                                    : Clamp(value, 0, 100);
                        }
    }
}
