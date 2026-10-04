#!/usr/bin/env python3
"""Static acceptance gate for CR4.4 Skender numerical stability and caching."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


parameters = read(
    "src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorParameters.cs"
)
settings = read("src/CFIP.Indicator/Core/Math/OssIndicatorSettings.cs")
cache = read(
    "src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteSeriesCache.cs"
)
cache_entry = read(
    "src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteCacheEntry.cs"
)
analyzer = read(
    "src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorConfluenceAnalyzer.cs"
)
contracts_project = read(
    "tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj"
)
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
benchmark_program = read("tools/CFIP.StockIndicators.Benchmark/Program.cs")
benchmark_report = read("tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkReport.cs")
roadmap = read("docs/CFIP-ROADMAP.md")


check(
    "OSS constants have one authoritative owner",
    "internal static class OssIndicatorParameters" in parameters
    and "GetBollingerBands(" not in parameters,
)
check(
    "fixed OSS periods and factors are centralized",
    "internal sealed class OssIndicatorSettings" in settings
    and "MacdSignalPeriod = 9" in settings
    and "BollingerPeriod = 20" in settings
    and "BollingerStandardDeviations = 2.0" in settings
    and "MfiPeriod = 14" in settings
    and "StochLookbackPeriod = 14" in settings
    and "StochSignalPeriod = 3" in settings
    and "StochSmoothPeriod = 3" in settings
    and "SuperTrendPeriod = 10" in settings
    and "SuperTrendMultiplier = 3.0" in settings
    and "AroonPeriod = 25" in settings
    and "CciPeriod = 20" in settings
    and "ParabolicSarAccelerationFactor = 0.02" in settings
    and "ParabolicSarMaximumAccelerationFactor = 0.20" in settings
    and "MacdSignalPeriod" not in parameters
    and "BollingerPeriod" not in parameters
    and "BollingerStandardDeviations" not in parameters
    and "MfiPeriod" not in parameters
    and "StochLookbackPeriod" not in parameters
    and "StochSignalPeriod" not in parameters
    and "StochSmoothPeriod" not in parameters
    and "SuperTrendPeriod" not in parameters
    and "SuperTrendMultiplier" not in parameters
    and "AroonPeriod" not in parameters
    and "CciPeriod" not in parameters
    and "ParabolicSarAccelerationFactor" not in parameters
    and "ParabolicSarMaximumAccelerationFactor" not in parameters,
)
check(
    "current warm-up contracts are centralized",
    "RsiMinimumHistory = 20" in parameters
    and "MacdWarmupMargin = 20" in parameters
    and "SuperTrendMinimumHistory = 60" in parameters
    and "ParabolicSarMinimumHistory = 60" in parameters,
)
check(
    "path-dependent indicators use bounded stable-window history",
    "GetOssStableQuotes(" in cache
    and "GetOssStableQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderRsi.cs"
    )
    and "GetOssStableQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs"
    )
    and "GetOssStableQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderSuperTrend.cs"
    )
    and "GetOssStableQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderParabolicSar.cs"
    ),
)
check(
    "rolling indicators use the bounded rolling cache",
    "GetOssQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderBollingerBands.cs"
    )
    and "GetOssQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMfi.cs"
    )
    and "GetOssQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderStoch.cs"
    )
    and "GetOssQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderAroon.cs"
    )
    and "GetOssQuotes(" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderCci.cs"
    ),
)
check(
    "OSS quote cache reuses a bounded stable window and advances incrementally",
    "StableQuotes" in cache_entry
    and "RollingQuotes" in cache_entry
    and "for (int i = startIndex;" in cache
    and "cache.StableQuotes.Add" in cache
    and "cache.StableQuotes.Count >" in cache
    and "cache.RollingQuotes.Add" in cache
    and "StableQuoteWindowSize" in cache,
)
check(
    "rolling cache is bounded and advances by append/remove",
    "RollingQuoteWindowSize" in cache
    and "RemoveAt(0)" in cache
    and "RollingQuotes.Add(" in cache
    and "CreateQuote(" in cache,
)
check(
    "production adapters contain no duplicated fixed OSS constants",
    "                    20," not in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderBollingerBands.cs"
    )
    and "                    14)" not in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMfi.cs"
    )
    and "                    25)" not in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderAroon.cs"
    )
    and "                    0.02," not in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderParabolicSar.cs"
    ),
)


check(
    "cache invalidates on cTrader history/reconnect events",
    "HistoryLoaded +=" in cache
    and "Reloaded +=" in cache
    and "InvalidationPending" in cache
    and "Bars_HistoryLoaded" in cache
    and "Bars_Reloaded" in cache,
)

check(
    "cache invalidation detects series replacement and cached-prefix mutation",
    "!ReferenceEquals(cache.Bars, bars)" in cache
    and "MatchesFirstBar(" in cache
    and "MatchesStableWindowBoundaries(" in cache,
)
check(
    "OBV remains available but is no longer counted as confluence evidence",
    "ObvBias = SkenderObvBias" in analyzer
    and "obvValid" not in analyzer
    and "(obvValid ? 1 : 0)" not in analyzer,
)
check(
    "OBV diagnostic status is explicit in code",
    "diagnostic-only" in analyzer or "diagnostic only" in analyzer,
)
check(
    "runtime contract compiles the OSS parameter owner",
    "OssIndicatorParameters.cs" in contracts_project
    and "VerifyOssIndicatorParameters();" in contracts,
)
check(
    "runtime contract protects the current OSS parameter values",
    "VerifyOssIndicatorParameters" in contracts
    and "RollingQuoteWindowSize" in contracts
    and "MacdSignalPeriod" in contracts
    and "ParabolicSarMaximumAccelerationFactor" in contracts,
)
check(
    "benchmark measures incremental-vs-rebuild quote materialization",
    "QuoteCacheBenchmark.Measure" in benchmark_program
    and "incremental" in benchmark_report.lower()
    and "rebuild" in benchmark_report.lower(),
)
check(
    "roadmap records CR4.4 without claiming target-terminal validation",
    "CR4.4" in roadmap and "target-terminal" in roadmap.lower(),
)

print("CR4.4 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)
print("CR4.4 STATIC GATE PASS")
