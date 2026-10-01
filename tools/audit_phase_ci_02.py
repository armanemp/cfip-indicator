#!/usr/bin/env python3
"""Static acceptance gate for CI-02 OSS numerical parity, warm-up and cache integrity."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append("missing file: " + relative)
        return ""
    return path.read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    print(("PASS" if condition else "FAIL") + " | " + name)
    if not condition:
        errors.append(name)


settings = read("src/CFIP.Indicator/Core/Math/OssIndicatorSettings.cs")
warmup = read("src/CFIP.Indicator/Core/Math/OssIndicatorWarmupPolicy.cs")
window_rule = read("src/CFIP.Indicator/Core/Math/OssQuoteWindowRule.cs")
projection_rule = read("src/CFIP.Indicator/Core/Math/OssQuoteProjectionRule.cs")
cache = read("src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteSeriesCache.cs")
entry = read("src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteCacheEntry.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
benchmark_workflow = read(".github/workflows/oss-benchmark.yml")
benchmark = read(
    "tools/CFIP.StockIndicators.Benchmark/Benchmark/SkenderProductionParityBenchmark.cs"
)
benchmark_program = read("tools/CFIP.StockIndicators.Benchmark/Program.cs")
benchmark_report = read("tools/CFIP.StockIndicators.Benchmark/Benchmark/BenchmarkReport.cs")
production_project = read("src/CFIP.Indicator/CFIP.Indicator.csproj")

stable_adapters = {
    "SkenderRsi.cs": "GetOssStableQuotes(",
    "SkenderMacd.cs": "GetOssStableQuotes(",
    "SkenderSuperTrend.cs": "GetOssStableQuotes(",
    "SkenderParabolicSar.cs": "GetOssStableQuotes(",
}
rolling_adapters = {
    "SkenderBollingerBands.cs": "GetOssQuotes(",
    "SkenderMfi.cs": "GetOssQuotes(",
    "SkenderStoch.cs": "GetOssQuotes(",
    "SkenderAroon.cs": "GetOssQuotes(",
    "SkenderCci.cs": "GetOssQuotes(",
    "SkenderObv.cs": "GetOssQuotes(",
}

check(
    "fixed Skender settings remain under one immutable owner",
    "internal static class OssIndicatorSettings" in settings
    or "internal sealed class OssIndicatorSettings" in settings
)
check(
    "stable and rolling bounds remain explicit",
    "StableQuoteWindowSize = 768" in warmup
    and "RollingQuoteWindowSize" in cache
)
check(
    "one canonical OSS window-rule owner exists",
    "class OssQuoteWindowRule" in window_rule
    and "ResolveFirstIndex(" in window_rule
    and "RequiresRebuild(" in window_rule
    and "IsBoundedCount(" in window_rule
)

check(
    "OSS quote-volume normalization has one pure owner",
    "class OssQuoteProjectionRule" in projection_rule
    and "NormalizeVolume(" in projection_rule
    and "double.IsNaN(volume)" in projection_rule
    and "double.IsInfinity(volume)" in projection_rule
)

check(
    "stable cache delegates window geometry to canonical owner",
    "OssQuoteWindowRule.ResolveFirstIndex(" in cache
    and "OssQuoteWindowRule.RequiresRebuild(" in cache
    and "cache.StableQuotes.Count >" in cache
)
check(
    "rolling cache delegates window geometry to canonical owner",
    "OssQuoteWindowRule.ResolveFirstIndex(" in cache
    and "cache.RollingQuotes.Count >" in cache
)
check(
    "stable cache keeps first and last fingerprints",
    "StableFirstOpenTime" in entry
    and "StableLastOpenTime" in entry
    and "MatchesStableWindowBoundaries" in cache
)
check(
    "history replacement/reload invalidates cached state",
    "bars.HistoryLoaded +=" in cache
    and "bars.Reloaded +=" in cache
    and "InvalidationPending" in cache
)

for filename, expected in stable_adapters.items():
    source = read("src/CFIP.Indicator/Analysis/Indicators/External/" + filename)
    check(
        f"{filename} uses stable bounded quotes",
        expected in source,
    )

for filename, expected in rolling_adapters.items():
    source = read("src/CFIP.Indicator/Analysis/Indicators/External/" + filename)
    check(
        f"{filename} uses rolling bounded quotes",
        expected in source,
    )

check(
    "recursive stable adapters avoid per-call result materialization",
    all(
        ".LastOrDefault()" in read(
            "src/CFIP.Indicator/Analysis/Indicators/External/" + filename
        )
        and ".ToList()" not in read(
            "src/CFIP.Indicator/Analysis/Indicators/External/" + filename
        )
        for filename in stable_adapters
    )
)

check(
    "rolling adapters avoid per-call result materialization",
    all(
        (
            filename == "SkenderObv.cs"
            and "foreach (var result in StockIndicator.GetObv(quotes))" in read(
                "src/CFIP.Indicator/Analysis/Indicators/External/" + filename
            )
            and "previous = current;" in read(
                "src/CFIP.Indicator/Analysis/Indicators/External/" + filename
            )
        )
        or (
            filename != "SkenderObv.cs"
            and ".LastOrDefault()" in read(
                "src/CFIP.Indicator/Analysis/Indicators/External/" + filename
            )
        )
    )
        and ".ToList()" not in read(
            "src/CFIP.Indicator/Analysis/Indicators/External/" + filename
        )
        for filename in rolling_adapters
    )
)

check(
    "zero-volume source values are not promoted to artificial unit volume",
    "OssQuoteProjectionRule.NormalizeVolume(" in cache
    and "Math.Max(1m," not in cache
)
check(
    "FacioQuo remains outside production cTrader assembly",
    'PackageReference Include="Skender.Stock.Indicators" Version="2.7.3"' in production_project
    and "FacioQuo.Stock.Indicators" not in production_project
)

check(
    "runtime contracts execute canonical OSS window semantics",
    "VerifyOssQuoteProjectionSemantics();" in runtime
    and "VerifyOssQuoteWindowSemantics();" in runtime
    and "OssQuoteWindowRule.cs" in runtime_project
    and "OssQuoteProjectionRule.cs" in runtime_project
    and "CI-02 OSS quote-window semantics contracts PASS" in runtime
    and "CI-02 OSS quote projection semantics contracts PASS" in runtime
)

required_benchmark_tokens = (
    "class SkenderProductionParityBenchmark",
    "StableWindowSize = 768",
    "RollingWindowSize = 161",
    "CompareScenario(",
    "BuildZeroVolumeVariant(",
    "EvaluateRsi(",
    "EvaluateMacd(",
    "EvaluateBollinger(",
    "EvaluateMfi(",
    "EvaluateStoch(",
    "EvaluateSuperTrend(",
    "EvaluateAroon(",
    "EvaluateCci(",
    "EvaluateObvDirection(",
    "EvaluateParabolicSar(",
)
check(
    "CI-02 benchmark covers all production OSS families and zero-volume fixtures",
    all(token in benchmark for token in required_benchmark_tokens)
)

check(
    "CI-02 benchmark checks stable direction and rolling exact parity",
    "directionMismatches == 0" in benchmark
    and "exactMismatches == 0" in benchmark
    and "NonFinitePairs" in benchmark
)

check(
    "CI-02 benchmark measures runtime and allocations",
    "MeasureTiming(false)" in benchmark
    and "MeasureTiming(true)" in benchmark
    and "GC.GetAllocatedBytesForCurrentThread()" in benchmark
)

check(
    "CI-02 benchmark is executed by the benchmark program",
    "SkenderProductionParityBenchmark.Measure()" in benchmark_program
    and "productionParity.Passed" in benchmark_program
)

check(
    "CI-02 benchmark result is surfaced in the report",
    "CI-02 production Skender boundary parity" in benchmark_report
    and "Bounded-window mean ms/iteration" in benchmark_report
)

check(
    "OSS benchmark workflow executes the production benchmark",
    "CFIP.StockIndicators.Benchmark.csproj" in benchmark_workflow
    and "dotnet run" in benchmark_workflow
)

check(
    "CI-02 static audit is accumulated after CI-01",
    "audit_phase_ci_00.py" in workflow
    and "audit_phase_ci_01.py" in workflow
    and "audit_phase_ci_02.py" in workflow
    and workflow.index("audit_phase_ci_02.py")
    > workflow.index("audit_phase_ci_01.py")
)

print("CI-02 OSS PARITY / WARM-UP / CACHE SUMMARY")
print("=" * 72)
print("Errors: " + str(len(errors)))

if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-02 STATIC GATE PASS")
