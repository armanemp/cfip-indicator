#!/usr/bin/env python3
"""Static acceptance gate for CR6.5 / F6 trap-risk and trigger/actionability semantics."""

from pathlib import Path
import re
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


policy = read("src/CFIP.Indicator/Core/Math/EntryActionabilityPolicy.cs")
trap_policy = read("src/CFIP.Indicator/Core/Math/EntryTrapRiskPolicy.cs")
trap = read("src/CFIP.Indicator/Core/Math/EntryTrapRiskRule.cs")
indicator = read("src/CFIP.Indicator/Core/Math/IndicatorActionabilityRule.cs")
thresholds = read("src/CFIP.Indicator/Core/Math/ActionabilityThresholdPolicy.cs")
evaluator = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
resolver = read("src/CFIP.Indicator/Planning/Execution/ExecutionModeResolver.cs")
trigger = read("src/CFIP.Indicator/Planning/Execution/TriggerGate.cs")
plan_gate = read("src/CFIP.Indicator/Trading/Validation/PlanCreationEligibility.cs")
market_entry = read("src/CFIP.Indicator/Planning/Execution/MarketEntryValidation.cs")
aggressive = read("src/CFIP.Indicator/Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs")
automatic = read("src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs")
pending = read("src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
parameters = read("src/CFIP.Indicator/Indicator/Parameters/07_entry_precision.cs") + read(
    "src/CFIP.Indicator/Indicator/Parameters/23_structural_execution.cs"
)

for token in (
    "LongExtremeRangePosition = 0.85",
    "ShortExtremeRangePosition = 0.15",
    "LongNearExtremeRangePosition = 0.75",
    "ShortNearExtremeRangePosition = 0.25",
    "MicroConflictAdverseM1Atr = 0.25",
    "MicroConflictEntryDistanceAtr = 0.10",
    "TriggerPipToleranceFraction = 0.10",
    "BreakoutLateExtensionFloorAtr = 0.10",
    "RetestLateDistanceFloorAtr = 0.05",
    "public static bool IsRetestReady(",
    "public static bool ShouldBlockTrapRisk(",
    "public static double ResolveAnchor(",
    "public static double ResolveActualEntry(",
    "public static bool IsLate(",
    "public static bool IsMicroConflict(",
):
    check("F6 actionability policy owner: " + token, token in policy)

for token in (
    "AdverseM5BlockAtr = 0.30",
    "AdverseM1BlockAtr = 0.45",
    "StrongAdverseM5Atr = 0.45",
    "StrongAdverseM1Atr = 0.40",
    "StrongAdverseRiskFloor = 75",
):
    check("F6 trap threshold owner: " + token, token in trap_policy)

for literal in ("0.85", "0.15", "0.75", "0.25"):
    check("trap rule has no duplicated range literal " + literal, literal not in trap)

for token in (
    "ActionabilityThresholdPolicy.IndicatorMinimumQuality",
    "ActionabilityThresholdPolicy.IndicatorRangeTransitionMinimumQuality",
    "ActionabilityThresholdPolicy.IndicatorMaximumConflict",
    "ActionabilityThresholdPolicy.IndicatorRangeTransitionMaximumConflict",
    "MarketRegimeIdentity.Compression",
    "MarketRegimeIdentity.Range",
    "MarketRegimeIdentity.Transition",
):
    check("indicator rule uses canonical owner: " + token, token in indicator)

for token in (
    "public const int IndicatorMinimumQuality = 60;",
    "public const int IndicatorRangeTransitionMinimumQuality = 58;",
    "public const int IndicatorMaximumConflict = 52;",
    "public const int IndicatorRangeTransitionMaximumConflict = 55;",
):
    check("indicator thresholds owned centrally: " + token, token in thresholds)

for token in (
    "EntryGeometryRule.Evaluate(",
    "EntryActionabilityPolicy.IsMicroConflict(",
    "EntryActionabilityPolicy.ShouldBlockTrapRisk(",
):
    check("trade actionability consumes canonical F6 geometry/trap policy: " + token, token in evaluator)

for token in (
    "EntryGeometryRule.Evaluate(",
    "EntryActionabilityPolicy.ExecutionZoneQualityFloor",
):
    check("execution mode resolver consumes canonical F6 geometry policy: " + token, token in resolver)

check(
    "trigger tolerance is centrally owned",
    "EntryActionabilityPolicy.ResolveTriggerTolerance(" in trigger,
)

check(
    "trigger continuation structural minimum is centrally owned",
    "EntryActionabilityPolicy.ResolveContinuationStructuralMinimum(" in trigger,
)

check(
    "canonical plan creation still requires confirmed trigger",
    "_decision.TriggerReady" in plan_gate and
    "if (!_decision.TriggerReady)" in plan_gate,
)

retest_match = re.search(
    r"if \(plan\.EntryMode ==\s*ExecutionMode\.RetestMarket\)(.*?)return true;",
    market_entry,
    re.S,
)
check(
    "Breakout requires trigger and Retest remains zone-driven",
    "ExecutionMode.BreakoutMarket" in market_entry and
    "IsTriggerReached(" in market_entry and
    "ExecutionMode.RetestMarket" in market_entry and
    retest_match is not None and
    "IsTriggerReached(" not in retest_match.group(1),
)

check(
    "Automatic Market consumes the same live ActionableNow decision result",
    "RefreshLiveDecisionActionability(" in automatic and
    "_decision.ActionableNow" in automatic,
)

check(
    "Aggressive entry remains a distinct reaction-driven eligibility path",
    "ObserveReactionSample(" in aggressive and
    "AggressiveRequireSmartAgreement" in aggressive and
    "EntryTrapRiskRule" not in aggressive,
)

check(
    "Pending submission retains canonical validation chain",
    "PlanRewardRiskQualityRule.Evaluate(" in pending and
    "IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(" in pending and
    "PassesAutoTradeSafetyGuards(" in pending,
)

for token in (
    "VerifyEntryActionabilityF6();",
    "EntryActionabilityPolicy.LongExtremeRangePosition",
    "EntryTrapRiskRule.Evaluate(",
    "IndicatorActionabilityRule.Evaluate(",
):
    check("runtime F6 contract coverage: " + token, token in runtime)

for token in (
    "Core/Math/EntryActionabilityPolicy.cs",
    "Core/Math/EntryTrapRiskRule.cs",
    "Core/Math/IndicatorActionabilityRule.cs",
):
    check("runtime project includes F6 owner: " + token, token in runtime_project)

check(
    "F6 static audit is accumulated immediately after F5",
    "audit_phase_6_4.py" in workflow and
    "audit_phase_6_5.py" in workflow and
    workflow.index("audit_phase_6_5.py") > workflow.index("audit_phase_6_4.py"),
)

for token in (
    'DefaultValue = 0.75',
    'DefaultValue = 0.45',
    'DefaultValue = 76',
    'DefaultValue = true',
):
    check("public entry defaults preserved: " + token, token in parameters)

check(
    "F6 policy introduces no public parameters",
    "[Parameter(" not in policy,
)

print("CR6.5 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.5 STATIC GATE PASS")
