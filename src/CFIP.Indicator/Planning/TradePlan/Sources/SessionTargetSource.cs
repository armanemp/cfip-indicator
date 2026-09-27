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
                                
                                            DateTime dayStart =
                                                new DateTime(
                                                    anchor.Year,
                                                    anchor.Month,
                                                    anchor.Day,
                                                    0,
                                                    0,
                                                    0);
                                
                                            bool overnight =
                                                startHour > endHour;
                                
                                            DateTime from;
                                            DateTime to;
                                
                                            if (!overnight)
                                            {
                                                from =
                                                    dayStart.AddHours(startHour);
                                
                                                to =
                                                    dayStart.AddHours(endHour);
                                            }
                                            else if (anchor.Hour < endHour)
                                            {
                                                from =
                                                    dayStart.AddDays(-1)
                                                        .AddHours(startHour);
                                
                                                to =
                                                    dayStart.AddHours(endHour);
                                            }
                                            else
                                            {
                                                from =
                                                    dayStart.AddHours(startHour);
                                
                                                to =
                                                    dayStart.AddDays(1)
                                                        .AddHours(endHour);
                                            }
                                
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
