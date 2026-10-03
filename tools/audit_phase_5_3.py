#!/usr/bin/env python3
"""Static acceptance gate for CR5.3 / E3 independent-evidence group counting."""

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


core = read(
    "src/CFIP.Indicator/Core/Math/IndependentEvidenceFusionRule.cs"
)
analyzer = read(
    "src/CFIP.Indicator/Analysis/Market/Decision/IndependentEvidenceAnalyzer.cs"
)
decision = read("src/CFIP.Indicator/Core/Models/Decision.cs")
candidate = read(
    "src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs"
)
builder = read(
    "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs"
)
orchestration = read(
    "src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs"
)
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")

check(
    "one Core owner exposes both legacy score and independent group count",
    "class IndependentEvidenceFusionRule" in core and
    "CalculateScore(" in core and
    "CountGroups(" in core,
)

check(
    "structural features collapse to one independent group",
    "input.Structure ||" in core and
    "input.Transition ||" in core and
    "input.Displacement)" in core,
)

check(
    "location features collapse to one independent group",
    "input.Liquidity ||" in core and
    "input.Fvg ||" in core and
    "input.OrderBlock)" in core,
)

check(
    "trend/momentum features collapse to one independent group",
    "input.Trend ||" in core and
    "input.Momentum ||" in core and
    "input.Macd ||" in core and
    "input.Vwap)" in core,
)

check(
    "context features collapse to one independent group",
    "input.Volume ||" in core and
    "input.Volatility ||" in core and
    "input.Rejection ||" in core and
    "input.EqualLevel)" in core,
)

check(
    "analysis evidence extraction delegates score and group semantics to Core",
    "IndependentEvidenceFusionRule.CalculateScore(" in analyzer and
    "IndependentEvidenceFusionRule.CountGroups(" in analyzer and
    "BuildIndependentEvidenceInput(" in analyzer,
)

check(
    "no duplicate legacy calculator remains",
    "IndependentEvidenceFusionCalculator" not in analyzer and
    not (ROOT / "src/CFIP.Indicator/Analysis/Market/Decision/IndependentEvidenceFusionCalculator.cs").exists(),
)

check(
    "decision exposes selected-direction independent group count",
    "IndependentEvidenceGroupCount" in decision and
    "decision.IndependentEvidenceGroupCount" in orchestration,
)

check(
    "parallel candidates expose both legacy score and group count",
    "IndependentEvidenceScore" in candidate and
    "IndependentEvidenceGroupCount" in candidate and
    "IndependentEvidenceScore =" in builder and
    "IndependentEvidenceGroupCount =" in builder,
)

check(
    "candidate quality still uses the legacy bounded score rather than changing thresholds",
    "IndependentEvidence(direction) * 5.0" in read(
        "src/CFIP.Indicator/Analysis/Market/Decision/DecisionTacticalOpportunityAnalyzer.cs"
    ),
)

check(
    "deterministic E3 contracts are wired",
    "VerifyIndependentEvidenceGroupSemantics();" in contracts and
    "one structural group" in contracts and
    "exactly four groups" in contracts,
)

check(
    "runtime contract project includes the canonical Core owner",
    "IndependentEvidenceFusionRule.cs" in project,
)

check(
    "E3 static gate is wired after E2",
    "audit_phase_5_2.py" in workflow and
    "audit_phase_5_3.py" in workflow and
    workflow.index("audit_phase_5_3.py") >
    workflow.index("audit_phase_5_2.py"),
)

check(
    "roadmap is already positioned on E3",
    "CR5.3 / E3" in roadmap,
)

print("CR5.3 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.3 STATIC GATE PASS")
