#!/usr/bin/env python3
"""Static acceptance gate for CR5.2 / E2 liquidity and session target-source semantics."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


above = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityAboveTargetSource.cs"
)
below = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityBelowTargetSource.cs"
)
smart = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/SmartExtraTargetSource.cs"
)
session = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/SessionTargetSource.cs"
)
supply = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/SupplyDemandLiquidityTargetSource.cs"
)
builder = read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
phase_doc = read("docs/PHASE-CR5-2-LIQUIDITY-TARGET-SOURCES.md")

for name, source, swing_token, list_method in [
    ("above-source", above, "IsCanonicalSwingHigh(", "FindLiquidityLevelsAbove("),
    ("below-source", below, "IsCanonicalSwingLow(", "FindLiquidityLevelsBelow("),
]:
    check(
        f"{name} extracts canonical structural swings",
        swing_token in source and list_method in source,
    )
    check(
        f"{name} rejects broken levels through canonical active-unbroken rule",
        "LiquiditySweepRule.IsActiveUnbrokenLevel(" in source,
    )
    check(
        f"{name} uses canonical directional and spacing rules",
        "LiquidityTargetCandidateRule.IsDirectionallyValid(" in source and
        "LiquidityTargetCandidateRule.OrderDistinctByDistance(" in source,
    )
    check(
        f"{name} orders candidates by distance",
        "LiquidityTargetCandidateRule.OrderByDistance(" in source,
    )

check(
    "above/below legacy nearest helpers now delegate to canonical lists",
    "FindLiquidityLevelsAbove(" in above and
    "FindLiquidityLevelsBelow(" in below and
    "levels[0]" in above and
    "levels[0]" in below,
)

check(
    "SmartExtra emits every valid liquidity forecast candidate",
    "List<double> forecasts" in smart and
    "FindLiquidityLevelsAbove(" in smart and
    "FindLiquidityLevelsBelow(" in smart and
    '"LIQUIDITY_FORECAST"' in smart and
    "for (int i = 0;" in smart,
)

check(
    "session forecast uses the canonical existing session window owner",
    "SessionWindowRule.TryResolveSessionWindow(" in session and
    "GetSessionRange(" in smart and
    "GetSessionRange(" in supply,
)

check(
    "session semantics remain parameter-driven rather than named-session replacement",
    "SessionStartUtc" in smart and
    "SessionEndUtc" in smart and
    "SessionStartUtc" in supply and
    "SessionEndUtc" in supply and
    "Asia" not in session and
    "London" not in session and
    "New York" not in session,
)

check(
    "target builder still feeds liquidity candidates into the normal target pipeline",
    "AddSupplyDemandAndLiquidityLevels(" in builder and
    "AddSmartExtraTargetLevels(" in builder,
)

check(
    "deterministic E2 contracts are wired",
    "VerifyLiquidityTargetCandidateSemantics();" in contracts and
    "liquidity candidates preserve BUY/SELL direction" in contracts and
    "high liquidity remains active while no later close breaks above it" in contracts and
    "low liquidity becomes invalid after a close breaks below it" in contracts and
    "valid liquidity target survives canonical reward-risk constraints" in contracts and
    "distant liquidity target remains bounded by the existing extension contract" in contracts,
)

check(
    "runtime contract project includes E2 owners",
    "LiquidityTargetCandidateRule.cs" in project and
    "TargetCandidateConstraintRule.cs" in project,
)

check(
    "E2 static gate is wired after CR5.1",
    "audit_phase_5_1.py" in workflow and
    "audit_phase_5_2.py" in workflow and
    workflow.index("audit_phase_5_2.py") >
    workflow.index("audit_phase_5_1.py"),
)

check(
    "phase documentation records the E2 safety boundary",
    "CR5.2" in phase_doc and
    "MinimumTpSpacingAtr" in phase_doc and
    "SessionWindowRule" in phase_doc and
    "no public" in phase_doc.lower(),
)

print("CR5.2 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.2 STATIC GATE PASS")
