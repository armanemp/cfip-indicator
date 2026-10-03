#!/usr/bin/env python3
"""Static acceptance gate for CR3.4 execution-control and alert-delivery boundaries."""
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
initialization = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
queue = read("src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs")
delivery = read("src/CFIP.Indicator/Core/Runtime/AlertDelivery.cs")
processor = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
rail = read("src/CFIP.Indicator/UI/Panel/PanelAlertMessageRenderer.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

errors = []

calculation = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")

checks = {
    "explicit re-arm is a dedicated state-machine operation": (
        "public bool RequestExplicitRearm()" in runtime and
        "if (_state != RuntimeFaultState.Healthy)" in runtime and
        "_entryArmed = true;" in runtime
    ),
    "execution controls remain status-only": (
        "IsEnabled = false" in factory and
        "ExecutionControlPresentationRule.ComposeStatusText(" in factory and
        "_autoTradingQuickToggle.Click +=" not in factory and
        "_automaticOrdersQuickToggle.Click +=" not in factory and
        "public static bool IsInteractive => false;" in rule
    ),
    "execution-control synchronization remains guarded": (
        "SyncQuickExecutionControls(" in sync and
        "_executionToggleSyncing = true" in sync and
        "RefreshCbotExecutionStateIfDue();" in sync and
        "EffectiveAutoTradingEnabled" in sync and
        "EffectiveAutomaticOrdersEnabled" in sync
    ),
    "bounded alert delivery queue remains runtime-owned": (
        "AlertDeliveryQueue(16)" in state and
        "_capacity" in queue and
        "return false;" in queue
    ),
    "critical deliveries retain priority": (
        "if (_critical.Count > 0)" in queue and
        "if (delivery.Critical)" in queue
    ),
    "alert engine uses one canonical delivery event": (
        "_alertDeliveryQueue.Enqueue(" in alerts and
        "new AlertDelivery(" in alerts and
        "ShowPopup(" not in alerts
    ),
    "panel rail is the sole visual message surface": (
        "RecordPanelAlertDelivery(next)" in processor and
        "CreatePanelAlertMessageRail(" in rail and
        "ResolvePanelAlertMessageColor(" in rail and
        "ShowPopup(" not in processor
    ),
    "sound remains after panel presentation at one delivery boundary": (
        processor.index("RecordPanelAlertDelivery(next)") <
        processor.index("Notifications.PlaySound(")
    ),
    "queue is drained at calculation/initialization boundaries": (
        "ProcessQueuedAlertPresentation();" in initialization and
        "ProcessQueuedAlertSoundDelivery();" in calculation
    ),
    "queue is cleared on destroy": (
        "_alertDeliveryQueue.ClearPendingAlerts();" in initialization
    ),
    "deterministic re-arm runtime contract is registered": (
        "VerifyRuntimeExplicitRearmSemantics();" in contracts
    ),
    "deterministic alert delivery runtime contract is registered": (
        "VerifyAlertDeliveryQueueSemantics();" in contracts
    ),
    "alert delivery queue is included in runtime contract build": (
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

if "cAlgo.API" in queue or "Indicator" in queue or "Chart." in queue:
    errors.append("alert delivery queue has a platform/UI dependency")
    print("FAIL | alert delivery queue has a platform/UI dependency")

production = "\n".join(
    p.read_text(encoding="utf-8")
    for p in (ROOT / "src/CFIP.Indicator").rglob("*.cs")
)

for forbidden in (
    "ShowPopup(",
    "_popup",
    "ShowPopupAlerts",
    "PopupCriticalOnly",
):
    if forbidden in production:
        print(f"FAIL | obsolete popup symbol remains: {forbidden}")
        errors.append(f"obsolete popup symbol remains: {forbidden}")

for relative in (
    "src/CFIP.Indicator/UI/Popup/PopupRenderer.cs",
    "src/CFIP.Indicator/UI/Popup/PopupRemover.cs",
    "src/CFIP.Indicator/UI/Popup/PopupExpirationCleaner.cs",
    "src/CFIP.Indicator/UI/Popup/AlertDeliveryProcessor.cs",
):
    if (ROOT.parent.parent / relative).exists():
        print(f"FAIL | obsolete popup file remains: {relative}")
        errors.append(f"obsolete popup file remains: {relative}")

print("CR3.4 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR3.4 STATIC GATE PASS")
