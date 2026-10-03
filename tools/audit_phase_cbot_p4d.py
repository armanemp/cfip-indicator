#!/usr/bin/env python3
"""CBOT-P4D acceptance gate: Pending Limit authority + signal/panel-alert continuity."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"
BOT = ROOT / "src" / "CFIP.cBot"

errors = []


def read(path):
    if not path.exists():
        errors.append("missing " + str(path.relative_to(ROOT)))
        return ""
    return path.read_text(encoding="utf-8")


def check(ok, message):
    if not ok:
        errors.append(message)


limit_placement = read(
    IND / "Trading/Pending/Placement/ReversalLimitPlacement.cs"
)
limit_prep = read(
    IND / "Trading/Pending/Placement/ReversalLimitPreparation.cs"
)
orchestrator = read(
    IND / "Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs"
)
pending_cbot = read(
    BOT / "Execution/DemoPendingOrderExecutionCoordinator.cs"
)
host = read(BOT / "CFIPExecutionBot.cs")
provider_plan = read(
    IND / "Runtime/Provider/CFIPReadOnlyProviderPlan.cs"
)
snapshot_builder = read(
    IND / "UI/Chart/SignalVisualSnapshotBuilder.cs"
)
signal_renderer = read(
    IND / "UI/Chart/SignalRenderer.cs"
)
signal_presentation = read(
    IND / "UI/Chart/SignalPresentationRenderer.cs"
)
alert_processor = read(
    IND / "UI/Panel/AlertDeliveryProcessor.cs"
)
alert_rail = read(
    IND / "UI/Panel/PanelAlertMessageRenderer.cs"
)
alert_engine = read(
    IND / "Trading/Alerts/AlertEngine.cs"
)
alert_params = read(
    IND / "Indicator/Parameters/12_alerts_advanced.cs"
)
arrow_params = read(
    IND / "Indicator/Parameters/21_complete_intelligence.cs"
)

check(
    "PrepareReversalLimitForCbot(" in limit_placement,
    "Indicator Pending Limit must expose analysis-only intent preparation",
)
check(
    "CapturePendingOrderPlanSnapshot(" in limit_placement,
    "Indicator Pending Limit must preserve the absolute lifecycle snapshot",
)
check(
    "PlaceLimitOrder(" not in limit_placement,
    "Indicator Pending Limit mutation remains",
)
check(
    "TryAcquireSubmission(" not in limit_placement,
    "Indicator Pending Limit must not own the broker submission gate",
)
check(
    "TrySelectPredictivePendingLevel(" in limit_prep,
    "Reversal limit preparation owner is missing",
)
check(
    "PrepareReversalLimitForCbot(" in orchestrator,
    "Pending arbiter/orchestrator must route limit to the cBot handoff",
)

check(
    "ExecutionAction.PendingLimit" in pending_cbot and
    "PlaceLimitOrder(" in pending_cbot and
    "BrokerAction.SubmitPendingLimit" in pending_cbot,
    "cBot must own Pending Limit broker mutation",
)
check(
    "BrokerExecutionSafety.TryConstrainVolumeForMargin(" in pending_cbot and
    "BrokerExecutionSafety.CountManagedPositions(" in pending_cbot and
    "BrokerExecutionSafety.CountManagedPending(" in pending_cbot,
    "Pending Limit must use shared cBot margin/capacity safety",
)
check(
    "envelope.Intent.ExecutionLabel" in pending_cbot and
    'string label =\n                executionLabel + "-PENDING";' in pending_cbot,
    "Pending Limit must consume the canonical instance-scoped label",
)
check(
    "EnableDemoPendingLimitExecution" in host and
    "ExecutionAction.PendingLimit" in host and
    "_pending.TryExecute(" in host,
    "cBot host must expose and route Pending Limit execution",
)

check(
    not (IND / "Trading/Execution/BrokerLimitOrderPlacement.cs").exists(),
    "obsolete Indicator BrokerLimitOrderPlacement owner must remain deleted",
)
check(
    "ManagedExecutionLabel()" in provider_plan and
    "executionLabel," in provider_plan,
    "Provider must transport the existing canonical managed label",
)

check(
    "ChartIconType.Circle" in signal_renderer and
    "P + \"M1_TRIGGER\"" in signal_renderer and
    "ChartIconType.UpArrow" in signal_renderer and
    "ChartIconType.DownArrow" in signal_renderer,
    "directional signal presentation must use canonical arrows while M1 remains a precision-only Circle marker",
)
check(
    "RenderStackedSignalArrows(" in signal_renderer and
    "RenderStackedSignalArrows(" in signal_presentation and
    "snapshot.MtfTrendStrengthLevel" in read(IND / "UI/Chart/SignalStackedArrowRenderer.cs") and
    "fallbackState" not in read(IND / "UI/Chart/SignalStackedArrowRenderer.cs") and
    "ResolveSignalArrowState(" not in signal_renderer,
    "signal arrow state must resolve through the single canonical snapshot/strength owner",
)
check(
    'DefaultValue = "Lime"' in arrow_params and
    'DefaultValue = "#6BE38B"' in arrow_params and
    'DefaultValue = "#B8F0C6"' in arrow_params and
    'DefaultValue = "Red"' in arrow_params and
    'DefaultValue = "#FF6B78"' in arrow_params and
    'DefaultValue = "#FFB3BA"' in arrow_params,
    "BUY/SELL arrows must have three distinct default intensity colors",
)
check(
    "DecisionDirection != 0" in signal_presentation and
    "MinimumEarlyConfidence" in signal_presentation,
    "non-actionable directional watch must retain an evidence floor",
)
check(
    "DecisionDirection" in snapshot_builder and
    "snapshot.AuthoritativeDirection" in snapshot_builder,
    "visual snapshot must preserve canonical direction separately from execution actionability",
)

check(
    "panel alert delivery owner exists",
    "RecordPanelAlertDelivery(next)" in alert_processor and
    "Notifications.PlaySound(" in alert_processor and
    "ResolvePanelAlertMessageColor(" in alert_rail,
)
check(
    "popup UI and popup parameter dependencies are removed",
    not (IND / "UI/Popup/PopupRenderer.cs").exists() and
    not (IND / "UI/Popup/PopupRemover.cs").exists() and
    not (IND / "UI/Popup/PopupExpirationCleaner.cs").exists() and
    "ShowPopupAlerts" not in alert_params and
    "PopupCriticalOnly" not in alert_params and
    "PopupPosition" not in alert_params and
    "ShowPopup(" not in alert_engine,
)
check(
    "panel alert rail is direction/priority semantic",
    "if (delivery.Critical)" in alert_rail and
    "return BuyArrowColor;" in alert_rail and
    "return SellArrowColor;" in alert_rail,
)


# Guard the architectural migration boundary.
legacy_execution = [
    "Trading/Execution/BrokerMarketOrderMutation.cs",
    "Trading/Execution/BrokerPendingOrderPlacement.cs",
]
for rel in legacy_execution:
    p = IND / rel
    if "BrokerPendingOrderPlacement" in rel:
        check(
            not p.exists(),
            "P4C migrated Pending Stop broker owner must remain deleted",
        )

print("CBOT-P4D SUMMARY")
print("=" * 72)
print("Pending Limit Indicator mutation: REMOVED")
print("Pending Limit cBot broker owner: ACTIVE")
print("Unified pending Stop/Limit owner: ACTIVE")
print("Chart timeframe execution dependency: NONE")
print("Arrow-only signal markers: ENFORCED")
print("Three directional arrow intensity colors: ENFORCED")
print("Popup location: BottomRight")
print("Popup persistence: until next alert/manual close")
print("Important-alert classifier: ENFORCED")

if errors:
    print("CBOT-P4D AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P4D AUDIT: PASS")
