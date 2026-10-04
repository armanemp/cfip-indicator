#!/usr/bin/env python3
"""Static acceptance gate for CI-11 entry geometry and signal timing integrity."""
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

geometry_model = read("src/CFIP.Indicator/Core/Models/EntryGeometrySnapshot.cs")
geometry_rule = read("src/CFIP.Indicator/Core/Math/EntryGeometryRule.cs")
timing_model = read("src/CFIP.Indicator/Core/Models/EntrySignalTiming.cs")
timing_rule = read("src/CFIP.Indicator/Core/Math/EntrySignalTimingRule.cs")
execution_model = read("src/CFIP.Indicator/Core/Models/ExecutionModel.cs")
plan_model = read("src/CFIP.Indicator/Core/Models/Plan.cs")
preview_model = read("src/CFIP.Indicator/Core/Models/TradeSetupPreview.cs")
execution_builder = read("src/CFIP.Indicator/Planning/Execution/ExecutionModelBuilder.cs")
execution_resolver = read("src/CFIP.Indicator/Planning/Execution/ExecutionModeResolver.cs")
plan_input = read("src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs")
market_entry = read("src/CFIP.Indicator/Planning/Execution/MarketEntryValidation.cs")
plan_market = read("src/CFIP.Indicator/Planning/TradePlan/PlanMarketConstraintValidator.cs")
actionability = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
preview_builder = read("src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs")
pending_snapshot = read("src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs")
pending_fill = read("src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs")
trigger_runtime = read("src/CFIP.Indicator/Planning/Entry/M1TriggerRuntimeUpdater.cs")
trigger_state = read("src/CFIP.Indicator/Planning/Entry/TriggerRuntimeState.cs")
timing_runtime = read("src/CFIP.Indicator/Trading/Intelligence/EntrySignalTimingRuntime.cs")
runtime_log = read("src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs")
live_cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs")
contracts_project = read("tools/CFIP.Decision.Contracts/CFIP.Decision.Contracts.csproj")
contracts = read("tools/CFIP.Decision.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
historical_roadmap = read("docs/archive/ROADMAP-LEGACY-2026-10-04.md")

check(
    "canonical entry geometry model/rule exist with all required concepts",
    "EntryGeometrySnapshot" in geometry_model and "struct EntryGeometrySnapshot" in geometry_model and
    "class EntryGeometryRule" in geometry_rule and
    "ZoneLow" in geometry_model and
    "ZoneHigh" in geometry_model and
    "ZoneTolerance" in geometry_model and
    "IdealEntry" in geometry_model and
    "Trigger" in geometry_model and
    "Anchor" in geometry_model and
    "ActualEntry" in geometry_model and
    "EntryDistanceAtr" in geometry_model and
    "TriggerExtensionAtr" in geometry_model and
    "IsLate" in geometry_model
)

check(
    "geometry owns one mode/inside/trigger/late calculation",
    "EntryGeometryRule.Evaluate(" in execution_resolver and
    "EntryGeometryRule.Evaluate(" in actionability and
    "EntryGeometryRule.Evaluate(" in plan_input and
    "EntryGeometryRule.Evaluate(" in plan_market
)

check(
    "execution model carries structural zone tolerance",
    "ZoneTolerance" in execution_model and
    "model.ZoneTolerance" in execution_builder
)

check(
    "plan and presentation preview preserve the same geometry inputs",
    "EntryZoneTolerance" in plan_model and
    "EntryZoneTolerance" in plan_input and
    "EntryZoneTolerance" in pending_snapshot and
    "EntryZoneTolerance" in pending_fill and
    "ZoneTolerance" in preview_model and
    "ZoneTolerance = Math.Max(0, execution.ZoneTolerance)" in preview_builder
)

check(
    "actionability no longer has an independent zone/anchor/late equation",
    "EntryGeometryRule.Evaluate(" in actionability and
    "EntryActionabilityPolicy.IsLate(" not in actionability and
    "EntryActionabilityPolicy.ResolveAnchor(" not in actionability
)

check(
    "market execution uses canonical trigger/zone predicates",
    "EntryGeometryRule.IsTriggerReached(" in market_entry and
    "EntryGeometryRule.IsInsideZone(" in market_entry and
    "GetCanonicalPriceSnapshot()" in market_entry
)

check(
    "plan market constraints consume canonical late geometry",
    "EntryGeometryRule.Evaluate(" in plan_market and
    "geometry.IsLate" in plan_market
)

check(
    "legacy actionability late API is only a compatibility facade",
    "return EntryGeometryRule.EvaluateLate(" in read("src/CFIP.Indicator/Core/Math/EntryActionabilityPolicy.cs")
)

check(
    "M1 trigger confirmation carries a causal timestamp",
    "ConfirmationUtc" in trigger_state and
    "ResolveClosedBarBoundaryUtc(" in trigger_runtime and
    "_triggerRuntime.ConfirmationUtc" in trigger_runtime
)

check(
    "signal timing is measured from causal event to first actionable event",
    "EntrySignalTiming" in timing_model and "struct EntrySignalTiming" in timing_model and
    "class EntrySignalTimingRule" in timing_rule and
    "EntrySignalTimingRule.Measure(" in timing_runtime and
    "ResolveEntrySignalCausalEventUtc(" in timing_runtime and
    "UpdateEntrySignalTiming(" in live_cycle
)

check(
    "M1 trigger causal time is preferred over display-bar time",
    "UseM1Trigger" in timing_runtime and
    "_triggerRuntime.ConfirmationUtc" in timing_runtime and
    "_m5Bars.OpenTimes[closedM5 + 1]" in timing_runtime
)

check(
    "timing telemetry is persisted without becoming a trading gate",
    "ACTIONABILITY_TIMING" in runtime_log and
    "Diagnostics must never become a trading failure path" in runtime_log and
    "_entrySignalTiming" in read("src/CFIP.Indicator/Indicator/State.cs")
)

check(
    "deterministic CI-11 contracts cover geometry symmetry, late states and timing",
    "VerifyEntryGeometry();" in contracts and
    "VerifyEntrySignalTiming();" in contracts and
    "EntryGeometryRule.Evaluate(" in contracts and
    "negative signal latency fails closed" in contracts
)

check(
    "contracts project explicitly compiles CI-11 Core owners",
    "Core/Models/EntryGeometrySnapshot.cs" in contracts_project and
    "Core/Math/EntryGeometryRule.cs" in contracts_project and
    "Core/Models/EntrySignalTiming.cs" in contracts_project and
    "Core/Math/EntrySignalTimingRule.cs" in contracts_project
)

check(
    "CI-11 static audit is wired after CI-10",
    "audit_phase_ci_10.py" in workflow and
    "audit_phase_ci_11.py" in workflow and
    workflow.index("audit_phase_ci_11.py") > workflow.index("audit_phase_ci_10.py")
)

check(
    "roadmap identifies CI-11 as the active entry-geometry/timing phase",
    "CI-11 — Entry geometry and signal-timing audit" in historical_roadmap
)

print("CI-11 ENTRY GEOMETRY / SIGNAL TIMING INTEGRITY SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-11 STATIC GATE PASS")
