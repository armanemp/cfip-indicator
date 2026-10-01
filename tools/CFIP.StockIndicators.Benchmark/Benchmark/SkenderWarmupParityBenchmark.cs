using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using V2Quote = Skender.Stock.Indicators.Quote;
using StockIndicator = Skender.Stock.Indicators.Indicator;

namespace CfipStockIndicatorsBenchmark;

internal readonly record struct SkenderWarmupParityBenchmarkResult(
    bool Passed,
    int ComparedPoints,
    int DirectionMismatches,
    int NonFinitePairs,
    double MaxAbsoluteError,
    double MeanAbsoluteError,
    double RootMeanSquareError,
    BenchmarkTiming FullWindowTiming,
    BenchmarkTiming BoundedWindowTiming);

internal static class SkenderWarmupParityBenchmark
{
    internal const int StableQuoteWindowSize = 768;
    internal const int ExtendedBarCount = 2048;

    private static readonly int[] Checkpoints =
        { 768, 1024, 1536, 2047 };

    internal static SkenderWarmupParityBenchmarkResult Measure()
    {
        int comparedPoints = 0;
        int directionMismatches = 0;
        int nonFinitePairs = 0;
        double maxError = 0;
        double sumError = 0;
        double sumSquaredError = 0;

        foreach (string scenario in BenchmarkFixtures.ScenarioNames)
        {
            IReadOnlyList<V2Quote> quotes =
                BenchmarkFixtures.BuildV2(
                    scenario,
                    ExtendedBarCount);

            foreach (int checkpoint in Checkpoints)
            {
                int firstIndex =
                    checkpoint -
                    StableQuoteWindowSize +
                    1;

                IReadOnlyList<V2Quote> fullPrefix =
                    quotes
                        .Take(checkpoint + 1)
                        .ToList();

                IReadOnlyList<V2Quote> bounded =
                    quotes
                        .Skip(firstIndex)
                        .Take(StableQuoteWindowSize)
                        .ToList();

                double close =
                    (double)quotes[checkpoint].Close;

                CompareRsi(
                    fullPrefix,
                    bounded,
                    ref comparedPoints,
                    ref directionMismatches,
                    ref nonFinitePairs,
                    ref maxError,
                    ref sumError,
                    ref sumSquaredError);

                CompareMacd(
                    quotes,
                    bounded,
                    ref comparedPoints,
                    ref directionMismatches,
                    ref nonFinitePairs,
                    ref maxError,
                    ref sumError,
                    ref sumSquaredError);

                ComparePriceRelative(
                    close,
                    EvaluateSuperTrend(quotes),
                    EvaluateSuperTrend(bounded),
                    ref comparedPoints,
                    ref directionMismatches,
                    ref nonFinitePairs,
                    ref maxError,
                    ref sumError,
                    ref sumSquaredError);

                ComparePriceRelative(
                    close,
                    EvaluateParabolicSar(quotes),
                    EvaluateParabolicSar(bounded),
                    ref comparedPoints,
                    ref directionMismatches,
                    ref nonFinitePairs,
                    ref maxError,
                    ref sumError,
                    ref sumSquaredError);
            }
        }

        BenchmarkTiming fullTiming =
            MeasureTiming(false);

        BenchmarkTiming boundedTiming =
            MeasureTiming(true);

        double meanError =
            comparedPoints == 0
                ? double.NaN
                : sumError / comparedPoints;

        double rmsError =
            comparedPoints == 0
                ? double.NaN
                : Math.Sqrt(
                    sumSquaredError /
                    comparedPoints);

        bool passed =
            comparedPoints > 0 &&
            directionMismatches == 0 &&
            nonFinitePairs == 0 &&
            double.IsFinite(maxError) &&
            double.IsFinite(meanError) &&
            double.IsFinite(rmsError);

        return new SkenderWarmupParityBenchmarkResult(
            passed,
            comparedPoints,
            directionMismatches,
            nonFinitePairs,
            maxError,
            meanError,
            rmsError,
            fullTiming,
            boundedTiming);
    }

    private static void CompareRsi(
        IReadOnlyList<V2Quote> full,
        IReadOnlyList<V2Quote> bounded,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs,
        ref double maxError,
        ref double sumError,
        ref double sumSquaredError)
    {
        double fullValue = EvaluateRsi(full);
        double boundedValue = EvaluateRsi(bounded);

        CompareNeutral(
            50,
            fullValue,
            boundedValue,
            ref comparedPoints,
            ref directionMismatches,
            ref nonFinitePairs,
            ref maxError,
            ref sumError,
            ref sumSquaredError);
    }

    private static void CompareMacd(
        IReadOnlyList<V2Quote> full,
        IReadOnlyList<V2Quote> bounded,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs,
        ref double maxError,
        ref double sumError,
        ref double sumSquaredError)
    {
        double fullValue = EvaluateMacd(full);
        double boundedValue = EvaluateMacd(bounded);

        CompareNeutral(
            0,
            fullValue,
            boundedValue,
            ref comparedPoints,
            ref directionMismatches,
            ref nonFinitePairs,
            ref maxError,
            ref sumError,
            ref sumSquaredError);
    }

