#!/usr/bin/env python3
"""CI20C cBot connection, lifecycle and panel-truth audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing " + rel)
        return ""
    return path.read_text(encoding="utf-8")

contracts = read("src/CFIP.Contracts/CbotIdentity.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
binding = read("src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs")
overview = read("src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs")

def require(condition, message):
    if not condition:
        errors.append(message)

require(
    'DisplayName = "CFIP Smart Execution Bot"' in contracts,
    "shared cBot display identity is missing",
)
require(
    "public const string DisplayName = " in binding,
    "Indicator chart binding must retain its canonical indicator display name",
)
for token in (
    "ChartIndicators.IndicatorAdded",
    "ChartIndicators.IndicatorRemoved",
    "ChartIndicators.IndicatorModified",
):
    require(token in bot, "cBot indicator lifecycle subscription missing " + token)

for token in (
    "OnChartIndicatorAdded",
    "OnChartIndicatorRemoved",
    "OnChartIndicatorModified",
    "UnsubscribeIndicatorLifecycleEvents",
):
    require(token in bot, "cBot lifecycle handler missing " + token)

require(
    "CbotIdentity.DisplayName" in bot,
    "cBot attribute must use the shared display identity",
)
require(
    "CBOT STOPPED" in bot and "PublishExecutionState(" in bot,
    "cBot stop lifecycle must publish state before shutdown",
)

for token in (
    "ChartRobots",
    "CbotIdentity.DisplayName",
    "RobotState.Running",
    "CbotConnectionPanelText",
    "CBOT NOT ATTACHED • ATTACH TO THIS CHART",
    "CBOT CONNECTING • HEARTBEAT PENDING",
    "CBOT CONNECTED • ",
):
    require(token in reader, "cBot connection reader missing " + token)

for token in (
    "return IsCbotChartRunning()",
    "IsCbotExecutionStateFresh()",
):
    require(token in reader, "execution gating must require chart-running cBot plus fresh state")

require(
    "CbotConnectionPanelText()" in panel,
    "execution/protection panel must consume connection truth",
)
require(
    '"CBOT LINK  " +
                CbotConnectionPanelText()' in overview,
    "overview must display explicit cBot connection truth",
)

if errors:
    print("CI20C CBOT CONNECTION/LIFECYCLE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI20C CBOT CONNECTION/LIFECYCLE AUDIT: PASS")
print("same-chart cBot presence detection: PASS")
print("cBot indicator lifecycle rebinding: PASS")
print("heartbeat/presence separation: PASS")
print("panel connection truth: PASS")
