#!/usr/bin/env python3
"""Static acceptance gate for CR4.1 learning-memory identity and account scoping."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing required CR4.1 file: {relative}")
    return path.read_text(encoding="utf-8")

memory = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryStore.cs")
identity = read("src/CFIP.Indicator/Core/Math/OutcomeMemoryIdentityRule.cs")
archive = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs")
runtime = read("src/CFIP.Indicator/Trading/Intelligence/RuntimeLogPersistence.cs")
portable = read("src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs")
account_switch = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeMemoryAccountSwitch.cs")
initialization = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")

errors = []

def check(name: str, ok: bool) -> None:
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

check("decision-only fingerprint is owned by Core rule",
      "BuildFingerprint(" in identity and "decisionOrResultOnly" in identity and
      "BuildFingerprint(" in memory)
check("presentation groups are excluded",
      'AlertsCoreGroupPrefix = "12 · ALERTS"' in identity and
      'DisplayGroupPrefix = "14 · DISPLAY"' in identity)
check("non-decision parameters are explicitly excluded",
      '"EnableAutoTrading"' in identity and '"AutoTradingReminder"' in identity and
      '"PanelWidth"' not in identity and '"EnableSoundAlerts"' not in identity)
check("decision parameters remain included",
      '"Tp1MinimumRR"' not in identity and '"MinimumConfidence"' not in identity)
check("current memory schema is versioned",
      'OutcomeMemorySchema = "CFIP-OUTCOME,2"' in memory and
      'PriorOutcomeMemorySchema = "CFIP-OUTCOME"' in memory)
check("new LocalStorage key is account scoped",
      "MemoryAccountScopeToken()" in memory and "BuildMemoryKey(" in memory and
      "Account.Number" in memory and "Account.AccountType" in memory and
      "Account.IsLive" in memory)
check("legacy migration uses legacy key and schema",
      "PriorOutcomeMemoryKey()" in memory and
      "PriorMemoryConfigurationFingerprint()" in memory and
      "IsLegacyOutcomeOwnedByCurrentAccount(" in memory)
check("legacy migration rejects unverified account ownership",
      "History.FindByPositionId(" in memory and "historicalTrades.Length > 0" in memory)
check("archive prefix is account scoped",
      "MemoryAccountScopeToken()" in archive and
      "_outcomeArchivePrefixIdentityCache" in archive)
check("runtime log prefix is account scoped",
      "MemoryAccountScopeToken()" in runtime and
      "_runtimeLogPrefixIdentityCache" in runtime)
check("portable snapshot is versioned and account scoped",
      '"CFIP-PORTABLE-MEMORY,2"' in portable and
      "MemoryAccountScopeToken()" in portable and
      "AccountNumber=" in portable and "AccountType=" in portable and
      "AccountIsLive=" in portable)
check("account switch clears and reloads memory",
      "Account.Switched += OnOutcomeMemoryAccountSwitched" in initialization and
      "Account.Switched -= OnOutcomeMemoryAccountSwitched" in initialization and
      "RestoreOutcomeHistory()" in account_switch and
      "_outcomeHistory.Clear();" in account_switch)
check("behavioral contract is present",
      "VerifyOutcomeMemoryIdentitySemantics();" in contracts and
      "presentation-only parameter changes do not change learning fingerprint" in contracts and
      "decision-affecting Tp1MinimumRR changes learning fingerprint" in contracts and
      "account number, account type and live/demo state are isolated" in contracts)
check("CR4.1 audit is wired into CI",
      "python tools/audit_phase_4_1.py" in workflow)

print("CR4.1 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)
print("CR4.1 STATIC GATE PASS")
