#!/usr/bin/env python3
"""CBOT broker-confirmed execution facts / immediate state propagation audit."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(path):
    p = ROOT / path
    if not p.exists():
        errors.append("missing " + path)
        return ""
    return p.read_text(encoding="utf-8")

def require(condition, message):
    if not condition:
        errors.append(message)

market = read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
snapshot = read("src/CFIP.Contracts/CbotExecutionStateSnapshot.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
management = read("src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs")
workflow = read(".github/workflows/source-check.yml")
architecture = read("docs/ARCHITECTURE.md")

require(
    "result.Position.StopLoss.Value" in market and
    "result.Position.TakeProfit.Value" in market,
    "market BrokerExecutionReport must carry broker-confirmed Stop/Target facts",
)
require(
    "ConfirmedEntry" in snapshot and
    "ConfirmedStop" in snapshot and
    "ConfirmedTarget" in snapshot,
    "BrokerExecutionReport confirmed-fact fields are missing",
)
require(
    "ReconcileBrokerState(true);" in bot and
    '"BROKER POSITION CONFIRMED"' in bot and
    '"BROKER PENDING ORDER CONFIRMED"' in bot,
    "cBot must immediately reconcile and publish after broker submission result",
)
require(
    "position.EntryPrice" in publisher and
    "position.StopLoss" in publisher and
    "position.TakeProfit" in publisher,
    "state publisher must consume live broker position facts",
)
require(
    "Broker state is authoritative after mutations." in architecture and
    "Broker-confirmed positions and pending orders are authoritative;" in architecture,
    "architecture must retain broker-confirmed state authority",
)
require(
    "audit_phase_cbot_broker_confirmed_facts_2026_10_03.py" in workflow,
    "dedicated broker-confirmed-facts audit is not wired into Source/Architecture CI",
)

if errors:
    print("CBOT BROKER-CONFIRMED FACTS AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT BROKER-CONFIRMED FACTS AUDIT: PASS")
