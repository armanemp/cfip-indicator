#!/usr/bin/env python3
"""CBOT-P7 UI/state cutover audit."""

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

contract = read("src/CFIP.Contracts/CbotExecutionStateSnapshot.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
events = read("src/CFIP.Indicator/Runtime/Cbot/CbotChartLifecycleEvents.cs")
init = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
factory = read("src/CFIP.Indicator/UI/Controls/ExecutionControlsFactory.cs")
sync = read("src/CFIP.Indicator/UI/Controls/ExecutionControlsSynchronizer.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs")
presentation = read("src/CFIP.Indicator/Core/Math/ExecutionControlPresentationRule.cs")
overview = read("src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs")
workflow = read(".github/workflows/source-check.yml")

for token in (
    "IndicatorAutoTradingEnabled",
    "IndicatorAutomaticOrdersEnabled",
    "EffectiveAutoTradingEnabled",
    "EffectiveAutomaticOrdersEnabled",
):
    require(token in contract, "shared execution snapshot missing " + token)
    require(token in publisher, "publisher missing " + token)

require(
    "CbotIndicatorExecutionSettings executionSettings" in publisher,
    "publisher must consume the bound Indicator settings snapshot",
)
require(
    "executionSettings != null &&" in publisher,
    "effective execution state must require bound Indicator settings",
)

for token in (
    "ChartRobots",
    "ChartRobot",
    "TryFindChartCbot",
    'CbotIdentity.DisplayName',
    '"Running"',
    "IsCbotExecutionStateFresh()",
):
    require(token in reader, "connection truth missing " + token)

for token in (
    "ChartRobots.RobotAdded",
    "ChartRobots.RobotRemoved",
    "ChartRobots.RobotModified",
    "ChartRobots.RobotStarted",
    "ChartRobots.RobotStopped",
):
    require(token in events, "missing event-driven cBot UI refresh " + token)

require(
    "SubscribeCbotChartLifecycleEvents();" in init and
    "UnsubscribeCbotChartLifecycleEvents();" in init,
    "Indicator cBot lifecycle event ownership missing",
)

require(
    "IsInteractive => false" in presentation,
    "execution controls must remain non-interactive",
)
require(
    "_autoTradingQuickToggle.Click +=" not in factory and
    "_automaticOrdersQuickToggle.Click +=" not in factory,
    "execution controls must not mutate state through click handlers",
)
require(
    "IsEnabled = false" in factory,
    "execution status controls must remain disabled",
)
require(
    "AutoTradingEnabled" not in factory and
    "AutomaticOrdersEnabled" not in factory,
    "execution controls factory must not consume Indicator runtime execution state",
)
require(
    "EnsureExecutionRuntimeState();" not in sync,
    "execution-control synchronizer must not own Indicator execution runtime state",
)
require(
    "EffectiveAutoTradingEnabled" in sync and
    "EffectiveAutomaticOrdersEnabled" in sync,
    "execution-control synchronizer must consume cBot effective state",
)

for token in (
    "EffectiveAutoTradingEnabled",
    "EffectiveAutomaticOrdersEnabled",
    "IndicatorAutoTradingEnabled",
    "IndicatorAutomaticOrdersEnabled",
):
    require(token in panel, "panel execution state missing cBot snapshot field " + token)

require(
    "CbotConnectionPanelText()" in overview and
    "GetAutoTradingPanelState()" in overview and
    "GetAutoOrdersPanelState()" in overview,
    "overview must use canonical cBot-backed presentation",
)

require(
    "ExecutionControlPresentationRule" in factory,
    "shared status presentation rule must remain in use",
)

require(
    "python tools/audit_phase_cbot_p7_ui_state_cutover.py" in workflow,
    "P7 dedicated audit must be accumulated in Source/Architecture CI",
)

if errors:
    print("CBOT-P7 UI/STATE CUTOVER AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P7 UI/STATE CUTOVER AUDIT: PASS")
print("shared effective execution state: PASS")
print("event-driven chart cBot lifecycle refresh: PASS")
print("read-only execution controls: PASS")
print("panel cBot snapshot authority: PASS")
print("no Indicator UI broker mutation: PASS")
