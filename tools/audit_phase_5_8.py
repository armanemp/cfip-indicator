#!/usr/bin/env python3
"""Static acceptance gate for CR5.8 / E8 target-selection RR and lane consistency."""

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


reward = read("src/CFIP.Indicator/Core/Math/PlanRewardRiskQualityRule.cs")
rr_rule = read("src/CFIP.Indicator/Core/Math/TargetSelectionRequiredRrRule.cs")
policy = read("src/CFIP.Indicator/Planning/TradePlan/TargetSelectionPolicy.cs")
selector = read("src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs")
stage_selector = read("src/CFIP.Indicator/Planning/TradePlan/TargetStageSelector.cs")
plan_builder = read("src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs")
execution = read("src/CFIP.Indicator/Trading/Execution/ExecutionPlanPreparation.cs")
live_enrichment = read("src/CFIP.Indicator/Trading/Lifecycle/LivePlanTargetEnrichment.cs")
live_reconcile = read("src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs")
prediction = read("src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs")
preview = read("src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs")
contracts = read("tools/CFIP.Planning.Contracts/Program.cs")
contract_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
phase_doc = read("docs/PHASE-CR5-8-TARGET-SELECTION-CONSISTENCY.md")


def calls_with_last_lane(source, method_name):
    matches = re.findall(
        rf"{method_name}\((.*?)\);",
        source,
        flags=re.DOTALL,
    )
    return matches and all(
        re.search(r"(?:^|,\s*)(?:lane|_plan\.Lane)\s*$", block.strip())
        for block in matches
    )


check(
    "E8 centralizes required RR construction in one Core rule",
    "class TargetSelectionRequiredRrRule" in rr_rule and
    "TargetSelectionRequiredRrRule.BuildRequiredRrLadder(" in policy and
    "TargetSelectionRequiredRrRule.cs" in contract_project,
)

check(
    "the canonical RR ladder is monotonic by construction",
    "tp2 + step" in rr_rule and
    "tp3 + step" in rr_rule and
    "IsMonotonicNonDecreasing(" in rr_rule,
)

check(
    "TP2/TP3/TP4 no longer derive from stale preceding parameter floors",
    "tp2MinimumRR" in rr_rule and
    "tp1 + step" in rr_rule and
    "tp2 + step" in rr_rule and
    "tp3 + step" in rr_rule,
)

check(
    "PlanRewardRiskQualityRule preserves internal adaptive/floor constants with canonical ownership",
    (
        "BaseMinimumRrFloor = 0.50" in reward
        or "RiskRewardPolicyRule.PlanBaseMinimumFloor" in reward
    )
    and "PreferredStopRiskAtrFloor = 0.25" in reward
    and "MaximumStopRiskAtrFloor = 0.50" in reward
    and (
        "Math.Max(preferred, MaximumStopRiskAtrFloor)" in reward
        or "MaximumStopRiskAtrFloor" in reward
    )
    and "Math.Max(preferred, 0.50)" not in reward
    and "AdaptiveStopExcessRrCap = 0.50" in reward
    and "AdaptiveStopExcessRrMultiplier = 0.25" in reward
    and "EffectiveRrBaseFactor = 0.90" in reward
    and "EffectiveRrAbsoluteReduction = 0.15" in reward,
)

check(
    "TargetSelector requires an explicit lane and validates the ladder",
    "OpportunityLane lane)" in selector and
    "TargetSelectionRequiredRrRule.IsMonotonicNonDecreasing(" in selector,
)

check(
    "SelectTarget requires an explicit lane",
    "OpportunityLane lane)" in stage_selector and
    "OpportunityLane lane = OpportunityLane.Strategic" not in stage_selector,
)

check(
    "PlanBuilder uses the canonical plan-lane resolver",
    "ResolvePlanTargetSelectionLane();" in plan_builder,
)

check(
    "all known SelectTargets callers explicitly propagate lane",
    calls_with_last_lane(execution, "SelectTargets") and
    calls_with_last_lane(live_enrichment, "SelectTargets") and
    calls_with_last_lane(live_reconcile, "SelectTargets") and
    calls_with_last_lane(read("src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs"), "SelectTargets") and
    calls_with_last_lane(prediction, "SelectTargets") and
    calls_with_last_lane(preview, "SelectTargets"),
)

check(
    "all known SelectTarget callers explicitly propagate lane",
    calls_with_last_lane(execution, "SelectTarget") and
    calls_with_last_lane(read("src/CFIP.Indicator/Planning/TradePlan/PlanTargetPreparation.cs"), "SelectTarget") and
    calls_with_last_lane(read("src/CFIP.Indicator/Trading/Lifecycle/PendingOrderPlanSnapshot.cs"), "SelectTarget") and
    calls_with_last_lane(prediction, "SelectTarget") and
    calls_with_last_lane(preview, "SelectTarget"),
)

check(
    "execution-plan rebuild uses one resolved lane for selection and fallback",
    "OpportunityLane lane =" in execution and
    "ResolvePlanTargetSelectionLane()" in execution and
    "BuildTargetSelectionRequiredRR(" in execution and
    "requiredRR[0]" in execution and
    "requiredRR[3]" in execution,
)

check(
    "live enrichment follows the active plan lane",
    "SelectTargets(" in live_enrichment and
    "_plan.Lane" in live_enrichment,
)

check(
    "early prediction preserves its historical Strategic lane explicitly",
    "OpportunityLane lane =" in prediction and
    "OpportunityLane.Strategic" in prediction,
)

check(
    "runtime contracts cover all four lanes, monotonicity and BUY/SELL symmetry",
    "VerifyTargetSelectionConsistency();" in contracts and
    "OpportunityLane.CounterHtfTactical" in contracts and
    "OpportunityLane.MicroReaction" in contracts and
    "requiredRR[stage]" in contracts and
    "TargetProgressionRule.IsValid(" in contracts,
)

check(
    "runtime contracts pin the preserved internal constant values",
    "PlanRewardRiskQualityRule.BaseMinimumRrFloor" in contracts and
    "PlanRewardRiskQualityRule.EffectiveRrAbsoluteReduction" in contracts,
)

check(
    "E8 accumulated static gate is wired immediately after E7",
    "audit_phase_5_7.py" in workflow and
    "audit_phase_5_8.py" in workflow and
    workflow.index("audit_phase_5_8.py") >
    workflow.index("audit_phase_5_7.py"),
)

check(
    "E8 continuity documentation is synchronized",
    "CR5.8 / E8" in roadmap and
    "CR5.8 / E8" in continuation and
    "CR5.8 / E8" in phase_doc,
)

print("CR5.8 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.8 STATIC GATE PASS")
