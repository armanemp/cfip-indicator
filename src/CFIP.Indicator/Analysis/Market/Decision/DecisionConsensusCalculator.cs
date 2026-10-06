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
            return Calculate(
                buy,
                sell,
                temperature,
                minimumDirectionShare,
                0,
                0);
        }

        public DecisionConsensusSnapshot Calculate(
            double buy,
            double sell,
            double temperature,
            int minimumDirectionShare,
            double eligibleFrameWeight,
            double directionalFrameWeight)
        {
            // Consensus is a bounded mathematical boundary. Invalid inputs
            // fail closed to a neutral state instead of allowing NaN/Infinity
            // to leak into direction selection.
            if (!NumericGuards.IsFiniteValue(buy) ||
                !NumericGuards.IsFiniteValue(sell) ||
                !NumericGuards.IsFiniteValue(temperature) ||
                temperature <= 0)
            {
                return CreateSnapshot(
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    eligibleFrameWeight,
                    directionalFrameWeight);
            }

            double safeBuy =
                Math.Max(0, buy);
            double safeSell =
                Math.Max(0, sell);

            double t =
                Math.Max(
                    1.0,
                    temperature);

            double centered =
                (safeBuy - safeSell) /
                t;

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
                Math.Max(
                    1e-9,
                    expBuy + expSell);

            int buyShare =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        100.0 * expBuy / total),
                    0,
                    100);

            int sellShare =
                100 -
                buyShare;

            int strongest =
                Math.Max(
                    buyShare,
                    sellShare);

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

            int edge =
                Math.Abs(
                    buyShare -
                    sellShare);

            return CreateSnapshot(
                buyShare,
                sellShare,
                direction,
                edge,
                safeBuy,
                safeSell,
                safeBuy + safeSell,
                eligibleFrameWeight,
                directionalFrameWeight);
        }

        private static DecisionConsensusSnapshot CreateSnapshot(
            int buyShare,
            int sellShare,
            int direction,
            int edge,
            double buyScore,
            double sellScore,
            double totalScore,
            double eligibleFrameWeight,
            double directionalFrameWeight)
        {
            double coverage =
                eligibleFrameWeight > 0
                    ? NumericGuards.ClampDouble(
                        100.0 *
                        Math.Max(
                            0,
                            directionalFrameWeight) /
                        eligibleFrameWeight,
                        0,
                        100)
                    : 0;

            int coveragePercent =
                NumericGuards.ClampInt(
                    (int)Math.Round(coverage),
                    0,
                    100);

            int confidence =
                NumericGuards.ClampInt(
                    (int)Math.Round(
                        edge *
                        coveragePercent /
                        100.0),
                    0,
                    100);

            return new DecisionConsensusSnapshot(
                buyShare,
                sellShare,
                direction,
                edge,
                buyScore,
                sellScore,
                totalScore,
                coveragePercent,
                confidence);
        }
    }
}
