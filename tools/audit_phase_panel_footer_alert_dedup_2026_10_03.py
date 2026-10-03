from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"

def read(rel: str) -> str:
    return (IND / rel).read_text(encoding="utf-8")

def root_read(rel: str) -> str:
    return (ROOT / rel).read_text(encoding="utf-8")

def check(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)

lamp = read("UI/Panel/PanelTrendTimeframeLampRow.cs")
panel = read("UI/Panel/PanelMainRenderer.cs")
surface = read("UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs")
queue = read("Core/Runtime/AlertDeliveryQueue.cs")
alerts = read("Trading/Alerts/AlertEngine.cs")
processor = read("UI/Panel/AlertDeliveryProcessor.cs")
contracts = root_read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = root_read(".github/workflows/source-check.yml")

check(
    "new StackPanel" in lamp and
    "_panelTrendTimeframeLampIndicators" in lamp and
    "_panelTrendTimeframeLampLabels" in lamp and
    'indicator.Text = "●"' in lamp and
    'label.Text = labels[i]' in lamp,
    "MTF lamps must render indicator and timeframe label as separate vertical elements",
)

check(
    "PanelTrendTimeframeLampTopSpacing" in lamp and
    "PanelTrendTimeframeLampBottomSpacing" in lamp and
    "PanelTrendTimeframeLampTopSpacing" in panel and
    "PanelTrendTimeframeLampBottomSpacing" in panel and
    "PanelTrendTimeframeLampTopSpacing" in surface,
    "MTF lamp spacing must be part of deterministic panel geometry",
)

check(
    "int alertRailHeight" in panel and
    "GetPanelAlertMessageRailHeight()" in panel and
    "alertRailHeight > 0" in panel and
    "PanelTrendTimeframeLampTopSpacing" in panel,
    "panel scroll budget must reserve the real alert footer and two-line MTF rail",
)

check(
    "_pendingAlertIds" in queue and
    "_pendingAlertIds.Contains(alertId)" in queue and
    "_pendingAlertIds.Add(alertId)" in queue and
    "RemovePendingAlertId(delivery)" in queue and
    "_pendingAlertIds.Clear()" in queue,
    "alert delivery queue must provide pending canonical-event idempotency",
)

check(
    'queue.Enqueue(normal1) &&\n                !queue.Enqueue(normal1)' in contracts and
    "same alert may be re-armed after it is actually delivered" in contracts,
    "runtime contracts must cover queue duplicate suppression and post-delivery rearming",
)

check(
    "Notifications.PlaySound(" not in alerts and
    processor.count("Notifications.PlaySound(") == 3 and
    "RecordPanelAlertDelivery(next)" in processor,
    "Indicator signal sound must keep one delivery owner and one queued event boundary",
)

check(
    "tools/audit_phase_panel_footer_alert_dedup_2026_10_03.py" in workflow,
    "new footer/alert dedup audit must run in Source/Architecture CI",
)

print("Panel footer / MTF lamp / alert dedup audit PASS")
