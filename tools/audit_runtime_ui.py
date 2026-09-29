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
if "SyncExecutionStatus(" not in sync:
    raise SystemExit("Execution status surfaces must have a dedicated synchronizer")

for forbidden in (
    "ApplyAutoTradingQuickToggleClick",
    "ApplyAutomaticOrdersQuickToggleClick",
    "SetAutoTradingRuntimeState",
    "SetAutomaticOrdersRuntimeState",
):
    if forbidden in handlers:
        raise SystemExit(f"Execution UI handler mutation remains: {forbidden}")

if "EnableAutoTrading" not in initialization or "EnableAutomaticOrders" not in initialization:
    raise SystemExit("Execution status settings must be sourced from the public cTrader parameters")
if "EnsureExecutionRuntimeState()" not in initialization:
    raise SystemExit("Execution settings must retain a canonical runtime synchronization boundary")

print("Runtime UI audit PASS")
print("Plan lines: 40-bar compact geometry anchored to latest chart candle")
print("Pending lines: no M5 mapping dependency")
print("AUTO TRADE/AUTO ORDERS: interactive runtime controls bound to canonical execution state")
print("Execution status: synchronized from canonical settings/runtime state")
