# Stock Indicators OSS benchmark

This project is the isolated numerical validation harness for the CFIP OSS
indicator boundary. It is not a trading engine and it cannot become a second
decision, risk, execution, broker, lifecycle, or presentation authority.

## Package boundary

Production remains:

- Skender.Stock.Indicators 2.7.3
- consumed only by the isolated cTrader adapters under
  src/CFIP.Indicator/Analysis/Indicators/External/
- net6-compatible production dependency

Research remains:

- FacioQuo.Stock.Indicators 3.0.1
- benchmark-only
- evaluated under the net8 benchmark target
- never referenced by production source

The benchmark deliberately preserves this boundary even when numerical parity
passes.

## Track 19.1 comparison contract

The comparison covers the complete OSS indicator set used by the production
confluence adapter:

1. RSI
2. MACD histogram
3. Bollinger %B
4. Bollinger width
5. MFI
6. Stochastic %K
7. Stochastic %D
8. SuperTrend
9. Aroon oscillator
10. CCI
11. OBV
12. Parabolic SAR

The metric list contains 12 measurements across 10 indicator families.

Each package is evaluated against the same 800-bar synthetic OHLCV fixture.
The fixture is generated once into a shared intermediate representation so the
two package generations receive identical timestamps, OHLC and volume values.

The suite uses four deterministic market-shape scenarios:

- TREND_UP
- TREND_DOWN
- RANGE
- REGIME_SHIFT

For every metric and scenario the harness:

- verifies complete output count;
- verifies timestamp alignment;
- ignores the first 120 bars for numerical parity to avoid warm-up effects;
- requires every comparable pair to be finite;
- computes maximum absolute error, mean absolute error and RMS error;
- applies a strict 1e-6 absolute tolerance;
- fails the process on any parity or coverage regression.

## Performance measurement

Batch timing and per-iteration allocation are measured on the same 800-bar
TREND_UP fixture for both package generations. These numbers are environment
measurements and are not trading-performance guarantees.

## CI

The workflow restores the isolated net8 benchmark and executes it on changes
to the benchmark or its workflow definition. The generated Markdown report is
also written to the GitHub Actions job summary.

A passing benchmark is necessary numerical evidence, but it is not sufficient
for promotion. Runtime compatibility and the CFIP authority-boundary checks
remain mandatory.

## CR4.4 cache benchmark

The benchmark also measures the quote materialization boundary used by the production OSS adapters on the deterministic 800-bar fixture. It compares a per-bar 161-quote rebuild with the incremental append/remove strategy.

This is a cache-materialization benchmark, not a claim about end-to-end cTrader latency. The path-dependent production adapters additionally use a stable history prefix so recursive Skender calculations are not reset by a moving window.
