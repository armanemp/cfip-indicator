using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using V2Indicator = Skender.Stock.Indicators.Indicator;
using V2Quote = Skender.Stock.Indicators.Quote;

namespace CfipStockIndicatorsBenchmark;

internal readonly record struct SkenderProductionParityBenchmarkResult(
    bool Passed,
    int ComparedPoints,
    int DirectionMismatches,
    int NonFinitePairs,
    int ExactMismatches,
    double MaxStableError,
    double MeanStableError,
    double RootMeanSquareStableError,
    double MaxRollingError,
    BenchmarkTiming FullPrefixTiming,
    BenchmarkTiming BoundedWindowTiming);

internal static class SkenderProductionParityBenchmark
{
    internal const int StableWindowSize = 768;
    internal const int RollingWindowSize = 161;
    internal const int ExtendedBarCount = 2048;

    private static readonly int[] Checkpoints =
        { 768, 1024, 1536, 2047 };

    internal static SkenderProductionParityBenchmarkResult Measure()
    {
        int comparedPoints = 0;
        int directionMismatches = 0;
        int nonFinitePairs = 0;
        int exactMismatches = 0;
        double maxStableError = 0;
        double stableErrorSum = 0;
        double stableErrorSquaredSum = 0;
        double maxRollingError = 0;

        foreach (string scenario in BenchmarkFixtures.ScenarioNames)
        {
            IReadOnlyList<V2Quote> source =
                BenchmarkFixtures.BuildV2(
                    scenario,
                    ExtendedBarCount);

            IReadOnlyList<V2Quote> zeroVolumeVariant =
                BuildZeroVolumeVariant(
                    source);

            CompareScenario(
                scenario,
                source,
                ref comparedPoints,
                ref directionMismatches,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxStableError,
                ref stableErrorSum,
                ref stableErrorSquaredSum,
                ref maxRollingError);

            CompareScenario(
                scenario + "_ZERO_VOLUME",
                zeroVolumeVariant,
                ref comparedPoints,
                ref directionMismatches,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxStableError,
                ref stableErrorSum,
                ref stableErrorSquaredSum,
                ref maxRollingError);
        }

        BenchmarkTiming fullPrefixTiming =
            MeasureTiming(
                false);

        BenchmarkTiming boundedWindowTiming =
            MeasureTiming(
                true);

        double meanStableError =
            comparedPoints == 0
                ? double.NaN
                : stableErrorSum / comparedPoints;

        double rmsStableError =
            comparedPoints == 0
                ? double.NaN
                : Math.Sqrt(
                    stableErrorSquaredSum /
                    comparedPoints);

        bool passed =
            comparedPoints > 0 &&
            directionMismatches == 0 &&
            nonFinitePairs == 0 &&
            exactMismatches == 0 &&
            double.IsFinite(maxStableError) &&
            double.IsFinite(meanStableError) &&
            double.IsFinite(rmsStableError) &&
            double.IsFinite(maxRollingError);

        return new SkenderProductionParityBenchmarkResult(
            passed,
            comparedPoints,
            directionMismatches,
            nonFinitePairs,
            exactMismatches,
            maxStableError,
            meanStableError,
            rmsStableError,
            maxRollingError,
            fullPrefixTiming,
            boundedWindowTiming);
    }

