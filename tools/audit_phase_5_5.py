#!/usr/bin/env python3
"""Static acceptance gate for CR5.5 / E5 parallel-scenario ownership and MicroReaction safety."""

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


selection = read(
    "src/CFIP.Indicator/Core/Math/ParallelScenarioSelectionRule.cs"
)
micro_rule = read(
    "src/CFIP.Indicator/Core/Math/MicroReactionSafetyRule.cs"
)
geometry = read(
    "src/CFIP.Indicator/Core/Math/ParallelScenarioGeometry.cs"
)
computation = read(
    "src/CFIP.Indicator/Analysis/Market/ParallelScenarioComputation.cs"
)
parallel = (
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs") +
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs")
)
candidate_builder = read(
    "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs"
)
timeframe = read(
    "src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs"
)
tactical = read(
    "src/CFIP.Indicator/Analysis/Market/Decision/DecisionTacticalOpportunityAnalyzer.cs"
)
preview = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs"
)
registry = read(
    "src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs"
)
reaction = read(
    "src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs"
)
decision = read(
    "src/CFIP.Indicator/Core/Models/Decision.cs"
)
state = read("src/CFIP.Indicator/Indicator/State.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
phase_doc = read("docs/PHASE-CR5-5-PARALLEL-SCENARIO-MICROREACTION.md")


check(
    "shared parallel geometry has one canonical owner and bounded M5/direction cache",
    "class ParallelScenarioGeometry" in geometry and
    "TryBuildParallelScenarioGeometry(" in computation and
    "_parallelGeometryCacheM5" in state and
    "_parallelGeometryCache" in state and
    "_parallelExecutionModelCache" in state and
    "TryGetParallelExecutionModel(" in computation and
    "BuildExecutionModel(" in computation and
    "BuildStructuralStop(" in computation,
)

check(
    "candidate materialization consumes shared geometry without rebuilding execution/stop",
    "TryBuildParallelScenarioGeometry(" in parallel and
    "TryGetParallelScenarioPreview(" in parallel and
    "BuildExecutionModel(" not in parallel and
    "BuildTradeSetupPreview(" not in parallel and
    "geometry.Execution" not in parallel and
    "BuildTradeSetupPreviewFromGeometry(" in preview,
)

check(
    "tactical parallel assessment consumes the same shared geometry",
    "TryBuildParallelScenarioGeometry(" in tactical and
    "geometry.Atr" in tactical and
    "geometry.Entry" in tactical and
    "geometry.Stop" in tactical and
    "geometry.ExecutionQuality" in tactical,
)

check(
    "preview geometry extraction preserves the existing regular planning path",
    "TryBuildScenarioGeometry(" in preview and
    "BuildTradeSetupPreviewFromGeometry(" in preview and
    "BuildTargetLevels(" in preview and
    "SelectTargets(" in preview,
)

check(
    "parallel candidate ownership is centralized in TradePlanRegistry",
    "UpsertScenario(" in registry and
    "ParallelScenarioSelectionRule.ShouldReplace(" in registry and
    (
        "_tradePlanRegistry.UpsertScenario(" in parallel or
        "_tradePlanRegistry.UpsertScenario(" in candidate_builder
    ) and
    "_opportunityCandidates.Clear();" in timeframe and
    "_tradePlanRegistry.SelectScenariosForDisplay(" in timeframe,
)

check(
    "deterministic identity, replacement and coverage semantics are Core-owned",
    "GetScenarioIdentity(" in selection and
    "CoverageKey(" in selection and
    "ShouldReplace(" in selection and
    "SelectForDisplay(" in selection and
    "SameIdentity(" in selection,
)

check(
    "same-identity replacement retains the established close-vs-newer geometry semantics",
    "geometryClose" in selection and
    "DisplayPriority(incoming) >" in selection and
    "incoming.CreatedM5 >=" in selection,
)

check(
    "MicroReaction parallel presentation uses confirmed closed-bar identity and direction",
    "MicroReactionSafetyRule.IsClosedBarSafe(" in parallel and
    "_reaction.ReactionConfirmedM5" in parallel and
    "_reaction.ReactionConfirmedDirection" in parallel and
    "_reaction.ReactionConfirmedQuality" in parallel and
    "_reaction.Confidence" not in parallel,
)

check(
    "reaction state resolves closed-bar direction independently before confirmation",
    "confirmedBuyQuality" in reaction and
    "confirmedSellQuality" in reaction and
    "ReactionQualificationRule.ResolveDirection(" in reaction and
    "ReactionConfirmedDirection =" in reaction and
    "ReactionConfirmedM5 =" in reaction and
    "confirmedDirection == direction" in reaction,
)

check(
    "Decision exposes explicit confirmed reaction identity",
    "ReactionConfirmedDirection" in decision and
    "ReactionConfirmedM5" in decision,
)

check(
    "MicroReaction safety rule fails closed on stale bar, direction mismatch, weak confirmation or unconfirmed state",
    "reactionConfirmedM5 == scenarioClosedM5" in micro_rule and
    "confirmedDirection != reactionDirection" in micro_rule and
    "!closedBarConfirmed" in micro_rule and
    "confirmedQuality >=" in micro_rule,
)

check(
    "runtime contracts cover replacement, coverage and MicroReaction safety",
    "VerifyParallelScenarioSelectionSemantics();" in contracts and
    "VerifyMicroReactionClosedBarSemantics();" in contracts and
    "same-identity close-geometry candidate replaces" in contracts and
    "exact matching confirmed closed bar" in contracts,
)

check(
    "runtime contract project includes E5 Core owners",
    "ParallelScenarioGeometry.cs" in project and
    "ParallelScenarioSelectionRule.cs" in project and
    "MicroReactionSafetyRule.cs" in project,
)

check(
    "E5 static gate is wired after E4",
    "audit_phase_5_4.py" in workflow and
    "audit_phase_5_5.py" in workflow and
    workflow.index("audit_phase_5_5.py") >
    workflow.index("audit_phase_5_4.py"),
)

check(
    "phase record and roadmap position E5 as the active remediation",
    "CR5.5" in phase_doc and
    "CR5.5 / E5" in roadmap,
)

print("CR5.5 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR5.5 STATIC GATE PASS")
