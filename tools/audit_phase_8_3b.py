#!/usr/bin/env python3
"""Static acceptance gate for CR8.3b / H3-B Skender warm-up/cache/parity."""

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


policy = read("src/CFIP.Indicator/Core/Math/OssIndicatorWarmupPolicy.cs")
cache = read(
    "src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteSeriesCache.cs"
)
entry = read(
    "src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteCacheEntry.cs"
)
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
benchmark = read(
    "tools/CFIP.StockIndicators.Benchmark/Benchmark/SkenderWarmupParityBenchmark.cs"
)
fixtures = read(
    "tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkFixtures.cs"
)
benchmark_program = read("tools/CFIP.StockIndicators.Benchmark/Program.cs")
benchmark_report = read(
    "tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkReport.cs"
)
workflow = read(".github/workflows/source-check.yml")


check(
    "bounded warm-up policy has one immutable non-public window",
    "internal static class OssIndicatorWarmupPolicy" in policy
    and "StableQuoteWindowSize = 768" in policy
    and "FitsCurrentPublicParameterEnvelope" in policy,
)

check(
    "warm-up guidance is encoded for the current production envelope",
    "RsiConvergenceMargin = 100" in policy
    and "MacdConvergenceMargin = 250" in policy
    and "SuperTrendConvergenceMargin = 250" in policy
    and "ParabolicSarConvergenceBars = 100" in policy
    and "safeRsi * 10" in policy
    and "safeMacdSlow +" in policy
    and "safeSuperTrend +" in policy,
)

check(
    "stable quote cache is bounded and incrementally maintained",
    "EnsureStableWindow(" in cache
    and "StableQuoteWindowSize" in cache
    and "cache.StableQuotes.Add(" in cache
    and "cache.StableQuotes.Count >" in cache
    and "cache.StableQuotes.GetRange" not in cache
    and "EnsureStablePrefix" not in cache,
)

check(
    "stable cache tracks first and last boundary fingerprints",
    "StableFirstIndex" in entry
    and "StableFirstOpenTime" in entry
    and "StableLastOpenTime" in entry
    and "MatchesStableWindowBoundaries" in cache,
)

for relative in [
    "SkenderRsi.cs",
    "SkenderMacd.cs",
    "SkenderSuperTrend.cs",
    "SkenderParabolicSar.cs",
]:
    source = read(
        "src/CFIP.Indicator/Analysis/Indicators/External/" + relative
    )
    check(
        f"{relative} consumes the stable bounded window",
        "GetOssStableQuotes(" in source,
    )
    check(
        f"{relative} avoids result-list allocation",
        ".LastOrDefault()" in source
        and ".ToList()" not in source,
    )

for relative in [
    "SkenderAroon.cs",
    "SkenderBollingerBands.cs",
    "SkenderCci.cs",
    "SkenderMfi.cs",
    "SkenderObv.cs",
    "SkenderStoch.cs",
]:
    source = read(
        "src/CFIP.Indicator/Analysis/Indicators/External/" + relative
    )
    check(
        f"{relative} avoids per-call result-list allocation",
        ".ToList()" not in source,
    )

check(
    "runtime contract protects bounded warm-up and public envelope",
    "VerifyOssWarmupPolicy();" in runtime
    and "OssIndicatorWarmupPolicy.cs" in runtime_project
    and "StableQuoteWindowSize == 768" in runtime
    and "FitsCurrentPublicParameterEnvelope" in runtime,
)

check(
    "extended deterministic fixtures are available for parity testing",
    "BuildV2(" in fixtures
    and "int barCount" in fixtures,
)

check(
    "H3-B benchmark measures bounded numerical parity",
    "SkenderWarmupParityBenchmark" in benchmark
    and "StableQuoteWindowSize = 768" in benchmark
    and "DirectionMismatches" in benchmark
    and "MaxAbsoluteError" in benchmark,
)

check(
    "H3-B benchmark is executed and reported",
    "SkenderWarmupParityBenchmark.Measure()" in benchmark_program
    and "warmupParity" in benchmark_program
    and "CR8.3b bounded stable-window numerical parity" in benchmark_report,
)

check(
    "H3-B audit is accumulated immediately after H3-A",
    "audit_phase_8_3a.py" in workflow
    and "audit_phase_8_3b.py" in workflow
    and workflow.index("audit_phase_8_3b.py") >
        workflow.index("audit_phase_8_3a.py"),
)

print("CR8.3b / H3-B SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR8.3b / H3-B STATIC GATE PASS")
