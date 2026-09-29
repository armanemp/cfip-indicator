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
auto_market = read("Trading/Execution/AutomaticMarket/AutomaticMarketBrokerExecution.cs")
aggressive = read("Trading/Execution/Aggressive/AggressiveBrokerExecution.cs")
pending_stop = read("Trading/Pending/Placement/ContinuationStopPlacement.cs")
pending_limit = read("Trading/Pending/Placement/ReversalLimitPlacement.cs")
ladder = read("Trading/Execution/ServerSideTakeProfitLadder.cs")
market_mutation = read("Trading/Execution/BrokerMarketOrderMutation.cs")
pending_mutation = read("Trading/Execution/BrokerPendingOrderPlacement.cs")
limit_mutation = read("Trading/Execution/BrokerLimitOrderPlacement.cs")
protection = read("Trading/LiveManagement/ProtectionManager.cs")
target_progression = read("Trading/LiveManagement/TargetProgression.cs")
partial_tp = read("Trading/LiveManagement/PartialTakeProfitExecutor.cs")
line = read("UI/Chart/PlanLineRenderer.cs")
labels = read("UI/Chart/PlanLabelRenderer.cs")
prediction_line = read("UI/Chart/PredictionLineRenderer.cs")
alert_renderer = read("UI/Chart/AlertSignalRenderer.cs")
alert_engine = read("Trading/Alerts/AlertEngine.cs")
visual_snapshot = read("UI/Chart/SignalVisualSnapshotBuilder.cs")
visual_lifecycle = read("Core/Math/SignalVisualLifecycleRule.cs")
popup_core = read("Indicator/Parameters/12_alerts_core.cs")
popup_advanced = read("Indicator/Parameters/12_alerts_advanced.cs")

execution_paths = {
    "automatic-market": auto_market,
    "aggressive-market": aggressive,
    "pending-stop": pending_stop,
    "pending-limit": pending_limit,
}
for name, source in execution_paths.items():
    for token in ("TryAcquireSubmission(", "TryBuildServerSideTakeProfitLadder("):
        if token not in source:
            raise SystemExit(f"{name}: missing shared {token}")

if "ValidateSingleExecutionCapacity(" not in read("Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs"):
    raise SystemExit("automatic-market: canonical capacity gate missing")
if "ValidateSingleExecutionCapacity(" not in read("Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs"):
    raise SystemExit("aggressive-market: canonical capacity gate missing")
if "ValidateSingleExecutionCapacity(" not in read("Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs"):
    raise SystemExit("pending: canonical capacity gate missing")

for source_name, source in (
    ("market mutation", market_mutation),
    ("pending-stop mutation", pending_mutation),
    ("pending-limit mutation", limit_mutation),
):
    if "StopLossBreakEven" not in source:
        raise SystemExit(f"{source_name}: server break-even transport missing")

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
telemetry = read("Trading/Execution/SubmissionGateCoordinator.cs")
if "AllowsQualityRecovery(" not in quality_rule:
    raise SystemExit("high-quality signal recovery gate is missing")
if "locationQuality <" not in evaluator or "timingQuality < 64" not in evaluator:
    raise SystemExit("actionability staging recovery boundary is missing")
if "RecordExecutionTelemetry(" not in telemetry or "RecordExecutionTelemetryFailure(" not in telemetry:
    raise SystemExit("broker submission telemetry owner is missing")
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

if "return Color.White" not in labels:
    raise SystemExit("level labels must use white text")
if "return\n                Math.Min(" not in line:
    raise SystemExit("plan signal line thickness must be fixed at one")
if "line.Thickness" not in prediction_line or "1;" not in prediction_line:
    raise SystemExit("prediction signal line thickness must be fixed at one")
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
if "public bool ShowPopupAlerts" not in popup_core or "DefaultValue = true" not in popup_core:
    raise SystemExit("popup alerts should be enabled by default")
if "public bool PopupCriticalOnly" not in popup_core or "DefaultValue = false" not in popup_core:
    raise SystemExit("popup must not be critical-only by default")
if "public PanelCorner PopupPosition" not in popup_advanced or "DefaultValue = PanelCorner.BottomLeft" not in popup_advanced:
    raise SystemExit("popup default position must be bottom-left")
if "public bool PopupBold" not in popup_core or "DefaultValue = true" not in popup_core:
    raise SystemExit("popup text should be bold by default")
if "Chart.DrawRectangle(" in labels:
    raise SystemExit("level label renderer must not create backgrounds")

parameter_source = "\n".join(
    p.read_text(encoding="utf-8") for p in PARAM_ROOT.glob("*.cs")
)
if len(re.findall(r"\[Parameter\s*\(", parameter_source)) != 552:
    raise SystemExit("public parameter contract changed unexpectedly")

print("Phase 9.10 accumulated auto-trade/protection audit PASS")
print("Automatic market / aggressive / pending paths: shared submission + server-protection hooks PASS")
print("Smart server TP + break-even ownership: PASS")
print("Local TP/BE mutation yields to broker-owned advanced protection: PASS")
print("All signal/plan level lines: Solid")
print("All level label text: White / background-free")
print("Public parameter contract: 552")
print("Signal lifecycle / quality recovery / broker telemetry: PASS")
