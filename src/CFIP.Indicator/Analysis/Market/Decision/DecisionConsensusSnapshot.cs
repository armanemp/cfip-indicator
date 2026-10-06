using System;

namespace cAlgo
{
    internal readonly struct DecisionConsensusSnapshot
    {
        public double BuyScore { get; }
        public double SellScore { get; }
        public double NetScore { get; }
        public double TotalScore { get; }
        public int BuyShare { get; }
        public int SellShare { get; }
        public int Direction { get; }
        public int Edge { get; }
        public int DirectionalCoveragePercent { get; }
        public int NeutralCoveragePercent { get; }
        public int VoteConfidence { get; }

        public DecisionConsensusSnapshot(
            int buyShare,
            int sellShare,
            int direction,
            int edge)
            : this(
                buyShare,
                sellShare,
                direction,
                edge,
                0,
                0,
                0,
                0,
                0)
        {
        }

        public DecisionConsensusSnapshot(
            int buyShare,
            int sellShare,
            int direction,
            int edge,
            double buyScore,
            double sellScore,
            double totalScore,
            int directionalCoveragePercent,
            int voteConfidence)
        {
            BuyScore =
                NumericGuards.IsFiniteValue(buyScore)
                    ? Math.Max(0, buyScore)
                    : 0;
            SellScore =
                NumericGuards.IsFiniteValue(sellScore)
                    ? Math.Max(0, sellScore)
                    : 0;
            NetScore =
                BuyScore -
                SellScore;
            TotalScore =
                NumericGuards.IsFiniteValue(totalScore)
                    ? Math.Max(0, totalScore)
                    : BuyScore + SellScore;

            BuyShare = NumericGuards.ClampInt(buyShare, 0, 100);
            SellShare = NumericGuards.ClampInt(sellShare, 0, 100);
            Direction = direction < 0 ? -1 : direction > 0 ? 1 : 0;
            Edge = NumericGuards.ClampInt(edge, 0, 100);
            DirectionalCoveragePercent =
                NumericGuards.ClampInt(
                    directionalCoveragePercent,
                    0,
                    100);
            NeutralCoveragePercent =
                100 -
                DirectionalCoveragePercent;
            VoteConfidence =
                NumericGuards.ClampInt(
                    voteConfidence,
                    0,
                    100);
        }
    }
}
