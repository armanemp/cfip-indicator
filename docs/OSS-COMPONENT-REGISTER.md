# OSS Component Register

## Production OSS component — Skender.Stock.Indicators 2.7.3

**Upstream:** https://github.com/facioquo/stock-indicators-dotnet
**NuGet:** https://www.nuget.org/packages/Skender.Stock.Indicators/2.7.3
**License:** Apache-2.0
**Production role:** numerical indicator confluence and numerical cross-check only.

**Adapter naming:** production adapter files and methods use the `Skender*` prefix because the production API consumed by CFIP is the `Skender.Stock.Indicators` package, even though the upstream repository retains the `FacioQuo` GitHub owner/name.

**Target-runtime compatibility:** package 2.7.3 provides a .NET Standard 2.0
asset and is compatible with the net6.0 production target.

**Adapter owners:**

- Analysis/Indicators/External/SkenderRsi.cs
- Analysis/Indicators/External/SkenderMacd.cs
- Analysis/Indicators/External/SkenderBollingerBands.cs
- Analysis/Indicators/External/SkenderMfi.cs
- Analysis/Indicators/External/SkenderStoch.cs
- Analysis/Indicators/External/SkenderSuperTrend.cs
- Analysis/Indicators/External/SkenderAroon.cs
- Analysis/Indicators/External/SkenderCci.cs
- Analysis/Indicators/External/SkenderObv.cs
- Analysis/Indicators/External/SkenderParabolicSar.cs
- Analysis/Indicators/External/OssIndicatorConfluenceAnalyzer.cs
- Analysis/Indicators/External/OssQuoteSeriesCache.cs
- Analysis/Indicators/External/OssIndicatorSnapshotCache.cs

**Authority boundary:** the dependency cannot own CFIP structure, decision,
planning, risk, execution, broker mutation, lifecycle or presentation.

**Verification:** the OSS benchmark checks deterministic parity against the
FacioQuo v3 research line for the common indicator suite and validates all 10
production OSS indicator series. The benchmark also measures batch calculation
time for both package generations.

**Maintenance note:** the v2 package line is being phased out upstream with
maintenance scheduled to end after 2026. CFIP therefore remains pinned to 2.7.3
until a maintained package line can target the actual cTrader production runtime.

## Research candidate — FacioQuo.Stock.Indicators 3.0.1

**Upstream:** https://github.com/facioquo/stock-indicators-dotnet
**NuGet:** https://www.nuget.org/packages/FacioQuo.Stock.Indicators/3.0.1
**License:** Apache-2.0

Package 3.0.1 targets .NET 8, .NET 9 and .NET 10. It is therefore not promoted
into the net6.0 cTrader production assembly.

The benchmark keeps the v3 API isolated and provides the migration reference for
the eventual package transition. No production source references the v3 namespace.

## Track 19 — OSS Numerical Benchmark

Phase 19.1 status: complete.

The benchmark preserves the production/research package boundary and compares
Skender 2.7.3 against FacioQuo 3.0.1 across four deterministic 800-bar market
shapes. Coverage includes all 10 production OSS indicator families, with
Bollinger %B/width and Stochastic %K/%D measured as separate numerical metrics.
The benchmark checks output counts, timestamp alignment, post-warm-up finite
coverage, maximum/mean/RMS error, repeatable batch timing and per-iteration
allocation. A parity pass does not promote v3 into production; the net6 cTrader
runtime gate and OSS authority rules remain mandatory.

Continuity document: docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md.

## Research candidate — TA-Lib.NETCore

**Upstream:** https://github.com/hmG3/TA-Lib.NETCore
**License:** LGPL-3.0

TA-Lib.NETCore remains a numerical cross-check candidate. Any bundled use requires
an explicit distribution and license review plus deterministic CFIP fixture parity.

## Research/reference candidate — QuantConnect LEAN

**Upstream:** https://github.com/QuantConnect/Lean
**License:** Apache-2.0

LEAN is a complete algorithmic-trading engine. It is restricted to offline
architectural comparison and research and must never become a second live trading
engine inside CFIP.

## Admission rules

Every production OSS component must have an upstream source, exact package version,
license/attribution, target-runtime compatibility evidence, isolated adapter ownership,
deterministic numerical coverage and benchmark evidence.

OSS never becomes the CFIP decision or execution authority.
