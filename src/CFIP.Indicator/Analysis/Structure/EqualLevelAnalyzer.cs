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
