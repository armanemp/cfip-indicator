# OSS Component Register

## Production boundary

CFIP remains the strategy and execution authority. OSS components are introduced only behind adapters or benchmark projects.

## QuanTAlib

Repository: https://github.com/mihakralj/QuanTAlib

Apache-2.0. The project advertises a large technical-indicator catalog and streaming/fixed-memory calculation design. The current package line reviewed for this project targets a newer .NET runtime than the cTrader net6 production target, so it is a benchmark candidate rather than a production dependency at this stage.

## FacioQuo Stock Indicators

Repository: https://github.com/facioquo/stock-indicators-dotnet

Apache-2.0. It provides broad technical-indicator coverage. The current 3.x package line targets newer .NET runtimes; the older 2.7.x line is a compatibility candidate for an adapter/benchmark against the net6 cTrader target.

## QuantConnect LEAN

Repository: https://github.com/QuantConnect/Lean

Apache-2.0 and highly modular. It is valuable as an offline research/backtesting architecture, but embedding it into this indicator would create a second trading engine and is therefore intentionally rejected for direct runtime integration.

## Adoption gate

An OSS component is promoted from benchmark to production only when it is compatible with the target runtime, numerically validated against CFIP fixtures, measurably useful, license-compatible and isolated from decision/execution authority.