#!/usr/bin/env python3
"""Static acceptance gate for CR6.3 / F4 effective-threshold transparency."""

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


policy = read("src/CFIP.Indicator/Core/Math/ActionabilityThresholdPolicy.cs")
gate = read("src/CFIP.Indicator/Trading/Validation/ActionableSignalQualityGate.cs")
evaluator = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
parallel = (
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs") +
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs")
)
quality_rule = read("src/CFIP.Indicator/Core/Math/ActionableSignalQualityRule.cs")
orchestration = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
accumulation = read("tools/audit_phase_accumulation.py")


for name, value in (
    ("upstream location floor", "public const int UpstreamEntryLocationQualityFloor = 64;"),
    ("upstream timing floor", "public const int UpstreamEntryTimingQualityFloor = 64;"),
    ("precision floor", "public const int PrecisionEntryQualityFloor = 40;"),
    ("final location floor", "public const int FinalEntryLocationQualityFloor = 70;"),
    ("final timing floor", "public const int FinalEntryTimingQualityFloor = 75;"),
    ("final position floor", "public const int FinalEntryPositionQualityFloor = 70;"),
    ("confidence margin", "public const int FinalConfidenceMargin = 4;"),
    ("smart margin", "public const int FinalSmartQualityMargin = 3;"),
    ("MTF margin", "public const int FinalTimeframeAgreementMargin = 3;"),
    ("evidence margin", "public const int FinalIndependentEvidenceMargin = 1;"),
    ("structure margin", "public const int FinalStructuralConfirmationsMargin = 1;"),
    ("parallel margin", "public const int ParallelCandidateQualityMargin = 3;"),
    ("recovery deficit allowance", "public const int QualityRecoveryDeficitAllowance = 8;"),
    ("recovery confidence margin", "public const int RecoveryConfidenceMargin = 5;"),
    ("recovery smart margin", "public const int RecoverySmartQualityMargin = 5;"),
    ("recovery MTF margin", "public const int RecoveryTimeframeAgreementMargin = 3;"),
    ("recovery evidence margin", "public const int RecoveryIndependentEvidenceMargin = 1;"),
    ("recovery structure margin", "public const int RecoveryStructuralConfirmationsMargin = 1;"),
    ("recovery RR margin", "public const double RecoveryTp1RrMargin = 0.35;"),
):
    check(name, value in policy)


check(
    "final policy preserves smart MTF precedence",
    "Math.Max(" in policy and "smartMinimumTimeframeAgreement" in policy
)
check(
    "final policy preserves smart evidence precedence",
    "smartMinimumIndependentEvidence" in policy
)
check(
    "final gate consumes canonical threshold snapshot",
    "ActionabilityThresholdPolicy.ResolveFinal(" in gate and
    "thresholds.FinalMinimumConfidence" in gate and
    "thresholds.FinalMinimumTp1RR" in gate
)
check(
    "final gate passes smart minimum parameters",
    "SmartMinimumTimeframeAgreement" in gate and
    "SmartMinimumIndependentEvidence" in gate
)
check(
    "final gate no longer owns hidden numeric margins directly",
    "MinimumConfidence + 4" not in gate and
    "+ 3" not in gate and
    "+ 1" not in gate and
    "minimumEntryTimingQuality = 75" not in gate and
    "minimumEntryPositionQuality = 70" not in gate
)
check(
    "staged actionability uses named upstream floors",
    "EffectiveUpstreamEntryLocationQuality(" in evaluator and
    "EffectiveUpstreamEntryTimingQuality()" in evaluator
)
check(
    "precision entry floor is named",
    "EffectivePrecisionEntryQualityFloor(" in evaluator
)
check(
    "parallel hidden quality margin has one named owner",
    "ApplyParallelCandidateQualityMargin(" in parallel and
    "minimumQuality + 3" not in parallel
)
check(
    "quality-recovery margins have one named owner",
    "ActionabilityThresholdPolicy.QualityRecoveryDeficitAllowance" in quality_rule and
    "ActionabilityThresholdPolicy.RecoveryTp1RrMargin" in quality_rule
)
check(
    "ActionableNow is finalized only after the final gate",
    "decision.ActionableNow =\n                        actionability.Actionable;" in orchestration and
    "EvaluateFinalActionableSignalQuality(" in orchestration and
    "decision.ActionableNow = false;" in orchestration
)
check(
    "panel exposes effective thresholds",
    "ActionabilityThresholdPolicy.ResolveFinal(" in panel and
    "ACTIONABILITY  PRE L/T " in panel and
    "FinalMinimumEntryPositionQuality" in panel
)
check(
    "runtime contract is wired",
    "VerifyActionabilityThresholdTransparency();" in runtime and
    "private static void VerifyActionabilityThresholdTransparency()" in runtime
)
check(
    "runtime project compiles the policy and rule",
    "Core/Math/ActionabilityThresholdPolicy.cs" in runtime_project and
    "Core/Math/ActionableSignalQualityRule.cs" in runtime_project
)
check(
    "F4 static audit is accumulated in Source/Architecture",
    "audit_phase_6_3.py" in workflow
)
check(
    "accumulated phase audit tracks named upstream threshold owners",
    "ActionabilityThresholdPolicy" in accumulation
)

print("CR6.3 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.3 STATIC GATE PASS")
