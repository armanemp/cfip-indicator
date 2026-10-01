using System;
using System.Collections.Generic;
using System.Linq;

namespace CfipStockIndicatorsBenchmark;

internal static class BenchmarkReport
{
    internal static string Format(
        IReadOnlyList<ComparisonResult> comparisons,
        BenchmarkTiming v2Timing,
        BenchmarkTiming v3Timing,
        QuoteCacheBenchmarkResult quoteCacheTiming,
        SkenderWarmupParityBenchmarkResult warmupParity)
    {
        bool passed = comparisons.All(x => x.Passed);
        int scenarioCount =
            comparisons
                .Select(x => x.Scenario)
                .Distinct(StringComparer.Ordinal)
                .Count();
        int metricCount =
            comparisons
                .Select(x => $"{x.IndicatorFamily}|{x.MetricName}")
                .Distinct(StringComparer.Ordinal)
                .Count();
        int indicatorFamilyCount =
            comparisons
                .Select(x => x.IndicatorFamily)
                .Distinct(StringComparer.Ordinal)
                .Count();

        var lines = new List<string>
        {
            "# Track 19.1 - FacioQuo numerical comparison",
            string.Empty,
            $"**Result:** {(passed ? "PASS" : "FAIL")}",
            "**Production:** Skender.Stock.Indicators 2.7.3",
            "**Research:** FacioQuo.Stock.Indicators 3.0.1",
            "**Production boundary:** unchanged; FacioQuo remains research-only.",
            string.Empty,
            $"Scenarios: **{scenarioCount}** - Indicator families: **{indicatorFamilyCount}** - Metrics: **{metricCount}**",
            string.Empty,
            "| Indicator family | Metric | Scenarios | Worst max abs error | Worst mean abs error | Result |",
            "| --- | --- | ---: | ---: | ---: | --- |"
        };

        foreach (var group in comparisons.GroupBy(
                     x => new { x.IndicatorFamily, x.MetricName }))
        {
            double worstMax =
                group.Max(x => x.MaxAbsoluteError);
            double worstMean =
                group.Max(x => x.MeanAbsoluteError);
            bool groupPassed =
                group.All(x => x.Passed);

            lines.Add(
                $"| {group.Key.IndicatorFamily} | {group.Key.MetricName} | " +
                $"{group.Count()} | {FormatError(worstMax)} | " +
                $"{FormatError(worstMean)} | {(groupPassed ? "PASS" : "FAIL")} |");
        }

        var failures =
            comparisons
                .Where(x => !x.Passed)
                .ToList();

        if (failures.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Failing comparisons");
            lines.Add(string.Empty);

            foreach (ComparisonResult failure in failures)
            {
                lines.Add(
                    $"- {failure.Scenario} / {failure.IndicatorFamily} / " +
                    $"{failure.MetricName}: {failure.FailureReason}");
            }
        }

        lines.Add(string.Empty);
        lines.Add("## Batch timing");
        lines.Add(string.Empty);
        lines.Add("| Package | Total ms | Mean ms/iteration | Mean allocated bytes/iteration |");
        lines.Add("| --- | ---: | ---: | ---: |");
        lines.Add(
            $"| Skender 2.7.3 | {v2Timing.TotalMilliseconds:F2} | " +
            $"{v2Timing.MeanMilliseconds:F4} | {v2Timing.MeanAllocatedBytes:N0} |");
        lines.Add(
            $"| FacioQuo 3.0.1 | {v3Timing.TotalMilliseconds:F2} | " +
            $"{v3Timing.MeanMilliseconds:F4} | {v3Timing.MeanAllocatedBytes:N0} |");

        lines.Add(string.Empty);
        lines.Add("## CR4.4 incremental quote-cache benchmark");
        lines.Add(string.Empty);
        lines.Add(
            "The following measurement compares per-bar quote-window rebuilds " +
            "with the incremental bounded cache strategy used by the production OSS boundary.");
        lines.Add(string.Empty);
        lines.Add("| Strategy | Total ms | Mean ms/iteration | Mean allocated bytes/iteration |");
        lines.Add("| --- | ---: | ---: | ---: |");
        lines.Add(
            $"| Rebuild per bar | {quoteCacheTiming.Rebuild.TotalMilliseconds:F2} | " +
            $"{quoteCacheTiming.Rebuild.MeanMilliseconds:F4} | " +
            $"{quoteCacheTiming.Rebuild.MeanAllocatedBytes:N0} |");
        lines.Add(
            $"| Incremental append/remove | {quoteCacheTiming.Incremental.TotalMilliseconds:F2} | " +
            $"{quoteCacheTiming.Incremental.MeanMilliseconds:F4} | " +
            $"{quoteCacheTiming.Incremental.MeanAllocatedBytes:N0} |");

        lines.Add(string.Empty);
        lines.Add("## CI-02 production Skender boundary parity");
        lines.Add(string.Empty);
        lines.Add(
            "This is the consolidated CR8.3b bounded stable-window numerical parity " +
            "benchmark and CI-02 production-boundary gate. It compares canonical " +
            "full-prefix Skender calculations with bounded stable and rolling windows.");
        lines.Add(string.Empty);
        lines.Add("| Metric | Result |");
        lines.Add("| --- | ---: |");
        lines.Add($"| Gate | {(warmupParity.Passed ? "PASS" : "FAIL")} |");
        lines.Add($"| Compared points | {productionParity.ComparedPoints:N0} |");
        lines.Add($"| Direction mismatches | {productionParity.DirectionMismatches:N0} |");
        lines.Add($"| Non-finite pairs | {productionParity.NonFinitePairs:N0} |");
        lines.Add($"| Rolling exact mismatches | {productionParity.ExactMismatches:N0} |");
        lines.Add($"| Stable max absolute error | {FormatError(productionParity.MaxStableError)} |");
        lines.Add($"| Stable mean absolute error | {FormatError(productionParity.MeanStableError)} |");
        lines.Add($"| Stable RMS error | {FormatError(productionParity.RootMeanSquareStableError)} |");
        lines.Add($"| Rolling max absolute error | {FormatError(productionParity.MaxRollingError)} |");
        lines.Add($"| Full-prefix mean ms/iteration | {productionParity.FullPrefixTiming.MeanMilliseconds:F4} |");
        lines.Add($"| Bounded-window mean ms/iteration | {productionParity.BoundedWindowTiming.MeanMilliseconds:F4} |");
        lines.Add($"| Full-prefix allocated bytes/iteration | {productionParity.FullPrefixTiming.MeanAllocatedBytes:N0} |");
        lines.Add($"| Bounded-window allocated bytes/iteration | {productionParity.BoundedWindowTiming.MeanAllocatedBytes:N0} |");

        lines.Add(string.Empty);
        lines.Add(
            passed && warmupParity.Passed
                ? "OSS numerical parity gate: **PASS**. FacioQuo remains research-only and the single production Skender boundary benchmark remains bounded."
                : "OSS numerical parity gate: **FAIL**. Promotion remains blocked and the failing OSS comparisons must be investigated.");

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatError(double value)
    {
        return double.IsFinite(value)
            ? value.ToString("0.000000000000E+00")
            : "INF";
    }
}
