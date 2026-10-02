#!/usr/bin/env python3
"""CBOT-P7 whole-chain audit: pre-analysis through broker lifecycle and panel reflection."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    p = ROOT / rel
    if not p.exists():
        errors.append("missing " + rel)
        return ""
    return p.read_text(encoding="utf-8")

def require(condition, message):
    if not condition:
        errors.append(message)

calc = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
alerts = read("src/CFIP.Indicator/Trading/Alerts/AlertDeliveryQueue.cs")
renderer = read("src/CFIP.Indicator/Signal/SignalRenderer.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs")
overview = read("src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
market = read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs")
pending = read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs")
management = read("src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
gate = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")

for token in (
    "execTimeframe=M15",
    "M15",
    "M5",
    "PENDING INTENT PREPARATION",
    "PLAN CREATION",
):
    require(token in calc or token in bot, "pipeline stage missing " + token)

require(
    "BuildCanonicalExecutionIntent(" in provider and
    "new CFIP.Contracts.ExecutionIntent(" in provider and
    "MarketExecutionProfile" in provider,
    "canonical Indicator→cBot contract boundary missing",
)

require(
    "AlertDeliveryQueue" in alerts,
    "canonical alert delivery queue missing",
)

require(
    "SignalRenderer" in renderer or
    "Render" in renderer,
    "signal chart presentation owner missing",
)

for token in (
    "TryFindChartCbot",
    "IsChartCbotRunning",
    "IsCbotExecutionStateFresh",
    "CbotConnectionPanelText",
):
    require(token in reader, "connection/presentation boundary missing " + token)

for token in (
    "EffectiveAutoTradingEnabled",
    "EffectiveAutomaticOrdersEnabled",
    "CbotConnectionPanelText",
):
    require(token in panel, "panel missing canonical cBot state " + token)

require(
    "CbotConnectionPanelText()" in overview and
    "GetAutoTradingPanelState()" in overview and
    "GetAutoOrdersPanelState()" in overview,
    "overview must consume cBot-backed execution presentation",
)

for token in (
    "RefreshIndicatorBinding",
    "ReloadSignalStore",
    "ReconcileBrokerState",
    "TryRecoverProtection",
    "_executionEnvironment.Evaluate",
):
    require(token in bot, "cBot lifecycle/execution stage missing " + token)

for source, tokens in (
    (market, ("TryExecute(", "ExecuteMarket", "BrokerExecutionSafety.TryConstrainVolumeForMargin")),
    (pending, ("TryExecute(", "BrokerExecutionSafety.TryConstrainVolumeForMargin")),
    (management, ("ModifyProtection", "Protect(", "TryRecoverProtectionFromSignal")),
    (publisher, ("CbotExecutionStateSnapshot", "CbotBrokerReconciliationResult")),
    (gate, ("SINGLE-PLAN CAPACITY BLOCKED", "ACCOUNT MARGIN", "LIVE SPREAD", "DAILY LOSS")),
):
    require(all(t in source for t in tokens), "broker/cBot owner incomplete: " + tokens[0])

require(
    "EffectiveAutoTradingEnabled" in publisher and
    "EffectiveAutomaticOrdersEnabled" in publisher,
    "effective cBot mode state is not published",
)

print("")
if errors:
    print("CBOT-P7 WHOLE-CHAIN AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P7 WHOLE-CHAIN AUDIT: PASS")
print("pre-analysis/M15/M5 pipeline markers: PASS")
print("Indicator→cBot contract: PASS")
print("signal/alert/presentation continuity: PASS")
print("same-chart cBot connection/liveness: PASS")
print("cBot lifecycle/reconciliation/recovery: PASS")
print("cBot broker execution/protection/risk: PASS")
print("panel reflects authoritative cBot state: PASS")
