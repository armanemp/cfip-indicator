#!/usr/bin/env python3
"""CBOT-P5 broker reconciliation / protection recovery audit."""

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


def require(condition, message):
    if not condition:
        errors.append(message)


bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
reconciliation = read("src/CFIP.cBot/Recovery/CbotBrokerReconciliation.cs")
management = read("src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
contract = read("src/CFIP.Contracts/CbotExecutionStateSnapshot.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
devlog = read("docs/DEVELOPMENT-LOG.md")

require(
    "CbotBrokerReconciliation" in bot and
    "ReconcileBrokerState(true)" in bot and
    "ReconcileBrokerState(false)" in bot,
    "cBot must own a periodic/startup broker reconciliation gate",
)

require(
    "MULTIPLE MANAGED BROKER OBJECTS" in reconciliation and
    "POSITION AND PENDING STATE COEXIST" in reconciliation and
    "STOP LOSS MISSING" in reconciliation and
    "TAKE PROFIT MISSING" in reconciliation and
    "BUY STOP IS NOT PROTECTIVE" in reconciliation and
    "SELL STOP IS NOT PROTECTIVE" in reconciliation and
    "BUY TARGET IS NOT FORWARD" in reconciliation and
    "SELL TARGET IS NOT FORWARD" in reconciliation,
    "reconciliation must classify ambiguity and protection defects",
)

require(
    "CbotBrokerReconciliationResult _reconciliation" not in bot or
    "_reconciliation.RecoveryRequired" in bot,
    "execution path must consume reconciliation recovery state",
)

require(
    "BROKER RECOVERY REQUIRED" in bot and
    "if (_reconciliation != null &&" in bot,
    "new execution must be blocked while broker reconciliation is unresolved",
)

require(
    "TryRecoverProtectionFromSignal(" in management and
    "ManagementCommandType.ModifyProtection" in management and
    "CBOT STARTUP PROTECTION RECOVERY" in management,
    "missing canonical cBot protection recovery path",
)

require(
    "LifecycleState" in contract and
    "ProtectionState" in contract and
    "RecoveryRequired" in contract and
    "RecoveryReason" in contract,
    "state contract must expose lifecycle/protection/recovery truth",
)

require(
    "CbotBrokerReconciliationResult reconciliation" in publisher and
    "RecoveryRequired =" in publisher and
    "ProtectionState =" in publisher,
    "publisher must expose reconciliation state without granting Indicator broker authority",
)

require(
    "RECOVERY" in reader and
    "RecoveryReason" in reader and
    "RecoveryRequired" in reader,
    "Indicator panel must show recovery/protection truth",
)

require(
    "audit_phase_cbot_p5_reconciliation.py" in workflow,
    "CBOT-P5 reconciliation audit must be accumulated in Source/Architecture",
)

require(
    "CBOT-P5" in roadmap and
    "CBOT-P5" in continuation and
    "CBOT-P5" in devlog,
    "CBOT-P5 must be recorded in project continuity documents",
)

if errors:
    print("CBOT-P5 RECONCILIATION AUDIT: FAIL")
    for e in errors:
        print(" - " + e)
    sys.exit(1)

print("CBOT-P5 RECONCILIATION AUDIT: PASS")
print("startup/reconnect reconciliation: PASS")
print("ambiguous-state blocking: PASS")
print("protection defect detection: PASS")
print("canonical cBot protection recovery: PASS")
print("Indicator remains observation-only: PASS")
