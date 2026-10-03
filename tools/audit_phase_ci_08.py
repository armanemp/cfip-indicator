#!/usr/bin/env python3
"""Static acceptance gate for CI-08 divergence, WaveTrend, reaction and early-signal integrity."""
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

divergence = read("src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs")
div_rule = read("src/CFIP.Indicator/Core/Math/DivergenceThresholdRule.cs")
wt_engine = read("src/CFIP.Indicator/Analysis/Market/WaveTrendEngine.cs")
wt_rule = read("src/CFIP.Indicator/Core/Math/WaveTrendEvidenceRule.cs")
wt_snapshot = read("src/CFIP.Indicator/Core/Models/WaveTrendSnapshot.cs")
wt_money = read("src/CFIP.Indicator/Core/Math/WaveTrendMoneyFlowRule.cs")
wt_analyzer = read("src/CFIP.Indicator/Analysis/Market/WaveTrendEvidenceAnalyzer.cs")
fusion = read("src/CFIP.Indicator/Core/Math/IndicatorEvidenceFusionRule.cs")
independence = read("src/CFIP.Indicator/Core/Math/IndicatorEvidenceIndependenceRule.cs")
scoring = read("src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs")
reaction = read("src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs")
reaction_rule = read("src/CFIP.Indicator/Core/Math/ReactionQualificationRule.cs")
reaction_timing = read("src/CFIP.Indicator/Core/Math/ReactionTimingRule.cs")
prediction = read("src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs")
closed_stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs")
alert_engine = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
decision_runtime = read("tools/CFIP.Decision.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")

check(
    "divergence engine explicitly supports regular/hidden BUY and SELL forms",
    all(token in divergence for token in (
        "REGULAR_BULL", "REGULAR_BEAR", "HIDDEN_BULL", "HIDDEN_BEAR",
        "EvaluateLowDivergence(", "EvaluateHighDivergence(", "GetWaveTrendSnapshot(",
    ))
)

check(
    "divergence strong-conflict threshold has one semantic owner",
    "StrongConflictQuality = 70" in div_rule and
    "MeetsStrongConflictQuality(" in div_rule and
    "input.DivergenceQuality >= 70" not in fusion
)

check(
    "WaveTrend minimum quality flows from the configured parameter into fusion",
    "MinimumWaveTrendQuality" in fusion and
    "MinimumWaveTrendQuality," in scoring and
    "WaveTrendEvidenceRule.MeetsMinimumQuality(" in fusion and
    "WaveTrendEvidenceRule.MeetsMinimumQuality(" in independence
)

check(
    "WaveTrend evidence no longer owns a hidden hardcoded 58 floor",
    "WaveTrendQuality >= 58" not in fusion and
    "WaveTrendQuality >= 58" not in independence and
    "NormalizeMinimumQuality(" in wt_rule and
    "MeetsMinimumQuality(" in wt_rule
)

check(
    "WaveTrend MFI uses canonical non-synthetic tick-volume semantics",
    "WaveTrendMoneyFlowRule.TryCalculateContribution(" in wt_engine and
    "Math.Max(\n                        1.0,\n                        _bars.TickVolumes[j])" not in wt_engine and
    "tickVolume <= 0" in wt_money and
    "positiveFlow = 0" in wt_money and
    "negativeFlow = 0" in wt_money
)

check(
    "WaveTrend evidence state is explicitly materialized and presented to Frame",
    "new WaveTrendSnapshot(" in wt_engine and
    "ApplyWaveTrendEvidence(" in wt_analyzer and
    all(token in wt_snapshot for token in ("BullCross", "BearCross", "Oversold", "Overbought"))
)

check(
    "reaction has explicit live-vs-closed temporal ownership",
    "ReactionTimingRule.IsSeparatedObservationAndConfirmation(" in reaction and
    "closedIndex = live - 1" in reaction and
    "ReactionQualificationRule.IsQualified(" in reaction and
    "IsClosedBarConfirmed(" in reaction_rule and
    "IsSeparatedObservationAndConfirmation(" in reaction_timing
)

prediction_call = closed_stage.find('_prediction =\n                BuildEarlyPrediction(')
decision_call = closed_stage.find('_decision =')
plan_calls = [
    closed_stage.find('TryEnsureAutomaticPlan('),
    closed_stage.find('EnsureSignalPlan('),
]
plan_calls = [position for position in plan_calls if position >= 0]
plan_call = min(plan_calls) if plan_calls else -1
check(
    "early prediction is downstream of the closed decision and before analysis-plan creation",
    decision_call >= 0 and prediction_call > decision_call and plan_call > prediction_call
)

check(
    "early prediction does not mutate authoritative decision/plan/lifecycle state",
    "_decision =" not in prediction and
    "_plan =" not in prediction and
    "SetLifecycleState(" not in prediction and
    "RequestClosePosition(" not in prediction
)

check(
    "WATCH/REACTION alert emission remains transport-owned rather than renderer-owned",
    "SendUnifiedAlert(" in alerts and
    "ProcessDecisionAlerts(" in closed_stage and
    "Notifications.PlaySound" not in alert_engine
)

check(
    "CI-08 deterministic contracts are registered and fusion constructor remains covered",
    "VerifyCi08DivergenceWaveTrendReactionEarlySignal();" in runtime and
    "new IndicatorEvidenceFusionInput(" in decision_runtime and
    "MinimumWaveTrendQuality" in fusion
)

check(
    "CI-08 static audit is wired immediately after CI-07",
    "audit_phase_ci_07.py" in workflow and
    "audit_phase_ci_08.py" in workflow and
    workflow.index("audit_phase_ci_08.py") > workflow.index("audit_phase_ci_07.py")
)

check(
    "CI-08 roadmap/continuation transition is recorded",
    "CI-08 implementation record" in roadmap and
    "## 2.0.1 — Current certification state" in roadmap and
    ("CI-17A" in roadmap or "CI-17" in roadmap) and
    "CI-08 implementation status" in continuation and
    "CI-17" in continuation
)

print("CI-08 DIVERGENCE / WAVETREND / REACTION / EARLY SIGNAL SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-08 STATIC GATE PASS")