    private static void CompareScenario(
        string scenario,
        IReadOnlyList<V2Quote> source,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs,
        ref int exactMismatches,
        ref double maxStableError,
        ref double stableErrorSum,
        ref double stableErrorSquaredSum,
        ref double maxRollingError)
    {
        foreach (int checkpoint in Checkpoints)
        {
            if (checkpoint >= source.Count)
                throw new InvalidOperationException(
                    $"{scenario}: checkpoint exceeds fixture.");

            IReadOnlyList<V2Quote> fullPrefix =
                source
                    .Take(checkpoint + 1)
                    .ToList();

            IReadOnlyList<V2Quote> stable =
                source
                    .Skip(
                        checkpoint -
                        StableWindowSize +
                        1)
                    .Take(StableWindowSize)
                    .ToList();

            IReadOnlyList<V2Quote> rolling =
                source
                    .Skip(
                        checkpoint -
                        RollingWindowSize +
                        1)
                    .Take(RollingWindowSize)
                    .ToList();

            DateTime expectedDate =
                source[checkpoint].Date;

            CompareStableValue(
                scenario,
                "RSI",
                expectedDate,
                EvaluateRsi(fullPrefix),
                EvaluateRsi(stable),
                50,
                ref comparedPoints,
                ref directionMismatches,
                ref nonFinitePairs,
                ref maxStableError,
                ref stableErrorSum,
                ref stableErrorSquaredSum);

            CompareStableValue(
                scenario,
                "MACD Histogram",
                expectedDate,
                EvaluateMacd(fullPrefix),
                EvaluateMacd(stable),
                0,
                ref comparedPoints,
                ref directionMismatches,
                ref nonFinitePairs,
                ref maxStableError,
                ref stableErrorSum,
                ref stableErrorSquaredSum);

            double close =
                (double)source[checkpoint].Close;

            CompareStablePriceRelative(
                scenario,
                "SuperTrend",
                expectedDate,
                close,
                EvaluateSuperTrend(fullPrefix),
                EvaluateSuperTrend(stable),
                ref comparedPoints,
                ref directionMismatches,
                ref nonFinitePairs,
                ref maxStableError,
                ref stableErrorSum,
                ref stableErrorSquaredSum);

            CompareStablePriceRelative(
                scenario,
                "Parabolic SAR",
                expectedDate,
                close,
                EvaluateParabolicSar(fullPrefix),
                EvaluateParabolicSar(stable),
                ref comparedPoints,
                ref directionMismatches,
                ref nonFinitePairs,
                ref maxStableError,
                ref stableErrorSum,
                ref stableErrorSquaredSum);

            CompareRollingValue(
                scenario,
                "Bollinger PercentB",
                expectedDate,
                EvaluateBollinger(fullPrefix, false),
                EvaluateBollinger(rolling, false),
                ref comparedPoints,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxRollingError);

            CompareRollingValue(
                scenario,
                "Bollinger Width",
                expectedDate,
                EvaluateBollinger(fullPrefix, true),
                EvaluateBollinger(rolling, true),
                ref comparedPoints,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxRollingError);

            CompareRollingValue(
                scenario,
                "MFI",
                expectedDate,
                EvaluateMfi(fullPrefix),
                EvaluateMfi(rolling),
                ref comparedPoints,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxRollingError);

            CompareRollingValue(
                scenario,
                "Stochastic K",
                expectedDate,
                EvaluateStoch(fullPrefix, true),
                EvaluateStoch(rolling, true),
                ref comparedPoints,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxRollingError);

            CompareRollingValue(
                scenario,
                "Stochastic D",
                expectedDate,
                EvaluateStoch(fullPrefix, false),
                EvaluateStoch(rolling, false),
                ref comparedPoints,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxRollingError);

            CompareRollingValue(
                scenario,
                "Aroon Oscillator",
                expectedDate,
                EvaluateAroon(fullPrefix),
                EvaluateAroon(rolling),
                ref comparedPoints,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxRollingError);

            CompareRollingValue(
                scenario,
                "CCI",
                expectedDate,
                EvaluateCci(fullPrefix),
                EvaluateCci(rolling),
                ref comparedPoints,
                ref nonFinitePairs,
                ref exactMismatches,
                ref maxRollingError);

            CompareObvDirection(
                scenario,
                expectedDate,
                EvaluateObvDirection(fullPrefix),
                EvaluateObvDirection(rolling),
                ref comparedPoints,
                ref directionMismatches,
                ref nonFinitePairs);
        }
    }

    private static void CompareStableValue(
        string scenario,
        string metric,
        DateTime expectedDate,
        double fullValue,
        double boundedValue,
        double neutral,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs,
        ref double maxError,
        ref double errorSum,
        ref double errorSquaredSum)
    {
        if (!double.IsFinite(fullValue) ||
            !double.IsFinite(boundedValue))
        {
            nonFinitePairs++;
            return;
        }

        comparedPoints++;

        int fullDirection =
            Math.Sign(
                fullValue -
                neutral);

        int boundedDirection =
            Math.Sign(
                boundedValue -
                neutral);

        if (fullDirection != boundedDirection)
            directionMismatches++;

        double error =
            Math.Abs(
                fullValue -
                boundedValue);

        maxError =
            Math.Max(
                maxError,
                error);

        errorSum += error;
        errorSquaredSum += error * error;
    }

    private static void CompareStablePriceRelative(
        string scenario,
        string metric,
        DateTime expectedDate,
        double close,
        double fullValue,
        double boundedValue,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs,
        ref double maxError,
        ref double errorSum,
        ref double errorSquaredSum)
    {
        if (!double.IsFinite(fullValue) ||
            !double.IsFinite(boundedValue))
        {
            nonFinitePairs++;
            return;
        }

        comparedPoints++;

        int fullDirection =
            Math.Sign(
                close -
                fullValue);

        int boundedDirection =
            Math.Sign(
                close -
                boundedValue);

        if (fullDirection != boundedDirection)
            directionMismatches++;

        double error =
            Math.Abs(
                fullValue -
                boundedValue);

        maxError =
            Math.Max(
                maxError,
                error);

        errorSum += error;
        errorSquaredSum += error * error;
    }

