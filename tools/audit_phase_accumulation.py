from pathlib import Path
import re

ROOT = Path("src/CFIP.Indicator")
PARAM_ROOT = ROOT / "Indicator" / "Parameters"

def read(rel: str) -> str:
    path = ROOT / rel
    if not path.exists():
        raise SystemExit(f"Missing required production owner: {rel}")
    return path.read_text(encoding="utf-8")

# Phase accumulation gate: every phase must keep the automatic trade/order
# pipeline coupled to one decision, one submission gate and one protection owner.
pending_stop = read("Trading/Pending/Placement/ContinuationStopPlacement.cs")
pending_stop_preparation = read("Trading/Pending/Placement/ContinuationStopPreparation.cs")
pending_limit = read("Trading/Pending/Placement/ReversalLimitPlacement.cs")
ladder = read("Trading/Execution/ServerSideTakeProfitLadder.cs")
ROOT_REPO = Path(__file__).resolve().parents[1]
pending_cbot = (
    ROOT_REPO / "src" / "CFIP.cBot" / "Execution" / "DemoPendingOrderExecutionCoordinator.cs"
).read_text(encoding="utf-8")
limit_mutation = ""
protection = read("Trading/LiveManagement/ProtectionManager.cs")
target_progression = read("Trading/LiveManagement/TargetProgression.cs")
partial_tp = read("Trading/LiveManagement/PartialTakeProfitExecutor.cs")
line = read("UI/Chart/PlanLineRenderer.cs")
line_presentation_rule = read("Core/Math/PlanLinePresentationRule.cs")
labels = read("UI/Chart/PlanLabelRenderer.cs")
prediction_line = read("UI/Chart/PredictionLineRenderer.cs")
alert_renderer = read("UI/Chart/AlertSignalRenderer.cs")
alert_engine = read("Trading/Alerts/AlertEngine.cs")
visual_snapshot = read("UI/Chart/SignalVisualSnapshotBuilder.cs")
visual_lifecycle = read("Core/Math/SignalVisualLifecycleRule.cs")
popup_core = read("Indicator/Parameters/12_alerts_core.cs")
popup_advanced = read("Indicator/Parameters/12_alerts_advanced.cs")

execution_paths = {
    "pending-stop": pending_stop,
    "pending-limit": pending_limit,
}
for name, source in execution_paths.items():
    if name == "pending-stop":
        for token in (
            "PrepareContinuationStopForCbot(",
        ):
            if token not in source:
                raise SystemExit(f"{name}: missing intent-only token {token}")
        if "PlaceStopOrder(" in source:
            raise SystemExit("pending-stop: Indicator broker mutation remains")
        if "ExecutionIntentKind.Stop" not in pending_stop_preparation:
            raise SystemExit("pending-stop: canonical Stop intent construction missing")
    else:
        for token in (
            "TryPrepareReversalLimit(",
            "PrepareReversalLimitForCbot(",
            "CapturePendingOrderPlanSnapshot(",
        ):
            if token not in source:
                raise SystemExit(f"{name}: missing canonical intent handoff {token}")
        if "PlaceLimitOrder(" in source:
            raise SystemExit("pending-limit: Indicator broker mutation remains")

if "ValidateSingleExecutionCapacity(" not in read("Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs"):
    raise SystemExit("pending: canonical capacity gate missing")

if "PlaceStopOrder(" not in pending_cbot:
    raise SystemExit("cBot Pending Stop mutation owner missing")
if "PlaceLimitOrder(" not in pending_cbot:
    raise SystemExit("cBot Pending Limit mutation owner missing")
if "ExecutionAction.PendingLimit" not in pending_cbot:
    raise SystemExit("cBot Pending Limit action routing missing")
if "StopLossBreakEven" not in ladder:
    raise SystemExit("server break-even transport missing from canonical ladder")


for token in (
    "SmartBreakEvenRule.Evaluate(",
    "new StopLossBreakEven(",
    "RelativeTakeProfitProtections",
    "RelativeTakeProfitProtection(",
    "RelativeTakeProfitLastProtection(",
    "_serverSideBreakEvenActive",
):
    if token not in ladder:
        raise SystemExit(f"server protection ladder missing {token}")

if "_serverSideBreakEvenActive" not in protection:
    raise SystemExit("local protection manager must yield to server-owned break-even")
if "_serverSideTakeProfitLadderActive" not in target_progression:
    raise SystemExit("target progression must yield to server-owned TP ladder")
if "_serverSideTakeProfitLadderActive" not in partial_tp:
    raise SystemExit("partial TP mutation must yield to server-owned TP ladder")

