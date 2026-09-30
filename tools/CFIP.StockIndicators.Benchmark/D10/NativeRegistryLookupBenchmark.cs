using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CfipStockIndicatorsBenchmark;

internal readonly record struct NativeRegistryLookupBenchmarkResult(
    double LinearRepeatedMilliseconds,
    double DictionaryRepeatedMilliseconds,
    double LastHitRepeatedMilliseconds,
    double LinearRoundRobinMilliseconds,
    double DictionaryRoundRobinMilliseconds,
    double LastHitRoundRobinMilliseconds);

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

        NativeRegistryCache cache =
            new NativeRegistryCache(linear);

        BenchmarkTiming linearRepeated =
            MeasureTiming(
                () => RunRepeatedLinearLookup(linear),
                WarmupIterations,
                TimingIterations);

        BenchmarkTiming dictionaryRepeated =
            MeasureTiming(
                () => RunRepeatedDictionaryLookup(dictionary),
                WarmupIterations,
                TimingIterations);

        BenchmarkTiming lastHitRepeated =
            MeasureTiming(
                () => RunRepeatedLastHitLookup(cache),
                WarmupIterations,
                TimingIterations);

        BenchmarkTiming linearRoundRobin =
            MeasureTiming(
                () => RunRoundRobinLinearLookup(linear),
                WarmupIterations,
                TimingIterations);

        BenchmarkTiming dictionaryRoundRobin =
            MeasureTiming(
                () => RunRoundRobinDictionaryLookup(dictionary),
                WarmupIterations,
                TimingIterations);

        BenchmarkTiming lastHitRoundRobin =
            MeasureTiming(
                () => RunRoundRobinLastHitLookup(cache),
                WarmupIterations,
                TimingIterations);

        return
            new NativeRegistryLookupBenchmarkResult(
                linearRepeated.MeanMilliseconds,
                dictionaryRepeated.MeanMilliseconds,
                lastHitRepeated.MeanMilliseconds,
                linearRoundRobin.MeanMilliseconds,
                dictionaryRoundRobin.MeanMilliseconds,
                lastHitRoundRobin.MeanMilliseconds);
    }

    internal static string Format(
        NativeRegistryLookupBenchmarkResult result)
    {
        return
            "NATIVE REGISTRY LOOKUP BENCHMARK" +
            Environment.NewLine +
            "Repeated same Bars (hot-path pattern)" +
            Environment.NewLine +
            "Linear List: " +
            result.LinearRepeatedMilliseconds.ToString("F3") +
            " ms/run" +
            Environment.NewLine +
            "Reference Dictionary: " +
            result.DictionaryRepeatedMilliseconds.ToString("F3") +
            " ms/run" +
            Environment.NewLine +
            "Last-hit cache: " +
            result.LastHitRepeatedMilliseconds.ToString("F3") +
            " ms/run" +
            Environment.NewLine +
            "Round-robin Bars (cold pattern)" +
            Environment.NewLine +
            "Linear List: " +
            result.LinearRoundRobinMilliseconds.ToString("F3") +
            " ms/run" +
            Environment.NewLine +
            "Reference Dictionary: " +
            result.DictionaryRoundRobinMilliseconds.ToString("F3") +
            " ms/run" +
            Environment.NewLine +
            "Last-hit cache: " +
            result.LastHitRoundRobinMilliseconds.ToString("F3") +
            " ms/run";
    }

    private static void RunRepeatedLinearLookup(
        List<ReferenceKey> values)
    {
        int checksum = 0;
        ReferenceKey target =
            values[values.Count - 1];

        for (int i = 0; i < LookupCount; i++)
        {
            for (int repeat = 0; repeat < 5; repeat++)
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
        }

        GC.KeepAlive(checksum);
    }

    private static void RunRepeatedDictionaryLookup(
        Dictionary<ReferenceKey, int> values)
    {
        int checksum = 0;
        ReferenceKey target = null;

        foreach (ReferenceKey key in values.Keys)
        {
            target = key;
            break;
        }

        for (int i = 0; i < LookupCount; i++)
        {
            for (int repeat = 0; repeat < 5; repeat++)
            {
                if (values.TryGetValue(
                        target,
                        out int index))
                    checksum += index;
            }
        }

        GC.KeepAlive(checksum);
    }

    private static void RunRepeatedLastHitLookup(
        NativeRegistryCache cache)
    {
        int checksum = 0;
        ReferenceKey target =
            cache.Values[cache.Values.Count - 1];

        for (int i = 0; i < LookupCount; i++)
        {
            for (int repeat = 0; repeat < 5; repeat++)
                checksum += cache.Get(target);
        }

        GC.KeepAlive(checksum);
    }

    private static void RunRoundRobinLinearLookup(
        List<ReferenceKey> values)
    {
        int checksum = 0;

        for (int i = 0; i < LookupCount; i++)
        {
            ReferenceKey target =
                values[i % values.Count];

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

    private static void RunRoundRobinDictionaryLookup(
        Dictionary<ReferenceKey, int> values)
    {
        int checksum = 0;
        ReferenceKey[] keys =
            new ReferenceKey[KeyCount];

        values.Keys.CopyTo(keys, 0);

        for (int i = 0; i < LookupCount; i++)
        {
            ReferenceKey target =
                keys[i % keys.Length];

            if (values.TryGetValue(
                    target,
                    out int index))
                checksum += index;
        }

        GC.KeepAlive(checksum);
    }

    private static void RunRoundRobinLastHitLookup(
        NativeRegistryCache cache)
    {
        int checksum = 0;

        for (int i = 0; i < LookupCount; i++)
        {
            ReferenceKey target =
                cache.Values[i % cache.Values.Count];

            checksum += cache.Get(target);
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

    private sealed class NativeRegistryCache
    {
        internal IReadOnlyList<ReferenceKey> Values { get; }

        private ReferenceKey _lastKey;
        private int _lastIndex = -1;

        internal NativeRegistryCache(
            IReadOnlyList<ReferenceKey> values)
        {
            Values = values;
        }

        internal int Get(
            ReferenceKey key)
        {
            if (ReferenceEquals(_lastKey, key))
                return _lastIndex;

            for (int i = 0; i < Values.Count; i++)
            {
                if (ReferenceEquals(Values[i], key))
                {
                    _lastKey = key;
                    _lastIndex = i;
                    return i;
                }
            }

            return -1;
        }
    }

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
