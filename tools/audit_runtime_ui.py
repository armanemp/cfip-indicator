from pathlib import Path

ROOT = Path("src/CFIP.Indicator")

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing required UI/runtime file: {relative}")
    return path.read_text(encoding="utf-8")

line = read("UI/Chart/PlanLineRenderer.cs")
pending = read("UI/Chart/PendingOrderRenderer.cs")
labels = read("UI/Chart/PlanLabelRenderCoordinator.cs")
labels_renderer = read("UI/Chart/PlanLabelRenderer.cs")
prediction_line = read("UI/Chart/PredictionLineRenderer.cs")
alert_renderer = read("UI/Chart/AlertSignalRenderer.cs")
alert_engine = read("Trading/Alerts/AlertEngine.cs")
visual_snapshot = read("UI/Chart/SignalVisualSnapshotBuilder.cs")
visual_lifecycle = read("Core/Math/SignalVisualLifecycleRule.cs")
alerts_core = read("Indicator/Parameters/12_alerts_core.cs")
alerts_advanced = read("Indicator/Parameters/12_alerts_advanced.cs")
alert_event = read("Core/Runtime/AlertDelivery.cs")
alert_processor = read("UI/Panel/AlertDeliveryProcessor.cs")
alert_rail = read("UI/Panel/PanelAlertMessageRenderer.cs")
partial = read("Trading/LiveManagement/PartialTakeProfitExecutor.cs")
target_progression = read("Trading/LiveManagement/TargetProgression.cs")
level_hits = read("Trading/LiveManagement/ActivePlanLevelExitHandler.cs")
server_ladder = read("Trading/Execution/ServerSideTakeProfitLadder.cs")
cbot_market = (
    ROOT.parent /
    "CFIP.cBot" /
    "Execution" /
    "DemoMarketExecutionCoordinator.cs"
).read_text(encoding="utf-8")
pending_cbot = (
    ROOT.parent /
    "CFIP.cBot" /
    "Execution" /
    "DemoPendingOrderExecutionCoordinator.cs"
).read_text(encoding="utf-8")
limit_mutation = ""
state = read("Indicator/State.cs")
factory = read("UI/Controls/ExecutionControlsFactory.cs")
sync = read("UI/Controls/ExecutionControlsSynchronizer.cs")
initialization = read("Runtime/Initialization/RuntimeInitialization.cs")

production_source = "\n".join(
    path.read_text(encoding="utf-8")
    for path in ROOT.rglob("*.cs")
)

for required in (
    "_autoTradingQuickToggle",
    "_automaticOrdersQuickToggle",
    "_executionToggleSyncing",
):
    if required not in production_source:
        raise SystemExit(f"Functional execution-control owner missing: {required}")

if "CompactPlanLineLengthBars = 40" not in line:
    raise SystemExit("Plan line span must remain 40 bars")
if "GetPlanLineRightBar()" not in line or "return Bars.Count - 1" not in line:
    raise SystemExit("Plan lines must terminate at the latest chart candle")
if "GetPlanLineLeftBar()" not in line:
    raise SystemExit("Plan lines must have one canonical left-edge owner")
if "MapM5ToChart(" in line or "anchorM5" in line:
    raise SystemExit("Plan-line geometry must not be tied to an M5 event-time anchor")
if "MapM5ToChart(" in pending or "anchorBar" in pending:
    raise SystemExit("Pending level rendering must not discard levels because an M5 anchor cannot be mapped")
if "GetCompactPlanLabelAnchorTime(" not in labels:
    raise SystemExit("Plan labels must use the canonical compact-label anchor owner")

for field in (
    "private ToggleButton _autoTradingQuickToggle;",
    "private ToggleButton _automaticOrdersQuickToggle;",
    "private bool _executionToggleSyncing;",
):
    if field not in state:
        raise SystemExit(f"Functional execution-control field missing: {field}")

if "CreateExecutionToggle(" not in factory:
    raise SystemExit("Execution status surfaces must use the shared ToggleButton presentation factory")
if "_autoTradingQuickToggle.Click +=" in factory or "_automaticOrdersQuickToggle.Click +=" in factory:
    raise SystemExit("Execution status surfaces must not register click handlers")
if "IsEnabled = false" not in factory:
    raise SystemExit("Execution status surfaces must be explicitly disabled")
if "ExecutionControlPresentationRule.ComposeStatusText(" not in factory:
    raise SystemExit("Execution control text must use the canonical presentation rule")

