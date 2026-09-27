# OSS Register

The oss/ directory is the explicit boundary for third-party source material.

## Admission policy
- Exact release/tag/commit must be pinned.
- License and provenance must be preserved.
- .NET 6 and cTrader Automate compatibility must be demonstrated.
- Native runtime, network service and second execution engines are not admitted into the indicator.
- CFIP decision, execution and risk semantics remain authoritative.

## Candidates

### FacioQuo Stock Indicators
Repository: https://github.com/facioquo/stock-indicators-dotnet
Apache-2.0. Broad technical indicator library with streaming/incremental support. Candidate for selected, dependency-light indicator implementations after cTrader compatibility testing.
Status: Candidate / Phase 9 benchmark

### QuantConnect LEAN
Repository: https://github.com/QuantConnect/Lean
Apache-2.0. Mature C#/Python algorithmic trading engine. Use as an architecture/algorithm reference and evaluate selected components rather than embedding the whole engine into cTrader.
Status: Reference / selected-component benchmark

### Trady
Repository: https://github.com/karlwancl/Trady
.NET Standard 2.0 technical analysis/backtesting library. Technically portable, but upstream describes it as a hobby project; benchmark only until quality is demonstrated for the needed component.
Status: Benchmark only

### StockSharp
Repository: https://github.com/StockSharp/StockSharp
Excluded. Upstream currently uses a custom/proprietary license notice rather than a general-purpose OSS license.
Status: Excluded
