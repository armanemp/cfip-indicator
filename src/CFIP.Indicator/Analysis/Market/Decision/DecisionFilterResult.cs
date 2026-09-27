namespace cAlgo
{
    internal readonly struct DecisionFilterResult
    {
        public bool Allowed { get; }
        public string Reason { get; }

        public DecisionFilterResult(bool allowed, string reason)
        {
            Allowed = allowed;
            Reason = reason ?? string.Empty;
        }
    }
}