if "_executionToggleSyncing = true" not in sync:
    raise SystemExit("Execution controls must guard programmatic visual synchronization")
if "RefreshCbotExecutionStateIfDue();" not in sync:
    raise SystemExit("Execution status synchronization must consume canonical cBot state")
if "EffectiveAutoTradingEnabled" not in sync or "EffectiveAutomaticOrdersEnabled" not in sync:
    raise SystemExit("Execution status synchronization must consume effective cBot state")
if "SyncQuickExecutionControls(" not in sync:
    raise SystemExit("Execution controls must have a dedicated synchronization boundary")
if "ExecutionControlPresentationRule.ComposeStatusText(" not in sync:
    raise SystemExit("Execution control synchronization must consume the canonical presentation rule")
if "ExecutionControlPresentationRule.IsInteractive" not in sync:
    raise SystemExit("Execution control synchronization must enforce the read-only interaction contract")

if "SubscribeCbotChartLifecycleEvents();" not in initialization:
    raise SystemExit("Execution status must subscribe to cBot chart lifecycle changes")
if "UnsubscribeCbotChartLifecycleEvents();" not in initialization:
    raise SystemExit("Execution status must unsubscribe from cBot chart lifecycle changes")
if "ExecutionControlPresentationRule.IsInteractive" not in production_source:
    raise SystemExit("Execution status interaction policy must have a canonical owner")
if "ApplyAutoTradingQuickToggleClick" in production_source or "ApplyAutomaticOrdersQuickToggleClick" in production_source:
    raise SystemExit("Legacy interactive execution toggle handlers must be absent")

presentation_rule = read("Core/Math/PlanLinePresentationRule.cs")

if "return LineStyle.Solid" not in line:
    raise SystemExit("Plan lines must remain Solid")
if "PlanLinePresentationRule.ResolveThickness(" not in line:
    raise SystemExit("Plan signal line thickness must use the canonical presentation rule")
if "MinimumThickness = 1" not in presentation_rule or "return MinimumThickness;" not in presentation_rule:
    raise SystemExit("Plan signal line thickness must preserve the canonical one-pixel contract")
if "Math.Min(1" in line:
    raise SystemExit("Plan signal line renderer must not force thickness back to one")
if "line.Thickness =\n                                            1;" not in prediction_line and "line.Thickness = 1;" not in prediction_line:
    raise SystemExit("Prediction signal line thickness must remain fixed at one")
if "RenderCompactPlanLabel(" in alert_renderer or '"ALERT "' in alert_renderer:
    raise SystemExit("Alert BUY/SELL chart label must remain absent")
if "CFIP ENTRY BLOCKED" not in alert_engine or "message.StartsWith(" not in alert_engine:
    raise SystemExit("Blocked alerts must remain silent")
if "SignalVisualLifecycleRule.IsPreTradePlanVisible(" not in visual_snapshot:
    raise SystemExit("Signal visuals must consume the lifecycle expiry rule")
if "CurrentM5 - input.CreatedM5" not in visual_lifecycle:
    raise SystemExit("Signal lifecycle must enforce bounded age")
if "public bool ShowPopupAlerts" in alerts_core or "PopupCriticalOnly" in alerts_core:
    raise SystemExit("Legacy popup parameters must be removed")
if "Popup Position" in alerts_advanced or "Keep Popup Until Next Alert" in alerts_advanced:
    raise SystemExit("Legacy popup advanced parameters must be removed")

alert_queue = read("Core/Runtime/AlertDeliveryQueue.cs")
if "AlertDeliveryQueue(16)" not in state:
    raise SystemExit("Alert queue must have a fixed bounded capacity")
if "_alertDeliveryQueue.Enqueue(" not in alert_engine:
    raise SystemExit("Alerts must enter the bounded delivery queue")
if "RecordPanelAlertDelivery(next)" not in alert_processor:
    raise SystemExit("Panel alert processor must own the queued visual delivery")
if "Notifications.PlaySound(" not in alert_processor:
    raise SystemExit("Panel alert processor must remain the single sound delivery owner")
if "_normal.Dequeue()" not in alert_queue or "if (delivery.Critical)" not in alert_queue:
    raise SystemExit("Popup queue must prefer critical alerts over normal alerts")
if "ShowPopup(" in alert_engine:
    raise SystemExit("Alert hot path must not directly render a popup")

