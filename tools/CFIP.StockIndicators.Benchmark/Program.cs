using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using FacioQuo.Stock.Indicators;
using V2Indicator = Skender.Stock.Indicators.Indicator;
using V2Quote = Skender.Stock.Indicators.Quote;
using V3Bar = FacioQuo.Stock.Indicators.Bar;

var v2Quotes = BuildV2Quotes();
var v3Bars = BuildV3Bars();

if (v2Quotes.Count != v3Bars.Count || v2Quotes.Count < 260)
    throw new InvalidOperationException("Deterministic benchmark fixture is invalid.");

var v2Common = CalculateV2Common(v2Quotes);
var v3Common = CalculateV3Common(v3Bars);

AssertNear("RSI", v2Common.Rsi, v3Common.Rsi);
AssertNear("MACD histogram", v2Common.MacdHistogram, v3Common.MacdHistogram);
AssertNear("Bollinger %B", v2Common.BollingerPercentB, v3Common.BollingerPercentB);
AssertNear("MFI", v2Common.Mfi, v3Common.Mfi);
AssertNear("Stochastic K", v2Common.StochK, v3Common.StochK);
AssertNear("Stochastic D", v2Common.StochD, v3Common.StochD);
AssertNear("SuperTrend", v2Common.SuperTrend, v3Common.SuperTrend);

ValidateV2ExtendedCoverage(v2Quotes);

const int iterations = 100;
TimeSpan v2Elapsed = Measure(
    iterations,
    () => CalculateV2Common(v2Quotes));

TimeSpan v3Elapsed = Measure(
    iterations,
    () => CalculateV3Common(v3Bars));

Console.WriteLine(
    $"OSS benchmark OK: {v2Quotes.Count} bars, " +
    "v2/v3 common-indicator parity passed at tolerance 1e-6.");
Console.WriteLine(
    $"v2 batch suite: {v2Elapsed.TotalMilliseconds:F2} ms " +
    $"({v2Elapsed.TotalMilliseconds / iterations:F3} ms/iteration).");
Console.WriteLine(
    $"v3 batch suite: {v3Elapsed.TotalMilliseconds:F2} ms " +
    $"({v3Elapsed.TotalMilliseconds / iterations:F3} ms/iteration).");
Console.WriteLine(
    "Production: Skender.Stock.Indicators 2.7.3. " +
    "Research: FacioQuo.Stock.Indicators 3.0.1.");

static List<V2Quote> BuildV2Quotes()
{
    var quotes = new List<V2Quote>();
    DateTime start = new DateTime(2020, 1, 1);

    for (int i = 0; i < 260; i++)
    {
        DateTime date = start.AddMinutes(i * 5);
        decimal open = 100m + i * 0.03m + (i % 7) * 0.08m;
        decimal close = open + ((i % 9) - 4) * 0.04m;
        decimal high = Math.Max(open, close) + 0.12m + (i % 3) * 0.02m;
        decimal low = Math.Min(open, close) - 0.10m - (i % 4) * 0.02m;

        quotes.Add(new V2Quote
        {
            Date = date,
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = 1000m + i * 5m
        });
    }

    return quotes;
}

static List<V3Bar> BuildV3Bars()
{
    var bars = new List<V3Bar>();
    DateTime start = new DateTime(2020, 1, 1);

    for (int i = 0; i < 260; i++)
    {
        DateTime date = start.AddMinutes(i * 5);
        decimal open = 100m + i * 0.03m + (i % 7) * 0.08m;
        decimal close = open + ((i % 9) - 4) * 0.04m;
        decimal high = Math.Max(open, close) + 0.12m + (i % 3) * 0.02m;
        decimal low = Math.Min(open, close) - 0.10m - (i % 4) * 0.02m;

        bars.Add(new V3Bar(
            Timestamp: date,
            Open: open,
            High: high,
            Low: low,
            Close: close,
            Volume: 1000m + i * 5m));
    }

    return bars;
}

