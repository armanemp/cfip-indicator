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
                " | VOTE " +
                decision.VoteCoverage +
                " | VCONF " +
                decision.VoteConfidence +
                " | EVID " +
                decision.IndependentEvidence +
                " | STRUCT " +
                decision.StructuralConfirmations +
                " | RETEST " +
                decision.RetestQuality +
                " | TOPDOWN " +
                (string.IsNullOrWhiteSpace(decision.TopDownStage)
                    ? "HTF SEARCH"
                    : decision.TopDownStage) +
                " | HTF " +
                decision.HtfAnchorDirection +
                "/" +
                decision.HtfAlignment +
                " | MID " +
                decision.MidframeDirection +
                "/" +
                decision.MidframeAlignment +
                " | ENTRY " +
                decision.EntryFrameAlignment +
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
