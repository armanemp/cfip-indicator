#!/usr/bin/env python3
"""CBOT-P9 static gate: unified alert rail, visual coherence and cBot preflight."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"
BOT = ROOT / "src" / "CFIP.cBot"
errors = []

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.exists():
        errors.append("missing file: " + str(path.relative_to(ROOT)))
        return ""
    return path.read_text(encoding="utf-8")

def check(name: str, ok: bool):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

state = read(IND, "Indicator/State.cs")
alert_engine = read(IND, "Trading/Alerts/AlertEngine.cs")
delivery = read(IND, "Core/Runtime/AlertDelivery.cs")
queue = read(IND, "Core/Runtime/AlertDeliveryQueue.cs")
processor = read(IND, "UI/Panel/AlertDeliveryProcessor.cs")
rail = read(IND, "UI/Panel/PanelAlertMessageRenderer.cs")
factory = read(IND, "UI/Panel/PanelFactory.cs")
visual_settings = read(IND, "UI/Panel/Theme/PanelVisualSettings.cs")
visual_opt = read(IND, "UI/Panel/PanelRenderOptimization.cs")
line = read(IND, "UI/Chart/PlanLineRenderer.cs")
labels = read(IND, "UI/Chart/PlanLabelRenderer.cs")
prediction = read(IND, "UI/Chart/PredictionLineRenderer.cs")
cinit = read(IND, "Runtime/Initialization/RuntimeInitialization.cs")
calcprep = read(IND, "Runtime/Calculation/CalculationPreparation.cs")
bot = read(BOT, "CFIPExecutionBot.cs")
preflight = read(BOT, "Execution/CbotSignalPreflight.cs")
transport = read(BOT, "Binding/CfipDeviceSignalTransport.cs")
gate = read(BOT, "Execution/CbotExecutionEnvironmentGate.cs")
workflow = (ROOT / ".github/workflows/source-check.yml").read_text(encoding="utf-8")

check(
    "legacy popup production files are removed",
    not any(
        (IND / rel).exists()
        for rel in (
            "UI/Popup/PopupRenderer.cs",
            "UI/Popup/PopupRemover.cs",
            "UI/Popup/PopupExpirationCleaner.cs",
            "UI/Popup/AlertDeliveryProcessor.cs",
        )
    ),
)
production = "\n".join(
    path.read_text(encoding="utf-8")
    for path in IND.rglob("*.cs")
)
for forbidden in ("ShowPopup(", "_popup", "ShowPopupAlerts", "PopupCriticalOnly", "Popup Position"):
    check("obsolete popup symbol absent: " + forbidden, forbidden not in production)

check(
    "single bounded alert queue remains canonical",
    "AlertDeliveryQueue(16)" in state and
    "_alertDeliveryQueue.Enqueue(" in alert_engine and
    "_capacity" in queue and
    "if (delivery.Critical)" in queue,
)
check(
    "panel rail is the only visual alert delivery surface",
    "RecordPanelAlertDelivery(next)" in processor and
    "CreatePanelAlertMessageRail(" in rail and
    "ResolvePanelAlertMessageColor(" in rail and
    "ShowPopup(" not in processor,
)
check(
    "panel rail uses semantic direction/priority colors",
    "if (delivery.Critical)" in rail and
    "return BuyArrowColor;" in rail and
    "return SellArrowColor;" in rail,
)
check(
    "new alert revisions invalidate panel presentation without a timer flood",
    "_panelAlertRevision" in state and
    "_panelAlertRevision" in visual_opt and
    "_panelAlertRevision++" in rail,
)
check(
    "alert rail sits beside the panel hide control",
    "CreatePanelAlertMessageRail();" in factory and
    "_buttonStack.AddChild(" in factory,
)
check(
    "alert rail contributes to bottom-panel geometry",
    "GetPanelAlertMessageRailHeight()" in visual_settings and
    "ApplyPanelAlertMessageRailLayout(" in visual_settings,
)
check(
    "alert rail refreshes from the same panel render boundary",
    "UpdatePanelAlertMessageRail();" in read(IND, "UI/Panel/PanelMainRenderer.cs"),
)
check(
    "sound remains centralized after visual delivery",
    "Notifications.PlaySound(" not in alert_engine and
    "RecordPanelAlertDelivery(next)" in processor and
    "Notifications.PlaySound(" in processor,
)
check(
    "blocked candidates stay silent",
    "blockedCandidateAlert" in alert_engine and
    "!blockedCandidateAlert" in alert_engine,
)
check(
    "all production chart line styles are solid-only",
    "return LineStyle.Solid;" in line and
    all(
        ("LineStyle." not in path.read_text(encoding="utf-8"))
        or ("LineStyle.Solid" in path.read_text(encoding="utf-8"))
        for path in (IND / "UI/Chart").glob("*.cs")
    ) and
    "LineStyle.Dots" not in production and
    "LineStyle.DotsRare" not in production and
    "LineStyle.LinesDots" not in production,
)
check(
    "plan levels retain canonical 40-bar geometry and semantic text color",
    "CompactPlanLineLengthBars = 40" in line and
    "PlanLinePresentationRule.ResolveThickness(" in line and
    "ResolveCanonicalPlanLineColor(" in line and
    "ResolveCanonicalPlanLineColor(" in labels and
    "semanticColor" in labels and
    "GetCompactPlanLabelAnchorTime(" in labels and
    "Chart.BarIndexToX(" in read("src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs") and
    "Chart.XToTime(" in read("src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs")
)
check(
    "prediction signal line remains thickness one",
    ("line.Thickness = 1;" in prediction) or
    ("line.Thickness =\n                                            1;" in prediction),
)
check(
    "popup lifecycle hooks are fully removed",
    "RemovePopup" not in cinit and
    "RemoveExpiredPopup" not in cinit and
    "RemoveExpiredPopup" not in calcprep,
)
check(
    "cBot reads canonical Indicator SignalEnvelope through device transport",
    "CfipDeviceSignalTransport.TryRead(" in bot and
    "SignalEnvelope" in bot and
    "LocalStorageScope.Device" in transport,
)
check(
    "cBot execution has one canonical signal preflight owner",
    "CbotSignalPreflight.TryValidate(" in bot and
    "TryValidate(" in preflight and
    "ProviderStaleAfterSeconds" in bot,
)
check(
    "cBot broker execution remains fail-closed behind its environment gate",
    "_executionEnvironment.Evaluate(" in bot and
    "BrokerExecutionSafety.CountManagedPositions(" in gate and
    "BrokerExecutionSafety.CountManagedPending(" in gate,
)
if "Max Concurrent Scenarios" in bot:
    check(
        "CBOT-6M scenario capacity supersedes the historical single-plan gate",
        "CountManagedScenarioObjects(" in gate and
        "CONCURRENT SCENARIO CAPACITY BLOCKED" in gate and
        "SINGLE-PLAN CAPACITY BLOCKED" not in gate,
    )
else:
    check(
        "single-plan safety remains intact until CBOT-6M",
        "MaximumOpenPositions" not in bot and
        "MaxDemoExecutionsPerSession" in bot and
        "SINGLE-PLAN CAPACITY" in gate,
    )
parameter_source = "\n".join(
    p.read_text(encoding="utf-8")
    for p in (IND / "Indicator/Parameters").glob("*.cs")
)
check(
    "popup parameter surface is removed",
    "ShowPopupAlerts" not in parameter_source and
    "PopupCriticalOnly" not in parameter_source and
    "PopupPosition" not in parameter_source,
)
check(
    "current Indicator parameter count is 545",
    len(re.findall(r"\[Parameter\s*\(", parameter_source)) == 545,
)
check(
    "P9 audit itself is in source CI",
    "python tools/audit_phase_cbot_p9.py" in workflow,
)

print("=" * 72)
print("CBOT-P9 UNIFIED ALERT RAIL / VISUAL COHERENCE SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("CBOT-P9 STATIC GATE PASS")
