using System;

namespace cAlgo
{
    internal sealed class DecisionReasonFormatter
    {
        public string Format(Decision decision, int buyShare, int sellShare)
        {
            if (decision == null)
                return string.Empty;

            return
                (decision.Direction == 1
                    ? "BUY"
                    : "SELL") +
                " | CONF " +
                decision.Confidence +
                " | EDGE " +
                decision.Edge +
                " | SMART " +
                decision.SmartQuality +
                " | MTF " +
                decision.TimeframeAgreement +
                " | EVID " +
                decision.IndependentEvidence +
                " | STRUCT " +
                decision.StructuralConfirmations +
                " | RETEST " +
                decision.RetestQuality +
                " | REGIME " +
                decision.Regime +
                " | " +
                buyShare +
                "/" +
                sellShare +
                (string.IsNullOrWhiteSpace(decision.BlockReason)
                    ? ""
                    : " | BLOCK " +
                      decision.BlockReason);
        }
    }
}
