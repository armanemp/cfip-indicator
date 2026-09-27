// ============================================================================
// CFIP Indicator — LiquidityAnalyzer.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
// ============================================================================

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
        private bool BullLiquiditySweep(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            if (!UseLiquiditySweep ||
                                bars == null ||
                                index < 5 ||
                                atr <= 0)
                                return false;
                
                            double prior =
                                Lowest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - LiquidityLookback),
                                    index - 1);
                
                            double minimumDepth =
                                Math.Max(
                                    Symbol.PipSize * 2,
                                    atr *
                                    LiquiditySweepMinimumDepthAtr);
                
                            double penetration =
                                prior -
                                bars.LowPrices[index];
                
                            return
                                prior > 0 &&
                                penetration >=
                                minimumDepth &&
                                bars.ClosePrices[index] >
                                prior &&
                                bars.ClosePrices[index] >
                                bars.OpenPrices[index];
                        }
        
        private bool BearLiquiditySweep(
                            Bars bars,
                            int index,
                            double atr)
                        {
                            if (!UseLiquiditySweep ||
                                bars == null ||
                                index < 5 ||
                                atr <= 0)
                                return false;
                
                            double prior =
                                Highest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - LiquidityLookback),
                                    index - 1);
                
                            double minimumDepth =
                                Math.Max(
                                    Symbol.PipSize * 2,
                                    atr *
                                    LiquiditySweepMinimumDepthAtr);
                
                            double penetration =
                                bars.HighPrices[index] -
                                prior;
                
                            return
                                prior > 0 &&
                                penetration >=
                                minimumDepth &&
                                bars.ClosePrices[index] <
                                prior &&
                                bars.ClosePrices[index] <
                                bars.OpenPrices[index];
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
                                    2,
                                    index -
                                    LiquidityLookback);
                
                            Dictionary<long, List<double>> buckets =
                                new Dictionary<long, List<double>>();
                
                            double best = 0;
                
                            for (int i = first;
                                 i < index - 1;
                                 i++)
                            {
                                double high =
                                    bars.HighPrices[i];
                
                                if (!IsFinitePositive(high))
                                    continue;
                
                                long bucket =
                                    (long)Math.Floor(
                                        high /
                                        tolerance);
                
                                for (long b = bucket - 1;
                                     b <= bucket + 1;
                                     b++)
                                {
                                    List<double> values;
                
                                    if (!buckets.TryGetValue(
                                            b,
                                            out values))
                                        continue;
                
                                    for (int j = 0;
                                         j < values.Count;
                                         j++)
                                    {
                                        if (Math.Abs(
                                                high -
                                                values[j]) >
                                            tolerance)
                                            continue;
                
                                        double level =
                                            Math.Max(
                                                high,
                                                values[j]);
                
                                        if (level > reference &&
                                            (best <= 0 ||
                                             level < best))
                                            best = level;
                
                                        break;
                                    }
                                }
                
                                List<double> bucketValues;
                
                                if (!buckets.TryGetValue(
                                        bucket,
                                        out bucketValues))
                                {
                                    bucketValues =
                                        new List<double>();
                
                                    buckets[bucket] =
                                        bucketValues;
                                }
                
                                bucketValues.Add(
                                    high);
                            }
                
                            return best;
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
                                    2,
                                    index -
                                    LiquidityLookback);
                
                            Dictionary<long, List<double>> buckets =
                                new Dictionary<long, List<double>>();
                
                            double best = 0;
                
                            for (int i = first;
                                 i < index - 1;
                                 i++)
                            {
                                double low =
                                    bars.LowPrices[i];
                
                                if (!IsFinitePositive(low))
                                    continue;
                
                                long bucket =
                                    (long)Math.Floor(
                                        low /
                                        tolerance);
                
                                for (long b = bucket - 1;
                                     b <= bucket + 1;
                                     b++)
                                {
                                    List<double> values;
                
                                    if (!buckets.TryGetValue(
                                            b,
                                            out values))
                                        continue;
                
                                    for (int j = 0;
                                         j < values.Count;
                                         j++)
                                    {
                                        if (Math.Abs(
                                                low -
                                                values[j]) >
                                            tolerance)
                                            continue;
                
                                        double level =
                                            Math.Min(
                                                low,
                                                values[j]);
                
                                        if (level < reference &&
                                            (best <= 0 ||
                                             level > best))
                                            best = level;
                
                                        break;
                                    }
                                }
                
                                List<double> bucketValues;
                
                                if (!buckets.TryGetValue(
                                        bucket,
                                        out bucketValues))
                                {
                                    bucketValues =
                                        new List<double>();
                
                                    buckets[bucket] =
                                        bucketValues;
                                }
                
                                bucketValues.Add(
                                    low);
                            }
                
                            return best;
                        }
    }
}
