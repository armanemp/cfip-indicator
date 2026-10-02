from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src" / "CFIP.Indicator"
CORE = SRC / "Core" / "Math"
PLANNING = SRC / "Planning" / "TradePlan"
LIFECYCLE = SRC / "Trading" / "Lifecycle"
LIVE = SRC / "Trading" / "LiveManagement"
ANALYSIS = SRC / "Analysis" / "Market"

errors = []

def check(name, condition):
    if condition:
        print("PASS | " + name)
    else:
        errors.append(name)

geometry = (CORE / "RiskRewardGeometryRule.cs").read_text(encoding="utf-8")
policy = (CORE / "RiskRewardPolicyRule.cs").read_text(encoding="utf-8")

check(
    "canonical risk-reward geometry owner exists",
    "class RiskRewardGeometryRule" in geometry and
    "RiskRewardGeometryResult" in geometry and
    "NominalRR" in geometry and
    "EffectiveRR" in geometry,
)

check(
    "canonical risk-reward policy owner exists",
    "class RiskRewardPolicyRule" in policy and
    "NormalizeMinimum" in policy and
    "NormalizeMaximum" in policy and
    "MeetsMinimum" in policy,
)

contract_project = (ROOT / "tools" / "CFIP.Planning.Contracts" / "CFIP.Planning.Contracts.csproj").read_text(encoding="utf-8")
contracts = (ROOT / "tools" / "CFIP.Planning.Contracts" / "Program.cs").read_text(encoding="utf-8")
workflow = (ROOT / ".github" / "workflows" / "source-check.yml").read_text(encoding="utf-8")

check(
    "planning contract project compiles canonical RR owners",
    "Core/Math/RiskRewardGeometryRule.cs" in contract_project and
    "Core/Math/RiskRewardPolicyRule.cs" in contract_project,
)

runtime_project = (
    ROOT / "tools" / "CFIP.Runtime.Contracts" /
    "CFIP.Runtime.Contracts.csproj"
).read_text(encoding="utf-8")

decision_project = (
    ROOT / "tools" / "CFIP.Decision.Contracts" /
    "CFIP.Decision.Contracts.csproj"
).read_text(encoding="utf-8")

check(
    "runtime contract project compiles canonical RR owners",
    "Core/Math/RiskRewardGeometryRule.cs" in runtime_project and
    "Core/Math/RiskRewardPolicyRule.cs" in runtime_project,
)

check(
    "decision contract project compiles canonical RR owners",
    "Core/Math/RiskRewardGeometryRule.cs" in decision_project and
    "Core/Math/RiskRewardPolicyRule.cs" in decision_project,
)

check(
    "deterministic CI-14 planning contract is wired",
    "VerifyCanonicalRiskReward();" in contracts and
    "RiskRewardGeometryRule.Evaluate(" in contracts and
    "RiskRewardPolicyRule.CalculateAdaptiveMinimum(" in contracts,
)

check(
    "CI-14 static audit is wired immediately after CI-13",
    re.search(
        r"audit_phase_ci_13\.py\s*\n\s*- run: python tools/audit_phase_ci_14\.py",
        workflow,
    ) is not None,
)

geometry_consumer_paths = {
    "PlanRewardRiskQualityRule": CORE / "PlanRewardRiskQualityRule.cs",
    "ExecutionPlanGeometryRule": CORE / "ExecutionPlanGeometryRule.cs",
    "TargetCandidateConstraintRule": CORE / "TargetCandidateConstraintRule.cs",
    "PlanMaterialization": PLANNING / "PlanMaterialization.cs",
    "PlanTargetPreparation": PLANNING / "PlanTargetPreparation.cs",
    "PlanRewardIntegrityValidator": PLANNING / "PlanRewardIntegrityValidator.cs",
    "StructuralStopCandidateEvaluator": PLANNING / "StructuralStopCandidateEvaluator.cs",
    "LiveExitGeometryRule": CORE / "LiveExitGeometryRule.cs",
    "LiveTargetCandidateEvaluator": LIVE / "LiveTargetCandidateEvaluator.cs",
    "PlanRiskRewardRecalculator": LIVE / "PlanRiskRewardRecalculator.cs",
    "LivePlanFactory": LIFECYCLE / "LivePlanFactory.cs",
    "PendingOrderPlanSnapshot": LIFECYCLE / "PendingOrderPlanSnapshot.cs",
    "ParallelOpportunityBuilder": ANALYSIS / "ParallelOpportunityBuilder.cs",
    "TradeActionabilityEvaluator": SRC / "Trading" / "Validation" / "TradeActionabilityEvaluator.cs",
}

