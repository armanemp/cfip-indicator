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
state = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluationState.cs")
plan_builder = read("src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs")
plan_inputs = read("src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs")
trace = read("src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs")
state_store = read("src/CFIP.Indicator/Indicator/State.cs")
execution_model = read("src/CFIP.Indicator/Core/Models/ExecutionModel.cs")
execution_resolver = read("src/CFIP.Indicator/Planning/Execution/ExecutionModeResolver.cs")
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
    "canonical builder derives stop and targets from actual execution entry",
    "execution.ActualEntry" in builder and
    "BuildStructuralStop(" in builder and
    "BuildTargetLevels(" in builder and
    "SelectTargets(" in builder and
    "TryBuildPlanTargets(" in builder,
)

check(
    "canonical builder validates stop risk envelope and target path",
    "StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(" in builder and
    'reason = "TARGET PATH INVALID"' in builder,
)

check(
    "actionability uses the prepared canonical path",
    "TryPrepareTradeActionability(" in actionability and
    "EvaluatePreparedTradeActionability(" in actionability and
    "TryBuildCanonicalTradePathGeometry(" in preparation and
    "state.EffectivePreview" in preparation,
)

check(
    "actionability rejects execution/actionability entry or mode drift",
    "EXECUTION GEOMETRY / ACTIONABILITY MISMATCH" in preparation and
    "canonicalPath.EntryMode != state.LiveMode" in preparation,
)

check(
    "actionability reward calculation uses canonical SL/TP",
    "state.EffectivePreview.Stop" in preparation and
    "state.EffectivePreview.Tp1" in preparation and
    "state.Tp1RR = canonicalPath.Tp1RR" in preparation,
)

check(
    "actionability gates remain one final canonical sequence",
    "if (!state.QualityReady)" in gates and
    "if (!IsActionabilityTriggerReady(" in gates and
    "if (state.Tp1RR <" in gates and
    "return new TradeActionabilityResult(" in gates,
)

check(
    "actionability refactor keeps bounded state container and no oversized owner",
    "class TradeActionabilityEvaluationState" in state and
    len(actionability) < 20 * 1024 and
    len(preparation) < 20 * 1024 and
    len(gates) < 20 * 1024,
)

check(
    "late-entry policy remains a real execution-model setting",
    "public bool IsLate" in execution_model and
    "model.IsLate = geometry.IsLate" in execution_resolver and
    "(!AvoidLateEntry ||" in execution_resolver and
    "if (AvoidLateEntry &&" in builder,
)

check(
    "canonical actionability path keeps bounded live-path caching",
    "_canonicalTradePathCache" in state_store and
    "_canonicalTradePathCacheEntry" in state_store and
    "_canonicalTradePathCacheSpread" in state_store and
    "entryTolerance" in builder and
    "spreadTolerance" in builder,
)

check(
    "plan preparation consumes the canonical actual-entry trade path",
    "OpportunityLane lane" in plan_inputs and
    "TryBuildCanonicalTradePathGeometry(" in plan_inputs and
    "canonicalPath.Entry" in plan_inputs and
    "canonicalPath.Stop" in plan_inputs and
    "canonicalPath.Risk" in plan_inputs,
)

check(
    "PlanBuilder has one canonical geometry owner and no duplicate SL/TP construction",
    "TryPreparePlanInputs(" in plan_builder and
    "CanonicalTradePathGeometry" in plan_builder and
    "EnrichPlanTargetMetadata(" not in plan_builder and
    "BuildStructuralStop(" not in plan_builder and
    "BuildTargetLevels(" not in plan_builder and
    "SelectTargets(" not in plan_builder and
    "TryBuildPlanTargets(" not in plan_builder,
)

check(
    "canonical path carries selected TP provenance",
    all(token in model for token in (
        "Tp1Source",
        "Tp1Quality",
        "Tp2Source",
        "Tp2Quality",
        "Tp3Source",
        "Tp3Quality",
        "Tp4Source",
        "Tp4Quality",
        "HtfTargetCount",
    )),
)

check(
    "trace lifecycle reports actionable state before generic trigger state",
    "if (decision.ActionableNow)" in trace and
    trace.index("if (decision.ActionableNow)") <
    trace.index("if (!decision.TriggerReady)"),
)

check(
    "historical trace geometry reuses canonical actual-entry path",
    "TryBuildCanonicalTradePathGeometry(" in trace and
    '"CANONICAL-ACTIONABILITY"' in trace and
    "canonicalPreview.Entry" in trace and
    "canonicalPreview.Stop" in trace and
    "canonicalPreview.Tp1" in trace,
)

check(
    "panel lifecycle reports actionable state before trigger waiting",
    "TRIGGER  NOT REQUIRED • ACTIONABLE" in panel and
    panel.index("TRIGGER  NOT REQUIRED • ACTIONABLE") <
    panel.index("TRIGGER  CONFIRMED"),
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
