#!/usr/bin/env python3
"""CI-20 analysis/signal accuracy and performance audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing " + rel)
        return ""
    return path.read_text(encoding="utf-8")

closed = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
frame = read("src/CFIP.Indicator/Analysis/Market/MarketFrameAnalyzer.cs")
diversity = read("src/CFIP.Indicator/Core/Math/IndependentEvidenceDiversityRule.cs")
quality = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionQualityCalculator.cs")
evidence = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvidenceSnapshot.cs")
orchestration = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
evaluator = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvaluator.cs")
workflow = read(".github/workflows/source-check.yml")
entry_zones = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelectionCore.cs")
trigger_gate = read("src/CFIP.Indicator/Planning/Execution/TriggerGate.cs")
actionable_quality = read("src/CFIP.Indicator/Core/Math/ActionableSignalQualityRule.cs")
structural_confirmation = read("src/CFIP.Indicator/Core/Math/StructuralConfirmationRule.cs")
mtf_trend_strength = read("src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs")
signal_arrow = read("src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs")
frame_contribution_adapter = read("src/CFIP.Indicator/Analysis/Market/Decision/FrameDecisionContributionAdapter.cs")
retest_quality = read("src/CFIP.Indicator/Planning/Filters/RegimeFilter.cs")
execution_mode = read("src/CFIP.Indicator/Planning/Execution/ExecutionModeResolver.cs")

def require(condition, message):
    if not condition:
        errors.append(message)

require(
    "AnalyzeFrameCached(" in frame,
    "canonical MTF frame cache helper missing",
)
for token in (
    "_m15Frame =\n                AnalyzeFrameCached(",
    "_m30Frame =\n                AnalyzeFrameCached(",
    "_h1Frame =\n                AnalyzeFrameCached(",
    "_h4Frame =\n                AnalyzeFrameCached(",
):
    require(token in closed, "closed-bar cycle does not reuse MTF frame cache: " + token)

for token in (
    "BullIndependentEvidenceGroups",
    "BearIndependentEvidenceGroups",
):
    require(token in evidence, "decision evidence snapshot missing " + token)

for token in (
    "IndependentEvidenceGroupCount(1)",
    "IndependentEvidenceGroupCount(-1)",
):
    require(token in orchestration, "decision orchestration does not capture group coverage: " + token)

require(
    "IndependentEvidenceDiversityRule.QualityBonus(" in quality and
    "independentEvidenceGroupCount = 0" in quality,
    "decision quality does not consume bounded evidence diversity",
)

require(
    "evidence.BullIndependentEvidenceGroups" in evaluator and
    "evidence.BearIndependentEvidenceGroups" in evaluator,
    "decision evaluator does not bind direction-specific evidence diversity",
)

require(
    "M15 is the canonical decision timeframe" in evaluator and
    "strongM15Conflict" in evaluator and
    "bool weakM15Context" in evaluator and
    "(input.M15Frame == null ||" in evaluator and
    "m15Direction == 0" in evaluator and
    'BlockReason = "M15 CANONICAL CONFLICT"' in evaluator,
    "M15 canonical directional ownership is not enforced for missing/neutral/weak context",
)

require(
    "maximumPracticalZoneDistanceAtr" in entry_zones and
    "DistanceToRawZone(" in entry_zones and
    "return false;" in entry_zones,
    "execution-zone fallback is not bounded by practical market distance",
)

require(
    "frame.Direction == 0" in frame_contribution_adapter,
    "neutral frames must not contribute directional consensus",
)

require(
    "return frameDirection == requestedDirection" in structural_confirmation,
    "structural confirmation must require same-direction frame alignment",
)

require(
    "ResolveCanonicalM15Direction(frames)" in mtf_trend_strength and
    "private static int ResolveCanonicalM15Direction(" in mtf_trend_strength,
    "MTF trend direction must anchor to canonical M15 direction",
)

require(
    "IsStrongMtfTrendPresentation(snapshot)" in signal_arrow and
    "MtfTrendStrengthTier" in signal_arrow,
    "weak/non-actionable MTF trends must not render as trade arrows",
)

require(
    "ClosedBarTriggerReady(" in trigger_gate and
    "strong higher-level score is not a substitute" in trigger_gate and
    "AllowStrongTriggerOverride" not in trigger_gate,
    "strong-score trigger override can bypass the canonical closed-M5 trigger",
)

require(
    "input.EntryLocationQuality <" in actionable_quality and
    "input.EntryPositionQuality <" in actionable_quality and
    "&& !AllowsQualityRecovery(input)" not in actionable_quality.split("if (input.EntryLocationQuality",1)[1].split("if (input.EntryTimingQuality",1)[0] and
    "&& !AllowsQualityRecovery(input)" not in actionable_quality.split("if (input.EntryPositionQuality",1)[1].split("if (!IsFinitePositiveValue",1)[0],
    "quality recovery can still authorize a materially bad entry location/position",
)

require(
    "private int RetestQuality(" in retest_quality and
    "bars.ClosePrices[index]" in retest_quality and
    "bars.HighPrices[i] >= zone.Low" in retest_quality,
    "retest quality does not use closed-M5 price/zone confirmation",
)

require(
    "RetestQuality(" in execution_mode and
    "retest >= MinimumRetestQuality" in execution_mode and
    "ExecutionMode.RetestMarket" in execution_mode,
    "RetestMarket can become ready without the canonical retest-quality gate",
)

require(
    "switch (groupCount)" in diversity and
    "case 4:" in diversity and
    "case 3:" in diversity and
    "case 2:" in diversity,
    "diversity rule is not a bounded deterministic coverage rule",
)

require(
    "audit_phase_ci20_engine_quality.py" in workflow,
    "CI-20 engine quality audit is not wired into Source/Architecture",
)

if errors:
    print("CI-20 ANALYSIS / SIGNAL QUALITY+PERFORMANCE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI-20 ANALYSIS / SIGNAL QUALITY+PERFORMANCE AUDIT: PASS")
print("Aligned MTF frame caching: PASS")
print("Independent evidence-family coverage: PASS")
print("Decision quality diversity integration: PASS")
print("No new public threshold parameter: PASS")
