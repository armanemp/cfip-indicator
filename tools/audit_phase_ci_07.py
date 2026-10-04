#!/usr/bin/env python3
"""Static acceptance gate for CI-07 MTF, regime and market-context integrity."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append("missing file: " + relative)
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


mtf_builder = read("src/CFIP.Indicator/Runtime/Mtf/MtfContextBuilder.cs")
mtf_cache = read("src/CFIP.Indicator/Runtime/Mtf/MtfClosedContextCache.cs")
mtf_model = read("src/CFIP.Indicator/Runtime/Mtf/MtfClosedContext.cs")
regime = read("src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs")
regime_model = read("src/CFIP.Indicator/Core/Models/MarketRegimeSnapshot.cs")
transition = read("src/CFIP.Indicator/Core/Math/MarketRegimeTransitionRule.cs")
frame = read("src/CFIP.Indicator/Analysis/Market/Models/Frame.cs")
frame_evidence = read("src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs")
state_builder = read("src/CFIP.Indicator/Analysis/Market/MarketStateSnapshotBuilder.cs")
state_model = read("src/CFIP.Indicator/Core/Models/MarketStateSnapshot.cs")
state_frame_model = read("src/CFIP.Indicator/Core/Models/MarketStateFrameSnapshot.cs")
closed_stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
decision = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
request = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputBuildRequest.cs")
factory = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs")
suitability = read("src/CFIP.Indicator/Trading/Risk/SuitabilityCalculator.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
historical_roadmap = read("docs/archive/ROADMAP-LEGACY-2026-10-04.md")
continuation = read("docs/CONTINUATION-STATE.md")

production_files = list((ROOT / "src" / "CFIP.Indicator").rglob("*.cs"))


check(
    "MTF builder resolves exactly eight canonical closed indices",
    mtf_builder.count("ClosedIndex(") == 8 and
    "reference," in mtf_builder and
    "TryGetStableContext(" in mtf_builder
)

check(
    "MTF cache is reference-aware and re-materializes exact reference identity",
    "DateTime reference" in mtf_cache and
    "reference >= _context.Reference" in mtf_cache and
    mtf_cache.count("IsReferenceStable(") >= 8 and
    "_context.WithReference(reference)" in mtf_cache
)

check(
    "MTF cache rejects reuse when any cached timeframe has crossed its next bar",
    "bars.OpenTimes[closedIndex + 1] > reference" in mtf_cache and
    "closedIndex >= count - 1" in mtf_cache
)

check(
    "MTF model remains the single index owner",
    all(token in mtf_model for token in (
        "public int M5", "public int M1", "public int M15",
        "public int M30", "public int H1", "public int H4",
        "public int D1", "public int W1",
        "public MtfClosedContext WithReference("
    ))
)

check(
    "regime transition has one pure owner",
    transition.count("internal static class MarketRegimeTransitionRule") == 1 and
    transition.count("public static string ClassifyTransition(") == 1
)

check(
    "every analyzed timeframe carries previous regime and transition",
    "PreviousRegime" in regime_model and
    "RegimeTransition" in regime_model and
    "ApplyRegimeTransition(" in regime and
    "MarketRegimeTransitionRule.ClassifyTransition(" in regime and
    "PreviousRegime" in frame and
    "RegimeTransition" in frame and
    "RegimeTransition" in frame_evidence
)

check(
    "one canonical market-state snapshot aggregates all eight frames plus context",
    "internal sealed class MarketStateSnapshot" in state_model and
    "internal sealed class MarketStateFrameSnapshot" in state_frame_model and
    state_builder.count('BuildMarketStateFrameSnapshot(') >= 8 and
    "PremiumDiscountBias(" in state_builder and
    "SessionWindowRule.IsInside(" in state_builder
)

check(
    "market-state snapshot is built once before decision evaluation",
    "BuildMarketStateSnapshot(" in closed_stage and
    "_marketStateSnapshot =\n                BuildMarketStateSnapshot(" in closed_stage and
    "_marketStateSnapshot == null" in closed_stage
)

check(
    "decision consumes canonical snapshot regime and premium/discount state",
    "_marketStateSnapshot.M5.Regime" in decision and
    "_marketStateSnapshot.PremiumDiscountBias" in decision and
    "_marketStateSnapshot.M5.RegimeQuality" in decision and
    "MarketStateSnapshot =\n                        _marketStateSnapshot" in decision
)

check(
    "decision request/factory enforce snapshot reference and all eight MTF indices",
    "MarketStateSnapshot { get; set; }" in request and
    "ValidateMarketStateSnapshot(" in factory and
    "MatchesReference(request.Reference)" in factory and
    "IsAlignedWithClosedIndices(" in factory and
    all(f"request.ClosedContext.{name}" in factory for name in (
        "M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1"
    ))
)

check(
    "suitability reuses canonical session and D1 closed index",
    "_marketStateSnapshot.SessionOpen" in suitability and
    "_lastMtfClosedContext.D1" in suitability and
    "ClosedIndex(_d1Bars, nowUtc)" not in suitability
)

check(
    "runtime contracts register CI-07 determinism",
    "VerifyCi07MarketStateSemantics();" in runtime and
    "CI-07 snapshot preserves exact reference identity" in runtime and
    "MarketRegimeTransitionRule.ClassifyTransition" in runtime and
    "MarketStateSnapshot.cs" in runtime_project and
    "MarketRegimeTransitionRule.cs" in runtime_project
)

check(
    "CI-07 audit is accumulated immediately after CI-06",
    "audit_phase_ci_06.py" in workflow and
    "audit_phase_ci_07.py" in workflow and
    workflow.index("audit_phase_ci_07.py") > workflow.index("audit_phase_ci_06.py")
)

check(
    "CI-07 roadmap/continuation transition is recorded",
    "CI-07" in historical_roadmap and
    "CI-07" in continuation
)

print("CI-07 MTF / REGIME / MARKET CONTEXT SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-07 STATIC GATE PASS")
