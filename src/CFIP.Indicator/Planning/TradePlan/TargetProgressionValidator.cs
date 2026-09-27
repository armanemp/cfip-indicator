// CFIP Indicator — TargetProgressionValidator.cs
// Single-responsibility planning/risk module.

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
        private bool IsProgressiveTarget(
                            int direction,
                            double previous,
                            double next)
                        {
                            return TargetProgressionRule.IsValid(
                                direction,
                                previous,
                                next);
                        }
    }
}
