#!/usr/bin/env python3
"""CI-20B protection, cBot management freshness and signal-hardening audit."""

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


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


protection = read(
    "src/CFIP.Indicator/Trading/LiveManagement/ProtectionManager.cs"
)
protection_rule = read(
    "src/CFIP.Indicator/Core/Math/IntelligentProtectionRule.cs"
)
smart_be = read(
    "src/CFIP.Indicator/Core/Math/SmartBreakEvenRule.cs"
)
progression = read(
    "src/CFIP.Indicator/Core/Math/ProtectionProgressionRule.cs"
)
management_ind = read(
    "src/CFIP.Indicator/Trading/Execution/ManagementCommandRequestCoordinator.cs"
)
management_cbot = read(
    "src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs"
)
cbot_host = read("src/CFIP.cBot/CFIPExecutionBot.cs")
contracts = read("src/CFIP.Contracts/ContractEnums.cs")
pullback_rule = read(
    "src/CFIP.Indicator/Core/Math/PrimaryPullbackTuningRule.cs"
)
confirmation = read(
    "src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfirmationGates.cs"
)
smart_gates = read(
    "src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartGates.cs"
)
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
runtime_program = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")

check(
    "ProtectionManager has one canonical intelligent protection call",
    protection.count("IntelligentProtectionRule.Evaluate(") == 1,
)

check(
    "ProtectionManager no longer contains duplicate BE/structural mutation arithmetic",
    protection.count("BreakEvenBufferPips") == 1 and
    "Math.Max(candidate, be)" not in protection and
    "Math.Min(candidate, be)" not in protection and
    "pressureTighten" not in protection,
)

check(
    "intelligent protection composes canonical BE semantics",
    "SmartBreakEvenRule.Evaluate(" in protection_rule and
    "_plan.Tp1" in protection and
    "serverBreakEvenActive" in protection_rule,
)

check(
    "intelligent protection never widens SL and uses canonical progression",
    "ProtectionProgressionRule.ShouldAdvanceStop(" in protection_rule and
    "STOP TOO CLOSE TO MARKET" in protection_rule,
)

check(
    "intelligent protection fails closed on invalid numeric state",
    "!IsFiniteProtectionNonNegative(peakRR)" in protection_rule and
    "PROTECTION INPUT INVALID" in protection_rule,
)

check(
    "cBot exposes a bounded management-command freshness policy",
    "Management Command Max Age Seconds" in cbot_host and
    "DefaultValue = 15" in cbot_host and
    "ManagementCommandMaxAgeSeconds" in cbot_host,
)

check(
    "cBot management coordinator expires stale commands",
    "maximumCommandAgeSeconds" in management_cbot and
    "MANAGEMENT COMMAND EXPIRED" in management_cbot and
    "BrokerReportStatus.Expired" in management_cbot,
)

check(
    "Indicator retires terminally expired management commands",
    "BrokerReportStatus.Expired" in management_ind,
)

check(
    "Expired is a dedicated broker-report terminal status",
    "Expired = 5" in contracts,
)

check(
    "qualified neutral-M5 primary pullback rule is explicit",
    "AllowsNeutralM5(" in pullback_rule and
    "m5Direction != 0" in pullback_rule and
    "m15Direction != selectedDirection" in pullback_rule and
    "h1Direction != selectedDirection" in pullback_rule,
)

check(
    "M5 confirmation uses the primary pullback rule while preserving opposite-M5 blocking",
    "PrimaryPullbackTuningRule.AllowsNeutralM5(" in confirmation and
    "if (!allowNeutralPrimaryPullback)" in confirmation and
    "_m5Frame.Direction != decision.Direction" in confirmation,
)

check(
    "M5 neutral pullback requires top-down eligibility and quality floor",
    "topDownEligible" in pullback_rule and
    "m15Quality >= floor" in pullback_rule and
    "h1Quality >= floor" in pullback_rule,
)

check(
    "M5 regime is reused inside smart gates instead of repeated same-cycle lookup",
    "MarketRegimeSnapshot activeRegime" in smart_gates and
    smart_gates.count("GetActiveM5Regime(closedM5)") == 1,
)

check(
    "runtime contracts compile the new pure rules",
    "IntelligentProtectionRule.cs" in runtime_project and
    "PrimaryPullbackTuningRule.cs" in runtime_project,
)

check(
    "runtime contracts execute the CI20B regression suite",
    "Ci20BProtectionAndSignalContracts.Run();" in runtime_program,
)

check(
    "all required CI20B verification is accumulated in Source/Architecture",
    "audit_phase_ci20b_protection_cbot_signal.py" in workflow,
)

if errors:
    print("CI-20B PROTECTION / CBOT / SIGNAL HARDENING AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI-20B PROTECTION / CBOT / SIGNAL HARDENING AUDIT: PASS")
print("Canonical SL protection owner: PASS")
print("Management-command freshness: PASS")
print("Neutral-M5 primary pullback hardening: PASS")
print("M5 regime lookup reuse: PASS")
print("Regression contract wiring: PASS")
