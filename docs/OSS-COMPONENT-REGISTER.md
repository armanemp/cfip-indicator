# OSS Component Register

## Production boundary

CFIP remains the strategy and execution authority. OSS components are introduced only behind adapters or benchmark projects.

## QuanTAlib

Repository: https://github.com/mihakralj/QuanTAlib

Apache-2.0. The project advertises a large technical-indicator catalog and streaming/fixed-memory calculation design. The current package line reviewed for this project targets a newer .NET runtime than the cTrader net6 production target, so it is a benchmark candidate rather than a production dependency at this stage.

## FacioQuo Stock Indicators (net6 compatibility candidate)

Repository: https://github.com/facioquo/stock-indicators-dotnet

Apache-2.0. The 2.7.3 line targets netstandard2.0/2.1 and is retained as a possible future production adapter candidate. No production dependency is introduced until numerical fixtures and runtime behavior are validated against CFIP.

## FacioQuo.Stock.Indicators 3.0.1

Repository: https://github.com/facioquo/stock-indicators-dotnet

Apache-2.0. The current v3 line provides a broad technical-indicator catalog and streaming/buffer-oriented APIs. Its current package target is newer than the net6 production target, so CFIP treats it as a .NET 8 research/validation benchmark rather than a direct production dependency.

The benchmark harness lives under `tools/CFIP.StockIndicators.Benchmark`. It never owns CFIP decision or broker execution.

## QuantConnect LEAN

Repository: https://github.com/QuantConnect/Lean

Apache-2.0 and highly modular. It is valuable as an offline research/backtesting architecture, but embedding it into this indicator would create a second trading engine and is therefore intentionally rejected for direct runtime integration.

## Adoption gate

An OSS component is promoted from benchmark to production only when it is compatible with the target runtime, numerically validated against CFIP fixtures, measurably useful, license-compatible and isolated from decision/execution authority.