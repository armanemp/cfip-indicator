#!/usr/bin/env python3
"""CBOT position-truth / restart-idempotency regression gate."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(path: str) -> str:
    p = ROOT / path
    if not p.exists():
        errors.append("missing: " + path)
        return ""
    return p.read_text(encoding="utf-8")

def check(name: str, ok: bool):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
reconciliation = read("src/CFIP.cBot/Recovery/CbotBrokerReconciliation.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
preflight = read("src/CFIP.cBot/Execution/CbotSignalPreflight.cs")
store = read("src/CFIP.cBot/Execution/CbotExecutionIdempotencyStore.cs")
market = read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs")
pending = read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs")
sixm = read("tools/audit_phase_cbot_6m.py")
workflow = read(".github/workflows/source-check.yml")

check(
    "scenario processing uses exact per-scenario broker reconciliation",
    "ReconcileScenarioState(" in bot and
    "_reconciliation =\n                ReconcileScenarioState(" in bot and
    "Evaluate(" in reconciliation
)
check(
    "aggregate broker reconciliation separates independent scenarios from same-scenario ambiguity",
    "EvaluateAggregate(" in reconciliation and
    'if (string.IsNullOrWhiteSpace(preferred))' in reconciliation and
    "BROKER MULTI-SCENARIO STATE RECONCILED" in reconciliation and
    "DUPLICATE MANAGED POSITION SCENARIO" in reconciliation and
    "POSITION AND PENDING STATE COEXIST FOR SAME SCENARIO" in reconciliation
)
check(
    "aggregate reconciliation exposes multi-scenario state without global ambiguity",
    '"ACTIVE / MULTI-SCENARIO"' in reconciliation and
    '"PENDING / MULTI-SCENARIO"' in reconciliation and
    '"MULTIPLE MANAGED BROKER OBJECTS"' in reconciliation
)
check(
    "aggregate publisher discovers managed objects without an active scenario label",
    "InstanceMarker +\n                indicatorInstanceId" in publisher and
    "MatchesInstanceScope(" in publisher
)
check(
    "protection recovery preserves per-scenario reconciliation truth",
    "TryRecoverProtection(" in bot and
    "CbotBrokerReconciliationResult reconciliation" in bot and
    "TryRecoverProtection(\n                        scenario,\n                        result," in bot and
    "_scenarioReconciliations[item.Key]" in bot
)
check(
    "cBot management is gated by the effective account-scoped arm",
    "EffectiveManagementExecutionEnabled" in bot and
    "ManagementExecutionCoordinator" in bot and
    "TryRecoverProtection(" in bot
)
check(
    "signal preflight validates contract and complete execution identity",
    "ContractVersion.Current" in preflight and
    "SIGNAL INTENT IDENTITY MISMATCH" in preflight and
    "SIGNAL EXECUTION IDENTITY INCOMPLETE" in preflight and
    "SIGNAL EXECUTION ACTION UNAVAILABLE" in preflight
)
check(
    "execution idempotency survives cBot restart through device storage",
    "LocalStorageScope.Device" in store and
    "RecordAttempt(" in store and
    "CanAttempt(" in store and
    "DUPLICATE IDEMPOTENCY KEY • PERSISTED CONFIRMED" in store and
    "CFIPExecIdem" in store
)
check(
    "market and pending coordinators share persistent idempotency",
    "CbotExecutionIdempotencyStore idempotencyStore" in market and
    "idempotencyStore.CanAttempt(" in market and
    "idempotencyStore.RecordAttempt(" in market and
    "CbotExecutionIdempotencyStore idempotencyStore" in pending and
    "idempotencyStore.CanAttempt(" in pending and
    "idempotencyStore.RecordAttempt(" in pending
)
check(
    "idempotency store is re-scoped on Indicator binding",
    "_idempotencyStore.Reload(" in bot and
    "RefreshIndicatorBinding" in bot
)
check(
    "live-account execution is explicit and fail-closed when unarmed",
    "Account.IsLive" in bot and
    "EnableLiveMarketExecution" in bot and
    "EnableLivePendingStopExecution" in bot and
    "EnableLivePendingLimitExecution" in bot and
    "LIVE EXECUTION NOT ARMED" in
    read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
)
check(
    "CBOT-6M identity and capacity boundary remains present",
    "ScenarioId" in sixm and
    "Max Concurrent Scenarios" in sixm and
    "CountManagedScenarioObjects(" in sixm
)
check(
    "concurrent scenario capacity remains bounded by cBot configuration",
    "Max Concurrent Scenarios" in bot and
    "MaxValue = 10" in bot
)
check(
    "dedicated position-truth audit is accumulated into Source/Architecture CI",
    "python tools/audit_phase_cbot_position_truth_hardening_2026_10_03.py" in workflow
)

if errors:
    print("=" * 72)
    print("FAIL | CBOT POSITION-TRUTH HARDENING")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("=" * 72)
print("CBOT POSITION-TRUTH HARDENING: PASS")
