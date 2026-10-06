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
                                    return StructuralTimeframeRule.IsHigherThanM5(
                                        timeframe);
                                }
    }
}
