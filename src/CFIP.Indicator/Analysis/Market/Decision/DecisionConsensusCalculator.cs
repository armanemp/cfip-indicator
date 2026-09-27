using System;

namespace cAlgo
{
    internal sealed class DecisionConsensusCalculator
    {
        public DecisionConsensusSnapshot Calculate(
            double buy,
            double sell,
            double temperature,
            int minimumDirectionShare)
        {
            double t = Math.Max(1.0, temperature);
            double centered = (buy - sell) / t;

            double expBuy =
                Math.Exp(NumericGuards.Clamp(centered, -12, 12));
            double expSell =
                Math.Exp(NumericGuards.Clamp(-centered, -12, 12));

            double total =
                Math.Max(1e-9, expBuy + expSell);

            int buyShare =
                NumericGuards.ClampInt(
                    (int)Math.Round(100.0 * expBuy / total),
                    0,
                    100);

            int sellShare = 100 - buyShare;
            int strongest = Math.Max(buyShare, sellShare);

            int direction =
                strongest >= Math.Max(50, minimumDirectionShare)
                    ? (buyShare >= sellShare ? 1 : -1)
                    : 0;

            return new DecisionConsensusSnapshot(
                buyShare,
                sellShare,
                direction,
                Math.Abs(buyShare - sellShare));
        }
    }
}