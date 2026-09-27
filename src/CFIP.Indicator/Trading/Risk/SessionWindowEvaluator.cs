// CFIP Indicator — SessionWindowEvaluator.cs
// Single-responsibility market risk/suitability module.

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
private bool IsInsideSessionWindow(DateTime utc)
                                {
                                    int start =
                                        ClampInt(SessionStartUtc, 0, 23) * 60;
                                    int end =
                                        ClampInt(SessionEndUtc, 0, 23) * 60;
                                    int now =
                                        utc.Hour * 60 + utc.Minute;
                        
                                    if (start == end)
                                        return true;
                        
                                    return start < end
                                        ? now >= start && now < end
                                        : now >= start || now < end;
                                }
    }
}
