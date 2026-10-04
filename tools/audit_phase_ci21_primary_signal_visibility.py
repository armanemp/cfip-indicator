#!/usr/bin/env python3
"""Static acceptance gate for CI-21 primary M15 signal visibility."""

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


builder = (
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs") +
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs")
)
timeframes = read("src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs")
candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
renderer = read("src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs")
execution_tf = read("src/CFIP.Indicator/Core/Math/ExecutionTimeframePolicy.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
devlog = read("docs/DEVELOPMENT-LOG.md")


check(
    "M15 remains the canonical execution clock and M5 remains defensive tuning",
    'PrimaryExecution = "M15"' in execution_tf and
    'LowerDefensiveM5 = "M5"' in execution_tf,
)

primary_pos = timeframes.find("PrimaryTimeframeSignalResult primary")
build_pos = timeframes.find("TradeOpportunityCandidate candidate")
check(
    "primary M15/H1 source validation happens before plan construction",
    primary_pos >= 0 and build_pos >= 0 and primary_pos < build_pos and
    "if (!primary.Allowed)" in timeframes,
)

check(
    "primary candidate construction explicitly enables presentation fallback",
    'names[i],\n                        true)' in timeframes,
)

check(
    "presentation fallback is owned by the canonical parallel opportunity builder",
    "allowPrimaryPresentationFallback" in builder and
    "BuildPrimaryPresentationCandidate(" in builder and
    "PRIMARY PLAN GEOMETRY UNAVAILABLE" in builder and
    "PRIMARY EXECUTION MODEL UNAVAILABLE" in builder and
    "PRIMARY PLAN PREVIEW UNAVAILABLE" in builder and
    "PRIMARY PLAN LEVELS INCOMPLETE" in builder,
)

check(
    "primary display floor is the same source-quality floor used by the primary rule",
    "Math.Max(\n                        60,\n                        TacticalOpportunityMinimumQuality - 5)" in builder and
    "if (candidate.IsPrimaryTimeframeSignal)" in builder,
)

check(
    "extra parallel-candidate quality margin cannot suppress primary M15/H1 source visibility",
    "if (!candidate.IsPrimaryTimeframeSignal &&\n                candidate.Lane != OpportunityLane.CounterHtfTactical" in builder,
)

check(
    "presentation-only state is explicit on the candidate model",
    "public bool PresentationOnly" in candidate,
)

check(
    "presentation-only candidates are fail-closed at execution-policy boundary",
    'if (candidate.PresentationOnly)' in policy and
    '"PRIMARY PRESENTATION ONLY"' in policy,
)

check(
    "presentation-only candidates render the primary setup marker without invalid zero plan levels",
    "if (candidate.PresentationOnly)\n                    continue;" in renderer,
)

check(
    "runtime contract proves presentation-only scenarios cannot execute",
    "PresentationOnly = true" in runtime and
    '"PRIMARY PRESENTATION ONLY"' in runtime,
)

check(
    "CI-21 audit is accumulated in source-check workflow",
    "audit_phase_ci21_primary_signal_visibility.py" in workflow,
)

check(
    "CI-21 documentation is recorded across roadmap, continuation state and development log",
    "CI-21" in roadmap and
    "CI-21" in continuation and
    "CI-21" in devlog,
)

if errors:
    print("CI-21 STATIC GATE FAIL")
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CI-21 STATIC GATE PASS")
print("Primary source: M15/H1")
print("Execution clock: M15")
print("Entry tuning: M5 (with M1 confirmation where enabled)")
print("Presentation-only primary setups: non-executable")
