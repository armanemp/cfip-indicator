#!/usr/bin/env python3
"""F3 static gate: price/time/closed-bar/MTF boundary integrity."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(path):
    p = ROOT / path
    if not p.exists():
        errors.append("missing file: " + path)
        return ""
    return p.read_text(encoding="utf-8")

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

closed = read("src/CFIP.Indicator/Core/Math/ClosedBarReferenceRule.cs")
index_math = read("src/CFIP.Indicator/Analysis/Market/Math/IndexMath.cs")
mtf = read("src/CFIP.Indicator/Runtime/Mtf/MtfContextBuilder.cs")
cache = read("src/CFIP.Indicator/Runtime/Mtf/MtfClosedContextCache.cs")
scenario = read("src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs")
direction = read("src/CFIP.Indicator/Analysis/Market/Decision/DirectionAcceptanceGate.cs")
pending = read("src/CFIP.Indicator/Planning/Execution/PredictivePendingLevelSelector.cs")
zones = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs")
stops = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
stop_collect = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs")
micro = read("src/CFIP.Indicator/Planning/TradePlan/Sources/M1MicroTargetSource.cs")
liquidity = read("src/CFIP.Indicator/Planning/TradePlan/Sources/SupplyDemandLiquidityTargetSource.cs")
prep = read("src/CFIP.Indicator/Runtime/Calculation/CalculationPreparation.cs")
closed_stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")

check(
    "ClosedBarReferenceRule is the canonical fully-closed index owner",
    "ResolveClosedIndex(" in closed and
    "IsFullyClosed(" in closed and
    "nextOpenTime <= reference" in closed
)

check(
    "closed-bar boundary reference has one reusable owner",
    "ClosedBarBoundaryReference(" in index_math and
    "bars.OpenTimes[closedIndex + 1]" in index_math and
    "nextOpen <= open" in index_math
)

check(
    "MTF context resolves all eight canonical closed indices from one reference",
    mtf.count("ClosedIndex(") == 8 and
    "BuildMtfClosedContext(" in mtf and
    "reference," in mtf
)

check(
    "MTF cache cannot reuse a context after any series crosses its next open",
    cache.count("IsReferenceStable(") >= 8 and
    "bars.OpenTimes[closedIndex + 1] > reference" in cache and
    "_context.WithReference(reference)" in cache
)

check(
    "scenario MTF lookups use the closed-bar boundary, not the closed bar's opening time",
    "ClosedBarBoundaryReference(" in scenario and
    "_m5Bars.OpenTimes[closedM5]" not in scenario
)

production = {
    "DirectionAcceptanceGate": direction,
    "PredictivePendingLevelSelector": pending,
    "ExecutionZoneCandidateSelectionCore": zones,
    "StructuralStopCandidateEvaluator": stops,
    "StructuralStopCandidateCollector": stop_collect,
    "M1MicroTargetSource": micro,
}
for name, text in production.items():
    check(
        f"{name} does not resolve MTF/M1 closed indices from the opening timestamp of the closed M5",
        "_m5Bars.OpenTimes[closedM5]" not in text
    )

check(
    "liquidity target age uses the same closed-bar boundary reference",
    "ClosedBarBoundaryReference(" in liquidity and
    "_m5Bars.OpenTimes[closedM5]" not in liquidity
)

check(
    "calculation captures one current UTC reference before MTF context creation",
    "DateTime now =\n                Server.TimeInUtc" in prep and
    "BuildMtfClosedContext(\n                    reference)" in prep
)

check(
    "closed-bar analysis consumes the prepared canonical MTF context",
    "RunClosedBarAnalysisStage(" in closed_stage and
    "MtfClosedContext mtf" in prep
)

check(
    "F3 gate is wired into Source / Architecture CI",
    "audit_phase_f3_price_time_closedbar_mtf_integrity.py" in workflow
)

check(
    "F3 roadmap and continuation state are recorded",
    "F3 — Price / Time / Closed-Bar / MTF Integrity" in roadmap and
    "F3 — Price / Time / Closed-Bar / MTF Integrity" in continuation
)

print("=" * 72)
print("F3 PRICE / TIME / CLOSED-BAR / MTF INTEGRITY SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("F3 STATIC GATE PASS")
