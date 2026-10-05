#!/usr/bin/env python3
"""Static acceptance gate for CI-00 canonical data/price/time semantics."""

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


canonical = read(
    "src/CFIP.Indicator/Core/Models/CanonicalPriceSnapshot.cs"
)
context = read(
    "src/CFIP.Indicator/Runtime/Calculation/CalculationMarketContext.cs"
)
builder = read(
    "src/CFIP.Indicator/Runtime/Calculation/CanonicalMarketContextBuilder.cs"
)
prep = read(
    "src/CFIP.Indicator/Runtime/Calculation/CalculationPreparation.cs"
)
state = read("src/CFIP.Indicator/Indicator/State.cs")
price_math = read("src/CFIP.Indicator/Trading/Execution/PriceMath.cs")
zone = read(
    "src/CFIP.Indicator/Planning/Execution/ExecutionZoneBuilder.cs"
)
market_range = ""
aggressive = ""
market_validation = read(
    "src/CFIP.Indicator/Planning/Execution/MarketEntryValidation.cs"
)
plan_constraints = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanMarketConstraintValidator.cs"
)
automatic_pretrade = ""
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")


check(
    "canonical price snapshot is the single price/quote owner",
    canonical.count("internal sealed class CanonicalPriceSnapshot") == 1
    and "ExecutableBuyPrice" in canonical
    and "ExecutableSellPrice" in canonical
    and "Midpoint" in canonical
    and "SpreadPips" in canonical
    and "PipSize" in canonical
    and "TickSize" in canonical,
)

check(
    "broker minimum-distance metadata is preserved with explicit unit semantics",
    "BrokerDistanceUnit" in canonical
    and "MinimumStopDistanceRaw" in canonical
    and "MinimumTakeProfitDistanceRaw" in canonical
    and "GetMinimumStopDistancePrice" in canonical
    and "GetMinimumTakeProfitDistancePrice" in canonical
    and "SymbolMinDistanceType.Pips" in builder
    and "SymbolMinDistanceType.Percentage" in builder,
)

check(
    "calculation market context binds quote, time and closed MTF context",
    "CalculationMarketContext" in context
    and "SignalReferenceUtc" in context
    and "QuoteObservedUtc" in context
    and "ClosedBars" in context
    and "AtrM5Index" in context
    and "AtrM1Index" in context,
)

check(
    "market context is refreshed independently of readiness probing",
    "_calculationMarketContext =" in prep
    and "BuildCalculationMarketContext(" in prep
    and "_lastMtfClosedContext != null" in prep,
)

check(
    "runtime state has exactly one canonical market-context slot",
    state.count("private CalculationMarketContext _calculationMarketContext;") == 1,
)

checks = [
    ("PriceMath", price_math),
    ("ExecutionZoneBuilder", zone),
    ("MarketEntryValidation", market_validation),
    ("PlanMarketConstraintValidator", plan_constraints),
]

for name, source in checks:
    check(
        f"{name} consumes the canonical price snapshot",
        "GetCanonicalPriceSnapshot(" in source
        or "GetCanonicalPriceSnapshot()" in source,
    )

for name, source in checks:
    check(
        f"{name} has no direct quote-owner access",
        "Symbol.Ask" not in source and "Symbol.Bid" not in source,
    )

check(
    "price normalization uses canonical tick/digits",
    "CanonicalPriceSnapshot market" in price_math
    and "market.TickSize" in price_math
    and "market.Digits" in price_math,
)

check(
    "runtime contracts compile the canonical pure-domain context",
    "CanonicalPriceSnapshot.cs" in runtime_project
    and "CalculationMarketContext.cs" in runtime_project
    and "VerifyCanonicalMarketContext();" in runtime,
)

check(
    "runtime contracts cover directional executable prices and broker distances",
    "canonical quote preserves BUY/SELL executable prices" in runtime
    and "pip broker distances convert to price distance consistently" in runtime
    and "percentage broker distances use the executable reference price" in runtime
    and "invalid crossed quote cannot become an executable price" in runtime,
)

check(
    "CI-00 audit is accumulated in Source/Architecture CI",
    "audit_phase_ci_00.py" in workflow,
)

print("CI-00 CANONICAL MARKET CONTEXT SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CI-00 STATIC GATE PASS")
