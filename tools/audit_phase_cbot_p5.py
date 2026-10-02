#!/usr/bin/env python3
"""CI-20 cBot P5 state-continuity audit."""

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

contracts = "\n".join([
    read("src/CFIP.Contracts/CbotExecutionStateSnapshot.cs"),
    read("src/CFIP.Contracts/CbotExecutionStateBus.cs"),
])
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
management = read("src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs")
indicator_state = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
indicator_panel = read("src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs")
workflow = read(".github/workflows/source-check.yml")

def require(condition, message):
    if not condition:
        errors.append(message)

for token in (
    "CbotExecutionStateSnapshot",
    "IndicatorInstanceId",
    "ObservedUtc",
    "ManagedPositions",
    "ManagedPendingOrders",
    "ManagementExecutionEnabled",
):
    require(token in contracts, "cBot state contract missing " + token)

for token in (
    "CbotExecutionStateBusKey",
    "CbotExecutionStateCodec",
    "JsonSerializer",
):
    require(token in contracts, "cBot state transport missing " + token)

for token in (
    "CbotExecutionStatePublisher",
    "SubscribeBrokerLifecycleEvents()",
    "PublishExecutionState("HEARTBEAT"",
    "PublishExecutionState("SIGNAL OBSERVED"",
    "Positions.Opened",
    "Positions.Modified",
    "Positions.Closed",
    "PendingOrders.Created",
    "PendingOrders.Modified",
    "PendingOrders.Filled",
    "PendingOrders.Cancelled",
):
    require(token in bot, "cBot lifecycle/state owner missing " + token)

require(
    "LocalStorage.Flush" in publisher and
    "CbotExecutionStateBusKey.ForIndicatorInstance" in publisher,
    "cBot state publisher must persist/flush through the canonical device bus",
)
require(
    "TryClosePosition" in management or
    "ManagementCommandType.FullClose" in management,
    "cBot management execution owner missing close path",
)
require(
    "TryModifyStop" not in indicator_state and
    "ExecuteMarketOrder" not in indicator_state,
    "Indicator state reader must remain observation-only",
)
for token in (
    "RefreshCbotExecutionStateIfDue",
    "IsCbotExecutionStateFresh",
    "CbotExecutionStatePanelText",
):
    require(token in indicator_state, "Indicator cBot state reader missing " + token)

require(
    "CbotExecutionStatePanelText()" in indicator_panel,
    "Panel execution state must consume cBot runtime state",
)
require(
    "audit_phase_cbot_p5.py" in workflow or
    "audit_phase_cbot_p5" in workflow,
    "CI-20 cBot P5 audit is not wired into Source/Architecture",
)

if errors:
    print("CI-20 CBOT P5 STATE CONTINUITY AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI-20 CBOT P5 STATE CONTINUITY AUDIT: PASS")
print("cBot lifecycle event observation: PASS")
print("cBot broker runtime state publication: PASS")
print("Indicator read-only state reflection: PASS")
print("Management broker authority remains cBot-owned: PASS")
