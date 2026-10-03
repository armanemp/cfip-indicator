#!/usr/bin/env python3
"""Static/source contract audit for Phase 11.4."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
RULE = ROOT / "src/CFIP.Indicator/Core/Math/PlanRewardRiskQualityRule.cs"
TARGET_RR_RULE = ROOT / "src/CFIP.Indicator/Core/Math/TargetSelectionRequiredRrRule.cs"
POLICY = ROOT / "src/CFIP.Indicator/Planning/TradePlan/TargetSelectionPolicy.cs"
STOP = ROOT / "src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs"
PLAN = ROOT / "src/CFIP.Indicator/Planning/TradePlan/PlanIntegrityValidator.cs"
ACTION = ROOT / "src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs"
PARALLEL = ROOT / "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs"
PARALLEL_CANDIDATE = ROOT / "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs"
SCENARIOS = ROOT / "src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs"
PARALLEL_SELECTION = ROOT / "src/CFIP.Indicator/Core/Math/ParallelScenarioSelectionRule.cs"
PLANNING_CONTRACT = ROOT / "tools/CFIP.Planning.Contracts/Program.cs"
PLANNING_PROJECT = ROOT / "tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj"
TRACE_MODEL = ROOT / "src/CFIP.Indicator/Core/Models/SignalEvaluationTrace.cs"
TRACE_RECORDER = ROOT / "src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs"
TRACE_ARCHIVE = ROOT / "src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchivePersistence.cs"
ANALYZER = ROOT / "tools/analyze_phase_11_3.py"
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
target_rr_rule = read(TARGET_RR_RULE)
policy = read(POLICY)
stop = read(STOP)
plan = read(PLAN)
action = read(ACTION)
parallel = read(PARALLEL) + read(PARALLEL_CANDIDATE)
scenarios = read(SCENARIOS)
parallel_selection = read(PARALLEL_SELECTION)
planning_contract = read(PLANNING_CONTRACT)
planning_project = read(PLANNING_PROJECT)
trace_model = read(TRACE_MODEL)
trace_recorder = read(TRACE_RECORDER)
trace_archive = read(TRACE_ARCHIVE)
analyzer = read(ANALYZER)
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

if not (
    "double tp1" in target_rr_rule
    and
    "IsTacticalLane(lane)" in target_rr_rule
    and
    "tacticalOpportunityMinimumRR" in target_rr_rule
):
    errors.append(
        "tactical lanes do not inherit canonical TP1 RR floor"
    )

if "EstimateBestTp1RRForStop(" not in stop or "PlanRewardRiskQualityRule.Evaluate(" not in stop:
    errors.append("structural stop selection is not reward-path aware")

if "PlanRewardRiskQualityRule.Evaluate(" not in plan:
    errors.append("plan integrity does not consume reward-risk rule")

if "PlanRewardRiskQualityRule.Evaluate(" not in action:
    errors.append("live actionability does not consume reward-risk rule")

if "PlanRewardRiskQualityRule.Evaluate(" not in parallel:
    errors.append("parallel candidate construction does not consume reward-risk rule")

if (
    "GetScenarioIdentity(" not in parallel_selection
    or "SameIdentity(" not in parallel_selection
):
    errors.append("parallel scenario identity isolation missing")

if (
    "CoverageKey(" not in parallel_selection
    or "SelectForDisplay(" not in parallel_selection
    or "SelectScenariosForDisplay(" not in scenarios
):
    errors.append("scenario coverage-preserving trim missing")

if "VerifyPlanRewardRiskQuality()" not in planning_contract:
    errors.append("planning contract does not execute reward-risk regression cases")

if "PlanRewardRiskQualityRule.cs" not in planning_project:
    errors.append("planning contract project does not include reward-risk rule")

for token in (
    "PlanRiskAtr",
    "EffectiveTp1RR",
    "RequiredTp1RR",
):
    if token not in trace_model or token not in trace_recorder or token not in trace_archive:
        errors.append("signal trace reward-risk field missing: " + token)

if not re.search(
    r'CFIP-SIGNAL-TRACE,(?:2|3)',
    trace_archive,
):
    errors.append("signal trace archive schema v2/v3 missing")

if '"CFIP-SIGNAL-TRACE,2"' not in analyzer or '"CFIP-SIGNAL-TRACE,1"' not in analyzer:
    errors.append("forensic analyzer must remain backward-compatible with signal trace v1/v2")

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
