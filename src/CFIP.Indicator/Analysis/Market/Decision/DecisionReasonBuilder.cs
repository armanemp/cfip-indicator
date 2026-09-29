namespace cAlgo
{
    internal sealed class DecisionReasonBuilder
    {
        private readonly DecisionReasonFormatter _formatter =
            new DecisionReasonFormatter();

        public string Build(Decision decision)
        {
            if (decision == null)
                return string.Empty;

            string reason =
                _formatter.Format(
                    decision,
                    decision.BuyShare,
                    decision.SellShare);

            if (!decision.ActionableNow &&
                !string.IsNullOrWhiteSpace(
                    decision.ActionabilityReason))
            {
                reason +=
                    " • " +
                    decision.ActionabilityReason;
            }

            if (decision.SmartQuality > 0)
            {
                reason +=
                    " • IND Q" +
                    (decision.Regime == null || _m5Frame == null
                        ? "?"
                        : _m5Frame.IndicatorConfluenceQuality.ToString()) +
                    " C" +
                    (_m5Frame == null
                        ? "?"
                        : _m5Frame.IndicatorConflict.ToString());
            }

            if (decision.DivergenceQuality >= 70 &&
                !string.Equals(
                    decision.DivergenceType,
                    "NONE",
                    System.StringComparison.OrdinalIgnoreCase))
            {
                reason +=
                    " • DIV " +
                    decision.DivergenceType +
                    " Q" +
                    decision.DivergenceQuality;
            }

            return reason;
        }
    }
}
