#!/usr/bin/env python3
"""Static acceptance gate for CR8.1 / H1 directional execution-fill semantics."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


rule = read("src/CFIP.Indicator/Core/Math/ExecutionFillAcceptanceRule.cs")
intent = read("src/CFIP.Indicator/Planning/Execution/ExecutionIntentValidation.cs")
market = read("src/CFIP.Indicator/Planning/Execution/MarketEntryValidation.cs")
aggressive = read(
    "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs"
)
automatic = read(
    "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketFillReconciliation.cs"
)
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

check(
    "Core owner exposes the single six-argument directional fill contract",
    "IsAcceptable(" in rule and
    "int direction" in rule and
    "double requestedEntry" in rule and
    "double actualFill" in rule and
    "double atr" in rule and
    "double maxAdverseExtensionAtr" in rule and
    "bool allowFavorable" in rule,
)

check(
    "favorable BUY/SELL movement is direction-aware",
    "direction == 1" in rule and
    "actualFill < requestedEntry" in rule and
    "actualFill > requestedEntry" in rule,
)

check(
    "only adverse movement is bounded by the ATR execution envelope",
    "allowedAdverseDistance" in rule and
    "adverseDistance" in rule and
    "adverseDistance <= allowedAdverseDistance" in rule,
)

check(
    "intent caller uses the canonical rule with favorable fills enabled",
    "ExecutionFillAcceptanceRule.IsAcceptable(" in intent and
    "intent.Direction" in intent and
    "MaximumEntryExtensionAtr" in intent and
    "true))" in intent,
)

check(
    "automatic-market fill caller uses the same canonical owner",
    "ExecutionFillAcceptanceRule.IsAcceptable(" in market and
    "plan.Direction" in market and
    "plan.Entry" in market and
    "MaximumEntryExtensionAtr" in market,
)

check(
    "automatic-market fill envelope uses the setup ATR rather than quote spread as its envelope",
    "Atr(" in market and
    "plan.CreatedM5" in market and
    "(Symbol.Ask - Symbol.Bid)" not in market,
)

check(
    "aggressive path has no second symmetric absolute-distance envelope",
    "maximumFillDistance" not in aggressive and
    "Math.Abs(" not in aggressive and
    "MaximumEntryExtensionAtr" not in aggressive,
)

check(
    "automatic fill is validated before actual-fill reconciliation mutates plan Entry",
    automatic.find("IsExecutableFillPrice(") >= 0 and
    automatic.find("IsExecutableFillPrice(") < automatic.find("ReconcileLivePlanToActualFill("),
)

check(
    "automatic market does not publish LivePosition before the fill envelope passes",
    automatic.find("IsExecutableFillPrice(") >= 0 and
    automatic.find("IsExecutableFillPrice(") < automatic.find("LifecycleState.LivePosition"),
)

check(
    "runtime contract directly exercises the directional owner",
    "VerifyDirectionalExecutionFillAcceptance();" in runtime and
    "ExecutionFillAcceptanceRule.IsAcceptable(" in runtime and
    "BUY favorable fill is accepted" in runtime and
    "SELL favorable fill is accepted" in runtime,
)

check(
    "runtime contract rejects adverse BUY/SELL beyond the envelope and covers invalid inputs",
    "BUY adverse fill beyond envelope is rejected" in runtime and
    "SELL adverse fill beyond envelope is rejected" in runtime and
    "invalid direction fails closed" in runtime and
    "invalid ATR fails closed" in runtime,
)

check(
    "runtime contract project compiles the canonical owner",
    "ExecutionFillAcceptanceRule.cs" in contracts,
)

check(
    "H1 audit is accumulated in Source/Architecture CI",
    "audit_phase_8_1.py" in workflow,
)

print("CR8.1 / H1 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR8.1 / H1 STATIC GATE PASS")