    private static void ComparePriceRelative(
        double close,
        double fullValue,
        double boundedValue,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs,
        ref double maxError,
        ref double sumError,
        ref double sumSquaredError)
    {
        if (!double.IsFinite(fullValue) ||
            !double.IsFinite(boundedValue))
        {
            nonFinitePairs++;
            return;
        }

        int fullDirection =
            Math.Sign(close - fullValue);
        int boundedDirection =
            Math.Sign(close - boundedValue);

        AddComparison(
            fullValue,
            boundedValue,
            fullDirection,
            boundedDirection,
            ref comparedPoints,
            ref directionMismatches,
            ref maxError,
            ref sumError,
            ref sumSquaredError);
    }

    private static void CompareNeutral(
        double neutral,
        double fullValue,
        double boundedValue,
        ref int comparedPoints,
        ref int directionMismatches,
        ref int nonFinitePairs,
        ref double maxError,
        ref double sumError,
        ref double sumSquaredError)
    {
        if (!double.IsFinite(fullValue) ||
            !double.IsFinite(boundedValue))
        {
            nonFinitePairs++;
            return;
        }

        int fullDirection =
            Math.Sign(fullValue - neutral);
        int boundedDirection =
            Math.Sign(boundedValue - neutral);

        AddComparison(
            fullValue,
            boundedValue,
            fullDirection,
            boundedDirection,
            ref comparedPoints,
            ref directionMismatches,
            ref maxError,
            ref sumError,
            ref sumSquaredError);
    }

    private static void AddComparison(
        double fullValue,
        double boundedValue,
        int fullDirection,
        int boundedDirection,
        ref int comparedPoints,
        ref int directionMismatches,
        ref double maxError,
        ref double sumError,
        ref double sumSquaredError)
    {
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

        sumError += error;
        sumSquaredError += error * error;
        comparedPoints++;
    }

    private static double EvaluateRsi(
        IReadOnlyList<V2Quote> quotes)
    {
        return StockIndicator.GetRsi(
                   quotes,
                   14)
               .LastOrDefault()
               ?.Rsi ??
               double.NaN;
    }

    private static double EvaluateMacd(
        IReadOnlyList<V2Quote> quotes)
    {
        return StockIndicator.GetMacd(
                   quotes,
                   12,
                   26,
                   9)
               .LastOrDefault()
               ?.Histogram ??
               double.NaN;
    }

    private static double EvaluateSuperTrend(
        IReadOnlyList<V2Quote> quotes)
    {
        var last =
            StockIndicator.GetSuperTrend(
                quotes,
                10,
                3.0)
            .LastOrDefault();

        return last == null || !last.SuperTrend.HasValue
            ? double.NaN
            : (double)last.SuperTrend.Value;
    }

    private static double EvaluateParabolicSar(
        IReadOnlyList<V2Quote> quotes)
    {
        var last =
            StockIndicator.GetParabolicSar(
                quotes,
                0.02,
                0.20)
            .LastOrDefault();

        return last == null || !last.Sar.HasValue
            ? double.NaN
            : (double)last.Sar.Value;
    }

    private static BenchmarkTiming MeasureTiming(
        bool bounded)
    {
        const int warmup = 2;
        const int iterations = 6;

        for (int i = 0; i < warmup; i++)
            RunSuite(bounded);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long beforeAllocated =
            GC.GetAllocatedBytesForCurrentThread();

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
            RunSuite(bounded);

        stopwatch.Stop();

        long allocated =
            GC.GetAllocatedBytesForCurrentThread() -
            beforeAllocated;

        return new BenchmarkTiming(
            stopwatch.Elapsed.TotalMilliseconds,
            stopwatch.Elapsed.TotalMilliseconds / iterations,
            allocated / iterations);
    }

    private static void RunSuite(
        bool bounded)
    {
        foreach (string scenario in BenchmarkFixtures.ScenarioNames)
        {
            IReadOnlyList<V2Quote> quotes =
                BenchmarkFixtures.BuildV2(
                    scenario,
                    ExtendedBarCount);

            foreach (int checkpoint in Checkpoints)
            {
                int firstIndex =
                    checkpoint -
                    StableQuoteWindowSize +
                    1;

                IReadOnlyList<V2Quote> input =
                    bounded
                        ? quotes
                            .Skip(firstIndex)
                            .Take(StableQuoteWindowSize)
                            .ToList()
                        : quotes
                            .Take(checkpoint + 1)
                            .ToList();

                _ = EvaluateRsi(input);
                _ = EvaluateMacd(input);
                _ = EvaluateSuperTrend(input);
                _ = EvaluateParabolicSar(input);
            }
        }
    }
}
