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

            return _formatter.Build(
                decision,
                decision.BuyShare,
                decision.SellShare);
        }
    }
}
