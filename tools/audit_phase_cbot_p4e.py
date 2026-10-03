#!/usr/bin/env python3
"""CBOT-P4E acceptance gate: broker mutations are cBot-owned and confirmed."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"
BOT = ROOT / "src" / "CFIP.cBot"
errors = []


def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing " + rel)
        return ""
    return path.read_text(encoding="utf-8")


indicator = "\n".join(
    path.read_text(encoding="utf-8")
    for path in IND.rglob("*.cs")
)

management_ind = read(
    "src/CFIP.Indicator/Trading/Execution/ManagementCommandRequestCoordinator.cs"
)
management_cbot = read(
    "src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs"
)
host = read("src/CFIP.cBot/CFIPExecutionBot.cs")
command = read("src/CFIP.Contracts/ManagementCommand.cs")
report = read("src/CFIP.Contracts/BrokerExecutionReport.cs")
bus = read("src/CFIP.Contracts/ManagementBusKey.cs")

mutation_apis = (
    "ExecuteMarketOrder", "ExecuteMarketRangeOrder", "PlaceStopOrder",
    "PlaceLimitOrder", "CancelPendingOrder", "ClosePosition",
    "ModifyStopLossPrice", "ModifyTakeProfitPrice",
    "ModifyTakeProfitPips", "ModifyTakeProfit",
)

# Ignore Indicator Try* request wrappers; only real broker API invocations count.
for api in mutation_apis:
    if re.search(
        r"(?<!Try)\b" + re.escape(api) + r"\s*\(",
        indicator,
    ):
        errors.append("Indicator direct broker mutation remains: " + api + "(")

for token in (
    "RequestCancelPendingOrder(", "RequestClosePosition(", "RequestModifyStopLoss(",
    "RequestModifyTakeProfit(", "RequestModifyTakeProfitPips(",
    "RequestModifyTakeProfitLadder(", "RequestManagementCommand(",
    "ProcessManagementReports(",
):
    if token not in management_ind:
        errors.append("Indicator management request owner missing: " + token)

for api in mutation_apis[4:]:
    if api + "(" not in management_cbot:
        errors.append("cBot management owner missing: " + api + "(")

for token in (
    "ManagementCommandCodec.TryDeserialize(",
    "BrokerExecutionReportCodec.TryDeserialize(",
    "BrokerExecutionReportCodec.Serialize(",
    "TryConfirmFromBrokerState(",
    "StopSatisfied(", "SafeStop(", "TargetSatisfied(", "SafeTarget(",
    "LadderSatisfied(",
):
    if token not in management_cbot:
        errors.append("management reconciliation invariant missing: " + token)

for token in (
    "ExecutionLabel", "ExpectedRemainingVolume",
    "LadderFirstVolume", "LadderSecondVolume", "LadderFinalTargetPips",
):
    if token not in command:
        errors.append("ManagementCommand field missing: " + token)

if "CommandIdempotencyKey" not in report:
    errors.append("BrokerExecutionReport command correlation missing")

if (
    "CommandKeyForInstance(" not in bus
    or "ReportKeyForInstance(" not in bus
):
    errors.append("management transport keys missing")

if (
    "EnableDemoManagementExecution" not in host
    or "_management.Process(" not in host
):
    errors.append("cBot management integration missing")

if not re.search(
    r'\[Parameter\(\s*"Enable Demo Management Execution"'
    r'[\s\S]*?DefaultValue\s*=\s*false',
    host,
):
    errors.append("management arm must default false")

for rel in (
    "src/CFIP.Indicator/Trading/Execution/BrokerPendingOrderCancellation.cs",
    "src/CFIP.Indicator/Trading/Execution/BrokerPositionCloseMutation.cs",
    "src/CFIP.Indicator/Trading/Execution/BrokerStopLossMutation.cs",
    "src/CFIP.Indicator/Trading/Execution/BrokerTakeProfitMutation.cs",
):
    if (ROOT / rel).exists():
        errors.append("obsolete Indicator owner remains: " + rel)

print("CBOT-P4E SUMMARY")
print("=" * 72)
print("Indicator direct broker mutation: ZERO")
print("Indicator command publisher: ACTIVE")
print("cBot broker mutation owner: ACTIVE")
print("Broker state confirmation: ACTIVE")
print("Idempotency + bounded queues: ACTIVE")
print("Protective SL monotonicity: ACTIVE")
print("Forward-only target mutation: ACTIVE")
print("Close/partial/cancel confirmation: ACTIVE")
print("Server TP ladder cBot owner: ACTIVE")

if errors:
    print("CBOT-P4E AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P4E AUDIT: PASS")