for name, path in geometry_consumer_paths.items():
    text = path.read_text(encoding="utf-8") if path.exists() else ""
    check(
        name + " consumes canonical nominal RR",
        "RiskRewardGeometryRule.CalculateNominalRR(" in text or
        "RiskRewardGeometryRule.CalculateNominalRRFromDistances(" in text or
        (
            name == "PlanRewardRiskQualityRule" and
            "RiskRewardGeometryRule.Evaluate(" in text
        ),
    )

policy_consumer_paths = {
    "ActionableSignalQualityRule": CORE / "ActionableSignalQualityRule.cs",
    "RangeSignalQualityRule": CORE / "RangeSignalQualityRule.cs",
    "PlanRewardRiskQualityRule": CORE / "PlanRewardRiskQualityRule.cs",
    "ExecutionPlanGeometryRule": CORE / "ExecutionPlanGeometryRule.cs",
    "TargetCandidateConstraintRule": CORE / "TargetCandidateConstraintRule.cs",
    "PlanRewardIntegrityValidator": PLANNING / "PlanRewardIntegrityValidator.cs",
    "TradeActionabilityEvaluator": SRC / "Trading" / "Validation" / "TradeActionabilityEvaluator.cs",
    "LiveTargetCandidateEvaluator": LIVE / "LiveTargetCandidateEvaluator.cs",
}

for name, path in policy_consumer_paths.items():
    text = path.read_text(encoding="utf-8") if path.exists() else ""
    check(
        name + " consumes canonical RR policy semantics",
        "RiskRewardPolicyRule." in text,
    )

legacy_patterns = [
    re.compile(r"Math\.Abs\(\s*(?:preview\.)?Tp[1-4]\s*-\s*(?:preview\.)?Entry\s*\)\s*/\s*Math\.Max\(\s*Symbol\.PipSize"),
    re.compile(r"Math\.Abs\(\s*(?:plan\.)?Tp[1-4]\s*-\s*(?:plan\.)?Entry\s*\)\s*/\s*(?:plan\.)?Risk"),
    re.compile(r"Math\.Abs\(\s*target\s*-\s*entry\s*\)\s*/\s*risk"),
    re.compile(r"Math\.Abs\(\s*candidate\.Price\s*-\s*entry\s*\)\s*/\s*risk"),
]

legacy_scan_paths = [
    geometry_geometry_consumer_paths["PlanRewardRiskQualityRule"],
    geometry_consumer_paths["ExecutionPlanGeometryRule"],
    geometry_consumer_paths["TargetCandidateConstraintRule"],
    geometry_consumer_paths["PlanMaterialization"],
    geometry_consumer_paths["PlanTargetPreparation"],
    geometry_consumer_paths["PlanRewardIntegrityValidator"],
    geometry_consumer_paths["StructuralStopCandidateEvaluator"],
    geometry_consumer_paths["LiveExitGeometryRule"],
    geometry_consumer_paths["LiveTargetCandidateEvaluator"],
    geometry_consumer_paths["PlanRiskRewardRecalculator"],
    geometry_consumer_paths["LivePlanFactory"],
    geometry_consumer_paths["PendingOrderPlanSnapshot"],
    geometry_consumer_paths["ParallelOpportunityBuilder"],
]

legacy_found = []
for path in legacy_scan_paths:
    text = path.read_text(encoding="utf-8") if path.exists() else ""
    for pattern in legacy_patterns:
        if pattern.search(text):
            legacy_found.append(str(path.relative_to(ROOT)))
            break

check(
    "legacy duplicated nominal-RR arithmetic is absent from migrated consumers",
    not legacy_found,
)

check(
    "pending/live plans preserve raw price-distance risk",
    "Risk = risk," in (LIFECYCLE / "PendingOrderPlanSnapshot.cs").read_text(encoding="utf-8") and
    "Risk = risk," in (LIFECYCLE / "LivePlanFactory.cs").read_text(encoding="utf-8"),
)

if legacy_found:
    print("LEGACY RR ARITHMETIC FOUND:")
    for path in legacy_found:
        print(" - " + path)

if errors:
    print("CI-14 RISK / REWARD / PROTECTION MATH AUDIT FAILED")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI-14 RISK / REWARD / PROTECTION MATH AUDIT PASS")
