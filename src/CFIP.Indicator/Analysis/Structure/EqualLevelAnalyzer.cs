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

            int maxOccurrences =
                Math.Max(
                    2,
                    Math.Min(
                        12,
                        LiquidityLookback));

            List<double> levels =
                new List<double>();

            for (int occurrence = 1;
                 occurrence <= maxOccurrences;
                 occurrence++)
            {
                double high =
                    FindSwingHigh(
                        bars,
                        index,
                        SwingStrength,
                        occurrence);

                if (!IsFinitePositive(high) ||
                    high <= reference)
                    continue;

                // A pair is compared directly to a fixed member. This avoids
                // accepting a transitive A~B~C chain when A is not close to C.
                for (int j = 0;
                     j < levels.Count;
                     j++)
                {
                    if (!SwingPlateauRule.IsWithinAnchor(
                            levels[j],
                            high,
                            tolerance))
                        continue;

                    return Math.Max(
                        levels[j],
                        high);
                }

                levels.Add(high);
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

            int maxOccurrences =
                Math.Max(
                    2,
                    Math.Min(
                        12,
                        LiquidityLookback));

            List<double> levels =
                new List<double>();

            for (int occurrence = 1;
                 occurrence <= maxOccurrences;
                 occurrence++)
            {
                double low =
                    FindSwingLow(
                        bars,
                        index,
                        SwingStrength,
                        occurrence);

                if (!IsFinitePositive(low) ||
                    low >= reference)
                    continue;

                for (int j = 0;
                     j < levels.Count;
                     j++)
                {
                    if (!SwingPlateauRule.IsWithinAnchor(
                            levels[j],
                            low,
                            tolerance))
                        continue;

                    return Math.Min(
                        levels[j],
                        low);
                }

                levels.Add(low);
            }

            return 0;
        }
    }
}
