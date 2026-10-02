from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(relative):
    path = ROOT / "src" / "CFIP.Indicator" / relative
    if not path.exists():
        raise SystemExit(f"M5: missing source owner: {relative}")
    return path.read_text(encoding="utf-8")


def require(condition, reason):
    if not condition:
        raise SystemExit(reason)


layout = read("UI/Panel/PanelLayoutManager.cs")
main_renderer = read("UI/Panel/PanelMainRenderer.cs")
content_refresh = read("UI/Panel/PanelContentRefresh.cs")
row_writer = read("UI/Panel/PanelRowWriter.cs")
rows_renderer = read("UI/Panel/PanelRowsRenderer.cs")
heartbeat = read("Runtime/Supervision/PanelHeartbeatLiveState.cs")
panel_exec = read("UI/Panel/PanelExecutionState.cs")
cbot_reader = read("Runtime/Cbot/CbotExecutionStateReader.cs")
execution_rows = read("UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs")
auto_state = read("Trading/Execution/State/AutoTradingStateStore.cs")
calc_live = read("Runtime/Calculation/CalculationLiveCycle.cs")
alerts = read("Runtime/Calculation/CalculationDecisionAlerts.cs")
alert_renderer = read("UI/Chart/AlertSignalRenderer.cs")
provider = read("Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")

contracts = ROOT / "tools" / "CFIP.Runtime.Contracts" / "M5PanelContracts.cs"
program = ROOT / "tools" / "CFIP.Runtime.Contracts" / "Program.cs"
workflow = ROOT.parent / "tools" / ".github" if False else ROOT.parent.parent / ".github" / "workflows" / "source-check.yml"
m3_audit = ROOT / "tools" / "audit_phase_m3_trade_truth.py"
m4_audit = ROOT / "tools" / "audit_phase_m4_time_history.py"

dimension_rule = ROOT / "src" / "CFIP.Indicator" / "Core" / "Math" / "PanelDimensionRule.cs"

require(
    dimension_rule.exists() and
    "EffectiveWidth(" in dimension_rule.read_text(encoding="utf-8") and
    "EffectiveContentWidth(" in dimension_rule.read_text(encoding="utf-8"),
    "M5: pure panel dimension owner missing",
)

require(
    "PanelDimensionRule.EffectiveWidth(" in layout and
    "PanelDimensionRule.EffectiveContentWidth(" in layout and
    "EffectivePanelContentWidth(" in main_renderer and
    "EffectivePanelContentWidth(" in content_refresh and
    "EffectivePanelContentWidth(" in heartbeat,
    "M5: panel width arithmetic is not centralized",
)

require(
    "RefreshCbotExecutionStateIfDue();" in content_refresh and
    "BuildSignalVisualSnapshot(" in content_refresh and
    "RenderPanelRows(" in content_refresh and
    "_panelContentRefreshSequence++" in content_refresh,
    "M5: live panel content refresh must use one current snapshot per refresh",
)

require(
    "_panelOverflowCount++" in row_writer and
    "_panelOverflowReported" in row_writer and
    "_panelOverflowCount = 0;" in rows_renderer,
    "M5: panel row overflow must be observable and reset per render",
)

require(
    "_plan == null" in heartbeat and
    "SetPanelRow(" in heartbeat and
    'SetPanelRow(\n                        _panelLiveRow' in heartbeat or
    '_panelLiveRow >= 0' in heartbeat,
    "M5: stale live rows must be cleared when a plan disappears",
)

require(
    "RefreshCbotExecutionStateIfDue();" in panel_exec and
    "IsCbotExecutionStateFresh()" in panel_exec and
    "CBOT CONNECTED • MARKET DISARMED" in panel_exec and
    "CBOT CONNECTED • PENDING DISARMED" in panel_exec,
    "M5: panel execution status must be cBot-authoritative",
)

require(
    "AUTO TRADING  •  OFF  •  MANUAL REVIEW" not in auto_state and
    "return" in auto_state,
    "M5: stale Indicator-owned AUTO TRADING OFF presentation remains",
)

require(
    '"EXECUTION BOT  "' in execution_rows and
    '"CBOT ORDERS  "' in execution_rows and
    '"CBOT PROTECTION  "' in execution_rows,
    "M5: execution panel rows must expose explicit cBot ownership",
)

require(
    "BuildSignalVisualSnapshot(" in calc_live and
    "RenderLatestAlertSignalMarker(" in calc_live and
    "RenderPanel()" in calc_live and
    calc_live.index("RefreshLiveDecisionActionability(") <
    calc_live.index("BuildSignalVisualSnapshot(") <
    calc_live.index("RenderLatestAlertSignalMarker(") <
    calc_live.index("RenderPanel()"),
    "M5: live calculation chain lost decision -> visual -> panel ordering",
)

require(
    "ProcessCanonicalActionableEntryAlert(" in alerts and
    "GetManagedPendingOrder() != null" in alerts,
    "M5: actionable alert must retain canonical hierarchy gates",
)

require(
    "Chart.DrawIcon(" not in alert_renderer and
    "RenderLatestAlertSignalMarker(" in alert_renderer,
    "M5: alert mirror must not create a duplicate chart marker",
)

require(
    "BuildCanonicalExecutionIntent(" in provider and
    "ContractIdentity identity" in provider and
    "new SignalEnvelope(" in provider,
    "M5: provider boundary lost canonical identity/envelope publication",
)

require(
    "CfipDeviceSignalTransport.TryRead(" in (ROOT.parent / "CFIP.cBot" / "CFIPExecutionBot.cs").read_text(encoding="utf-8") and
    "_market.TryExecute(" in (ROOT.parent / "CFIP.cBot" / "CFIPExecutionBot.cs").read_text(encoding="utf-8") and
    "_pending.TryExecute(" in (ROOT.parent / "CFIP.cBot" / "CFIPExecutionBot.cs").read_text(encoding="utf-8"),
    "M5: cBot signal-to-broker execution handoff is incomplete",
)

require(
    contracts.exists() and
    "M5PanelContracts.Run();" in program.read_text(encoding="utf-8"),
    "M5: deterministic panel contracts are not wired into Runtime Acceptance",
)

require(
    m3_audit.exists() and
    m4_audit.exists() and
    workflow.exists(),
    "M5: accumulated M3/M4 audit foundation is missing",
)

workflow_text = workflow.read_text(encoding="utf-8")
require(
    "python tools/audit_phase_m3_trade_truth.py" in workflow_text and
    "python tools/audit_phase_m4_time_history.py" in workflow_text and
    "python tools/audit_phase_m5_panel_live.py" in workflow_text,
    "M5: full-chain accumulated audits are not wired into Source/Architecture CI",
)

print("=" * 72)
print("M5 Panel Live Content / Responsiveness audit: PASS")
print("=" * 72)
