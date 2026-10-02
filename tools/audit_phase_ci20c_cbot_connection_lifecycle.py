#!/usr/bin/env python3
"""Stable CI20C architecture audit for cBot connection/lifecycle ownership."""

from pathlib import Path
import re
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
overview = read(
    "src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs"
)

def require(condition, message):
    if not condition:
        errors.append(message)

# Shared identity must have one stable owner.
require(
    re.search(
        r'public\s+const\s+string\s+DisplayName\s*=\s*"CFIP Smart Execution Bot"',
        contracts,
    ) is not None,
    "shared cBot display identity is missing",
)
require(
    re.search(
        r'public\s+const\s+string\s+DisplayName\s*=\s*"CFIP Smart Indicator"',
        binding,
    ) is not None,
    "Indicator binding identity is missing",
)
require(
    "CbotIdentity.DisplayName" in bot,
    "cBot Robot attribute must use shared identity",
)

# Late attach / reload must be event-driven, not tick-only.
for token in (
    "ChartIndicators.IndicatorAdded",
    "ChartIndicators.IndicatorRemoved",
    "ChartIndicators.IndicatorModified",
):
    require(token in bot, "missing cBot lifecycle subscription: " + token)

for token in (
    "OnChartIndicatorAdded",
    "OnChartIndicatorRemoved",
    "OnChartIndicatorModified",
    "UnsubscribeIndicatorLifecycleEvents",
):
    require(token in bot, "missing cBot lifecycle handler: " + token)

require(
    "PublishExecutionState(" in bot and "CBOT STOPPED" in bot,
    "cBot stop lifecycle must publish terminal state",
)

# Direct chart presence is the physical connection authority; heartbeat adds
# freshness and execution-state truth.
require(
    "ChartRobots" in reader and
    "CbotIdentity.DisplayName" in reader and
    "TryFindChartCbot" in reader and
    "CbotExecutionStateSnapshot" in reader and
    "ForIndicatorInstance(" in reader and
    "InstanceId" in reader and
    "IsCbotExecutionStateFresh()" in reader,
    "Indicator reader must use exact-instance fresh heartbeat contract",
)
require(
    "CbotConnectionPanelText" in reader,
    "canonical cBot connection presentation owner is missing",
)
for token in (
    "CBOT NOT CONNECTED",
    "CBOT RECONNECTING",
    "CBOT CONNECTED",
):
    require(token in reader, "missing canonical connection state: " + token)

# Execution capability must be freshness-gated and must not bypass the reader.
for method in (
    "CbotCanMarketExecute",
    "CbotCanPendingExecute",
    "CbotCanManage",
):
    match = re.search(
        rf"private\s+bool\s+{method}\s*\(.*?\n\s*\}}",
        reader,
        re.DOTALL,
    )
    require(
        match is not None and "IsCbotExecutionStateFresh()" in match.group(0),
        method + " must require fresh cBot state",
    )

require(
    panel.count("CbotConnectionPanelText()") >= 1,
    "execution panel must consume canonical cBot connection presentation",
)
require(
    "CbotConnectionPanelText()" in overview,
    "overview panel must consume canonical cBot connection presentation",
)

# The supported ChartRobots API is required so the panel can distinguish
# physically attached/stopped cBots from an absent cBot before consulting
# LocalStorage heartbeat freshness.
require(
    "foreach (ChartRobot candidate in ChartRobots)" in reader,
    "Indicator connection reader must enumerate chart robots directly",
)
require(
    "robot.State" in reader and
    'string.Equals(\n                state,\n                "Running"' in reader,
    "Indicator connection reader must use cBot runtime state",
)

if errors:
    print("CI20C CBOT CONNECTION/LIFECYCLE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI20C CBOT CONNECTION/LIFECYCLE AUDIT: PASS")
print("shared identity ownership: PASS")
print("event-driven cBot rebind: PASS")
print("exact-instance fresh heartbeat connection proof: PASS")
print("freshness-gated execution capability: PASS")
print("canonical panel connection presentation: PASS")
print("Indicator SDK compatibility boundary: PASS")
