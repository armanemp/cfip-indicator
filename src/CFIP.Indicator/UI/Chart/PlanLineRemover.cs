using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

// CFIP Indicator — PlanLineRemover.cs
// Single-responsibility chart plan renderer.


namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RemovePlanLine(
                                    string name)
                                {
                                    Chart.RemoveObject(name);
                                }
    }
}
