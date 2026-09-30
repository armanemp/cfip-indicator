#!/usr/bin/env python3
"""Static acceptance gate for CR5.1 / E1 structural-stop ceiling semantics."""

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


consumer_paths = [
    "src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs",
    "src/CFIP.Indicator/Planning/TradePlan/PlanInputPreparation.cs",
    "src/CFIP.Indicator/Planning/TradePlan/PlanIntegrityValidator.cs",
    "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs",
    "src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs",
    "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs",
    "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs",
    "src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs",
    "src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs",
]

consumer_text = {path: read(path) for path in consumer_paths}
canonical_call = re.compile(
    r"StructuralStopRiskRule.EffectiveMaximumStopRiskAtrs*("
)
nested_formula = re.compile(
    r"Math.Mins*(s*"
    r"Math.Maxs*(s*MinimumSlAtrs*,s*MaximumSlAtrs*)s*,s*"
    r"Math.Maxs*(s*MinimumSlAtrs*,s*MaximumStructuralStopAtrs*)s*"
    r")",
    re.S,
)

check(
    "all structural-stop ceiling consumers use the canonical owner",
    all(canonical_call.search(text) for text in consumer_text.values()),
)

check(
    "duplicate caller-side ceiling formula is removed",
    not any(nested_formula.search(text) for text in consumer_text.values()),
)

candidate = consumer_text[consumer_paths[0]]
candidate_guard = candidate.find(
    "StructuralStopRiskRule.IsWithinEffectiveMaximumStopRiskAtr("
)
candidate_target = candidate.find("EstimateBestTp1RRForStop(")
check(
    "candidate over-ceiling rejection occurs before target estimation",
    candidate_guard >= 0 and
    candidate_target >= 0 and
    candidate_guard < candidate_target,
)

rule = read("src/CFIP.Indicator/Core/Math/StructuralStopRiskRule.cs")
check(
    "canonical owner contains only the effective ceiling semantics",
    "EffectiveMaximumStopRiskAtr(" in rule and
    "IsWithinEffectiveMaximumStopRiskAtr(" in rule and
    "Math.Min(" in rule and
    "MaximumSlAtr" not in rule and
    "MaximumStructuralStopAtr" not in rule,
)

reward = read("src/CFIP.Indicator/Core/Math/PlanRewardRiskQualityRule.cs")
check(
    "reward-risk rule cannot promote the maximum through PreferredStopRiskAtr",
    "double maximum =" in reward and
    "Math.Max(
                0.50,
                maximumStopRiskAtr)" in reward and
    "Math.Max(
                    preferred,
                    0.50)" not in reward,
)

parameters = read("src/CFIP.Indicator/Indicator/Parameters/09_risk_targets.cs")
advanced = read("src/CFIP.Indicator/Indicator/Parameters/15_control_advanced.cs")
check(
    "Maximum SL ATR public parameter semantics are preserved",
    '[Parameter("Maximum SL ATR"' in parameters and
    "DefaultValue = 1.80" in parameters and
    "public double MaximumSlAtr { get; set; }" in parameters,
)
check(
    "Maximum Structural Stop ATR public parameter semantics are preserved",
    '[Parameter("Maximum Structural Stop ATR"' in advanced and
    "DefaultValue = 2.25" in advanced and
    "public double MaximumStructuralStopAtr { get; set; }" in advanced,
)

contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
check(
    "runtime contracts cover truth table, symmetry, early rejection and preferred-ceiling interaction",
    "VerifyStructuralStopRiskSemantics();" in contracts and
    "effective maximum structural-stop risk preserves the Min/Maximum truth table" in contracts and
    "BUY and SELL structural-stop ceiling acceptance is symmetric" in contracts and
    "preferred stop risk cannot promote the effective maximum ceiling" in contracts,
)
check(
    "runtime contract project includes the E1 canonical rule",
    "StructuralStopRiskRule.cs" in project,
)

workflow = read(".github/workflows/source-check.yml")
check(
    "E1 static gate is wired after D10",
    "audit_phase_4_10.py" in workflow and
    "audit_phase_5_1.py" in workflow and
    workflow.index("audit_phase_5_1.py") >
    workflow.index("audit_phase_4_10.py"),
)

roadmap = read("docs/ROADMAP.md").lower()
continuation = read("docs/CONTINUATION-STATE.md").lower()
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md").lower()
phase_doc = read("docs/PHASE-CR5-1-EFFECTIVE-STOP-RISK.md").lower()

check(
    "documentation records CR5.1 completion and transition",
    "cr5.1" in roadmap and
    "implementation complete" in roadmap and
    "cr5.1" in continuation and
    "cr5.2" in continuation and
    "cr5.1" in review and
    "cr5.2" in phase_doc,
)

check(
    "parameter/no-tuning/manual-terminal boundaries are documented",
    "no public parameter" in phase_doc and
    "no default" in phase_doc and
    "target-terminal" in phase_doc,
)

print("CR5.1 SUMMARY")
print("=" * 72)
print(f"Ceiling consumers: {len(consumer_paths)}")
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.1 STATIC GATE PASS")
