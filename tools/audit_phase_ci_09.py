#!/usr/bin/env python3
"""Static acceptance gate for CI-09 decision-engine mathematical integrity."""
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

score = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreCalculator.cs")
score_snapshot = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreSnapshot.cs")
consensus = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusCalculator.cs")
quality = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionQualityCalculator.cs")
confidence = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfidenceCalculator.cs")
evaluator = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvaluator.cs")
filters = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionFilters.cs")
threshold = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionThresholdFilterEvaluator.cs")
smart_consensus = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartConsensusFilterEvaluator.cs")
penalty = read("src/CFIP.Indicator/Analysis/Market/Decision/HigherTimeframePenaltyCalculator.cs")
calibration = read("src/CFIP.Indicator/Analysis/Market/Decision/ConfidenceCalibrationCollector.cs")
program = read("tools/CFIP.Decision.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")

check(
    "score has one executable owner and explicit component provenance",
    "class DecisionScoreCalculator" in score and
    "Calculate(DecisionScoreInput input)" in score and
    "Calculate(DecisionInputSnapshot input)" not in score and
    score.count("new DecisionScoreSnapshot(") == 1 and
    all(token in score_snapshot for token in (
        "M5BullContribution", "M15BullContribution",
        "M30BullContribution", "H1BullContribution",
        "H4BullContribution", "D1BullContribution",
        "W1BullContribution", "AdvancedConfluenceBuy",
        "PremiumDiscountBuy", "AdaptiveRegimeBuy",
        "ConflictPenaltyBuy", "ChoppinessFactor"
    ))
)

check(
    "M1 remains confirmation-only and does not vote in directional score",
    "M1 is a trigger confirmation, not an independent directional vote." in score and
    "input.M1Frame.Direction" not in score
)

check(
    "score decomposition has explicit pre-conflict, conflict and final stages",
    "preConflictBuy =" in score and
    "preConflictSell =" in score and
    "conflictPenaltyBuy" in score and
    "conflictPenaltySell" in score and
    "ResolveChoppinessFactor(" in score and
    "? 0.90" in score
)

check(
    "conflict reduction is directionally symmetric at an exact score tie",
    "if (preConflictBuy > preConflictSell)" in score and
    "else if (preConflictSell > preConflictBuy)" in score and
    "conflictPenaltyBuy = penalty;" in score and
    "conflictPenaltySell = penalty;" in score
)

check(
    "frame and advanced-score inputs fail closed when non-finite",
    "SafeNonNegative(input.M5Contribution.Bull)" in score and
    "NumericGuards.IsFiniteValue(value)" in score
)

check(
    "consensus clips the exponent input and rejects non-finite/invalid temperature",
    "NumericGuards.IsFiniteValue(buy)" in consensus and
    "NumericGuards.IsFiniteValue(sell)" in consensus and
    "NumericGuards.IsFiniteValue(temperature)" in consensus and
    "NumericGuards.ClampDouble(" in consensus
)

check(
    "exact BUY/SELL consensus ties are neutral rather than BUY-biased",
    "buyShare > sellShare" in consensus and
    "sellShare > buyShare" in consensus and
    "buyShare >= sellShare" not in consensus
)

check(
    "quality coefficients form explicit normalized weighted sums",
    "strongestShare, 0, 100) * 0.25" in quality and
    "timeframeAgreement, 0, 100) * 0.20" in quality and
    "normalizedIndependentEvidence * 0.20" in quality and
    "normalizedStructural * 0.15" in quality and
    "regimeQuality, 0, 100) * 0.10" in quality and
    "effectiveRetestQuality * 0.10" in quality and
    "indicatorConfluenceQuality, 0, 100) * 0.10" in quality
)

check(
    "confidence coefficients sum to one before calibration/penalty",
    "strongestShare * 0.45" in confidence and
    "timeframeAgreement * 0.25" in confidence and
    "smartQuality * 0.30" in confidence and
    "calibrationAdjustment -" in confidence
)

check(
    "higher-timeframe influence is penalty-only and never changes direction",
    "return" in penalty and
    "HigherTfPenalty +" in penalty and
    "decision.Direction" not in penalty
)

check(
    "empirical calibration is applied after lane/context resolution",
    "ApplyEmpiricalCalibration(" in evaluator or
    "ApplyEmpiricalCalibration(" in calibration and
    "decision.BaseConfidence" in calibration and
    "snapshot.Adjustment" in calibration
)

check(
    "threshold filters consume canonical decision fields at one explicit mapping boundary",
    "new DecisionThresholdFilterInput(" in filters and
    "decision == null ? 0 : decision.Confidence" in filters and
    "decision == null ? 0 : decision.Edge" in filters and
    "decision == null ? 0 : decision.SmartQuality" in filters and
    "decision == null ? 0 : decision.TimeframeAgreement" in filters and
    "input.Confidence < input.MinimumConfidence" in threshold and
    "input.Edge < input.MinimumEdge" in threshold
)

check(
    "filter ordering keeps threshold, top-down, confirmation, smart, structure, market and lifecycle stages explicit",
    all(token in filters for token in (
        "thresholdResult", "topDownResult", "confirmationResult",
        "smartResult", "structureResult", "marketResult", "lifecycleResult"
    ))
)

check(
    "deterministic runtime contracts cover consensus boundaries and score provenance",
    "VerifyConsensusSymmetry();" in program and
    "VerifyDecisionScoreBoundariesAndTraceability();" in program and
    "CI-09 score boundary and provenance contracts PASS" in program and
    "score snapshot exposes frame/confluence component provenance" in program
)

check(
    "CI-09 static audit is wired immediately after CI-08",
    "audit_phase_ci_08.py" in workflow and
    "audit_phase_ci_09.py" in workflow and
    workflow.index("audit_phase_ci_09.py") > workflow.index("audit_phase_ci_08.py")
)

check(
    "CI-09 continuity is recorded without losing CI-08 historical marker",
    "CI-08 implementation record" in roadmap and
    "Current implementation phase: CI-09" in roadmap and
    "CI-09 implementation record" in continuation
)

print("CI-09 DECISION ENGINE MATHEMATICAL INTEGRITY SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-09 STATIC GATE PASS")
