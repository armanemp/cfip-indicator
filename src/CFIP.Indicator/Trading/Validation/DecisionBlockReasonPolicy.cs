// CFIP Indicator — DecisionBlockReasonPolicy.cs
// Hard decision-block reason classification policy.

using System;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool IsHardDecisionBlockReason(
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return false;

            switch (reason.Trim().ToUpperInvariant())
            {
                case "SESSION":
                case "FRIDAY":
                case "SPREAD":
                case "VOLATILITY GUARD":
                case "REGIME NO-TRADE":
                case "NEWS":
                case "COOLDOWN":
                case "DIRECTION FLIP":
                case "CHOP":
                case "M1 MISALIGNMENT":
                    return true;

                default:
                    return false;
            }
        }
    }
}
