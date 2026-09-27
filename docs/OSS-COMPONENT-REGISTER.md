# OSS Component Register

## Admission policy

OSS is an isolated source and research boundary. A component is promoted to production only after runtime compatibility, numerical validation, performance value, license review and architectural isolation have all been demonstrated.

## Current production policy

The cTrader indicator currently has no mandatory third-party runtime dependency.

OSS components may provide:

- numerical indicator cross-checks;
- optional secondary confluence;
- offline research and benchmark tooling;
- source-level reference implementations.

OSS components may not become a second decision engine, execution engine, broker layer or lifecycle authority.

## Candidate: FacioQuo Stock Indicators

Repository: https://github.com/facioquo/stock-indicators-dotnet

License: Apache-2.0.

The current 3.x package line provides a broad technical-indicator catalog and streaming-oriented APIs, but its NuGet package currently targets .NET 8/9/10. It is therefore a research and numerical-validation candidate for the current cTrader target, not a direct production dependency. citeturn867652search0turn358101search7

The previous 2.x package line remains useful as a compatibility reference because the 2.7.3 package was published with .NET Standard 2.0 compatibility. Its maintenance trajectory must be reviewed before any long-lived production dependency is chosen. citeturn867652search5

## Candidate: TA-Lib.NETCore

Repository: https://github.com/hmG3/TA-Lib.NETCore

License: LGPL-3.0.

The implementation is written in C# with no .NET platform dependencies, making it technically interesting for fixture-level numerical validation. The copyleft license requires a dedicated legal/package-boundary review before commercial distribution. It remains a benchmark/adapter candidate until that review is complete. citeturn358101search1

## Candidate: QuantConnect LEAN

Repository: https://github.com/QuantConnect/Lean

License: Apache-2.0.

LEAN is a mature modular algorithmic-trading engine, but its current development and build requirements target modern .NET and it would introduce a second trading-engine abstraction if embedded in the cTrader indicator. It is therefore an architectural reference and offline research candidate, not a cTrader runtime dependency. citeturn867652search2turn867652search10

## Current OSS rule

Do not copy a complete external trading engine into the cTrader indicator. Prefer narrow adapters around individual numerical indicators or research tools, and retain CFIP as the sole authority for structure, decision, risk, execution and broker lifecycle.
