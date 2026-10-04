#!/usr/bin/env python3
"""Static acceptance gate for CR5.6 / E6 directional-bias and timeframe semantics."""

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


premium = read("src/CFIP.Indicator/Analysis/Market/PremiumDiscountAnalyzer.cs")
premium_rule = read("src/CFIP.Indicator/Core/Math/PremiumDiscountBiasRule.cs")
live = read("src/CFIP.Indicator/Analysis/Market/LiveBiasAnalyzer.cs")
live_rule = read("src/CFIP.Indicator/Core/Math/LiveM5BiasRule.cs")
healthy = read("src/CFIP.Indicator/Analysis/Market/HealthyVolatilityAnalyzer.cs")
healthy_rule = read("src/CFIP.Indicator/Core/Math/HealthyVolatilityRule.cs")
evidence = read("src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs")
orchestration = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
closed_calc = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
score = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreCalculator.cs")
parameters_decision = read("src/CFIP.Indicator/Indicator/Parameters/01_decision.cs")
parameters_entry = read("src/CFIP.Indicator/Indicator/Parameters/07_entry_precision.cs")
parameters_confluence = read(
    "src/CFIP.Indicator/Indicator/Parameters/20_confluence_extensions.cs"
)
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
phase_doc = read("docs/PHASE-CR5-6-DIRECTIONAL-BIAS-TIMEFRAME.md")
current_phase_doc = read(
    "docs/PHASE-CR5-8-TARGET-SELECTION-CONSISTENCY.md"
)
continuation = read("docs/CONTINUATION-STATE.md")


check(
    "premium/discount geometry has one Core owner and preserves mean-reversion semantics",
    "PremiumDiscountBiasRule.Evaluate(" in premium and
    "class PremiumDiscountBiasRule" in premium_rule and
    "if (close < midpoint)" in premium_rule and
    "return 1;" in premium_rule and
    "if (close > midpoint)" in premium_rule and
    "return -1;" in premium_rule,
)

check(
    "premium/discount remains an explicit context contribution without regime retuning",
    "if (input.UsePremiumDiscount)" in score and
    "premiumBuy = 6" in score and
    "premiumSell = 6" in score and
    "class PremiumDiscountBiasRule" in premium_rule,
)

check(
    "live bias is authoritative on closed M5 rather than chart timeframe",
    "_m5Bars" in live and
    "closedM5" in live and
    "ClosedBarReferenceRule.IsFullyClosed(" in live and
    "LiveM5BiasRule.Evaluate(" in live and
    re.search(r"(?<![A-Za-z0-9_])Bars\.", live) is None and
    re.search(r"(?<![A-Za-z0-9_])Bars\b", live) is None and
    "MapM5ToClosedChart" not in live,
)

check(
    "decision orchestration passes the canonical closed M5 and reference into live bias",
    "LiveBias(" in orchestration and
    "closedM5" in orchestration and
    "reference" in orchestration and
    "closedChartIndex" not in orchestration and
    "MapM5ToClosedChart" not in orchestration,
)

check(
    "closed-bar calculation builds the M5 frame from the canonical closed-M5 context",
    "_m5Frame =" in closed_calc and
    "_m5Bars" in closed_calc and
    "closedM5" in closed_calc,
)

check(
    "healthy volatility has a single ATR-ratio owner and no trigger-body coupling",
    "HealthyVolatilityRule.IsHealthy(" in healthy and
    "class HealthyVolatilityRule" in healthy_rule and
    "MinimumTriggerBodyAtr" not in healthy and
    "body" not in healthy,
)

check(
    "directional volatility evidence is derived after directionless volatility health",
    "bool healthyVolatility" in evidence and
    "HasHealthyVolatility(" in evidence and
    "healthyVolatility &&" in evidence and
    "ClosePrices[index]" in evidence and
    "OpenPrices[index]" in evidence,
)

check(
    "healthy-volatility parameters remain unchanged",
    "DefaultValue = 0.85" in parameters_confluence and
    "DefaultValue = 1.80" in parameters_confluence and
    "DefaultValue = 0.12" in parameters_entry,
)

check(
    "advanced confluence participation remains unchanged",
    '"Use Advanced Confluence"' in parameters_decision and
    "DefaultValue = true" in parameters_decision,
)

check(
    "E6 deterministic contracts are wired and cover M5/M15/H1 closed-bar boundaries",
    "VerifyDirectionalBiasTimeframeSemantics();" in contracts and
    "PremiumDiscountBiasRule.Evaluate(" in contracts and
    "LiveM5BiasRule.Evaluate(" in contracts and
    "HealthyVolatilityRule.IsHealthy(" in contracts and
    "M5/M15/H1 closed-bar fixtures" in contracts,
)

check(
    "E6 Core owners are included in the runtime contract project",
    "PremiumDiscountBiasRule.cs" in project and
    "LiveM5BiasRule.cs" in project and
    "HealthyVolatilityRule.cs" in project,
)

check(
    "E6 static gate is wired immediately after E5",
    "audit_phase_5_5.py" in workflow and
    "audit_phase_5_6.py" in workflow and
    workflow.index("audit_phase_5_6.py") >
    workflow.index("audit_phase_5_5.py"),
)

check(
    "E6 continuity documentation remains represented after later remediation phases",
    "CR5.6 / E6" in phase_doc and
    "CR5.7 / E7" in roadmap and
    (
        "CR5.7 / E7" in continuation or
        "CR5.8 / E8" in continuation
    ),
)

print("CR5.6 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.6 STATIC GATE PASS")
