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
            // Consensus is a bounded mathematical boundary. Invalid inputs
            // fail closed to a neutral state instead of allowing NaN/Infinity
            // to leak into direction selection.
            if (!NumericGuards.IsFiniteValue(buy) ||
                !NumericGuards.IsFiniteValue(sell) ||
                !NumericGuards.IsFiniteValue(temperature) ||
                temperature <= 0)
            {
                return new DecisionConsensusSnapshot(
                    50,
                    50,
                    0,
                    0);
            }

            double t = Math.Max(1.0, temperature);
            double centered = (buy - sell) / t;

            double expBuy =
                Math.Exp(
                    NumericGuards.ClampDouble(
                        centered,
                        -12,
                        12));

            double expSell =
                Math.Exp(
                    NumericGuards.ClampDouble(
                        -centered,
                        -12,
                        12));

            double total =
                Math.Max(1e-9, expBuy + expSell);

            int buyShare =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        100.0 * expBuy / total),
                    0,
                    100);

            int sellShare = 100 - buyShare;
            int strongest = Math.Max(buyShare, sellShare);
            int directionThreshold =
                Math.Max(
                    50,
                    minimumDirectionShare);

            // Exact ties are neutral. A >= comparison here would create a
            // deterministic BUY bias and break the BUY/SELL mirror contract.
            int direction =
                strongest >= directionThreshold &&
                buyShare > sellShare
                    ? 1
                    : strongest >= directionThreshold &&
                      sellShare > buyShare
                        ? -1
                        : 0;

            return new DecisionConsensusSnapshot(
                buyShare,
                sellShare,
                direction,
                Math.Abs(buyShare - sellShare));
        }
    }
}
