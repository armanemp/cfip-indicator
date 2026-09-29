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
partial = read("Trading/LiveManagement/PartialTakeProfitExecutor.cs")
target_progression = read("Trading/LiveManagement/TargetProgression.cs")
level_hits = read("Trading/LiveManagement/ActivePlanLevelExitHandler.cs")
server_ladder = read("Trading/Execution/ServerSideTakeProfitLadder.cs")
market_mutation = read("Trading/Execution/BrokerMarketOrderMutation.cs")
pending_mutation = read("Trading/Execution/BrokerPendingOrderPlacement.cs")
limit_mutation = read("Trading/Execution/BrokerLimitOrderPlacement.cs")
state = read("Indicator/State.cs")
factory = read("UI/Controls/ExecutionControlsFactory.cs")
sync = read("UI/Controls/ExecutionControlsSynchronizer.cs")
handlers = read("UI/Controls/ExecutionToggleHandlers.cs")
initialization = read("Runtime/Initialization/RuntimeInitialization.cs")

production_source = "\n".join(
    path.read_text(encoding="utf-8")
    for path in ROOT.rglob("*.cs")
)

for required in (
    "_autoTradingQuickToggle",
    "_automaticOrdersQuickToggle",
    "ApplyAutoTradingQuickToggleClick",
    "ApplyAutomaticOrdersQuickToggleClick",
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
if "GetPlanLineLeftBar(" not in labels:
    raise SystemExit("Plan labels must reuse the canonical plan-line left edge")
if "GetCompactPlanLineLeftBar(" in labels:
    raise SystemExit("Legacy compact-line anchor helper must not remain")

for field in (
    "private ToggleButton _autoTradingQuickToggle;",
    "private ToggleButton _automaticOrdersQuickToggle;",
    "private bool _executionToggleSyncing;",
):
    if field not in state:
        raise SystemExit(f"Functional execution-control field missing: {field}")

if "CreateExecutionToggle(" not in factory:
    raise SystemExit("Execution controls must be real operator ToggleButton surfaces")
for token in (
    "_autoTradingQuickToggle.Click +=",
    "_automaticOrdersQuickToggle.Click +=",
):
    if token not in factory:
        raise SystemExit(f"Execution toggle event owner missing: {token}")
if "_executionToggleSyncing = true" not in sync:
    raise SystemExit("Execution controls must guard programmatic visual synchronization")

if "EnsureExecutionRuntimeState();" not in sync:
    raise SystemExit("Execution status synchronization must consume canonical runtime/settings state")
if "SyncQuickExecutionControls(" not in sync:
    raise SystemExit("Execution controls must have a dedicated synchronization boundary")

for required in (
    "ApplyAutoTradingQuickToggleClick",
    "ApplyAutomaticOrdersQuickToggleClick",
):
    if required not in handlers:
        raise SystemExit(f"Execution UI click handler missing: {required}")

for required_setter in (
    "SetAutoTradingRuntimeState",
    "SetAutomaticOrdersRuntimeState",
):
    if required_setter not in handlers:
        raise SystemExit(f"Execution UI handler must call canonical runtime setter: {required_setter}")

if "EnableAutoTrading" not in initialization or "EnableAutomaticOrders" not in initialization:
    raise SystemExit("Execution status settings must be sourced from the public cTrader parameters")
if "EnsureExecutionRuntimeState()" not in initialization:
    raise SystemExit("Execution settings must retain a canonical runtime synchronization boundary")

if "return Color.White" not in labels_renderer:
    raise SystemExit("All compact plan-level text must be white")
if "Chart.DrawRectangle(" in labels_renderer:
    raise SystemExit("Plan label renderer must not create text backgrounds")
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
if "Chart.DrawRectangle(" in labels:
    raise SystemExit("Plan label coordinator must not create text backgrounds")
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
if "TryExecuteMarketRangeOrderWithTakeProfitLadder" not in market_mutation:
    raise SystemExit("Market-range server TP mutation owner missing")
if "TryPlaceStopOrderWithTakeProfitLadder" not in pending_mutation:
    raise SystemExit("Pending stop server TP mutation owner missing")
if "TryPlaceLimitOrderWithTakeProfitLadder" not in limit_mutation:
    raise SystemExit("Pending limit server TP mutation owner missing")

print("Runtime UI audit PASS")
print("Plan lines: 40-bar compact geometry anchored to latest chart candle")
print("Pending lines: no M5 mapping dependency")
print("AUTO TRADE/AUTO ORDERS: interactive runtime controls bound to canonical execution state")
print("Execution status: synchronized from canonical settings/runtime state")
