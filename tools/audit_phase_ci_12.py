#!/usr/bin/env python3
"""Static acceptance gate for CI-12 structural stop integrity."""
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

geometry = read("src/CFIP.Indicator/Core/Math/StructuralStopGeometryRule.cs")
snapshot = read("src/CFIP.Indicator/Core/Models/StructuralStopGeometrySnapshot.cs")
risk = read("src/CFIP.Indicator/Core/Math/StructuralStopRiskRule.cs")
collector = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs")
evaluator = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
selector = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateSelector.cs")
planner = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopPlanner.cs")
plan_input = read("src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs")
preview = read("src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs")
parallel = read("src/CFIP.Indicator/Analysis/Market/ParallelScenarioComputation.cs")
pending = read("src/CFIP.Indicator/Trading/Lifecycle/PendingFillPlanBuilder.cs")
recovery = read("src/CFIP.Indicator/Trading/Lifecycle/ManagedLivePlanRecovery.cs")
orphan = read("src/CFIP.Indicator/Trading/Execution/Aggressive/OrphanManagedProtection.cs")
prediction = read("src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
runtime_contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
historical_roadmap = read("docs/archive/ROADMAP-LEGACY-2026-10-04.md")
continuation = read("docs/CONTINUATION-STATE.md")
phase_doc = read("docs/PHASE-CI-12-STRUCTURAL-SL.md")

check(
    "one canonical structural stop geometry owner exists",
    "class StructuralStopGeometryRule" in geometry and
    "Evaluate(" in geometry and
    "EvaluateFallback(" in geometry and
    "ResolveBufferAtr(" in geometry and
    "StructuralStopGeometrySnapshot" in snapshot
)

check(
    "structural geometry owns buffer calculation and final normalization",
    "ResolveBufferAtr(" in geometry and
    "NormalizePrice(" in geometry and
    "StructuralStopGeometryRule.Evaluate(" in evaluator
)

check(
    "candidate evaluation uses the evaluated stop instead of rematerializing it",
    "out double bestStop" in evaluator and
    "selectedStop = stop;" in evaluator and
    "out double selectedStop" in selector and
    "return selectedStop;" in selector
)

check(
    "duplicate structural stop finalizer is removed",
    not (ROOT / "src/CFIP.Indicator/Planning/TradePlan/StructuralStopFinalizer.cs").exists() and
    "MaterializeStructuralStop(" not in selector and
    "MaterializeStructuralStop(" not in planner
)

check(
    "structural candidate path preserves broker-distance validation after geometry",
    "StructuralStopGeometryRule.Evaluate(" in evaluator and
    "IsValidStop(" in evaluator and
    "geometry.Stop" in evaluator
)

check(
    "candidate risk envelope is canonical and cannot be raised by spread",
    "IsWithinPlanningRiskEnvelope(" in risk and
    "StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(" in evaluator and
    "EffectiveMaximumStopRiskAtr(" in risk and
    "maximumRiskAtr =" in evaluator
)

check(
    "plan fallback uses the same canonical fallback geometry",
    "StructuralStopGeometryRule.EvaluateFallback(" in plan_input and
    "StructuralStopGeometryRule.EvaluateFallback(" in preview and
    "StructuralStopGeometryRule.EvaluateFallback(" in parallel
)

check(
    "lifecycle fallback paths are centralized and fail closed on invalid risk/geometry",
    "StructuralStopGeometryRule.EvaluateFallback(" in pending and
    "StructuralStopGeometryRule.EvaluateFallback(" in recovery and
    "StructuralStopGeometryRule.EvaluateFallback(" in orphan and
    "IsWithinPlanningRiskEnvelope(" in pending and
    "IsWithinPlanningRiskEnvelope(" in recovery and
    "IsWithinPlanningRiskEnvelope(" in orphan
)

check(
    "early prediction fallback uses the canonical geometry owner",
    "StructuralStopGeometryRule.EvaluateFallback(" in prediction
)

legacy_formula = "entry - atr * FallbackSlAtr"
legacy_formula_sell = "entry + atr * FallbackSlAtr"
check(
    "legacy inline fallback arithmetic is absent from planning/lifecycle paths",
    legacy_formula not in plan_input and
    legacy_formula_sell not in plan_input and
    legacy_formula not in preview and
    legacy_formula_sell not in preview and
    legacy_formula not in parallel and
    legacy_formula_sell not in parallel and
    legacy_formula not in pending and
    legacy_formula_sell not in pending and
    legacy_formula not in recovery and
    legacy_formula_sell not in recovery and
    legacy_formula not in orphan and
    legacy_formula_sell not in orphan and
    legacy_formula not in prediction and
    legacy_formula_sell not in prediction
)

check(
    "planning preview and parallel geometry are still risk bounded",
    "IsWithinPlanningRiskEnvelope(" in preview and
    "IsWithinPlanningRiskEnvelope(" in parallel
)

check(
    "M5 and HTF structural candidates remain the existing source families",
    "FindSwingLowBelow(" in collector and
    "FindSwingHighAbove(" in collector and
    "FindNearestFvg(" in collector and
    "FindNearestOrderBlock(" in collector and
    "M15" in collector and "H1" in collector and "H4" in collector
)

check(
    "runtime project compiles the CI-12 Core owners",
    "Core/Models/StructuralStopGeometrySnapshot.cs" in runtime_project and
    "Core/Math/StructuralStopGeometryRule.cs" in runtime_project and
    "Core/Math/StructuralStopRiskRule.cs" in runtime_project
)

check(
    "deterministic CI-12 contracts cover geometry and risk boundaries",
    "VerifyStructuralStopGeometryCi12();" in runtime_contracts and
    "CI-12 structural stop geometry and risk contracts PASS" in runtime_contracts and
    "spread pressure cannot raise the configured maximum stop-risk ceiling" in runtime_contracts
)

check(
    "CI-12 audit is wired after CI-11",
    "audit_phase_ci_11.py" in workflow and
    "audit_phase_ci_12.py" in workflow and
    workflow.index("audit_phase_ci_12.py") > workflow.index("audit_phase_ci_11.py")
)

check(
    "CI-12 phase documentation is recorded",
    "CI-12 — Structural SL" in historical_roadmap and
    "PHASE-CI-12-STRUCTURAL-SL.md" in historical_roadmap and
    "PHASE-CI-12-STRUCTURAL-SL.md" in continuation and
    "## Active phase" in continuation and
    "CI-17" in continuation
)

check(
    "CI-12 phase document states the full audit boundary",
    all(token in phase_doc for token in (
        "M5 swing", "M5 FVG", "M5 OB", "HTF structure",
        "buffer", "minimum risk", "maximum risk", "spread",
        "broker distance", "fallback", "BUY/SELL symmetry"
    ))
)

print("CI-12 STRUCTURAL SL INTEGRITY SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-12 STATIC GATE PASS")
