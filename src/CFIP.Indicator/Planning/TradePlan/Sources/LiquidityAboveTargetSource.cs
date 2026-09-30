using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private List<double> FindLiquidityLevelsAbove(
            Bars bars,
            int index,
            double price,
            double atr)
        {
            List<double> levels =
                new List<double>();

            if (bars == null ||
                index < 10 ||
                !IsFinitePositive(price) ||
                !IsFinitePositive(atr))
                return levels;

            int first =
                Math.Max(
                    SwingStrength,
                    index -
                    LiquidityLookback);

            int last =
                Math.Min(
                    index -
                    SwingStrength,
                    bars.Count -
                    SwingStrength -
                    1);

            for (int i = first;
                 i <= last;
                 i++)
            {
                int plateauStart;
                int plateauEnd;
                double level;

                if (!IsCanonicalSwingHigh(
                        bars,
                        i,
                        index,
                        SwingStrength,
                        out plateauStart,
                        out plateauEnd,
                        out level))
                    continue;

                if (!LiquidityTargetCandidateRule.IsDirectionallyValid(
                        1,
                        price,
                        level))
                    continue;

                int confirmationIndex =
                    plateauEnd +
                    Math.Max(
                        1,
                        SwingStrength);

                if (!LiquiditySweepRule.IsActiveUnbrokenLevel(
                        -1,
                        confirmationIndex,
                        index,
                        level,
                        Symbol.PipSize * 2,
                        x => bars.ClosePrices[x]))
                    continue;

                if (!LiquidityTargetCandidateRule.IsDistinct(
                        level,
                        levels,
                        atr,
                        MinimumTpSpacingAtr))
                    continue;

                levels.Add(level);
            }

            return
                LiquidityTargetCandidateRule.OrderByDistance(
                    1,
                    price,
                    levels);
        }

        private double FindNextLiquidityAbove(
            Bars bars,
            int index,
            double price)
        {
            double atr =
                bars == null
                    ? 0
                    : Atr(
                        bars,
                        Math.Max(
                            0,
                            Math.Min(
                                index,
                                bars.Count - 1)));

            List<double> levels =
                FindLiquidityLevelsAbove(
                    bars,
                    index,
                    price,
                    atr);

            return levels.Count == 0
                ? 0
                : levels[0];
        }
    }
}
