using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CfipStockIndicatorsBenchmark;

internal readonly record struct NativeRegistryLookupBenchmarkResult(
    double LinearMeanMilliseconds,
    double DictionaryMeanMilliseconds,
    double Speedup);

internal static class NativeRegistryLookupBenchmark
{
    private const int LookupCount = 1_000_000;
    private const int WarmupIterations = 3;
    private const int TimingIterations = 10;
    private const int KeyCount = 8;

    internal static NativeRegistryLookupBenchmarkResult Measure()
    {
        var keys = new ReferenceKey[KeyCount];

        for (int i = 0; i < keys.Length; i++)
            keys[i] = new ReferenceKey();

        var linear = new List<ReferenceKey>(keys);

        var dictionary =
            new Dictionary<ReferenceKey, int>(
                ReferenceKeyComparer.Instance);

        for (int i = 0; i < keys.Length; i++)
            dictionary[keys[i]] = i;

        ReferenceKey target =
            keys[keys.Length - 1];

        BenchmarkTiming linearTiming =
            MeasureTiming(
                () => RunLinearLookup(linear, target),
                WarmupIterations,
                TimingIterations);

        BenchmarkTiming dictionaryTiming =
            MeasureTiming(
                () => RunDictionaryLookup(dictionary, target),
                WarmupIterations,
                TimingIterations);

        double speedup =
            dictionaryTiming.MeanMilliseconds <= 0
                ? 0
                : linearTiming.MeanMilliseconds /
                  dictionaryTiming.MeanMilliseconds;

        return
            new NativeRegistryLookupBenchmarkResult(
                linearTiming.MeanMilliseconds,
                dictionaryTiming.MeanMilliseconds,
                speedup);
    }

    internal static string Format(
        NativeRegistryLookupBenchmarkResult result)
    {
        return
            "NATIVE REGISTRY LOOKUP BENCHMARK" +
            Environment.NewLine +
            "Linear List (pre-D10): " +
            result.LinearMeanMilliseconds.ToString("F3") +
            " ms/run" +
            Environment.NewLine +
            "Reference Dictionary (D10): " +
            result.DictionaryMeanMilliseconds.ToString("F3") +
            " ms/run" +
            Environment.NewLine +
            "Lookup speedup: " +
            result.Speedup.ToString("F2") +
            "x";
    }

    private static void RunLinearLookup(
        List<ReferenceKey> values,
        ReferenceKey target)
    {
        int checksum = 0;

        for (int i = 0; i < LookupCount; i++)
        {
            for (int j = 0; j < values.Count; j++)
            {
                if (ReferenceEquals(values[j], target))
                {
                    checksum += j;
                    break;
                }
            }
        }

        GC.KeepAlive(checksum);
    }

    private static void RunDictionaryLookup(
        Dictionary<ReferenceKey, int> values,
        ReferenceKey target)
    {
        int checksum = 0;

        for (int i = 0; i < LookupCount; i++)
        {
            if (values.TryGetValue(target, out int index))
                checksum += index;
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

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
            action();

        stopwatch.Stop();

        return
            new BenchmarkTiming(
                stopwatch.Elapsed.TotalMilliseconds,
                stopwatch.Elapsed.TotalMilliseconds /
                    Math.Max(1, iterations));
    }

    private readonly record struct BenchmarkTiming(
        double TotalMilliseconds,
        double MeanMilliseconds);

    private sealed class ReferenceKey
    {
    }

    private sealed class ReferenceKeyComparer :
        IEqualityComparer<ReferenceKey>
    {
        internal static readonly ReferenceKeyComparer Instance =
            new ReferenceKeyComparer();

        public bool Equals(
            ReferenceKey x,
            ReferenceKey y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(
            ReferenceKey obj)
        {
            return
                RuntimeHelpers.GetHashCode(obj);
        }
    }
}
