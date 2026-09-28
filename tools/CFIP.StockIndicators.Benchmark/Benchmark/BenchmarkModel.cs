using System;
using System.Collections.Generic;

namespace CfipStockIndicatorsBenchmark;

internal readonly record struct IndicatorPoint(
    DateTime Timestamp,
    double? Value);

internal sealed record MetricDefinition(
    string IndicatorFamily,
    string MetricName,
    int WarmupBars,
    double AbsoluteTolerance,
    Func<IReadOnlyList<Skender.Stock.Indicators.Quote>, IReadOnlyList<IndicatorPoint>> V2Projector,
    Func<IReadOnlyList<FacioQuo.Stock.Indicators.Bar>, IReadOnlyList<IndicatorPoint>> V3Projector);

internal sealed record ComparisonResult(
    string Scenario,
    string IndicatorFamily,
    string MetricName,
    int InputCount,
    int WarmupBars,
    int ComparedPairs,
    int ValidPairs,
    double MaxAbsoluteError,
    double MeanAbsoluteError,
    double RootMeanSquareError,
    bool Passed,
    string FailureReason);

internal readonly record struct BenchmarkTiming(
    double TotalMilliseconds,
    double MeanMilliseconds,
    long MeanAllocatedBytes);