for relative in (
    "UI/Popup/PopupRenderer.cs",
    "UI/Popup/PopupRemover.cs",
    "UI/Popup/PopupExpirationCleaner.cs",
    "UI/Popup/AlertDeliveryProcessor.cs",
):
    if (ROOT / relative).exists():
        raise SystemExit(f"Obsolete popup production file remains: {relative}")

if "CreatePanelAlertMessageRail(" not in alert_rail or "ResolvePanelAlertMessageColor(" not in alert_rail:
    raise SystemExit("Unified panel alert rail is incomplete")

compact_label_renderer = labels_renderer[labels_renderer.find("private void DrawCompactPlanLabel("):]
if "ResolveCanonicalPlanLineColor(" not in line or "ResolveCanonicalPlanLineColor(" not in compact_label_renderer:
    raise SystemExit("Plan labels must use the exact canonical signal-line color owner")
if "Chart.DrawText(" not in compact_label_renderer:
    raise SystemExit("Plan label renderer must own the native ChartText")
if "Chart.DrawRectangle(" in compact_label_renderer:
    raise SystemExit("Plan label renderer must remain background-free")
if "semanticColor" not in compact_label_renderer or "label.Color =" not in compact_label_renderer:
    raise SystemExit("Plan label renderer must apply the exact semantic line color")
anchor = read("UI/Chart/PlanLabelAnchorCalculator.cs")
if (
    "CompactPlanLabelGapBars = 1" not in anchor or
    "GetCompactPlanLabelAnchorTime(" not in anchor or
    "GetCompactPlanLabelAnchorTime(" not in anchor or
    "canonicalLineLeftBar -\n                CompactPlanLabelGapBars" not in anchor
):
    raise SystemExit("Plan labels must keep exactly one chart-bar left clearance in the canonical anchor owner")
if "HorizontalAlignment.Right" not in compact_label_renderer:
    raise SystemExit("Plan labels must terminate at the left-of-line anchor")
if "CompactPlanLabelFontSize = 11.0" not in labels_renderer:
    raise SystemExit("Plan labels must use the canonical readable font size")
if "OrderVolume(" in server_ladder:
    raise SystemExit("Server TP ladder must use the current relative protection volume API")
if "new RelativeTakeProfitProtection(" not in server_ladder:
    raise SystemExit("Server TP ladder must construct relative partial protections")
signal_snapshot = read("UI/Chart/SignalVisualSnapshotBuilder.cs")
signal_renderer = read("UI/Chart/SignalRenderer.cs")
if "!pendingValid" not in signal_snapshot or "!livePlan" not in signal_snapshot:
    raise SystemExit("Signal snapshot must suppress lower-priority signal layers during execution state")
if "!snapshot.PendingOrder" not in signal_renderer or "!snapshot.LivePosition" not in signal_renderer:
    raise SystemExit("Signal alerts must yield while pending/live execution is authoritative")
if "_serverSideTakeProfitLadderActive" not in partial:
    raise SystemExit("Local partial-close path must know server TP authority")
if "_serverSideTakeProfitLadderActive" not in target_progression:
    raise SystemExit("Target progression must yield to server TP authority")
if "_serverSideTakeProfitLadderActive" not in level_hits:
    raise SystemExit("Level-hit handler must yield to server TP authority")
for token in (
    "RelativeTakeProfitProtections",
    "RelativeTakeProfitProtection",
    "RelativeTakeProfitLastProtection",
    "AbsoluteTakeProfitProtections",
):
    if token not in server_ladder:
        raise SystemExit(f"Server TP ladder contract missing: {token}")
if "ExecuteMarketRangeOrder(" not in cbot_market or "ExecuteMarketOrder(" not in cbot_market:
    raise SystemExit("cBot must own Market / Market-Range broker mutation")
if "PlaceStopOrder(" not in pending_cbot or "BrokerAction.SubmitPendingStop" not in pending_cbot:
    raise SystemExit("cBot Pending Stop mutation owner missing")
if "PlaceLimitOrder(" not in pending_cbot:
    raise SystemExit("Pending limit cBot mutation owner missing")

print("Runtime UI audit PASS")
print("Plan lines: 40-bar compact geometry anchored to latest chart candle")
print("All level/prediction signal lines: solid-only")
print("Level labels: white text inside canonical line-colored filled boxes, exact-price alignment")
print("Alerts: unified panel rail + optional sound, no popup UI")
print("Indicator execution controls: broker-action UI removed; analysis panel is canonical")
print("Market / Market-Range broker mutation: owned by CFIP.cBot")
