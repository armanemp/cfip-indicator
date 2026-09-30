#!/usr/bin/env python3
"""Static acceptance gate for CR2.2 reaction/reversal semantics."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

rule = read("src/CFIP.Indicator/Core/Math/ReactionQualificationRule.cs")
reaction = read("src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs")
pending = read("src/CFIP.Indicator/Trading/Pending/Policy/PendingOrderPolicy.cs")
aggressive = read("src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs")
decision = read("src/CFIP.Indicator/Core/Models/Decision.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

required = {
    "canonical reaction rule exists": "internal static class ReactionQualificationRule" in rule,
    "direction ties are neutral": "return 0;" in rule and "ResolveDirection(" in rule,
    "prior counter-move semantics": "HasPriorCounterMove(" in rule,
    "swing interaction semantics": "HasSwingInteraction(" in rule,
    "explicit no-zone behavior": "HasQualifyingContext(" in rule and "if (zonePresent)" in rule,
    "closed-bar confirmation owner": "IsClosedBarConfirmed(" in rule and "requireClosedBar" in rule,
    "shared reaction qualification": "ReactionQualificationRule.IsQualified(" in reaction,
    "pending uses canonical reaction rule": "ReactionQualificationRule.IsQualified(" in pending,
    "aggressive uses canonical reaction rule": "ReactionQualificationRule.IsQualified(" in aggressive,
    "reaction exposes closed confirmation": "ReactionConfirmedHasContext" in decision and "ReactionClosedBarConfirmed" in decision,
    "runtime contract invoked": "VerifyReactionQualificationSemantics();" in contracts,
    "runtime contract project links rule": "ReactionQualificationRule.cs" in contracts_project,
}

for name, ok in required.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# The old pending/aggressive split must not return: pending should not qualify
# reversal from the moving reaction confidence alone.
for forbidden in (
    "_reaction.EntryAllowed &&",
):
    if forbidden in pending:
        errors.append(
            f"legacy independent pending reversal qualification remains: {forbidden}"
        )
        print(f"FAIL | {errors[-1]}")

# A single open-bar reaction is observation state, not closed-bar confirmation.
if "requireClosedBar" not in rule or "closedBarConfirmed" not in rule:
    errors.append("reaction rule does not expose an explicit temporal confirmation boundary")

# The analyzer must retain the two temporal states separately.
if (
    "ReactionIntrabarQuality" not in reaction or
    "ReactionConfirmedQuality" not in reaction or
    "ReactionClosedBarConfirmed" not in reaction
):
    errors.append("reaction analyzer does not retain separate intrabar/confirmed state")

print("CR2.2 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.2 STATIC GATE PASS")
