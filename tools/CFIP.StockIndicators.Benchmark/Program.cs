using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FacioQuo.Stock.Indicators;

namespace CfipStockIndicatorsBenchmark;

internal static class Program
{
    private const int TimingWarmupIterations = 5;
    private const int TimingIterations = 20;

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var comparisons = new List<ComparisonResult>();

        foreach (string scenario in BenchmarkFixtures.ScenarioNames)
        {
            IReadOnlyList<Skender.Stock.Indicators.Quote> v2Quotes =
                BenchmarkFixtures.BuildV2(scenario);
            IReadOnlyList<Bar> v3Bars =
                BenchmarkFixtures.BuildV3(scenario);

            if (v2Quotes.Count != BenchmarkFixtures.BarCount ||
                v3Bars.Count != BenchmarkFixtures.BarCount)
            {
                throw new InvalidOperationException(
                    $"{scenario}: deterministic fixture count mismatch.");
            }

            IndicatorComparison.ValidateIndicatorCoverage(
                scenario,
                v2Quotes,
                v3Bars);

            comparisons.AddRange(
                IndicatorComparison.Compare(
                    scenario,
                    v2Quotes,
                    v3Bars));
        }

        IReadOnlyList<Skender.Stock.Indicators.Quote> timingV2Quotes =
            BenchmarkFixtures.BuildV2("TREND_UP");
        IReadOnlyList<Bar> timingV3Bars =
            BenchmarkFixtures.BuildV3("TREND_UP");

        BenchmarkTiming v2Timing =
            Measure(
                TimingWarmupIterations,
                TimingIterations,
                () => RunV2Suite(timingV2Quotes));

        BenchmarkTiming v3Timing =
            Measure(
                TimingWarmupIterations,
                TimingIterations,
                () => RunV3Suite(timingV3Bars));

        (BenchmarkTiming rebuildQuotes, BenchmarkTiming incrementalQuotes) =
            QuoteCacheBenchmark.Measure(
                timingV2Quotes);

        QuoteCacheBenchmarkResult quoteCacheTiming =
            new QuoteCacheBenchmarkResult(
                rebuildQuotes,
                incrementalQuotes);

        SkenderWarmupParityBenchmarkResult warmupParity =
            SkenderWarmupParityBenchmark.Measure();

        string report =
            BenchmarkReport.Format(
                comparisons,
                v2Timing,
                v3Timing,
                quoteCacheTiming,
                warmupParity);

        Console.WriteLine(report);

        NativeRegistryLookupBenchmarkResult nativeRegistryTiming =
            NativeRegistryLookupBenchmark.Measure();

        Console.WriteLine();
        Console.WriteLine(
            NativeRegistryLookupBenchmark.Format(
                nativeRegistryTiming));

        bool passed =
            comparisons.All(x => x.Passed) &&
            comparisons.Count ==
                BenchmarkFixtures.ScenarioNames.Count *
                IndicatorComparison.Definitions.Count &&
            warmupParity.Passed;

        Console.WriteLine();
        Console.WriteLine(
            passed
                ? "TRACK 19.1 + CR8.3b + CI-02 COMPLETE: OSS numerical benchmark passed."
                : "OSS numerical benchmark FAILED: a deterministic parity gate failed.");

        return passed ? 0 : 1;
    }

    private static BenchmarkTiming Measure(
        int warmupIterations,
        int iterations,
        Action action)
    {
        for (int i = 0; i < warmupIterations; i++)
            action();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long beforeAllocated =
            GC.GetAllocatedBytesForCurrentThread();

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
            action();

        stopwatch.Stop();

        long allocated =
            GC.GetAllocatedBytesForCurrentThread() -
            beforeAllocated;

        return new BenchmarkTiming(
            stopwatch.Elapsed.TotalMilliseconds,
            stopwatch.Elapsed.TotalMilliseconds / iterations,
            allocated / iterations);
    }

    private static void RunV2Suite(
        IReadOnlyList<Skender.Stock.Indicators.Quote> quotes)
    {
        _ = Skender.Stock.Indicators.Indicator.GetRsi(quotes, 14).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetMacd(quotes, 12, 26, 9).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetBollingerBands(quotes, 20, 2).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetMfi(quotes, 14).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetStoch(quotes, 14, 3, 3).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetSuperTrend(quotes, 10, 3).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetAroon(quotes, 25).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetCci(quotes, 20).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetObv(quotes).ToList();
        _ = Skender.Stock.Indicators.Indicator.GetParabolicSar(quotes, 0.02, 0.20).ToList();
    }

    private static void RunV3Suite(
        IReadOnlyList<Bar> bars)
    {
        _ = bars.ToRsi(14).ToList();
        _ = bars.ToMacd(12, 26, 9).ToList();
        _ = bars.ToBollingerBands(20, 2).ToList();
        _ = bars.ToMfi(14).ToList();
        _ = bars.ToStoch(14, 3, 3).ToList();
        _ = bars.ToSuperTrend(10, 3).ToList();
        _ = bars.ToAroon(25).ToList();
        _ = bars.ToCci(20).ToList();
        _ = bars.ToObv().ToList();
        _ = bars.ToParabolicSar(0.02, 0.20).ToList();
    }
}
