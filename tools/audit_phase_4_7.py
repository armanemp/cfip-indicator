#!/usr/bin/env python3
"""Static acceptance gate for CR4.7 TP pipeline feasibility and telemetry."""

from pathlib import Path
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


level = read("src/CFIP.Indicator/Core/Models/Level.cs")
age = read("src/CFIP.Indicator/Core/Math/TargetAgeSemanticsRule.cs")
constraints = read("src/CFIP.Indicator/Core/Math/TargetCandidateConstraintRule.cs")
envelope = read("src/CFIP.Indicator/Core/Math/TargetRewardEnvelopeRule.cs")
reasons = read("src/CFIP.Indicator/Core/Math/TargetCandidateRejectionReasons.cs")
evaluator = read("src/CFIP.Indicator/Planning/TradePlan/TargetCandidateEvaluator.cs")
validator = read(
    "src/CFIP.Indicator/Trading/Validation/TargetObstacleValidator.cs"
)
selector = read("src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs")
stage_builder = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetLadderStageCandidateBuilder.cs"
)
telemetry = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetStageRejectionTelemetry.cs"
)
stage_gate = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetStageFeasibilityGate.cs"
)
merger = read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelMerger.cs")
htf = read("src/CFIP.Indicator/Planning/TradePlan/Sources/HtfTargetSource.cs")
prior = read("src/CFIP.Indicator/Planning/TradePlan/Sources/PreviousPeriodTargetSource.cs")
pivot = read("src/CFIP.Indicator/Planning/TradePlan/Sources/DailyPivotTargetSource.cs")
d1_liq = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/SupplyDemandLiquidityTargetSource.cs"
)
plan_reward = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanRewardIntegrityValidator.cs"
)
contracts = read("tools/CFIP.Planning.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
phase_doc = read("docs/PHASE-CR4-7-TP-PIPELINE.md")

check(
    "Level preserves source timeframe age independently of M5 setup age",
    "public int Age;" in level and "public double SourceAgeMinutes;" in level,
)
check(
    "M5 and HTF age semantics have one deterministic Core owner",
    "class TargetAgeSemanticsRule" in age
    and "M5" in age
    and "GetMaximumHtfAgeMinutes" in age
    and "ElapsedMinutes" in age,
)
check(
    "HTF source producers attach elapsed age",
    "TargetAgeSemanticsRule.ElapsedMinutes" in htf
    and "TargetAgeSemanticsRule.ElapsedMinutes" in prior
    and "TargetAgeSemanticsRule.ElapsedMinutes" in pivot
    and "TargetAgeSemanticsRule.ElapsedMinutes" in d1_liq,
)
check(
    "target candidate constraints have a pure deterministic owner",
    "class TargetCandidateConstraintRule" in constraints
    and "RR_BELOW_MIN" in reasons
    and "RR_ABOVE_MAX" in reasons
    and "TARGET_TOO_FAR" in reasons
    and "TARGET_PROGRESSION_INVALID" in reasons,
)
check(
    "candidate evaluation delegates target-side and HTF constraints",
    "TargetCandidateConstraintRule.Evaluate(" in evaluator
    and "requireHtf," in evaluator
    and "htf," in evaluator
    and "!IsValidTarget(" not in evaluator,
)
check(
    "stage telemetry is bounded and deduplicated per M5",
    "MaxTargetStageTelemetryReasonsPerM5 = 24" in telemetry
    and "_targetStageTelemetryM5" in telemetry
    and "TargetStageRejectionTelemetry" not in selector
    and "_targetStageTelemetryKeys" in telemetry
    and "RecordExecutionTelemetryHistory(" in telemetry
    and '"PLAN_TARGET"' in telemetry,
)
check(
    "obstacle failures have explicit stage rejection reasons",
    "OBSTACLE_SWING" in reasons
    and "OBSTACLE_EQ" in reasons
    and "OBSTACLE_OPPOSING_ZONE" in reasons
    and "OBSTACLE_HTF_ZONE" in reasons
    and (
        "TargetCandidateRejectionReasons.M5Obstacle" in evaluator
        or "m5Obstacle.Reason" in evaluator
    )
    and "TargetCandidateRejectionReasons.EqualHighLowObstacle" in validator
    and "TargetCandidateRejectionReasons.OpposingZoneObstacle" in evaluator
    and "TargetCandidateRejectionReasons.HtfZoneObstacle" in evaluator
    and "TargetObstacleEvaluation" in validator
    and "EvaluateTargetObstacle(" in validator,
)
check(
    "unreachable stages are identified before the candidate scan",
    "TryValidateTargetStageFeasibility(" in stage_builder
    and "TargetRewardEnvelopeRule.CanReachStage(" in stage_gate
    and "STAGE_UNREACHABLE_BY_EXTENSION" in reasons,
)
check(
    "reward envelope is pure and exposes maximum reachable RR",
    "class TargetRewardEnvelopeRule" in envelope
    and "MaximumReachableRR" in envelope
    and "CanReachStage" in envelope,
)
check(
    "target-source merge preserves elapsed age",
    "match.SourceAgeMinutes =" not in merger
    and "Preserve match.Price, Kind, Timeframe, Age and" in merger,
)
check(
    "plan-level reward rejection telemetry remains intact",
    '"PLAN_REWARD"' in plan_reward
    and "_lastPlanRewardRejectionM5" in plan_reward,
)
check(
    "planning contracts cover age, stage RR and BUY/SELL symmetry",
    "VerifyTargetPipelineD7();" in contracts
    and "TargetAgeSemanticsRule" in contracts
    and "TargetCandidateConstraintRule" in contracts
    and "TargetRewardEnvelopeRule" in contracts,
)
check(
    "planning contract project includes all D7 Core rules",
    "TargetCandidateRejectionReasons.cs" in contracts_project
    and "TargetAgeSemanticsRule.cs" in contracts_project
    and "TargetCandidateConstraintRule.cs" in contracts_project
    and "TargetRewardEnvelopeRule.cs" in contracts_project
    and "StructuralTimeframeRule.cs" in contracts_project,
)
check(
    "D7 source audit is wired after CR4.6",
    "audit_phase_4_6.py" in workflow
    and "audit_phase_4_7.py" in workflow
    and workflow.index("audit_phase_4_7.py") >
    workflow.index("audit_phase_4_6.py"),
)
check(
    "phase documentation preserves no-tuning and manual-terminal boundaries",
    "CR4.7" in roadmap
    and "CR4.7" in continuation
    and "no public parameter name/type/defaultvalue changed" in phase_doc.lower()
    and "no default rr" in phase_doc.lower()
    and "target-terminal" in phase_doc.lower(),
)

print("CR4.7 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR4.7 STATIC GATE PASS")
