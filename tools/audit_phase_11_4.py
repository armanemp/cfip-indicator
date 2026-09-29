#!/usr/bin/env python3
"""Static/source contract audit for Phase 11.4."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
RULE = ROOT / "src/CFIP.Indicator/Core/Math/PlanRewardRiskQualityRule.cs"
POLICY = ROOT / "src/CFIP.Indicator/Planning/TradePlan/TargetSelectionPolicy.cs"
STOP = ROOT / "src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs"
PLAN = ROOT / "src/CFIP.Indicator/Planning/TradePlan/PlanIntegrityValidator.cs"
ACTION = ROOT / "src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs"
PARALLEL = ROOT / "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs"
SCENARIOS = ROOT / "src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs"
MARKET = ROOT / "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs"
AGG = ROOT / "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs"
PENDING = ROOT / "src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs"
ROUTINE = ROOT / "ROUTINE.md"

errors = []

def read(path):
    if not path.exists():
        errors.append(f"missing file: {path}")
        return ""
    return path.read_text(encoding="utf-8")

rule = read(RULE)
policy = read(POLICY)
stop = read(STOP)
plan = read(PLAN)
action = read(ACTION)
parallel = read(PARALLEL)
scenarios = read(SCENARIOS)
market = read(MARKET)
agg = read(AGG)
pending = read(PENDING)
routine = read(ROUTINE)

for token in (
    "PlanRewardRiskQualityResult",
    "effectiveRR",
    "RequiredRR",
    "riskAtr",
    "REWARD TOO LOW FOR STOP",
    "RR TOO LOW AFTER SPREAD",
    "STOP RISK TOO HIGH",
):
    if token not in rule:
        errors.append("reward-risk rule missing: " + token)

if not re.search(
    r"Math\.Max\(\s*Math\.Max\(\s*Tp1MinimumRR[\s\S]*TacticalOpportunityMinimumRR",
    policy,
):
    errors.append("tactical lanes do not inherit canonical TP1 RR floor")

if "EstimateBestTp1RRForStop(" not in stop or "PlanRewardRiskQualityRule.Evaluate(" not in stop:
    errors.append("structural stop selection is not reward-path aware")

if "PlanRewardRiskQualityRule.Evaluate(" not in plan:
    errors.append("plan integrity does not consume reward-risk rule")

if "PlanRewardRiskQualityRule.Evaluate(" not in action:
    errors.append("live actionability does not consume reward-risk rule")

if "PlanRewardRiskQualityRule.Evaluate(" not in parallel:
    errors.append("parallel candidate construction does not consume reward-risk rule")

if "ScenarioIdentity(" not in parallel or "Different timeframe/lane scenarios are intentionally distinct" not in parallel:
    errors.append("parallel scenario identity isolation missing")

if "ScenarioCoverageKey(" not in scenarios:
    errors.append("scenario coverage-preserving trim missing")

for path, label in (
    (market, "automatic-market"),
    (agg, "aggressive"),
    (pending, "pending"),
):
    if "PlanRewardRiskQualityRule.Evaluate(" not in path:
        errors.append(label + " final reward-risk mutation guard missing")

for token in (
    "effective RR",
    "stop",
    "Tactical",
    "scenario",
    "market",
    "aggressive",
    "pending",
):
    if token.lower() not in routine.lower():
        errors.append("ROUTINE missing Phase 11.4 concept: " + token)

if errors:
    print("Phase 11.4 audit FAILED")
    for error in errors:
        print(" - " + error)
    raise SystemExit(1)

print("Phase 11.4 audit OK")