    private static void CompareRollingValue(
        string scenario,
        string metric,
        DateTime expectedDate,
        IndicatorPoint full,
        IndicatorPoint bounded,
        ref int comparedPoints,
        ref int nonFinitePairs,
        ref int exactMismatches,
        ref double maxError)
    {
        if (full.Timestamp != expectedDate ||
            bounded.Timestamp != expectedDate ||
            !full.Value.HasValue ||
            !bounded.Value.HasValue ||
            !double.IsFinite(full.Value.Value) ||
            !double.IsFinite(bounded.Value.Value))
        {
            nonFinitePairs++;
            return;
        }

        comparedPoints++;

        double error =
            Math.Abs(
                full.Value.Value -
                bounded.Value.Value);

        maxError =
            Math.Max(
                maxError,
                error);

        if (error > 1e-12)
            exactMismatches++;
    }

    private static void CompareObvDirection(
        string scenario,
        DateTime expectedDate,
        int fullDirection,
        int boundedDirection,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs)
    {
        if (fullDirection == int.MinValue ||
            boundedDirection == int.MinValue)
        {
            nonFinitePairs++;
            return;
        }

        comparedPoints++;

        if (fullDirection != boundedDirection)
            directionMismatches++;
    }

    private static double EvaluateRsi(
        IReadOnlyList<V2Quote> quotes)
    {
        return V2Indicator
                   .GetRsi(
                       quotes,
                       14)
                   .LastOrDefault()
                   ?.Rsi ??
               double.NaN;
    }

    private static double EvaluateMacd(
        IReadOnlyList<V2Quote> quotes)
    {
        return V2Indicator
                   .GetMacd(
                       quotes,
                       12,
                       26,
                       9)
                   .LastOrDefault()
                   ?.Histogram ??
               double.NaN;
    }

    private static double EvaluateBollinger(
        IReadOnlyList<V2Quote> quotes,
        bool width)
    {
        var last =
            V2Indicator
                .GetBollingerBands(
                    quotes,
                    20,
                    2)
                .LastOrDefault();

        if (last == null)
            return double.NaN;

        return width
            ? last.Width ?? double.NaN
            : last.PercentB ?? double.NaN;
    }

    private static double EvaluateMfi(
        IReadOnlyList<V2Quote> quotes)
    {
        return V2Indicator
                   .GetMfi(
                       quotes,
                       14)
                   .LastOrDefault()
                   ?.Mfi ??
               double.NaN;
    }

    private static double EvaluateStoch(
        IReadOnlyList<V2Quote> quotes,
        bool k)
    {
        var last =
            V2Indicator
                .GetStoch(
                    quotes,
                    14,
                    3,
                    3)
                .LastOrDefault();

        if (last == null)
            return double.NaN;

        return k
            ? last.K ?? double.NaN
            : last.D ?? double.NaN;
    }

    private static double EvaluateSuperTrend(
        IReadOnlyList<V2Quote> quotes)
    {
        var last =
            V2Indicator
                .GetSuperTrend(
                    quotes,
                    10,
                    3)
                .LastOrDefault();

        return last == null || !last.SuperTrend.HasValue
            ? double.NaN
            : (double)last.SuperTrend.Value;
    }

    private static double EvaluateAroon(
        IReadOnlyList<V2Quote> quotes)
    {
        return V2Indicator
                   .GetAroon(
                       quotes,
                       25)
                   .LastOrDefault()
                   ?.Oscillator ??
               double.NaN;
    }

    private static double EvaluateCci(
        IReadOnlyList<V2Quote> quotes)
    {
        return V2Indicator
                   .GetCci(
                       quotes,
                       20)
                   .LastOrDefault()
                   ?.Cci ??
               double.NaN;
    }

    private static double EvaluateObvDirection(
        IReadOnlyList<V2Quote> quotes)
    {
        bool hasCurrent = false;
        double previous = double.NaN;
        double current = double.NaN;

        foreach (var result in V2Indicator.GetObv(quotes))
        {
            previous = current;
            current = result.Obv;
            hasCurrent = true;
        }

        if (!hasCurrent ||
            !double.IsFinite(previous) ||
            !double.IsFinite(current))
            return int.MinValue;

        return Math.Sign(
            current -
            previous);
    }

