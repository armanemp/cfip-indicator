#!/usr/bin/env python3
"""Static acceptance gate for CR6.7 / F8 target-obstacle telemetry semantics."""

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


validator = read("src/CFIP.Indicator/Trading/Validation/TargetObstacleValidator.cs")
evaluator = read("src/CFIP.Indicator/Planning/TradePlan/TargetCandidateEvaluator.cs")
telemetry = read("src/CFIP.Indicator/Planning/TradePlan/TargetStageRejectionTelemetry.cs")
reasons = read("src/CFIP.Indicator/Core/Math/TargetCandidateRejectionReasons.cs")
accumulator = read("src/CFIP.Indicator/Core/Math/TargetObstacleTelemetryAccumulator.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
csproj = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
phase = read("docs/PHASE-CR6-7-F8-TARGET-OBSTACLE-TELEMETRY.md")


check(
    "M5 obstacle evaluation preserves a single scan result for validation and telemetry",
    "EvaluateTargetObstacle(" in validator and
    "TargetObstacleEvaluation m5Obstacle" in evaluator and
    evaluator.count("EvaluateTargetObstacle(") == 1
)

check(
    "M5 swing obstacles keep their existing rejection category",
    'M5Obstacle = "OBSTACLE_SWING"' in reasons and
    "TargetCandidateRejectionReasons.M5Obstacle" in validator
)

check(
    "equal-high/low obstacles are now distinguished from ordinary swings",
    'EqualHighLowObstacle = "OBSTACLE_EQ"' in reasons and
    "TargetCandidateRejectionReasons.EqualHighLowObstacle" in validator
)

check(
    "opposing-zone and HTF-zone rejection categories remain distinct",
    'OpposingZoneObstacle = "OBSTACLE_OPPOSING_ZONE"' in reasons and
    'HtfZoneObstacle = "OBSTACLE_HTF_ZONE"' in reasons and
    "TargetCandidateRejectionReasons.OpposingZoneObstacle" in evaluator and
    "TargetCandidateRejectionReasons.HtfZoneObstacle" in evaluator
)

check(
    "obstacle telemetry measures target distance in ATR and its position inside the existing extension envelope",
    "TargetObstacleTelemetryAccumulator" in accumulator and
    "MinTargetDistanceAtr" in accumulator and
    "MaxTargetDistanceAtr" in accumulator and
    "MaxTargetExtensionPercent" in accumulator and
    "MaximumTargetExtensionAtr" in evaluator
)

check(
    "obstacle telemetry can carry obstacle depth when the scanner knows the price level",
    "ObstacleDistanceAtr" in accumulator and
    "m5Obstacle.ObstacleDistanceAtr" in evaluator
)

check(
    "zone-only obstacle telemetry does not invent a precise obstacle distance",
    'TargetCandidateRejectionReasons.OpposingZoneObstacle' in evaluator and
    'TargetCandidateRejectionReasons.HtfZoneObstacle' in evaluator and
    "                        -1," in evaluator
)

check(
    "target obstacle telemetry is bounded and deduplicated per closed M5/stage/reason",
    "MaxTargetObstacleTelemetryReasonBucketsPerM5" in telemetry and
    "_targetObstacleTelemetry.Clear();" in telemetry and
    "_targetStageTelemetryKeys.Add(key)" in telemetry
)

check(
    "telemetry summary is emitted through the existing PLAN_TARGET channel",
    'RecordExecutionTelemetryHistory(' in telemetry and
    '"PLAN_TARGET"' in telemetry and
    "obstacleTelemetry.FormatSummary()" in telemetry
)

check(
    "no new public target/risk parameter is introduced",
    "[Parameter(" not in accumulator and
    "[Parameter(" not in validator and
    "[Parameter(" not in telemetry
)

check(
    "runtime contracts cover near/far target observations, missing depth, invalid input and taxonomy",
    "VerifyTargetObstacleTelemetryF8();" in runtime and
    "target obstacle telemetry aggregates target/obstacle distance" in runtime and
    "zone-only obstacle telemetry retains target-distance evidence" in runtime and
    "OBSTACLE_EQ" in runtime
)

check(
    "runtime project compiles the telemetry accumulator",
    "Core/Math/TargetObstacleTelemetryAccumulator.cs" in csproj
)

check(
    "F8 audit is accumulated immediately after F7",
    "audit_phase_6_6.py" in workflow and
    "audit_phase_6_7.py" in workflow and
    workflow.index("audit_phase_6_7.py") > workflow.index("audit_phase_6_6.py")
)

check(
    "F8 documentation and continuation advance to F9",
    "CR6.7 / F8 closeout" in roadmap and
    "CR6.8 / F9" in roadmap and
    "CR6.8 / F9" in continuation and
    "CR6.7 / F8" in review and
    "CR6.8 / F9" in review and
    "CR6.7 / F8" in phase
)

print("CR6.7 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.7 STATIC GATE PASS")
