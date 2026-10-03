#!/usr/bin/env python3
"""Static acceptance gate for CR5.4 / E4 pending-fill absolute SL/TP reconciliation."""

from pathlib import Path
import sys
import re

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


stop_placement = read(
    "src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs"
)
limit_placement = read(
    "src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs"
)
pending_filled = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs"
)
position_opened = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PositionOpenedHandler.cs"
)
pending_cancelled = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PendingCancelledHandler.cs"
)
snapshot = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs"
)
fill_reconciliation = read(
    "src/CFIP.Indicator/Trading/Lifecycle/LiveFillReconciliation.cs"
)
fill_reconciler = read(
    "src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs"
)
fill_protection = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PendingFillProtectionCoordinator.cs"
)
server_ladder = read(
    "src/CFIP.Indicator/Trading/Execution/ServerSideTakeProfitLadder.cs"
)
core = read(
    "src/CFIP.Indicator/Core/Math/PendingFillExitResolutionRule.cs"
)
runtime_contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
phase_doc = read("docs/PHASE-CR5-4-PENDING-FILL-ABSOLUTE-RECONCILIATION.md")


def has_pending_snapshot(source):
    normalized = re.sub(r"\s+", " ", source)
    return (
        "Plan pendingSnapshot = CapturePendingOrderPlanSnapshot(" in normalized and
        "_pendingOrderPlanSnapshot = pendingSnapshot;" in normalized
    )


check(
    "pending Stop/Limit placement preserves an absolute snapshot from ExecutionIntent",
    has_pending_snapshot(stop_placement) and
    has_pending_snapshot(limit_placement),
)

check(
    "snapshot reconstructs all pending TP stages through the canonical target pipeline",
    "BuildTargetLevels(" in snapshot and
    "SelectTargets(" in snapshot and
    "SelectTarget(" in snapshot and
    "EffectiveAutoTpStage()" in snapshot,
)

check(
    "snapshot preserves the original pending lane and execution context",
    "ResolveContinuationPendingLane(" in snapshot and
    "OpportunityLane.MicroReaction" in snapshot and
    "EntryMode = " in snapshot,
)

check(
    "pending fill consumes the absolute snapshot before broker protection adoption",
    "_pendingOrderPlanSnapshot" in pending_filled and
    "Plan priorPlan" in pending_filled and
    "ReconcileLivePlanToActualFill(" in pending_filled and
    "priorPlan)" in pending_filled,
)

check(
    "pending fill failure is fail-closed and blocks lifecycle/execution",
    "PENDING FILL • ABSOLUTE EXIT RECONCILIATION FAILED" in pending_filled and
    "LifecycleState.RecoveryRequired" in pending_filled and
    "_autoExecutionBlockReason" in pending_filled,
)

check(
    "server-side TP ladder is reconciled using actual fill price",
    "ReconcilePendingFillServerProtection(" in pending_filled and
    "TryBuildServerSideTakeProfitLadder(" in fill_protection and
    "position.EntryPrice" in fill_protection and
    "RequestModifyTakeProfitLadder(" in fill_protection,
)

check(
    "server ladder remains broker-owned after successful reconciliation",
    "return AdoptServerSideTakeProfitLadder(position);" in fill_protection and
    "position.AbsoluteTakeProfitProtections" in server_ladder,
)

check(
    "actual-fill reconciler accepts an explicit absolute reference plan",
    "absoluteReferencePlan = null" in fill_reconciliation and
    "absoluteReferencePlan = null" in fill_reconciler and
    "absoluteReferencePlan ?? _plan" in fill_reconciler,
)

check(
    "absolute stop/TP and broker-confirmed state resolve through one Core rule",
    "ResolveProtectiveStop(" in core and
    "ResolveProgressiveTarget(" in core and
    "LiveExitGeometryRule" in core,
)

check(
    "more-protective broker SL survives fill reconciliation",
    '"BROKER MORE PROTECTIVE"' in core,
)

check(
    "more-progressive broker TP survives fill reconciliation",
    '"BROKER MORE PROGRESSIVE"' in core,
)

check(
    "PositionOpened does not discard the preserved pending snapshot",
    "if (!boundToActivePlan &&" in position_opened and
    "_pendingOrderPlanSnapshot == null" in position_opened and
    "WAITING PENDING FILL RECONCILIATION" in position_opened,
)

check(
    "pending cancellation clears stale absolute snapshot state",
    "ClearPendingOrderPlanSnapshot();" in pending_cancelled,
)

check(
    "deterministic E4 contracts are wired",
    "VerifyPendingFillExitResolutionSemantics();" in runtime_contracts and
    "positive-slippage fill" in runtime_contracts and
    "negative-slippage fill" in runtime_contracts and
    "PENDING_FILLED" in runtime_contracts,
)

check(
    "runtime project compiles the new pure Core rule",
    "PendingFillExitResolutionRule.cs" in runtime_project and
    "LiveExitGeometryRule.cs" in runtime_project,
)

check(
    "E4 static gate is wired after E3",
    "audit_phase_5_3.py" in workflow and
    "audit_phase_5_4.py" in workflow and
    workflow.index("audit_phase_5_4.py") >
    workflow.index("audit_phase_5_3.py"),
)

check(
    "phase record and roadmap reference E4",
    "CR5.4" in phase_doc and
    "CR5.4 / E4" in roadmap,
)

print("CR5.4 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.4 STATIC GATE PASS")