static (double Rsi, double MacdHistogram, double BollingerPercentB,
        double Mfi, double StochK, double StochD, double SuperTrend)
    CalculateV2Common(
        IReadOnlyList<V2Quote> quotes)
{
    var rsi = V2Indicator.GetRsi(quotes, 14).ToList();
    var macd = V2Indicator.GetMacd(quotes, 12, 26, 9).ToList();
    var bollinger = V2Indicator.GetBollingerBands(quotes, 20, 2).ToList();
    var mfi = V2Indicator.GetMfi(quotes, 14).ToList();
    var stochastic = V2Indicator.GetStoch(quotes, 14, 3, 3).ToList();
    var superTrend = V2Indicator.GetSuperTrend(quotes, 10, 3).ToList();

    return (
        ToDouble(rsi[^1].Rsi),
        ToDouble(macd[^1].Histogram),
        ToDouble(bollinger[^1].PercentB),
        ToDouble(mfi[^1].Mfi),
        ToDouble(stochastic[^1].K),
        ToDouble(stochastic[^1].D),
        ToDouble(superTrend[^1].SuperTrend));
}

static (double Rsi, double MacdHistogram, double BollingerPercentB,
        double Mfi, double StochK, double StochD, double SuperTrend)
    CalculateV3Common(
        IReadOnlyList<V3Bar> bars)
{
    var rsi = bars.ToRsi(14).ToList();
    var macd = bars.ToMacd(12, 26, 9).ToList();
    var bollinger = bars.ToBollingerBands(20, 2).ToList();
    var mfi = bars.ToMfi(14).ToList();
    var stochastic = bars.ToStoch(14, 3, 3).ToList();
    var superTrend = bars.ToSuperTrend(10, 3).ToList();

    return (
        ToDouble(rsi[^1].Rsi),
        ToDouble(macd[^1].Histogram),
        ToDouble(bollinger[^1].PercentB),
        ToDouble(mfi[^1].Mfi),
        ToDouble(stochastic[^1].K),
        ToDouble(stochastic[^1].D),
        ToDouble(superTrend[^1].SuperTrend));
}

static void ValidateV2ExtendedCoverage(
    IReadOnlyList<V2Quote> quotes)
{
    var results = new (string Name, int Count)[]
    {
        ("RSI", V2Indicator.GetRsi(quotes, 14).Count()),
        ("MACD", V2Indicator.GetMacd(quotes, 12, 26, 9).Count()),
        ("Bollinger", V2Indicator.GetBollingerBands(quotes, 20, 2).Count()),
        ("MFI", V2Indicator.GetMfi(quotes, 14).Count()),
        ("Stochastic", V2Indicator.GetStoch(quotes, 14, 3, 3).Count()),
        ("SuperTrend", V2Indicator.GetSuperTrend(quotes, 10, 3).Count()),
        ("Aroon", V2Indicator.GetAroon(quotes, 25).Count()),
        ("CCI", V2Indicator.GetCci(quotes, 20).Count()),
        ("OBV", V2Indicator.GetObv(quotes).Count()),
        ("Parabolic SAR", V2Indicator.GetParabolicSar(quotes, 0.02, 0.20).Count())
    };

    foreach (var result in results)
    {
        if (result.Count != quotes.Count)
            throw new InvalidOperationException(
                $"Extended OSS indicator coverage failed for {result.Name}.");
    }
}

static TimeSpan Measure(
    int iterations,
    Action action)
{
    var stopwatch = Stopwatch.StartNew();

    for (int i = 0; i < iterations; i++)
        action();

    stopwatch.Stop();
    return stopwatch.Elapsed;
}

static double ToDouble(object value)
{
    if (value == null)
        return double.NaN;

    return Convert.ToDouble(
        value,
        CultureInfo.InvariantCulture);
}

static void AssertNear(
    string name,
    double expected,
    double actual,
    double tolerance = 0.000001)
{
    if (double.IsNaN(expected) ||
        double.IsInfinity(expected) ||
        double.IsNaN(actual) ||
        double.IsInfinity(actual))
    {
        throw new InvalidOperationException(
            $"{name} produced a non-finite result: " +
            $"v2={expected}, v3={actual}.");
    }

    double difference = Math.Abs(expected - actual);

    if (difference > tolerance)
    {
        throw new InvalidOperationException(
            $"{name} parity failed: v2={expected:G17}, " +
            $"v3={actual:G17}, diff={difference:G17}.");
    }
}
