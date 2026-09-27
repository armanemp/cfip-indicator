# OSS Component Register

## Production boundary

CFIP remains the strategy and execution authority. OSS components are admitted only behind narrow adapters or benchmark projects.

## Candidate: FacioQuo Stock Indicators

Repository: https://github.com/facioquo/stock-indicators-dotnet

License: Apache-2.0.

The upstream project provides a broad technical-indicator catalog and streaming-oriented APIs. The current package line targets a newer .NET runtime than the production cTrader target, so it remains a research and numerical-validation candidate rather than a direct production dependency. citeturn867652search0turn358101search7

Its earlier compatibility line is useful as a reference for .NET Standard-based numerical validation, but no external package is promoted to the production indicator until runtime compatibility and fixture parity are proven. citeturn867652search5

## Candidate: TA-Lib.NETCore

Repository: https://github.com/hmG3/TA-Lib.NETCore

License: LGPL-3.0.

The implementation is written entirely in C# with no .NET platform dependencies. It is technically suitable for numerical cross-checking, but the license requires a separate commercial distribution review before any bundled use. It remains a benchmark/adapter candidate. citeturn358101search1

## Candidate: QuantConnect LEAN

Repository: https://github.com/QuantConnect/Lean

License: Apache-2.0.

LEAN is a modular algorithmic-trading engine and a useful architectural reference, but embedding it in the cTrader indicator would introduce a second trading-engine abstraction. It is therefore restricted to offline research and architectural benchmarking. citeturn867652search2turn867652search10

## Current OSS rule

Do not copy a complete external trading engine into the cTrader indicator.

Prefer narrow numerical adapters, offline benchmarks and fixture comparison. OSS components never own CFIP structure, decision, risk, order placement, broker mutation or lifecycle authority.

## Promotion gate

A candidate becomes a production dependency only after:

- target cTrader/.NET compatibility is demonstrated;
- numerical fixture parity is demonstrated;
- performance value is measured;
- license and attribution are reviewed;
- the adapter has an isolated owner;
- decision and execution authority remain inside CFIP.
