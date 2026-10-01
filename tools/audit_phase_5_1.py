#!/usr/bin/env python3
"""Static acceptance gate for CR5.1 / E1 structural-stop ceiling ownership."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = ROOT / "src" / "CFIP.Indicator"
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


rule = read("src/CFIP.Indicator/Core/Math/StructuralStopRiskRule.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
phase_doc = read("docs/PHASE-CR5-1-STRUCTURAL-STOP-CEILING.md")

check(
    "canonical E1 owner exists",
    "class StructuralStopRiskRule" in rule and
    "EffectiveMaximumStopRiskAtr(" in rule and
    "maximumStructuralStopAtr" in rule and
    "Math.Min(" in rule,
)

dual_consumers = []
for path in SOURCE_ROOT.rglob("*.cs"):
    rel = path.relative_to(ROOT).as_posix()
    content = path.read_text(encoding="utf-8")
    if "MaximumSlAtr" in content and "MaximumStructuralStopAtr" in content:
        dual_consumers.append((rel, content))

parameter_file = "src/CFIP.Indicator/Indicator/Parameters/09_risk_targets.cs"
owner_file = "src/CFIP.Indicator/Core/Math/StructuralStopRiskRule.cs"

expected_scope = {
    "src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs",
    "src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs",
    "src/CFIP.Indicator/Planning/TradePlan/PlanIntegrityValidator.cs",
    "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs",
    "src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs",
    "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs",
    "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs",
    "src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs",
    "src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs",
    "src/CFIP.Indicator/Analysis/Market/ParallelScenarioComputation.cs",
    "src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs",
    "src/CFIP.Indicator/Trading/Execution/Aggressive/OrphanManagedProtection.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/ManagedLivePlanRecovery.cs",
    "src/CFIP.Indicator/Trading/Lifecycle/PendingFillPlanBuilder.cs",
}

consumer_set = {p for p, _ in dual_consumers}
production_consumers = consumer_set - {parameter_file, owner_file}
missing_expected = expected_scope - production_consumers
unexpected = production_consumers - expected_scope
missing_owner = [
    rel for rel, content in dual_consumers
    if rel not in {parameter_file, owner_file} and
    "StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(" not in content
]

duplicate_formula = []
for rel, content in dual_consumers:
    normalized = re.sub(r"\s+", " ", content)
    if re.search(
        r"Math\.Min\( Math\.Max\( (?:MinimumSlAtr|minRiskAtr) , MaximumSlAtr \) , "
        r"Math\.Max\( (?:MinimumSlAtr|minRiskAtr) , MaximumStructuralStopAtr \)",
        normalized,
    ):
        duplicate_formula.append(rel)

candidate = read(
    "src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs"
)

check(
    "all expected dual-cap consumers are identified",
    not missing_expected and not unexpected,
)
check(
    "all dual-cap consumers use the canonical owner",
    not missing_owner,
)
check(
    "duplicate inline maximum-stop ceiling formulas are removed",
    not duplicate_formula,
)
check(
    "structural-stop candidates reject over-ceiling risk at candidate boundary",
    ("StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(" in candidate and
     "EffectiveMaximumStopRiskAtr(" in candidate),
)

parameter_content = read(parameter_file)
check(
    "public risk parameters remain unchanged",
    '[Parameter("Minimum SL ATR"' in parameter_content and
    '[Parameter("Maximum SL ATR"' in parameter_content and
    '[Parameter("Fallback SL ATR Multiplier"' in parameter_content and
    "DefaultValue = 0.55" in parameter_content and
    "DefaultValue = 1.80" in parameter_content,
)
check(
    "deterministic E1 truth-table contract is wired",
    "VerifyStructuralStopRiskCeilingSemantics();" in contracts and
    "legacy default effective ceiling remains 1.80 ATR" in contracts,
)
check(
    "runtime contract project includes E1 owner",
    "StructuralStopRiskRule.cs" in project,
)
check(
    "E1 static gate is wired after CR4.10",
    "audit_phase_4_10.py" in workflow and
    "audit_phase_5_1.py" in workflow and
    workflow.index("audit_phase_5_1.py") >
    workflow.index("audit_phase_4_10.py"),
)
check(
    "phase documentation records scope and preserved semantics",
    "CR5.1" in phase_doc and
    "MaximumSlAtr" in phase_doc and
    "MaximumStructuralStopAtr" in phase_doc and
    "no public" in phase_doc.lower(),
)

print("CR5.1 SUMMARY")
print("=" * 72)
print(f"Dual-cap consumer files discovered: {len(dual_consumers)}")
print(f"Missing expected consumers: {len(missing_expected)}")
print(f"Unexpected dual-cap consumers: {len(unexpected)}")
print(f"Missing canonical owners: {len(missing_owner)}")
print(f"Duplicate inline formulas: {len(duplicate_formula)}")

for item in sorted(missing_expected):
    print(f"- missing expected: {item}")
for item in sorted(unexpected):
    print(f"- unexpected dual-cap consumer: {item}")
for item in missing_owner:
    print(f"- missing owner: {item}")
for item in duplicate_formula:
    print(f"- duplicate formula: {item}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.1 STATIC GATE PASS")
