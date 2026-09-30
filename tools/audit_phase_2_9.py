#!/usr/bin/env python3
"""Static acceptance gate for CR2.9 structural stop, divergence and rejection semantics."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

stop = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
stop_rule = read("src/CFIP.Indicator/Core/Math/StructuralStopScoringRule.cs")
resolver = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
div = read("src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs")
div_rule = read("src/CFIP.Indicator/Core/Math/DivergenceThresholdRule.cs")
rej = read("src/CFIP.Indicator/Core/Math/RejectionRule.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

checks = {
    "unknown structural timeframe still fails closed": (
        'case "W1":' in resolver and
        "return null;" in resolver and
        "StructuralTimeframeRule.IsSupported(" in resolver
    ),
    "structural stop reward-path bonus is centralized": (
        "StructuralStopScoringRule.CalculateRewardPathBonus(" in stop and
        "RewardPathBonusCap" in stop_rule
    ),
    "structural stop risk balance is centralized": (
        "StructuralStopScoringRule.CalculateRiskBalance(" in stop and
        "PreferredRiskFloorAtr" in stop_rule
    ),
    "reward-path bonus is bounded": (
        "Math.Min(" in stop_rule and
        "RewardPathBonusCap" in stop_rule
    ),
    "divergence thresholds have one semantic owner": (
        "DivergenceThresholdRule.MinimumQuality" in div and
        "DivergenceThresholdRule.ConflictQualityMargin" in div and
        "DivergenceThresholdRule.RegularRsiDelta" in div and
        "DivergenceThresholdRule.HiddenRsiDelta" in div
    ),
    "divergence price thresholds are canonical": (
        "DivergenceThresholdRule.RegularPriceAtr" in div and
        "DivergenceThresholdRule.HiddenPriceAtr" in div
    ),
    "divergence scoring constants are centralized": (
        "DivergenceThresholdRule.CalculateQuality(" in div and
        "QualityBase" in div_rule and
        "PriceExcursionContributionCap" in div_rule
    ),
    "divergence conflict remains non-directional": (
        "DivergenceResult.CreateConflict(" in div and
        'Type == "CONFLICT"' in contracts
    ),
    "rejection/doji thresholds are canonical": (
        "MinimumBodyPips" in rej and
        "MinimumBodyRangeFraction" in rej and
        "WickToBodyRatio" in rej and
        "MinimumWickRangeFraction" in rej
    ),
    "doji uses the same meaningful-body rule as rejection": (
        "!HasMeaningfulBody(" in rej and
        "ResolveMinimumMeaningfulBody(" in rej
    ),
    "runtime contracts cover CR2.9": (
        "VerifyStructuralStopScoringSemantics();" in contracts and
        "VerifyDivergenceThresholdSemantics();" in contracts and
        "VerifyRejectionThresholdSemantics();" in contracts
    ),
    "runtime project links CR2.9 rules": (
        "DivergenceThresholdRule.cs" in project and
        "StructuralStopScoringRule.cs" in project
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# B10 is deliberately evidence-preserving rather than blindly retuned:
# the selector must use the bounded rule components, while empirical bias is
# decided from fixtures/replay rather than from hard-coded tuning.
for forbidden in (
    "bestTp1RR - rewardRisk.RequiredRR) * 20",
    "bestTp1RR - rewardRisk.RequiredRR) * 30",
):
    if forbidden in stop:
        errors.append("unapproved structural-stop reward-path tuning detected")
        print("FAIL | unapproved structural-stop reward-path tuning detected")
        break

if "2.0" in div and "DivergenceThresholdRule.RegularRsiDelta" not in div:
    errors.append("raw divergence RSI threshold remains")
if "1.50" in div and "DivergenceThresholdRule.RegularWaveDelta" not in div:
    errors.append("raw divergence WaveTrend threshold remains")

print("CR2.9 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.9 STATIC GATE PASS")
