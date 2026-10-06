namespace cAlgo
{
    internal readonly struct DecisionFrameContribution
    {
        public double Bull { get; }
        public double Bear { get; }
        public int Evidence { get; }
        public double Weight { get; }
        public int Direction { get; }
        public bool Eligible { get; }

        public DecisionFrameContribution(
            double bull,
            double bear,
            int evidence)
            : this(
                bull,
                bear,
                evidence,
                0,
                0,
                false)
        {
        }

        public DecisionFrameContribution(
            double bull,
            double bear,
            int evidence,
            double weight,
            int direction,
            bool eligible)
        {
            Bull = bull;
            Bear = bear;
            Evidence = evidence;
            Weight = weight > 0 ? weight : 0;
            Direction = direction > 0 ? 1 : direction < 0 ? -1 : 0;
            Eligible = eligible;
        }
    }
}