quality_rule = read("Core/Math/ActionableSignalQualityRule.cs")
evaluator = read("Trading/Validation/TradeActionabilityEvaluator.cs")
# Extracted decision-gate owner keeps the staged threshold logic cohesive.
decision_gate = read("Trading/Validation/TradeActionabilityDecisionGate.cs")
telemetry = read("Trading/Execution/SubmissionGateCoordinator.cs")
outcome_telemetry = read("Trading/Intelligence/OutcomeTelemetryEngine.cs")
outcome_model = read("Trading/Intelligence/OutcomeObservation.cs")

policy = read("Core/Math/ActionabilityThresholdPolicy.cs")

if "AllowsQualityRecovery(" not in quality_rule:
    raise SystemExit("high-quality signal recovery gate is missing")
if "ActionabilityThresholdPolicy.QualityRecoveryDeficitAllowance" not in quality_rule:
    raise SystemExit("quality-recovery threshold ownership is missing")
if "EffectiveUpstreamEntryLocationQuality(" not in decision_gate or "EffectiveUpstreamEntryTimingQuality()" not in decision_gate:
    raise SystemExit("actionability staging threshold ownership is missing")
if "UpstreamEntryLocationQualityFloor = 64" not in policy or "UpstreamEntryTimingQualityFloor = 64" not in policy:
    raise SystemExit("actionability upstream 64/64 threshold owner is missing")
if "RecordExecutionTelemetry(" not in telemetry or "RecordExecutionTelemetryFailure(" not in telemetry:
    raise SystemExit("broker submission telemetry owner is missing")
if "RecordExecutionTelemetryHistory(" not in telemetry:
    raise SystemExit("bounded execution telemetry history owner is missing")
if "RecordManagedOutcome(" not in outcome_telemetry:
    raise SystemExit("managed outcome recorder is missing")
if "_outcomeHistory" not in read("Indicator/State.cs"):
    raise SystemExit("bounded outcome history state is missing")
if "_executionTelemetryHistory" not in read("Indicator/State.cs"):
    raise SystemExit("bounded execution telemetry history state is missing")
if "CalculateRecentContextual(" not in read("Analysis/Market/Decision/EmpiricalConfidenceCalibrator.cs"):
    raise SystemExit("recent contextual calibration owner is missing")
if "CalculateRecentContextual(" not in read("Analysis/Market/Decision/ConfidenceCalibrationCollector.cs"):
    raise SystemExit("collector does not consume recent calibration")
if "RecordLifecycleTelemetry(" not in read("Trading/Execution/State/LifecycleStateStore.cs"):
    raise SystemExit("recovery lifecycle telemetry owner is missing")
if "RecordManagedOutcome(" not in read("Trading/Lifecycle/PositionClosedHandler.cs"):
    raise SystemExit("broker-close outcome path is not centralized")

if "stopDistance" not in ladder:
    raise SystemExit("server SL/TP geometry freshness guard is missing")

if "return LineStyle.Solid" not in line:
    raise SystemExit("all plan level lines must be Solid")
if "LineStyle.Dots" in line or "LineStyle.DotsRare" in line or "LineStyle.LinesDots" in line:
    raise SystemExit("non-solid plan line styles remain")

for chart_path in sorted((ROOT / "UI" / "Chart").glob("*.cs")):
    chart_source = chart_path.read_text(encoding="utf-8")
    if "LineStyle." in chart_source and "LineStyle.Solid" not in chart_source:
        raise SystemExit(f"{chart_path.name}: chart line style is not Solid-only")
    for forbidden in ("LineStyle.Dots", "LineStyle.DotsRare", "LineStyle.LinesDots"):
        if forbidden in chart_source:
            raise SystemExit(f"{chart_path.name}: forbidden non-solid line style {forbidden}")

compact_label_renderer = labels[labels.find("private void DrawCompactPlanLabel("):]
if "ResolveCanonicalPlanLineColor(" not in compact_label_renderer or "semanticColor" not in compact_label_renderer:
    raise SystemExit("level labels must use the exact canonical signal-line color resolver")
if "Chart.DrawText(" not in compact_label_renderer:
    raise SystemExit("level labels must own their native ChartText object")
if "Chart.DrawRectangle(" in compact_label_renderer:
    raise SystemExit("level labels must remain background-free")
label_anchor = read("UI/Chart/PlanLabelAnchorCalculator.cs")
if (
    "CompactPlanLabelGapBars = 1" not in label_anchor or
    "Chart.BarIndexToX(" not in label_anchor or
    "GetCompactPlanLabelAnchorBar(" not in label_anchor or
    "canonicalLineLeftBar -\n                CompactPlanLabelGapBars" not in label_anchor
):
    raise SystemExit("level labels must keep exactly one chart-bar left clearance in the canonical anchor owner")
