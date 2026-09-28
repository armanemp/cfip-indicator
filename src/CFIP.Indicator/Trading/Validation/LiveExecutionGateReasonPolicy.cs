// CFIP Indicator — LiveExecutionGateReasonPolicy.cs
// Live execution-gate reason classification policy.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool IsLiveExecutionGateReason(
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return false;

            switch (reason.Trim().ToUpperInvariant())
            {
                case "TRIGGER":
                case "M5 TRIGGER":
                case "ENTRY LOCATION":
                    return true;

                default:
                    return false;
            }
        }
    }
}
