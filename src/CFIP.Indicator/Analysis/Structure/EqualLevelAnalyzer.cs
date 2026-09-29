// CFIP Indicator — EqualLevelAnalyzer.cs
// Single-responsibility structure module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double FindEqualHigh(
            Bars bars,
            int index,
            double reference,
            double atr)
        {
            if (!UseEqualHighLow ||
                bars == null ||
                index < 10 ||
                atr <= 0)
                return 0;

            double tolerance =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr *
                    Math.Max(
                        0.02,
                        EqualLevelToleranceAtr));

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

            List<double> levels =
                new List<double>();

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
                        out level) ||
                    level <= reference)
                    continue;

                for (int j = 0;
                     j < levels.Count;
                     j++)
                {
                    if (!SwingPlateauRule.IsWithinAnchor(
                            levels[j],
                            level,
                            tolerance))
                        continue;

                    return Math.Max(
                        levels[j],
                        level);
                }

                levels.Add(level);
            }

            return 0;
        }

        private double FindEqualLow(
            Bars bars,
            int index,
            double reference,
            double atr)
        {
            if (!UseEqualHighLow ||
                bars == null ||
                index < 10 ||
                atr <= 0)
                return 0;

            double tolerance =
                Math.Max(
                    Symbol.PipSize * 2,
                    atr *
                    Math.Max(
                        0.02,
                        EqualLevelToleranceAtr));

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

            List<double> levels =
                new List<double>();

            for (int i = first;
                 i <= last;
                 i++)
            {
                int plateauStart;
                int plateauEnd;
                double level;

                if (!IsCanonicalSwingLow(
                        bars,
                        i,
                        index,
                        SwingStrength,
                        out plateauStart,
                        out plateauEnd,
                        out level) ||
                    level >= reference)
                    continue;

                for (int j = 0;
                     j < levels.Count;
                     j++)
                {
                    if (!SwingPlateauRule.IsWithinAnchor(
                            levels[j],
                            level,
                            tolerance))
                        continue;

                    return Math.Min(
                        levels[j],
                        level);
                }

                levels.Add(level);
            }

            return 0;
        }
    }
}
