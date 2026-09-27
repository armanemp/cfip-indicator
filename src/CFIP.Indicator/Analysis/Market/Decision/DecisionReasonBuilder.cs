namespace cAlgo
{
    internal sealed class DecisionReasonBuilder
    {
        public string Build(Decision decision)
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
                decision.BuyShare +
                "/" +
                decision.SellShare +
                (string.IsNullOrWhiteSpace(
                    decision.BlockReason)
                    ? ""
                    : " | BLOCK " +
                      decision.BlockReason);
        }
    }
}
