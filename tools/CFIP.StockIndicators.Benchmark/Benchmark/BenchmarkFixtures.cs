using System;
using System.Collections.Generic;
using FacioQuo.Stock.Indicators;
using V2Quote = Skender.Stock.Indicators.Quote;

namespace CfipStockIndicatorsBenchmark;

internal static class BenchmarkFixtures
{
    internal const int BarCount = 800;
    internal const int DefaultWarmupBars = 120;

    internal static IReadOnlyList<string> ScenarioNames =>
        new[]
        {
            "TREND_UP",
            "TREND_DOWN",
            "RANGE",
            "REGIME_SHIFT"
        };

    internal static IReadOnlyList<V2Quote> BuildV2(string scenario)
    {
        var source = BuildSyntheticBars(scenario);
        var quotes = new List<V2Quote>(source.Count);

        foreach (SyntheticBar bar in source)
        {
            quotes.Add(
                new V2Quote
                {
                    Date = bar.Timestamp,
                    Open = bar.Open,
                    High = bar.High,
                    Low = bar.Low,
                    Close = bar.Close,
                    Volume = bar.Volume
                });
        }

        return quotes;
    }

    internal static IReadOnlyList<Bar> BuildV3(string scenario)
    {
        var source = BuildSyntheticBars(scenario);
        var bars = new List<Bar>(source.Count);

        foreach (SyntheticBar bar in source)
        {
            bars.Add(
                new Bar(
                    Timestamp: bar.Timestamp,
                    Open: bar.Open,
                    High: bar.High,
                    Low: bar.Low,
                    Close: bar.Close,
                    Volume: bar.Volume));
        }

        return bars;
    }

    private static IReadOnlyList<SyntheticBar> BuildSyntheticBars(string scenario)
    {
        if (!ScenarioNames.Contains(scenario, StringComparer.Ordinal))
            throw new ArgumentException(
                $"Unknown benchmark scenario: {scenario}",
                nameof(scenario));

        var bars = new List<SyntheticBar>(BarCount);
        DateTime start = new DateTime(
            2020,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        for (int i = 0; i < BarCount; i++)
        {
            double open;
            double close;
            double volatility;

            switch (scenario)
            {
                case "TREND_UP":
                    open =
                        100.0 +
                        (i * 0.035) +
                        (Math.Sin(i / 9.0) * 0.18) +
                        ((i % 7) * 0.015);
                    close =
                        open +
                        (0.045 * Math.Sin(i / 3.0)) +
                        0.025;
                    volatility = 0.18;
                    break;

                case "TREND_DOWN":
                    open =
                        145.0 -
                        (i * 0.035) +
                        (Math.Sin(i / 9.0) * 0.18) +
                        ((i % 7) * 0.015);
                    close =
                        open +
                        (0.045 * Math.Sin(i / 3.0)) -
                        0.025;
                    volatility = 0.18;
                    break;

                case "RANGE":
                    open =
                        120.0 +
                        (1.35 * Math.Sin(i / 13.0)) +
                        (0.65 * Math.Sin(i / 31.0)) +
                        (((i % 5) - 2) * 0.04);
                    close =
                        open +
                        (0.16 * Math.Sin(i / 2.7)) -
                        (0.04 * Math.Cos(i / 5.0));
                    volatility = 0.28;
                    break;

                default:
                    double direction =
                        i < BarCount / 2
                            ? i * 0.045
                            : (BarCount / 2 * 0.045) -
                              ((i - BarCount / 2) * 0.055);

                    double shock =
                        i >= BarCount / 2
                            ? Math.Sin(i / 2.3) * 0.42
                            : Math.Sin(i / 7.0) * 0.16;

                    open =
                        112.0 +
                        direction +
                        shock +
                        (((i % 7) - 3) * 0.025);
                    close =
                        open +
                        (i < BarCount / 2 ? 0.035 : -0.04) +
                        (Math.Sin(i / 3.1) * (i < BarCount / 2 ? 0.07 : 0.18));
                    volatility =
                        i < BarCount / 2
                            ? 0.20
                            : 0.42;
                    break;
            }

            double wickUp =
                volatility +
                (0.03 * (i % 4));
            double wickDown =
                (volatility * 0.9) +
                (0.025 * ((i + 1) % 5));

            double high =
                Math.Max(open, close) + wickUp;
            double low =
                Math.Min(open, close) - wickDown;

            decimal volume =
                1000m +
                (i * 7m) +
                ((i % 9) * 23m);

            if (scenario == "REGIME_SHIFT" && i >= BarCount / 2)
                volume += 500m + ((i % 11) * 17m);

            bars.Add(
                new SyntheticBar(
                    start.AddMinutes(i * 5),
                    ToDecimal(open),
                    ToDecimal(high),
                    ToDecimal(low),
                    ToDecimal(close),
                    volume));
        }

        return bars;
    }

    private static decimal ToDecimal(double value)
    {
        return decimal.Round(
            (decimal)value,
            8,
            MidpointRounding.ToEven);
    }

    private readonly record struct SyntheticBar(
        DateTime Timestamp,
        decimal Open,
        decimal High,
        decimal Low,
        decimal Close,
        decimal Volume);
}
