#!/usr/bin/env python3
"""T8 risk/margin hardening audit.

Guards the canonical margin-volume safety owner against infinite reduction,
non-progressing volume steps, and unnormalized disabled-guard output.
"""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append("missing file: " + relative)
        return ""
    return path.read_text(encoding="utf-8")

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

risk = read("src/CFIP.Indicator/Trading/Risk/MarginSafetyCalculator.cs")
policy = read("src/CFIP.Indicator/Trading/Risk/RiskPercentPolicy.cs")
parameters = read("src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs")
planning_contracts = read("tools/CFIP.Planning.Contracts/Program.cs")

check(
    "margin sizing has one owner and normalizes before the guard bypass",
    "private double AdjustVolumeForMargin(" in risk and
    "double normalizedVolume =" in risk and
    "if (!UseAutoMarginGuard)" in risk and
    "return normalizedVolume;" in risk,
)

check(
    "margin reduction fails closed when broker volume step is invalid",
    "double volumeStep =" in risk and
    "if (!IsFinitePositive(volumeStep))" in risk and
    "return 0;" in risk,
)

check(
    "margin reduction cannot spin forever",
    "maxReductionIterations" in risk and
    "reductionIterations++" in risk and
    "nextReduced >= reduced" in risk and
    "return 0;" in risk,
)

check(
    "final margin volume remains normalized",
    "Symbol.NormalizeVolumeInUnits(" in risk and
    "reduced," in risk and
    "RoundingMode.Down" in risk,
)

check(
    "public risk-percent bounds remain explicit and aligned",
    "RiskPercentEquity" in parameters and
    "MinValue = 0.05" in parameters and
    "MaxValue = 5" in parameters and
    "NumericGuards.Clamp(" in policy,
)

check(
    "planning contract retains canonical risk-percent behavior",
    "RiskPercentPolicy.Calculate(2, false, 0.5) == 2" in planning_contracts and
    "RiskPercentPolicy.Calculate(10, true, 0.5) == 2.5" in planning_contracts,
)

print("T8 RISK/MARGIN HARDENING SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("T8 STATIC GATE PASS")
