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
        private void GetSessionRange(
                                            Bars bars,
                                            int index,
                                            int startHour,
                                            int endHour,
                                            out double high,
                                            out double low)
                                        {
                                            high = 0;
                                            low = 0;
                                
                                            if (bars == null || index < 5)
                                                return;
                                
                                            DateTime anchor =
                                                bars.OpenTimes[index];
                                            DateTime sessionStartUtc;
                                            DateTime sessionEndUtc;

                                            if (!SessionWindowRule.TryResolveSessionWindow(
                                                    anchor,
                                                    startHour,
                                                    endHour,
                                                    out sessionStartUtc,
                                                    out sessionEndUtc))
                                                return;

                                            DateTime from =
                                                sessionStartUtc;
                                            DateTime to =
                                                sessionEndUtc;

                                            int first = -1;
                                            int last = -1;

                                
                                            for (int i = index;
                                                 i >= Math.Max(0, index - 400);
                                                 i--)
                                            {
                                                DateTime t = bars.OpenTimes[i];
                                
                                                if (t < from)
                                                    break;
                                
                                                if (t < to)
                                                {
                                                    first = i;
                                                    if (last < 0)
                                                        last = i;
                                                }
                                            }
                                
                                            if (first < 0 || last < first)
                                                return;
                                
                                            high =
                                                Highest(
                                                    bars,
                                                    first,
                                                    last);
                                
                                            low =
                                                Lowest(
                                                    bars,
                                                    first,
                                                    last);
                                        }
    }
}
