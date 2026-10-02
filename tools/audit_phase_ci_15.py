#!/usr/bin/env python3
"""Static acceptance gate for CI-15 end-to-end execution geometry and broker boundary."""

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


intent_rule = read("src/CFIP.Indicator/Core/Math/ExecutionIntentGeometryRule.cs")
intent_builder = read("src/CFIP.Indicator/Planning/Execution/ExecutionIntentBuilder.cs")
intent_validation = read("src/CFIP.Indicator/Planning/Execution/ExecutionIntentValidation.cs")
market_validator = read("src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs")
market_execution = read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs")
aggressive_execution = ""
pending_stop = read("src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs")
pending_stop_cbot = read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs")
pending_limit = read("src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs")
pending_fill = read("src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs")
pending_snapshot = read("src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs")
fill_rule = read("src/CFIP.Indicator/Core/Math/ExecutionFillAcceptanceRule.cs")
submission = read("src/CFIP.Indicator/Trading/Execution/SubmissionGateCoordinator.cs")
planning_project = read("tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj")
planning_contracts = read("tools/CFIP.Planning.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
phase_doc = read("docs/PHASE-CI-15-EXECUTION-GEOMETRY-BROKER-BOUNDARY.md")

check(
    "one canonical execution-intent geometry owner exists",
    "class ExecutionIntentGeometryRule" in intent_rule
    and "Evaluate(" in intent_rule
    and "StopPips" in intent_rule
    and "TargetPips" in intent_rule
    and "PriceProtectionRule.ValidateStop(" in intent_rule
    and "PriceProtectionRule.ValidateTarget(" in intent_rule,
)

check(
    "ExecutionIntentBuilder delegates pip projection to canonical geometry",
    "ExecutionIntentGeometryRule.Evaluate(" in intent_builder
    and "StopPips = geometry.StopPips" in intent_builder
    and "TargetPips = geometry.TargetPips" in intent_builder
    and "Math.Abs(entry - stop)" not in intent_builder
    and "Math.Abs(target - entry)" not in intent_builder,
)

check(
    "ExecutionIntent validation re-checks exact pip geometry",
    "ExecutionIntentGeometryRule.Evaluate(" in intent_validation
    and "EXECUTION INTENT PIP GEOMETRY MISMATCH" in intent_validation
    and "intent.StopPips - geometry.StopPips" in intent_validation
    and "intent.TargetPips - geometry.TargetPips" in intent_validation,
)

check(
    "Automatic Market returns the validated intent",
    "out ExecutionIntent marketIntent" in market_validator
    and "marketIntent =" in market_validator
    and "ValidateExecutionIntent(" in market_validator,
)

check(
    "cBot Market broker path consumes canonical Market/Range geometry",
    "ExecuteMarketRangeOrder(" in market_execution
    and "ExecuteMarketOrder(" in market_execution
    and "stopPips" in market_execution
    and "targetPips" in market_execution
    and "MarketProfile" in market_execution,
)

check(
    "Aggressive Indicator broker owner is intentionally removed in P4A",
    aggressive_execution == ""
)

check(
    "Pending Stop handoff uses one intent from Indicator analysis to cBot broker submission/protection",
    "PrepareContinuationStopForCbot(" in pending_stop
    and "CapturePendingOrderPlanSnapshot(" in pending_stop
    and "PlaceStopOrder(" not in pending_stop
    and "envelope.Intent.RequestedEntry" in pending_stop_cbot
    and "envelope.Intent.Stop" in pending_stop_cbot
    and "envelope.Intent.InitialTarget" in pending_stop_cbot
    and "envelope.Intent.RequestedVolume" in pending_stop_cbot
    and "stopPips" in pending_stop_cbot
    and "targetPips" in pending_stop_cbot,
)

check(
    "Pending Limit broker path uses the same intent for submission and server protection",
    "pendingIntent.StopPips" in pending_limit
    and "pendingIntent.TargetPips" in pending_limit
    and "pendingIntent.RequestedEntry" in pending_limit
    and "pendingIntent.Target" in pending_limit
    and "pendingIntent.Volume" in pending_limit,
)

check(
    "all submission paths record the exact intent geometry",
    "ExecutionIntent intent = null" in submission
    and "FormatExecutionIntentTrace(intent)" in submission
    and "INTENT ENTRY=" in submission
    and " SL=" in submission
    and " TP=" in submission
    and " SL_PIPS=" in submission
    and " TP_PIPS=" in submission,
)

check(
    "fill acceptance remains a single direction-aware owner",
    "class ExecutionFillAcceptanceRule" in fill_rule
    and "requestedEntry" in fill_rule
    and "actualFill" in fill_rule,
)

check(
    "pending fill preserves absolute pre-fill intent as the reconciliation anchor",
    "_pendingOrderPlanSnapshot" in pending_snapshot
    and "intent.RequestedEntry" in pending_snapshot
    and "intent.Stop" in pending_snapshot
    and "intent.Target" in pending_snapshot
    and "ReconcileLivePlanToActualFill(" in pending_fill,
)

check(
    "Planning Contracts compile and exercise CI-15 geometry",
    "ExecutionIntentGeometryRule.cs" in planning_project
    and "VerifyExecutionIntentGeometry();" in planning_contracts
    and "CI-15 BUY intent geometry projection" in planning_contracts
    and "CI-15 wrong-side intent geometry fails closed" in planning_contracts
    and "CI-15 intent preserves physical sub-pip distances" in planning_contracts,
)

check(
    "CI-15 accumulated Source/Architecture wiring follows CI-14",
    "audit_phase_ci_14.py" in workflow
    and "audit_phase_ci_15.py" in workflow
    and workflow.index("audit_phase_ci_15.py") > workflow.index("audit_phase_ci_14.py"),
)

check(
    "CI-15 continuity documentation is recorded",
    "CI-15" in roadmap
    and "CI-15" in continuation
    and "CI-15" in phase_doc,
)

check(
    "CI-15 remains a geometry/traceability correction, not strategy tuning",
    "no public parameter" in phase_doc.lower()
    and "retune strategy thresholds" in phase_doc.lower()
    and "target-terminal" in phase_doc.lower(),
)

if errors:
    for error in errors:
        print(f"[FAIL] {error}")
    sys.exit(1)

print(
    "CI-15 execution-geometry/broker-boundary audit PASS: "
    "one intent geometry, exact submission handoff, fill envelope and telemetry trace"
)
