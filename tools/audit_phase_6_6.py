#!/usr/bin/env python3
"""Static acceptance gate for CR6.6 / F7 timeframe-scenario semantics."""

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


def exists(relative):
    return (ROOT / relative).exists()


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


builder = (
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs") +
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs")
)
timeframes = read("src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs")
enrichment = read("src/CFIP.Indicator/Analysis/Market/ScenarioEvidenceEnrichment.cs")
policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
preview = read("src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
computation = read("src/CFIP.Indicator/Analysis/Market/ParallelScenarioComputation.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
csproj = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

check(
    "independent timeframe scenarios explicitly declare M5 as canonical geometry base",
    'BasePlanTimeframe' in candidate and
    'BasePlanTimeframe = "M5"' in builder
)

check(
    "timeframe candidates pass their frame identity into candidate construction",
    "names[i]," in timeframes and
    "BuildLaneCandidate(" in timeframes and
    "true)" in timeframes and
    'BasePlanTimeframe = "M5"' in timeframes
)

check(
    "timeframe candidates reuse the common parallel preview path",
    "TryGetParallelScenarioPreview(" in builder and
    "BuildTradeSetupPreviewFromGeometry(" not in builder
)

check(
    "parallel scenario preview work is cached and invalidated per closed M5",
    "_parallelPreviewCache" in state and
    "_parallelPreviewCache.TryGetValue(" in preview and
    "_parallelPreviewCache[cacheKey] = preview" in preview and
    computation.count("_parallelPreviewCache.Clear();") == 2
)

check(
    "one Core owner contains candidate eligibility and execution authorization",
    "class ScenarioExecutionPolicyRule" in policy and
    "CandidateEligible" in policy and
    "ExecutionAuthorized" in policy
)

check(
    "independent timeframe scenarios are structurally eligible but observe-only",
    '"INDEPENDENT HTF STRUCTURALLY ELIGIBLE"' in policy and
    '"OBSERVE-ONLY HTF SCENARIO"' in policy
)

check(
    "display stage/reason uses the same policy result as execution authorization",
    "ScenarioExecutionPolicyRule.Evaluate(" in builder and
    "candidate.ExecutionPolicyAllowed =" in builder and
    "candidate.ExecutionPolicyReason =" in builder and
    "candidate.ExecutionPolicyReason" in builder
)

check(
    "duplicate Analysis/Trading policy files are removed",
    not exists("src/CFIP.Indicator/Analysis/Market/ScenarioExecutionPolicy.cs") and
    not exists("src/CFIP.Indicator/Trading/Execution/ScenarioExecutionPolicy.cs")
)

check(
    "evidence enrichment is no longer an execution-policy owner",
    "ScenarioExecutionPolicy" not in enrichment and
    "EnrichScenarioEvidence" in enrichment
)

check(
    "runtime contract covers aligned canonical, observe-only TF, direction mismatch and distinct frame identity",
    "VerifyIndependentTimeframeScenarioSemanticsF7();" in runtime and
    "ScenarioExecutionPolicyRule.Evaluate(" in runtime and
    "TF-H1-BUY" in runtime and
    "TF-H4-BUY" in runtime
)

check(
    "runtime project references only the canonical Core policy",
    "Core/Math/ScenarioExecutionPolicyRule.cs" in csproj and
    "Trading/Execution/ScenarioExecutionPolicy.cs" not in csproj
)

check(
    "F7 audit is accumulated immediately after F6",
    "audit_phase_6_5.py" in workflow and
    "audit_phase_6_6.py" in workflow and
    workflow.index("audit_phase_6_6.py") > workflow.index("audit_phase_6_5.py")
)

check(
    "F7 documentation records completion and advances to F8",
    "CR6.6 / F7 closeout" in roadmap and
    "CR6.7 / F8" in roadmap and
    "CR6.7 / F8" in continuation and
    "CR6.6 / F7" in review and
    "CR6.7 / F8" in review
)

print("CR6.6 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.6 STATIC GATE PASS")
