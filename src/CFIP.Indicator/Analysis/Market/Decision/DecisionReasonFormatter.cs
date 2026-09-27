// CFIP Indicator — DecisionReasonFormatter.cs
// Single-responsibility analysis module.

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
        private string BuildReason(
                            Decision d,
                            int buyShare,
                            int sellShare)
                        {
                            return
                                (d.Direction == 1
                                    ? "BUY"
                                    : "SELL") +
                                " | CONF " +
                                d.Confidence +
                                " | EDGE " +
                                d.Edge +
                                " | SMART " +
                                d.SmartQuality +
                                " | MTF " +
                                d.TimeframeAgreement +
                                " | EVID " +
                                d.IndependentEvidence +
                                " | STRUCT " +
                                d.StructuralConfirmations +
                                " | RETEST " +
                                d.RetestQuality +
                                " | REGIME " +
                                d.Regime +
                                " | " +
                                buyShare +
                                "/" +
                                sellShare +
                                (string.IsNullOrWhiteSpace(d.BlockReason)
                                    ? ""
                                    : " | BLOCK " +
                                      d.BlockReason);
                        }
    }
}
