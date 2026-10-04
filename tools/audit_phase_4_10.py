#!/usr/bin/env python3
"""Static acceptance gate for CR4.10 / D10 native-indicator safety and registry performance."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


# Direct native-holder inventory deliberately excludes calls to the public
# wrapper methods (Atr/Rsi/Ema/etc.), which are too broad for consumer inventory.
# It tracks GetNative() and field access on the Native holder itself.
direct_native_access_re = re.compile(
    r"\bGetNative\s*\(|"
    r"\b(?:set|native)\.(?:Atr|Rsi|Dms|Fast|Slow|MacdFast|MacdSlow)\b"
)
actual_native_callers = set()

for path in (ROOT / "src" / "CFIP.Indicator").rglob("*.cs"):
    rel = path.relative_to(ROOT).as_posix()
    content = path.read_text(encoding="utf-8")
    if direct_native_access_re.search(content):
        actual_native_callers.add(rel)

expected_native_owner_files = {
    "src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs",
    "src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs",
    "src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs",
    "src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs",
    "src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs",
    "src/CFIP.Indicator/Analysis/Indicators/MacdIndicator.cs",
    "src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs",
    "src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs",

    "src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs",

}


registry = read("src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs")
native = read("src/CFIP.Indicator/Analysis/Indicators/Native/Native.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
readiness = read("src/CFIP.Indicator/Core/Math/NativeIndicatorReadinessRule.cs")
numeric = read("src/CFIP.Indicator/Core/Math/NumericGuards.cs")
atr = read("src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs")
rsi = read("src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs")
adx = read("src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs")
dmi = read("src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs")
ema = read("src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs")
macd = read("src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs")
frame = read("src/CFIP.Indicator/Analysis/Market/Models/Frame.cs")
evidence = read("src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs")
scoring = read("src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs")
regime = read("src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs")
reaction = read("src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
benchmark = read("tools/CFIP.StockIndicators.Benchmark/D10/NativeRegistryLookupBenchmark.cs")
benchmark_program = read("tools/CFIP.StockIndicators.Benchmark/Program.cs")
workflow = read(".github/workflows/source-check.yml")
phase_doc = read("docs/PHASE-CR4-10-NATIVE-INDICATOR-SAFETY.md")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")


check(
    "native indicator direct-holder inventory is present",
    expected_native_owner_files.issubset(actual_native_callers) and
    len(actual_native_callers) >= len(expected_native_owner_files),
)
check(
    "Native remains a storage-only indicator holder",
    "class Native" in native and
    "Execute" not in native and
    "Submit" not in native and
    "Decision" not in native,
)
check(
    "registry uses deterministic Bars-reference dictionary",
    "Dictionary<Bars, Native>" in state and
    "NativeBarsReferenceComparer" in state and
    "TryGetValue(" in registry and
    "ReferenceEquals(x, y)" in registry and
    "RuntimeHelpers.GetHashCode(obj)" in registry and
    "_native.FirstOrDefault" not in registry,
)
check(
    "readiness rule is centralized and platform-neutral",
    "class NativeIndicatorReadinessRule" in readiness and
    "IsIndexedSeriesReady(" in readiness and
    "IsIndexedWindowReady(" in readiness and
    "IsFrameReady(" in readiness and
    "NumericGuards.IsFiniteValue" in readiness,
)
check(
    "ATR is fail-closed at warm-up boundary",
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in atr and
    "set.Atr.Result.Count" in atr and
    "Math.Max(2, AtrPeriod)" in atr,
)
check(
    "RSI warm-up/invalid data cannot fabricate frame evidence",
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in rsi and
    "Math.Max(2, RsiPeriod)" in rsi and
    "return 50;" in rsi,
)
check(
    "ADX/DMI non-finite data remains fail-closed",
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in adx and
    "DmiBiasRule.Calculate(" in dmi and
    "IsDmiFiniteNonNegative(" in read(
        "src/CFIP.Indicator/Core/Math/DmiBiasRule.cs"
    ),
)
check(
    "EMA and MACD prior-sample reads are readiness bounded",
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in ema and
    "NativeIndicatorReadinessRule.IsIndexedWindowReady(" in macd and
    "previousIndex" in macd,
)
check(
    "frame scoring is blocked until all core native inputs are ready",
    "NativeIndicatorsReady" in frame and
    "IsFrameReady(" in evidence and
    "if (!f.NativeIndicatorsReady)" in evidence and
    "!f.NativeIndicatorsReady" in scoring,
)
check(
    "regime and reaction consumers fail closed on unusable native values",
    "IsFinitePositiveNative(fast)" in regime and
    "IsFinitePositiveNative(slow)" in regime and
    "if (atr <= 0)" in reaction,
)
check(
    "runtime contracts cover readiness and fabricated-neutral safety",
    "VerifyNativeIndicatorReadinessSemantics();" in contracts and
    "fabricated RSI-neutral value 50 cannot create directional evidence" in contracts and
    "native frame readiness rejects an unusable EMA/volatility value" in contracts,
)
check(
    "runtime contract project includes the D10 owner",
    "NativeIndicatorReadinessRule.cs" in contracts_project,
)
check(
    "registry benchmark compares reference-list and reference-dictionary lookup",
    "RunLinearLookup" in benchmark and
    "RunDictionaryLookup" in benchmark and
    "LookupCount = 1_000_000" in benchmark and
    "ReferenceKeyComparer" in benchmark,
)
check(
    "benchmark is executed by the existing benchmark entry point",
    "NativeRegistryLookupBenchmark.Measure()" in benchmark_program,
)
check(
    "D10 static gate is wired after CR4.9",
    "audit_phase_4_9.py" in workflow and
    "audit_phase_4_10.py" in workflow and
    workflow.index("audit_phase_4_10.py") >
    workflow.index("audit_phase_4_9.py"),
)
check(
    "project documentation records D10 completion and CR-FINAL transition",
    "CR4.10 / D10" in roadmap and
    "implementation complete" in roadmap.lower() and
    "CR4.10" in continuation and
    "CR-FINAL" in continuation and
    "CR4.10" in review and
    "CR-FINAL" in phase_doc,
)
phase_doc_lower = phase_doc.lower().replace("`", "")
check(
    "D10 preserves no-tuning and manual-terminal boundaries",
    "no public parameter name/type/" in phase_doc_lower and
    "defaultvalue changed" in phase_doc_lower and
    "no default rr" in phase_doc_lower and
    "target-terminal" in phase_doc_lower,
)

print("CR4.10 SUMMARY")
print("=" * 72)
print(f"Expected native-owner files: {len(expected_native_owner_files)}")
print(f"Actual native-call files: {len(actual_native_callers)}")
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR4.10 STATIC GATE PASS")
