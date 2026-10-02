from pathlib import Path

ROOT = Path("src/CFIP.Indicator")

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing G4 owner: {relative}")
    return path.read_text(encoding="utf-8")

rule = read("Core/Math/ExecutionProtectionPanelStateRule.cs")
panel = read("UI/Panel/PanelExecutionState.cs")
overview_rows = read("UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs")
optimization = read("UI/Panel/PanelRenderOptimization.cs")
state = read("Indicator/State.cs")
program_path = Path("tools/CFIP.Runtime.Contracts/Program.cs")
program = program_path.read_text(encoding="utf-8")
runtime_csproj = Path("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj").read_text(encoding="utf-8")

for token in (
    "enum ExecutionPanelStateKind",
    "enum ProtectionPanelStateKind",
    "ResolveAutoTrading(",
    "ResolveAutoOrders(",
    "ResolveProtection(",
    "recoveryRequired",
    "brokerStopValid",
    "targetRequired",
    "serverLadderActive",
):
    if token not in rule:
        raise SystemExit(f"G4 canonical state rule missing: {token}")

for token in (
    "ExecutionProtectionPanelStateRule.ResolveAutoTrading(",
    "ExecutionProtectionPanelStateRule.ResolveAutoOrders(",
    "ExecutionProtectionPanelStateRule.ResolveProtection(",
    "IsExistingManagedStopHealthy(",
    "IsValidTarget(",
    "_serverSideTakeProfitLadderActive",
    "_brokerProtectionRecoveryRequired",
):
    if token not in panel:
        raise SystemExit(f"G4 panel state owner missing: {token}")

auto_color = panel.split("private Color GetAutoTradingPanelColor()", 1)[1].split("private string GetAutoOrdersPanelState()", 1)[0]
if "_decision" in auto_color or "_reaction" in auto_color:
    raise SystemExit("Auto-trade panel color must not infer operational state from decision/reaction")

PANEL_ROWS = read("UI/Panel/PanelRowsRenderer.cs")
if "RenderPanelAutoTradingRows(" in PANEL_ROWS:
    raise SystemExit("Indicator panel must not render the removed Auto Trading execution block")
if "GetCanonicalSignalPanelStatus()" not in PANEL_ROWS.replace("\n", " "):
    signal_status = read("UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs")
    if "GetCanonicalSignalPanelStatus()" not in signal_status:
        raise SystemExit("Indicator panel must expose canonical analysis signal status")

for token in (
    "GetAutoTradingPanelState()",
    "GetAutoOrdersPanelState()",
    "GetAutoProtectionPanelState()",
    "_brokerProtectionRecoveryRequired",
):
    if token not in optimization:
        raise SystemExit(f"G4 panel presentation key is missing state dependency: {token}")

for field in (
    "_autoTradingEnabledRuntime",
    "_automaticOrdersEnabledRuntime",
    "_brokerProtectionRecoveryRequired",
):
    if field not in state:
        raise SystemExit(f"G4 runtime state field missing: {field}")

if "VerifyExecutionProtectionPanelStateG4();" not in program:
    raise SystemExit("G4 runtime contract is not invoked")
if "ExecutionProtectionPanelStateRule.cs" not in runtime_csproj:
    raise SystemExit("G4 runtime contract project must compile the new Core state rule")

print("CR7.4 / G4 panel execution/protection state audit PASS")
print("Legacy execution panel block: removed from Indicator")
print("Canonical signal state: analysis-owned")
print("Broker protection/execution UI: cBot-owned in the staged migration")
