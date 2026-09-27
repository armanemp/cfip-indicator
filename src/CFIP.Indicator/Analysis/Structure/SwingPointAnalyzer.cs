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
                                bool swing = true;
                
                                for (int j = 1;
                                     j <= strength;
                                     j++)
                                {
                                    if (bars.HighPrices[i] <=
                                        bars.HighPrices[i - j] ||
                                        bars.HighPrices[i] <=
                                        bars.HighPrices[i + j])
                                    {
                                        swing = false;
                                        break;
                                    }
                                }
                
                                if (!swing)
                                    continue;
                
                                found++;
                
                                if (found ==
                                    Math.Max(
                                        1,
                                        occurrence))
                                    return bars.HighPrices[i];
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
                                bool swing = true;
                
                                for (int j = 1;
                                     j <= strength;
                                     j++)
                                {
                                    if (bars.LowPrices[i] >=
                                        bars.LowPrices[i - j] ||
                                        bars.LowPrices[i] >=
                                        bars.LowPrices[i + j])
                                    {
                                        swing = false;
                                        break;
                                    }
                                }
                
                                if (!swing)
                                    continue;
                
                                found++;
                
                                if (found ==
                                    Math.Max(
                                        1,
                                        occurrence))
                                    return bars.LowPrices[i];
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
                                bool swing = true;
                
                                for (int j = 1;
                                     j <= SwingStrength;
                                     j++)
                                {
                                    if (bars.HighPrices[i] <=
                                        bars.HighPrices[i - j] ||
                                        bars.HighPrices[i] <=
                                        bars.HighPrices[i + j])
                                    {
                                        swing = false;
                                        break;
                                    }
                                }
                
                                if (swing &&
                                    bars.HighPrices[i] >
                                    price &&
                                    (best == 0 ||
                                     bars.HighPrices[i] <
                                     best))
                                    best =
                                        bars.HighPrices[i];
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
                                bool swing = true;
                
                                for (int j = 1;
                                     j <= SwingStrength;
                                     j++)
                                {
                                    if (bars.LowPrices[i] >=
                                        bars.LowPrices[i - j] ||
                                        bars.LowPrices[i] >=
                                        bars.LowPrices[i + j])
                                    {
                                        swing = false;
                                        break;
                                    }
                                }
                
                                if (swing &&
                                    bars.LowPrices[i] <
                                    price &&
                                    (best == 0 ||
                                     bars.LowPrices[i] >
                                     best))
                                    best =
                                        bars.LowPrices[i];
                            }
                
                            return best;
                        }
    }
}
