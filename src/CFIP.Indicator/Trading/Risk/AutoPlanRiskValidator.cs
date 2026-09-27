// CFIP Indicator — AutoPlanRiskValidator.cs
// Single-responsibility risk module.

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
private bool IsAutoPlanValid(
                            int direction,
                            double entry,
                            double stop,
                            double target)
                        {
                            return
                                (direction == 1 ||
                                 direction == -1) &&
                                IsFinitePositive(entry) &&
                                IsValidStop(
                                    direction,
                                    entry,
                                    stop) &&
                                IsValidTarget(
                                    direction,
                                    entry,
                                    target);
                        }
    }
}
