from pathlib import Path

ROOT = Path("src/CFIP.Indicator")


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing G6B source: {relative}")
    return path.read_text(encoding="utf-8")


factory = read("UI/Controls/ExecutionControlsFactory.cs")
sync = read("UI/Controls/ExecutionControlsSynchronizer.cs")
state = read("Indicator/State.cs")
initialization = read("Runtime/Initialization/RuntimeInitialization.cs")
presentation_rule = read("Core/Math/ExecutionControlPresentationRule.cs")
runtime_program = Path("tools/CFIP.Runtime.Contracts/Program.cs").read_text(encoding="utf-8")
runtime_csproj = Path("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj").read_text(encoding="utf-8")
workflow = Path(".github/workflows/source-check.yml").read_text(encoding="utf-8")
production_source = "\n".join(
    path.read_text(encoding="utf-8")
    for path in ROOT.rglob("*.cs")
)

legacy_handlers = ROOT / "UI/Controls/ExecutionToggleHandlers.cs"
if legacy_handlers.exists():
    raise SystemExit("G6B legacy interactive execution-toggle handler file must be removed")

for token in (
    "CreateExecutionToggle(",
    "ExecutionControlPresentationRule.ComposeStatusText(",
    "IsEnabled = false",
):
    if token not in factory:
        raise SystemExit(f"G6B factory contract missing: {token}")

for token in (
    "_autoTradingQuickToggle.Click +=",
    "_automaticOrdersQuickToggle.Click +=",
):
    if token in factory:
        raise SystemExit(f"G6B status-only execution surface still has a click handler: {token}")

for token in (
    "RefreshCbotExecutionStateIfDue();",
    "EffectiveAutoTradingEnabled",
    "EffectiveAutomaticOrdersEnabled",
    "ExecutionControlPresentationRule.ComposeStatusText(",
    "ExecutionControlPresentationRule.IsInteractive",
    "_autoTradingQuickToggle.IsEnabled",
    "_automaticOrdersQuickToggle.IsEnabled",
):
    if token not in sync:
        raise SystemExit(f"G6B synchronizer contract missing: {token}")

if "Click +=" in production_source:
    # Existing non-execution panel buttons are allowed; only legacy quick-toggle
    # handlers are forbidden by their method names below.
    if "ApplyAutoTradingQuickToggleClick" in production_source:
        raise SystemExit("G6B interactive AUTO TRADE handler remains in production source")
    if "ApplyAutomaticOrdersQuickToggleClick" in production_source:
        raise SystemExit("G6B interactive AUTO ORDERS handler remains in production source")

for token in ("EnableAutoTrading", "EnableAutomaticOrders", "EnsureExecutionRuntimeState()"):
    if token not in initialization:
        raise SystemExit(f"G6B settings synchronization boundary missing: {token}")

if "_autoTradingEnabledRuntime" not in state or "_automaticOrdersEnabledRuntime" not in state:
    raise SystemExit("G6B runtime execution-state ownership is missing")

if "public static bool IsInteractive => false;" not in presentation_rule:
    raise SystemExit("G6B canonical presentation rule must be explicitly read-only")
for token in (
    "string normalizedCaption",
    'enabled ? "ON" : "OFF"',
):
    if token not in presentation_rule:
        raise SystemExit(f"G6B canonical presentation rule missing: {token}")

if "VerifyExecutionControlPresentationG6B();" not in runtime_program:
    raise SystemExit("G6B runtime contract is not invoked")
if "ExecutionControlPresentationRule.cs" not in runtime_csproj:
    raise SystemExit("G6B presentation rule is not included in Runtime Contracts")
if "python tools/audit_phase_7_6b.py" not in workflow:
    raise SystemExit("G6B static audit is not wired into Source/Architecture CI")

print("CR7.6b / G6B execution-control truth audit PASS")
print("AUTO TRADE/AUTO ORDERS are status-only surfaces")
print("public cTrader settings remain the execution configuration authority")
print("no in-panel execution mutation handler remains")
