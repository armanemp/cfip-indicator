# Stock Indicators OSS benchmark

This is a research/validation harness, not a second CFIP trading engine.

## Production package

CFIP production uses Skender.Stock.Indicators 2.7.3 through the isolated
adapters under Analysis/Indicators/External.

The feature is disabled by default at the strategy level, but the package
remains a production build/runtime dependency because these adapters are
compiled into the indicator assembly.

## Research package

FacioQuo.Stock.Indicators 3.0.1 is benchmark-only. It targets newer .NET
runtimes than the net6.0 cTrader production target.

## Benchmark contract

The benchmark:

- generates a deterministic 260-bar OHLCV fixture;
- compares the v2 and v3 packages for RSI, MACD histogram, Bollinger %B, MFI, Stochastic K/D and SuperTrend;
- validates complete series output for all 10 OSS indicators used by the production confluence adapter;
- measures batch calculation time for the common indicator suite for both package generations.

The benchmark must pass before OSS changes are promoted. Printed timings are
environment measurements, not trading-performance guarantees.
