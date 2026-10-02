#!/usr/bin/env python3
"""Static acceptance gate for CR4.8 / D8 TP1 directional defensive validation."""

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


protection = read(
    "src/CFIP.Indicator/Core/Math/PriceProtectionRule.cs"
)
constraints = read(
    "src/CFIP.Indicator/Core/Math/TargetCandidateConstraintRule.cs"
)
selector = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs"
)
stage_builder = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetLadderStageCandidateBuilder.cs"
)
materialization = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanMaterialization.cs"
)
builder = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs"
)
plan_protection = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanProtectionIntegrityValidator.cs"
)
plan_reward = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanRewardIntegrityValidator.cs"
)
progression = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetProgressionRule.cs"
)
contracts = read(
    "tools/CFIP.Planning.Contracts/Program.cs"
)
workflow = read(
    ".github/workflows/source-check.yml"
)
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
phase_doc = read("docs/PHASE-CR4-8-TP1-DIRECTIONAL-DEFENCE.md")


check(
    "canonical target-side owner exists",
    "class PriceProtectionRule" in protection
    and "ValidateTarget(" in protection
    and "direction == 1" in protection
    and "direction == -1" in protection,
)

check(
    "candidate constraints delegate target-side semantics to the canonical owner",
    (
        "PriceProtectionRule.ValidateTarget(" in constraints
        or "RiskRewardMathRule.EvaluateFromRisk(" in constraints
    )
    and "TargetCandidateRejectionReasons.TargetSideInvalid" in constraints
    and "direction == 1 && target <= entry" not in constraints
    and "direction == -1 && target >= entry" not in constraints,
)

check(
    "TargetSelector remains orchestration-only",
    "TargetLadderSelectionRule.SelectBestPath(" in selector
    and "TryBuildTargetLadderStageOptions(" in selector
    and "TryScoreTargetCandidate(" in stage_builder
    and "TargetCandidateConstraintRule" not in selector
    and "!IsValidTarget(" not in selector,
)

check(
    "TP1 is defended before plan materialization",
    "PriceProtectionRule.ValidateTarget(" in materialization
    and "tp1" in materialization
    and "return null" in materialization,
)

check(
    "materialization rejection propagates fail-closed to BuildPlan",
    "if (p == null)" in builder
    and "return null;" in builder,
)

check(
    "plan protection boundary independently validates TP1 direction",
    "PriceProtectionRule.ValidateTarget(" in plan_protection
    and "plan.Tp1" in plan_protection,
)

check(
    "plan reward boundary independently validates TP1 direction",
    "PriceProtectionRule.ValidateTarget(" in plan_reward
    and "plan.Tp1" in plan_reward
    and "TP1 DIRECTION INVALID" in plan_reward,
)

check(
    "target progression keeps explicit directional semantics",
    "class TargetProgressionRule" in progression
    and "direction == 1" in progression
    and "direction == -1" in progression,
)

check(
    "deterministic contracts cover valid and wrong-side TP1 for both directions",
    "VerifyTp1DirectionalDefence();" in contracts
    and "BUY wrong-side TP1 rejected" in contracts
    and "SELL wrong-side TP1 rejected" in contracts
    and "BUY wrong-side TP1 candidate rejected canonically" in contracts
    and "SELL wrong-side TP1 candidate rejected canonically" in contracts
    and "TP progression BUY/SELL symmetry remains intact" in contracts,
)

check(
    "accumulated Source/Architecture audit wires CR4.8 after CR4.7",
    "audit_phase_4_7.py" in workflow
    and "audit_phase_4_8.py" in workflow
    and workflow.index("audit_phase_4_8.py") >
    workflow.index("audit_phase_4_7.py"),
)

check(
    "project documentation records the completed phase and next transition",
    "CR4.8" in roadmap
    and "CR4.8" in continuation
    and "CR4.8" in review
    and "CR4.8" in phase_doc
    and "CR4.9" in roadmap
    and "CR4.9" in continuation,
)

check(
    "phase keeps the no-tuning/manual-terminal boundaries",
    "no public parameter name/type/defaultvalue changed" in phase_doc.lower()
    and "no default rr" in phase_doc.lower()
    and "target-terminal" in phase_doc.lower()
    and "no new decision or execution authority" in phase_doc.lower(),
)

print("CR4.8 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR4.8 STATIC GATE PASS")
