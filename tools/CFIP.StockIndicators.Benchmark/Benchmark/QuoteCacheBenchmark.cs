using System;
using System.Collections.Generic;
using System.Diagnostics;
using V2Quote = Skender.Stock.Indicators.Quote;

namespace CfipStockIndicatorsBenchmark;

internal static class QuoteCacheBenchmark
{
    internal static (BenchmarkTiming Rebuild, BenchmarkTiming Incremental)
        Measure(IReadOnlyList<V2Quote> source)
    {
        BenchmarkTiming rebuild =
            MeasureTiming(
                () => RunRebuild(source),
                3,
                12);

        BenchmarkTiming incremental =
            MeasureTiming(
                () => RunIncremental(source),
                3,
                12);

        return (rebuild, incremental);
    }

    private static void RunRebuild(
        IReadOnlyList<V2Quote> source)
    {
        long checksum = 0;

        for (int index = 0;
             index < source.Count;
             index++)
        {
            int firstIndex =
                Math.Max(
                    0,
                    index - 160);

            var quotes =
                new List<V2Quote>(
                    index - firstIndex + 1);

            for (int i = firstIndex;
                 i <= index;
                 i++)
            {
                quotes.Add(Clone(source[i]));
            }

            checksum += quotes.Count;
        }

        GC.KeepAlive(checksum);
    }

    private static void RunIncremental(
        IReadOnlyList<V2Quote> source)
    {
        var quotes =
            new List<V2Quote>(161);

        int firstIndex = 0;
        long checksum = 0;

        for (int index = 0;
             index < source.Count;
             index++)
        {
            quotes.Add(Clone(source[index]));

            while (quotes.Count > 161)
            {
                quotes.RemoveAt(0);
                firstIndex++;
            }

            checksum +=
                firstIndex +
                quotes.Count;
        }

        GC.KeepAlive(checksum);
    }

    private static BenchmarkTiming MeasureTiming(
        Action action,
        int warmupIterations,
        int iterations)
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

    private static V2Quote Clone(V2Quote source)
    {
        return new V2Quote
        {
            Date = source.Date,
            Open = source.Open,
            High = source.High,
            Low = source.Low,
            Close = source.Close,
            Volume = source.Volume
        };
    }
}
