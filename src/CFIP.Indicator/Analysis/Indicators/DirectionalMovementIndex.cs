// ============================================================================
// CFIP Indicator — DirectionalMovementIndex.cs
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
            if (bars == null ||
                index < 0 ||
                index >= bars.Count)
                return 0;

            Native set = GetNative(bars);
            int period =
                Math.Max(
                    2,
                    AdxPeriod);

            if (set == null ||
                set.Dms == null ||
                !NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                    index,
                    set.Dms.DIPlus.Count,
                    period) ||
                !NativeIndicatorReadinessRule.IsIndexedSeriesReady(
                    index,
                    set.Dms.DIMinus.Count,
                    period))
                return 0;

            return DmiBiasRule.Calculate(
                set.Dms.DIPlus[index],
                set.Dms.DIMinus[index]);
        }
    }
}
