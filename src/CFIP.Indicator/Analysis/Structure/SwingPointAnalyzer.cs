// CFIP Indicator — SwingPointAnalyzer.cs
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
        private double SwingPlateauTolerance(
            Bars bars,
            int index)
        {
            double atr =
                bars == null ||
                index < 0 ||
                index >= bars.Count
                    ? 0
                    : Atr(bars, index);

            return Math.Max(
                Symbol.PipSize * 2,
                atr > 0
                    ? atr * 0.02
                    : Symbol.PipSize * 2);
        }

        private bool IsCanonicalSwingHigh(
            Bars bars,
            int candidateIndex,
            int closedIndex,
            out int plateauStart,
            out int plateauEnd,
            out double level)
        {
            return SwingPlateauRule.TryGetHighPlateau(
                bars == null ? 0 : bars.Count,
                candidateIndex,
                SwingStrength,
                closedIndex,
                SwingPlateauTolerance(bars, closedIndex),
                i => bars.HighPrices[i],
                out plateauStart,
                out plateauEnd,
                out level);
        }

        private bool IsCanonicalSwingLow(
            Bars bars,
            int candidateIndex,
            int closedIndex,
            out int plateauStart,
            out int plateauEnd,
            out double level)
        {
            return SwingPlateauRule.TryGetLowPlateau(
                bars == null ? 0 : bars.Count,
                candidateIndex,
                SwingStrength,
                closedIndex,
                SwingPlateauTolerance(bars, closedIndex),
                i => bars.LowPrices[i],
                out plateauStart,
                out plateauEnd,
                out level);
        }

        private double FindSwingHigh(
            Bars bars,
            int index,
            int strength,
            int occurrence)
        {
            if (bars == null ||
                index < strength * 2 + 1)
                return 0;

            int first =
                Math.Max(
                    strength,
                    index -
                    StructureLookback);

            int last =
                Math.Min(
                    index -
                    strength,
                    bars.Count -
                    strength -
                    1);

            int found = 0;

            for (int i = last;
                 i >= first;
                 i--)
            {
                int plateauStart;
                int plateauEnd;
                double level;

                if (!IsCanonicalSwingHigh(
                        bars,
                        i,
                        index,
                        out plateauStart,
                        out plateauEnd,
                        out level))
                    continue;

                found++;

                if (found ==
                    Math.Max(
                        1,
                        occurrence))
                    return level;
            }

            return 0;
        }

        private double FindSwingLow(
            Bars bars,
            int index,
            int strength,
            int occurrence)
        {
            if (bars == null ||
                index < strength * 2 + 1)
                return 0;

            int first =
                Math.Max(
                    strength,
                    index -
                    StructureLookback);

            int last =
                Math.Min(
                    index -
                    strength,
                    bars.Count -
                    strength -
                    1);

            int found = 0;

            for (int i = last;
                 i >= first;
                 i--)
            {
                int plateauStart;
                int plateauEnd;
                double level;

                if (!IsCanonicalSwingLow(
                        bars,
                        i,
                        index,
                        out plateauStart,
                        out plateauEnd,
                        out level))
                    continue;

                found++;

                if (found ==
                    Math.Max(
                        1,
                        occurrence))
                    return level;
            }

            return 0;
        }

        private double FindSwingHighAbove(
            Bars bars,
            int index,
            double price)
        {
            if (bars == null ||
                index < 10)
                return 0;

            int first =
                Math.Max(
                    SwingStrength,
                    index -
                    StructureLookback);

            int last =
                Math.Min(
                    index -
                    SwingStrength,
                    bars.Count -
                    SwingStrength -
                    1);

            double best = 0;

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
                        out plateauStart,
                        out plateauEnd,
                        out level))
                    continue;

                if (level > price &&
                    (best == 0 ||
                     level < best))
                    best = level;
            }

            return best;
        }

        private double FindSwingLowBelow(
            Bars bars,
            int index,
            double price)
        {
            if (bars == null ||
                index < 10)
                return 0;

            int first =
                Math.Max(
                    SwingStrength,
                    index -
                    StructureLookback);

            int last =
                Math.Min(
                    index -
                    SwingStrength,
                    bars.Count -
                    SwingStrength -
                    1);

            double best = 0;

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
                        out plateauStart,
                        out plateauEnd,
                        out level))
                    continue;

                if (level < price &&
                    (best == 0 ||
                     level > best))
                    best = level;
            }

            return best;
        }
    }
}