if "HorizontalAlignment.Right" not in compact_label_renderer:
    raise SystemExit("level labels must use right-aligned text at the left-of-line anchor")
if "PlanLinePresentationRule.ResolveThickness(" not in line:
    raise SystemExit("plan signal line thickness must use the canonical presentation rule")
if "MinimumThickness = 1" not in line_presentation_rule or "return MinimumThickness;" not in line_presentation_rule:
    raise SystemExit("plan signal line thickness must preserve the canonical one-pixel contract")
if "Math.Min(1" in line:
    raise SystemExit("plan signal line renderer must not force valid thickness back to one")
if "line.Thickness" not in prediction_line or "1;" not in prediction_line:
    raise SystemExit("prediction signal line thickness must remain fixed at one")
if "RenderCompactPlanLabel(" in alert_renderer:
    raise SystemExit("legacy alert chart-label rendering remains")
if '"ALERT "' in alert_renderer:
    raise SystemExit("alert BUY/SELL chart text remains")
if "message.StartsWith(" not in alert_engine or "CFIP ENTRY BLOCKED" not in alert_engine:
    raise SystemExit("blocked alerts must be silently discarded")
if "SignalVisualLifecycleRule.IsPreTradePlanVisible(" not in visual_snapshot:
    raise SystemExit("visual snapshot must consume signal lifecycle expiry rule")
if "CurrentM5 - input.CreatedM5" not in visual_lifecycle:
    raise SystemExit("visual lifecycle must enforce bounded pre-trade age")
if "public bool ShowPopupAlerts" in popup_core or "PopupCriticalOnly" in popup_core:
    raise SystemExit("legacy popup core parameters remain")
if "public PanelCorner PopupPosition" in popup_advanced or "KeepPopupUntilNextAlert" in popup_advanced:
    raise SystemExit("legacy popup advanced parameters remain")
if not (ROOT / "UI" / "Panel" / "AlertDeliveryProcessor.cs").exists():
    raise SystemExit("unified panel alert processor is missing")
if "RecordPanelAlertDelivery(" not in read("UI/Panel/AlertDeliveryProcessor.cs"):
    raise SystemExit("panel alert delivery handoff is missing")
if "ResolvePanelAlertMessageColor(" not in read("UI/Panel/PanelAlertMessageRenderer.cs"):
    raise SystemExit("panel alert semantic color owner is missing")
if "ResolveCanonicalPlanLineColor(" not in labels:
    raise SystemExit("level label renderer must consume the canonical signal-line color resolver")
if "Chart.DrawRectangle(" in labels:
    raise SystemExit("level label renderer must remain background-free")
if "CompactPlanLabelGapBars = 1" not in label_anchor or "double targetX =\n                lineX -" not in label_anchor or "Chart.BarIndexToX(" not in label_anchor or "Chart.XToTime(" not in label_anchor:
    raise SystemExit("level label renderer must retain the one-bar left clearance in the canonical anchor owner")

# Phase 7.4 / G4 — analysis-only panel after execution UI extraction.
g4_overview_rows = read("UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs")
panel_rows = read("UI/Panel/PanelRowsRenderer.cs")

if "RenderPanelAutoTradingRows(" in panel_rows:
    raise SystemExit("legacy Auto Trading execution rows must not be rendered by Indicator")

if "GetCanonicalSignalPanelStatus()" not in g4_overview_rows:
    raise SystemExit("canonical signal status must remain visible in the overview panel")


parameter_source = "\n".join(
    p.read_text(encoding="utf-8") for p in PARAM_ROOT.glob("*.cs")
)
EXPECTED_CURRENT_PARAMETERS = 545
if len(re.findall(r"\[Parameter\s*\(", parameter_source)) != EXPECTED_CURRENT_PARAMETERS:
    raise SystemExit("public parameter contract changed unexpectedly")

print("Phase 9.10 accumulated auto-trade/protection audit PASS")
print("Remaining Indicator pending paths: shared submission + server-protection hooks PASS")
print("Smart server TP + break-even ownership: PASS")
print("Local TP/BE mutation yields to broker-owned advanced protection: PASS")
print("All signal/plan level lines: Solid")
print("Plan Level Line Thickness: canonical 1px mapping for all configured values")
print("All compact level labels: exact line color, no background, one-bar left clearance")
print(f"Public parameter contract: {EXPECTED_CURRENT_PARAMETERS}")
print("Signal lifecycle / recent calibration / broker telemetry: PASS")
