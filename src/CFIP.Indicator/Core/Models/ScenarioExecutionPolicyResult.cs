namespace cAlgo
{
    internal sealed class ScenarioExecutionPolicyResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public ScenarioExecutionPolicyResult(
            bool allowed,
            string reason)
        {
            Allowed = allowed;
            Reason =
                string.IsNullOrWhiteSpace(reason)
                    ? (allowed
                        ? "POLICY ALLOWED"
                        : "POLICY BLOCKED")
                    : reason;
        }
    }
}