    private static double EvaluateParabolicSar(
        IReadOnlyList<V2Quote> quotes)
    {
        var last =
            V2Indicator
                .GetParabolicSar(
                    quotes,
                    0.02,
                    0.20)
                .LastOrDefault();

        return last == null || !last.Sar.HasValue
            ? double.NaN
            : (double)last.Sar.Value;
    }

    private static IReadOnlyList<V2Quote> BuildZeroVolumeVariant(
        IReadOnlyList<V2Quote> source)
    {
        var result =
            new List<V2Quote>(
                source.Count);

        for (int i = 0;
             i < source.Count;
             i++)
        {
            V2Quote sourceQuote =
                source[i];

            bool zeroVolume =
                i % 17 == 0 ||
                (i >= 900 && i <= 920);

            result.Add(
                new V2Quote
                {
                    Date = sourceQuote.Date,
                    Open = sourceQuote.Open,
                    High = sourceQuote.High,
                    Low = sourceQuote.Low,
                    Close = sourceQuote.Close,
                    Volume = zeroVolume
                        ? 0m
                        : sourceQuote.Volume
                });
        }

        return result;
    }

    private static BenchmarkTiming MeasureTiming(
        bool bounded)
    {
        const int warmupIterations = 2;
        const int iterations = 4;

        for (int i = 0;
             i < warmupIterations;
             i++)
            RunTimingSuite(
                bounded);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long beforeAllocated =
            GC.GetAllocatedBytesForCurrentThread();

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        for (int i = 0;
             i < iterations;
             i++)
            RunTimingSuite(
                bounded);

        stopwatch.Stop();

        long allocated =
            GC.GetAllocatedBytesForCurrentThread() -
            beforeAllocated;

        return new BenchmarkTiming(
            stopwatch.Elapsed.TotalMilliseconds,
            stopwatch.Elapsed.TotalMilliseconds / iterations,
            allocated / iterations);
    }

    private static void RunTimingSuite(
        bool bounded)
    {
        IReadOnlyList<V2Quote> source =
            BenchmarkFixtures.BuildV2(
                "TREND_UP",
                ExtendedBarCount);

        foreach (int checkpoint in Checkpoints)
        {
            int windowSize =
                bounded
                    ? RollingWindowSize
                    : checkpoint + 1;

            int firstIndex =
                Math.Max(
                    0,
                    checkpoint -
                    windowSize +
                    1);

            IReadOnlyList<V2Quote> input =
                source
                    .Skip(firstIndex)
                    .Take(windowSize)
                    .ToList();

            if (bounded)
            {
                _ = V2Indicator.GetRsi(
                    input,
                    14)
                    .LastOrDefault();

                _ = V2Indicator.GetMacd(
                    input,
                    12,
                    26,
                    9)
                    .LastOrDefault();

                _ = V2Indicator.GetSuperTrend(
                    input,
                    10,
                    3)
                    .LastOrDefault();

                _ = V2Indicator.GetParabolicSar(
                    input,
                    0.02,
                    0.20)
                    .LastOrDefault();

                _ = V2Indicator.GetBollingerBands(
                    input,
                    20,
                    2)
                    .LastOrDefault();

                _ = V2Indicator.GetMfi(
                    input,
                    14)
                    .LastOrDefault();

                _ = V2Indicator.GetStoch(
                    input,
                    14,
                    3,
                    3)
                    .LastOrDefault();

                _ = V2Indicator.GetAroon(
                    input,
                    25)
                    .LastOrDefault();

                _ = V2Indicator.GetCci(
                    input,
                    20)
                    .LastOrDefault();

                _ = V2Indicator.GetObv(
                    input)
                    .LastOrDefault();
            }
            else
            {
                _ = V2Indicator.GetRsi(input, 14).LastOrDefault();
                _ = V2Indicator.GetMacd(input, 12, 26, 9).LastOrDefault();
                _ = V2Indicator.GetSuperTrend(input, 10, 3).LastOrDefault();
                _ = V2Indicator.GetParabolicSar(input, 0.02, 0.20).LastOrDefault();
                _ = V2Indicator.GetBollingerBands(input, 20, 2).LastOrDefault();
                _ = V2Indicator.GetMfi(input, 14).LastOrDefault();
                _ = V2Indicator.GetStoch(input, 14, 3, 3).LastOrDefault();
                _ = V2Indicator.GetAroon(input, 25).LastOrDefault();
                _ = V2Indicator.GetCci(input, 20).LastOrDefault();
                _ = V2Indicator.GetObv(input).LastOrDefault();
            }
        }
    }
}
