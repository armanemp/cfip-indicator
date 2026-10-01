#!/usr/bin/env python3
"""Static acceptance gate for CR3.4 execution UI re-arm and popup reliability."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing required CR3.4 file: {relative}")
    return path.read_text(encoding="utf-8")

runtime = read("src/CFIP.Indicator/Runtime/Calculation/RuntimeFaultStateMachine.cs")
factory = read("src/CFIP.Indicator/UI/Controls/ExecutionControlsFactory.cs")
rule = read("src/CFIP.Indicator/Core/Math/ExecutionControlPresentationRule.cs")
sync = read("src/CFIP.Indicator/UI/Controls/ExecutionControlsSynchronizer.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
alerts = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")
renderer = read("src/CFIP.Indicator/UI/Popup/PopupRenderer.cs")
processor = read("src/CFIP.Indicator/UI/Popup/AlertDeliveryProcessor.cs")
remover = read("src/CFIP.Indicator/UI/Popup/PopupRemover.cs")
initialization = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
queue = read("src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

errors = []

checks = {
    "explicit re-arm is a dedicated state-machine operation": (
        "public bool RequestExplicitRearm()" in runtime and
        "if (_state != RuntimeFaultState.Healthy)" in runtime and
        "_entryArmed = true;" in runtime
    ),
    "explicit re-arm remains owned by the runtime state machine": (
        "RequestExplicitRearm()" in runtime and
        "public bool RequestExplicitRearm()" in runtime
    ),
    "execution controls remain status-only and do not mutate runtime authority": (
        "IsEnabled = false" in factory and
        "ExecutionControlPresentationRule.ComposeStatusText(" in factory and
        "_autoTradingQuickToggle.Click +=" not in factory and
        "_automaticOrdersQuickToggle.Click +=" not in factory and
        "public static bool IsInteractive => false;" in rule
    ),
    "execution-control synchronization remains guarded": (
        "SyncQuickExecutionControls(" in sync and
        "_executionToggleSyncing = true" in sync and
        "EnsureExecutionRuntimeState();" in sync
    ),
    "popup queue is owned by runtime state": (
        "AlertDeliveryQueue(16)" in state and
        "_popupCritical" in state
    ),
    "popup queue is bounded": (
        "_capacity" in queue and
        "return false;" in queue
    ),
    "critical popup alerts have priority": (
        "if (_critical.Count > 0)" in queue and
        "if (critical)" in queue
    ),
    "normal popup overflow cannot grow the queue": (
        "else" in queue and
        "return false;" in queue
    ),
    "alert engine queues popups instead of overwriting the live control": (
        "_alertDeliveryQueue.Enqueue(" in alerts and
        "ShowPopup(message)" not in alerts
    ),
    "popup renderer tracks priority": (
        "private void ShowPopup(" in renderer and
        "bool critical)" in renderer and
        "_popupCritical =\n                                critical;" in renderer
    ),
    "popup processor is the only queue-to-render handoff": (
        "ShowPopup(\n                    next.Message,\n                    next.Critical)" in processor and
        "_alertDeliveryQueue.TryPeek" in processor
    ),
    "critical queued alert may preempt a normal active popup": (
        "if (popupActive && !next.Critical)" in processor
    ),
    "popup priority resets on removal": (
        "_popupCritical = false;" in remover
    ),
    "popup queue is drained outside Calculate": (
        "RemoveExpiredPopup();\n                HandleRuntimeHeartbeat();\n                ProcessQueuedAlertDelivery();" in initialization
    ),
    "popup queue is cleared on destroy": (
        "_alertDeliveryQueue.ClearPendingAlerts();" in initialization
    ),
    "deterministic re-arm runtime contract is registered": (
        "VerifyRuntimeExplicitRearmSemantics();" in contracts
    ),
    "deterministic popup queue runtime contract is registered": (
        "VerifyAlertDeliveryQueueSemantics();" in contracts
    ),
    "popup queue is included in runtime contract build": (
        "Core/Runtime/AlertDeliveryQueue.cs" in runtime_project
    ),
    "CR3.4 static gate is wired into CI": (
        "python tools/audit_phase_3_4.py" in workflow
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# The handler must never reintroduce a parameter-dependent re-arm branch.
# The queue itself must remain platform-neutral.
if "cAlgo.API" in queue or "Indicator" in queue or "Chart." in queue:
    print("FAIL | popup queue has a platform/UI dependency")
    errors.append("popup queue has a platform/UI dependency")

# There should be one production popup render owner, plus the queue processor.
production = "\n".join(
    p.read_text(encoding="utf-8")
    for p in (ROOT / "src/CFIP.Indicator").rglob("*.cs")
)
if production.count("ShowPopup(\n                    next.Message,\n                    next.Critical)") != 1:
    print("FAIL | queued popup render handoff count is not exactly one")
    errors.append("queued popup render handoff count is not exactly one")

for path in (ROOT / "src/CFIP.Indicator").rglob("*.cs"):
    relative = path.relative_to(ROOT).as_posix()
    if relative in {
        "src/CFIP.Indicator/UI/Popup/PopupRenderer.cs",
        "src/CFIP.Indicator/UI/Popup/PopupQueueProcessor.cs",
    }:
        continue
    if "ShowPopup(" in path.read_text(encoding="utf-8"):
        print(f"FAIL | direct popup render caller: {relative}")
        errors.append(f"direct popup render caller: {relative}")

print("CR3.4 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR3.4 STATIC GATE PASS")
