// CFIP Indicator — HtfTimeframeClassifier.cs
// Single-responsibility planning module.

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
private bool IsHtfTimeframe(
                                    string timeframe)
                                {
                                    if (string.IsNullOrWhiteSpace(
                                            timeframe))
                                        return false;
                        
                                    return
                                        timeframe == "M15" ||
                                        timeframe == "M30" ||
                                        timeframe == "H1" ||
                                        timeframe == "H4" ||
                                        timeframe == "D1" ||
                                        timeframe == "W1";
                                }
    }
}
