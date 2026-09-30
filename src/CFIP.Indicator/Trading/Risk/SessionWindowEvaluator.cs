// CFIP Indicator — SessionWindowEvaluator.cs
// Single-responsibility market risk/suitability module.

using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private bool IsInsideSessionWindow(
            DateTime utc)
        {
            return SessionWindowRule.IsInside(
                utc,
                SessionStartUtc,
                SessionEndUtc);
        }
    }
}
