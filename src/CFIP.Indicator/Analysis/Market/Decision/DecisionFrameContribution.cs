namespace cAlgo
{
    internal readonly struct DecisionFrameContribution
    {
        public double Bull { get; }
        public double Bear { get; }
        public int Evidence { get; }

        public DecisionFrameContribution(
            double bull,
            double bear,
            int evidence)
        {
            Bull = bull;
            Bear = bear;
            Evidence = evidence;
        }
    }
}