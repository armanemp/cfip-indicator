using System;
using System.Collections.Generic;
using System.Linq;
using FacioQuo.Stock.Indicators;
using V2Indicator = Skender.Stock.Indicators.Indicator;
using V2Quote = Skender.Stock.Indicators.Quote;
using V3Bar = FacioQuo.Stock.Indicators.Bar;

namespace CfipStockIndicatorsBenchmark;

internal static class IndicatorComparison
{
    internal static IReadOnlyList<MetricDefinition> Definitions { get; } =
        new[]
        {
            new MetricDefinition(
                "RSI",
                "RSI",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetRsi(quotes, 14)
                    .Select(x => new IndicatorPoint(x.Date, x.Rsi))
                    .ToList(),
                bars => bars
                    .ToRsi(14)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Rsi))
                    .ToList()),

            new MetricDefinition(
                "MACD",
                "Histogram",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetMacd(quotes, 12, 26, 9)
                    .Select(x => new IndicatorPoint(x.Date, x.Histogram))
                    .ToList(),
                bars => bars
                    .ToMacd(12, 26, 9)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Histogram))
                    .ToList()),

            new MetricDefinition(
                "Bollinger Bands",
                "PercentB",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetBollingerBands(quotes, 20, 2)
                    .Select(x => new IndicatorPoint(x.Date, x.PercentB))
                    .ToList(),
                bars => bars
                    .ToBollingerBands(20, 2)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.PercentB))
                    .ToList()),

            new MetricDefinition(
                "Bollinger Bands",
                "Width",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetBollingerBands(quotes, 20, 2)
                    .Select(x => new IndicatorPoint(x.Date, x.Width))
                    .ToList(),
                bars => bars
                    .ToBollingerBands(20, 2)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Width))
                    .ToList()),

            new MetricDefinition(
                "MFI",
                "MFI",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetMfi(quotes, 14)
                    .Select(x => new IndicatorPoint(x.Date, x.Mfi))
                    .ToList(),
                bars => bars
                    .ToMfi(14)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Mfi))
                    .ToList()),

            new MetricDefinition(
                "Stochastic",
                "%K",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetStoch(quotes, 14, 3, 3)
                    .Select(x => new IndicatorPoint(x.Date, x.K))
                    .ToList(),
                bars => bars
                    .ToStoch(14, 3, 3)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.K))
                    .ToList()),

            new MetricDefinition(
                "Stochastic",
                "%D",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetStoch(quotes, 14, 3, 3)
                    .Select(x => new IndicatorPoint(x.Date, x.D))
                    .ToList(),
                bars => bars
                    .ToStoch(14, 3, 3)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.D))
                    .ToList()),

            new MetricDefinition(
                "SuperTrend",
                "SuperTrend",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetSuperTrend(quotes, 10, 3)
                    .Select(x => new IndicatorPoint(
                        x.Date,
                        x.SuperTrend.HasValue
                            ? (double?)Convert.ToDouble(x.SuperTrend.Value)
                            : null))
                    .ToList(),
                bars => bars
                    .ToSuperTrend(10, 3)
                    .Select(x => new IndicatorPoint(
                        x.Timestamp,
                        x.SuperTrend.HasValue
                            ? (double?)Convert.ToDouble(x.SuperTrend.Value)
                            : null))
                    .ToList()),

            new MetricDefinition(
                "Aroon",
                "Oscillator",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetAroon(quotes, 25)
                    .Select(x => new IndicatorPoint(x.Date, x.Oscillator))
                    .ToList(),
                bars => bars
                    .ToAroon(25)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Oscillator))
                    .ToList()),

            new MetricDefinition(
                "CCI",
                "CCI",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetCci(quotes, 20)
                    .Select(x => new IndicatorPoint(x.Date, x.Cci))
                    .ToList(),
                bars => bars
                    .ToCci(20)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Cci))
                    .ToList()),

            new MetricDefinition(
                "OBV",
                "OBV",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetObv(quotes)
                    .Select(x => new IndicatorPoint(x.Date, x.Obv))
                    .ToList(),
                bars => bars
                    .ToObv()
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Obv))
                    .ToList()),

            new MetricDefinition(
                "Parabolic SAR",
                "SAR",
                BenchmarkFixtures.DefaultWarmupBars,
                1e-6,
                quotes => V2Indicator
                    .GetParabolicSar(quotes, 0.02, 0.20)
                    .Select(x => new IndicatorPoint(x.Date, x.Sar))
                    .ToList(),
                bars => bars
                    .ToParabolicSar(0.02, 0.20)
                    .Select(x => new IndicatorPoint(x.Timestamp, x.Sar))
                    .ToList())
        };

    internal static IReadOnlyList<ComparisonResult> Compare(
        string scenario,
        IReadOnlyList<V2Quote> v2Quotes,
        IReadOnlyList<V3Bar> v3Bars)
    {
        var results = new List<ComparisonResult>(Definitions.Count);

        foreach (MetricDefinition definition in Definitions)
        {
            IReadOnlyList<IndicatorPoint> v2 =
                definition.V2Projector(v2Quotes);
            IReadOnlyList<IndicatorPoint> v3 =
                definition.V3Projector(v3Bars);

            results.Add(
                CompareMetric(
                    scenario,
                    definition,
                    v2,
                    v3));
        }

        return results;
    }

    internal static void ValidateIndicatorCoverage(
        string scenario,
        IReadOnlyList<V2Quote> v2Quotes,
        IReadOnlyList<V3Bar> v3Bars)
    {
        foreach (MetricDefinition definition in Definitions)
        {
            IReadOnlyList<IndicatorPoint> v2 =
                definition.V2Projector(v2Quotes);
            IReadOnlyList<IndicatorPoint> v3 =
                definition.V3Projector(v3Bars);

            if (v2.Count != v2Quotes.Count)
            {
                throw new InvalidOperationException(
                    $"{scenario}/{definition.IndicatorFamily}/{definition.MetricName}: " +
                    $"v2 output count {v2.Count} does not match input count {v2Quotes.Count}.");
            }

            if (v3.Count != v3Bars.Count)
            {
                throw new InvalidOperationException(
                    $"{scenario}/{definition.IndicatorFamily}/{definition.MetricName}: " +
                    $"v3 output count {v3.Count} does not match input count {v3Bars.Count}.");
            }

            for (int i = 0; i < v2.Count; i++)
            {
                if (v2[i].Timestamp != v3[i].Timestamp)
                {
                    throw new InvalidOperationException(
                        $"{scenario}/{definition.IndicatorFamily}/{definition.MetricName}: " +
                        $"timestamp mismatch at index {i}: " +
                        $"v2={v2[i].Timestamp:O}, v3={v3[i].Timestamp:O}.");
                }
            }
        }
    }

    private static ComparisonResult CompareMetric(
        string scenario,
        MetricDefinition definition,
        IReadOnlyList<IndicatorPoint> v2,
        IReadOnlyList<IndicatorPoint> v3)
    {
        int inputCount = Math.Min(v2.Count, v3.Count);

        if (v2.Count != v3.Count)
        {
            return new ComparisonResult(
                scenario,
                definition.IndicatorFamily,
                definition.MetricName,
                inputCount,
                definition.WarmupBars,
                0,
                0,
                double.PositiveInfinity,
                double.PositiveInfinity,
                double.PositiveInfinity,
                false,
                $"output-count mismatch: v2={v2.Count}, v3={v3.Count}");
        }

        int start = Math.Min(
            definition.WarmupBars,
            inputCount);

        var absoluteErrors = new List<double>(
            Math.Max(0, inputCount - start));
        string failureReason = string.Empty;

        for (int i = start; i < inputCount; i++)
        {
            if (v2[i].Timestamp != v3[i].Timestamp)
            {
                return new ComparisonResult(
                    scenario,
                    definition.IndicatorFamily,
                    definition.MetricName,
                    inputCount,
                    definition.WarmupBars,
                    inputCount - start,
                    absoluteErrors.Count,
                    double.PositiveInfinity,
                    double.PositiveInfinity,
                    double.PositiveInfinity,
                    false,
                    $"timestamp mismatch at index {i}");
            }

            if (!IsFinite(v2[i].Value) ||
                !IsFinite(v3[i].Value))
            {
                if (failureReason.Length == 0)
                    failureReason =
                        $"non-finite comparable pair at index {i}";

                continue;
            }

            double error =
                Math.Abs(v2[i].Value!.Value - v3[i].Value!.Value);

            absoluteErrors.Add(error);

            if (error > definition.AbsoluteTolerance &&
                failureReason.Length == 0)
            {
                failureReason =
                    $"tolerance exceeded at index {i}: " +
                    $"v2={v2[i].Value.Value:G17}, " +
                    $"v3={v3[i].Value.Value:G17}, " +
                    $"absError={error:G17}, " +
                    $"tolerance={definition.AbsoluteTolerance:G17}";
            }
        }

        int comparedPairs = inputCount - start;
        int validPairs = absoluteErrors.Count;

        double maxError =
            absoluteErrors.Count == 0
                ? double.PositiveInfinity
                : absoluteErrors.Max();

        double meanError =
            absoluteErrors.Count == 0
                ? double.PositiveInfinity
                : absoluteErrors.Average();

        double rmsError =
            absoluteErrors.Count == 0
                ? double.PositiveInfinity
                : Math.Sqrt(
                    absoluteErrors
                        .Select(error => error * error)
                        .Average());

        bool passed =
            comparedPairs > 0 &&
            validPairs == comparedPairs &&
            maxError <= definition.AbsoluteTolerance;

        if (!passed && failureReason.Length == 0)
        {
            failureReason =
                $"valid-pair coverage {validPairs}/{comparedPairs} " +
                "or numerical threshold failed";
        }

        return new ComparisonResult(
            scenario,
            definition.IndicatorFamily,
            definition.MetricName,
            inputCount,
            definition.WarmupBars,
            comparedPairs,
            validPairs,
            maxError,
            meanError,
            rmsError,
            passed,
            failureReason);
    }

    private static bool IsFinite(double? value)
    {
        return value.HasValue &&
               !double.IsNaN(value.Value) &&
               !double.IsInfinity(value.Value);
    }
}
