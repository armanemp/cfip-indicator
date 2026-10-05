using CFIP.Contracts;

// CFIP Indicator — TradeLabelFormatter.cs
// Single-responsibility execution state module.

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private string NormalizeLabel()
        {
            return CbotIdentity.ManagedLabel;
        }

        private string ManagedExecutionLabel()
        {
            string label;

            return
                ManagedIdentityRule.TryBuildLabel(
                    NormalizeLabel(),
                    InstanceId,
                    out label)
                    ? label
                    : string.Empty;
        }
    }
}
