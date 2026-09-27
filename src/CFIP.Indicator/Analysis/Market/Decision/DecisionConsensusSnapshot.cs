namespace cAlgo
{
    internal readonly struct DecisionConsensusSnapshot
    {
        public int BuyShare { get; }
        public int SellShare { get; }
        public int Direction { get; }
        public int Edge { get; }

        public DecisionConsensusSnapshot(
            int buyShare,
            int sellShare,
            int direction,
            int edge)
        {
            BuyShare = NumericGuards.ClampInt(buyShare, 0, 100);
            SellShare = NumericGuards.ClampInt(sellShare, 0, 100);
            Direction = direction < 0 ? -1 : direction > 0 ? 1 : 0;
            Edge = NumericGuards.ClampInt(edge, 0, 100);
        }
    }
}