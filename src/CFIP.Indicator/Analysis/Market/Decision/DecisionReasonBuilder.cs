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
