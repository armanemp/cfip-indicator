#!/usr/bin/env python3
"""cBot management-policy ownership / mutation-throttling regression gate."""
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

settings = read("src/CFIP.cBot/Execution/CbotIndicatorExecutionSettings.cs")
policy = read("src/CFIP.cBot/Execution/CbotManagementPolicyRule.cs")
manager = read("src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
tests = read("tools/CFIP.cBot.Shadow.Tests/Program.cs")
workflow = read(".github/workflows/source-check.yml")
boundary = read("docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md")

for token in (
    "EnableLiveExitManagement",
    "EnablePartialTakeProfit",
    "AutoBrokerProtection",
    "AutoProtectBrokerPositions",
    "SyncBrokerTakeProfit",
    "ManagedActionsOnly",
    "BrokerModifyCooldownMs",
):
    require(token in settings, "cBot execution settings missing: " + token)

require(
    "CbotManagementPolicyRule.Allows(" in manager and
    "static bool Allows(" in policy,
    "management commands do not pass through the canonical cBot policy owner",
)
require(
    "ManagementCommandType.PartialClose" in policy and
    "PARTIAL TAKE PROFIT DISABLED" in policy,
    "partial close is not cBot-policy gated",
)
require(
    "ManagementCommandType.ModifyProtection" in policy and
    "BROKER PROTECTION DISABLED" in policy,
    "protection mutation is not cBot-policy gated",
)
require(
    "ManagementCommandType.AdvanceTarget" in policy and
    "BROKER TAKE PROFIT SYNC DISABLED" in policy,
    "TP advance is not cBot-policy gated",
)
require(
    "_lastProtectionMutationUtcByPosition" in manager and
    "TryAllowProtectionMutation(" in manager and
    "BROKER MODIFY COOLDOWN" in manager,
    "broker modification cooldown is not position-scoped",
)

cooldown_call = manager.find("if (!TryAllowProtectionMutation(")
if cooldown_call >= 0:
    execute_call = manager.find("ExecuteOne(", cooldown_call)
    cooldown_block = manager[cooldown_call:execute_call]
    require(
        "StoreReport(" not in cooldown_block and
        "BrokerReportStatus.Accepted" not in cooldown_block,
        "cooldown deferment must not persist an Accepted broker fact that freezes the command",
    )
else:
    require(False, "cooldown gate call is missing")
require(
    "_management.Process(" in bot and
    "_executionSettings," in bot,
    "cBot does not supply current execution policy to management owner",
)
require(
    "using CFIP.cBot.Execution;" in tests and
    "ManagementPolicy();" in tests and
    "CbotManagementPolicyRule.Allows(" in tests and
    "ManagementCommandType.AdvanceTarget" in tests,
    "management policy behavioral coverage is missing",
)
require(
    "ManagementExecutionCoordinator.cs" in boundary and
    "cBot — P4E" in boundary and
    "Definition of extraction completeness" in boundary,
    "CBOT execution boundary documentation is missing the mutation ownership contract",
)
require(
    "audit_phase_cbot_management_policy_hardening_2026_10_03.py" in workflow,
    "dedicated management-policy audit is not wired into Source/Architecture CI",
)

if errors:
    print("CBOT MANAGEMENT POLICY HARDENING AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT MANAGEMENT POLICY HARDENING AUDIT: PASS")
