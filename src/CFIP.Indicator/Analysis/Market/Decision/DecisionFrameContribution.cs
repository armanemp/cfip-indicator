namespace cAlgo
{
    internal readonly struct DecisionFrameContribution
    {
        public int Bull { get; }
        public int Bear { get; }
        public int Evidence { get; }

        public DecisionFrameContribution(
            int bull,
            int bear,
            int evidence)
        {
            Bull = bull;
            Bear = bear;
            Evidence = evidence;
        }
    }
}