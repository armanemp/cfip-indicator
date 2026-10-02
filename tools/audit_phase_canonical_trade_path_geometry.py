#!/usr/bin/env python3
"""Static acceptance audit for canonical actual-entry trade-path geometry."""

from pathlib import Path
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


model = read("src/CFIP.Indicator/Core/Models/CanonicalTradePathGeometry.cs")
builder = read("src/CFIP.Indicator/Planning/TradePlan/CanonicalTradePathGeometryBuilder.cs")
actionability = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
preparation = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityPreparation.cs")
gates = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityGateEvaluation.cs")
plan_builder = read("src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs")
plan_inputs = read("src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs")
trace = read("src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
workflow = read(".github/workflows/source-check.yml")

check(
    "canonical trade-path model carries actual entry, SL, TP1 and RR",
    all(token in model for token in (
        "public double Entry",
        "public double Stop",
        "public double Tp1",
        "public double Risk",
        "public double Tp1RR",
    )),
)

check(
    "canonical builder derives stop from actual execution entry",
    "execution.ActualEntry" in builder and
    "BuildStructuralStop(" in builder and
    "BuildTargetLevels(" in builder and
    "SelectTargets(" in builder and
    "TryBuildPlanTargets(" in builder,
)

check(
    "canonical builder validates stop risk envelope and target path",
    "StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(" in builder and
    "reason = "TARGET PATH INVALID"" in builder,
)

check(
    "actionability consumes canonical actual-entry geometry",
    "TryBuildCanonicalTradePathGeometry(" in preparation and
    "state.EffectivePreview" in preparation and
    "TryPrepareTradeActionability(" in actionability,
)

check(
    "actionability rejects execution/actionability entry or mode drift",
    "EXECUTION GEOMETRY / ACTIONABILITY MISMATCH" in preparation and
    "canonicalPath.EntryMode != state.LiveMode" in preparation,
)

check(
    "actionability reward calculation uses canonical SL/TP rather than ideal preview",
    "state.EffectivePreview.Stop" in preparation and
    "state.EffectivePreview.Tp1" in preparation,
)

check(
    "canonical actionability path keeps bounded live-path caching",
    "_canonicalTradePathCache" in state and
    "_canonicalTradePathCacheEntry" in state and
    "_canonicalTradePathCacheSpread" in state and
    "entryTolerance" in builder and
    "spreadTolerance" in builder,
)

check(
    "existing plan builder uses actual execution entry",
    "execution.ActualEntry" in plan_inputs and
    "BuildStructuralStop(" in plan_inputs and
    "BuildTargetLevels(" in plan_builder,
)

check(
    "trace lifecycle reports actionable state before generic trigger state",
    "if (decision.ActionableNow)" in trace and
    trace.index("if (decision.ActionableNow)") <
    trace.index('if (!decision.TriggerReady)'),
)

check(
    "historical trace geometry reuses the canonical actual-entry path",
    "TryBuildCanonicalTradePathGeometry(" in trace and
    '"CANONICAL-ACTIONABILITY"' in trace and
    "canonicalPreview.Entry" in trace and
    "canonicalPreview.Stop" in trace and
    "canonicalPreview.Tp1" in trace,
)

check(
    "panel lifecycle reports actionable state before trigger waiting",
    "_decision.ActionableNow" in panel and
    panel.index('_decision.ActionableNow
                                                            ? "TRIGGER  NOT REQUIRED') <
    panel.index('_decision.TriggerReady
                                                                ? "TRIGGER  CONFIRMED"'),
)

check(
    "canonical trade-path audit is accumulated in Source/Architecture",
    "audit_phase_canonical_trade_path_geometry.py" in workflow,
)

if errors:
    print("CANONICAL TRADE-PATH GEOMETRY AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CANONICAL TRADE-PATH GEOMETRY AUDIT: PASS")
