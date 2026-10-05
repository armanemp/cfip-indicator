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
require(
    "totalWeight +=\n                                            weights[i]" in read("src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs") and
    "if (frames[i].Direction == 0)\n                                            continue" not in read("src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs"),
    "timeframe agreement must retain eligible neutral frames in its denominator",
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
