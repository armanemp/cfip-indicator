#!/usr/bin/env python3
"""Static acceptance gate for CR4.3 signal-trace temporal lineage."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


decision = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
trace_model = read("src/CFIP.Indicator/Core/Models/SignalEvaluationTrace.cs")
trace_rule = read("src/CFIP.Indicator/Core/Math/SignalTraceIdentityRule.cs")
lineage_rule = read("src/CFIP.Indicator/Core/Math/SignalTraceLineageRule.cs")
recorder = read("src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceRecorder.cs")
trace_archive = read("src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchivePersistence.cs")
plan = read("src/CFIP.Indicator/Core/Models/Plan.cs")
plan_builder = read("src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs")
outcome_model = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeObservation.cs")
outcome_archive = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeHistoryArchiveStore.cs")
outcome_engine = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs")
analyzer = read("tools/analyze_signal_trace.py")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")

check(
    "trace capture no longer occurs inside BuildDecision orchestration",
    "RecordSignalEvaluationTrace(" not in decision,
)
check(
    "calculation propagates the new-closed-bar boundary",
    "closedM5,
                        newClosedBar" in cycle,
)
check(
    "trace capture is guarded by newClosedBar",
    "if (newClosedBar)" in stage and
    "RecordSignalEvaluationTrace(" in stage and
    '"SIGNAL TRACE • CLOSED-M5"' in stage,
)
check(
    "trace has explicit identity and geometry lineage fields",
    "SignalTraceId" in trace_model and
    "GeometryBarOpenTimeUtcTicks" in trace_model and
    "GeometrySource" in trace_model,
)
check(
    "identity is deterministic and scope-aware",
    'CurrentPrefix = "CFIP-ST1"' in trace_rule and
    "barOpenTimeUtcTicks" in trace_rule and
    "accountScope" in trace_rule and
    "configurationFingerprint" in trace_rule,
)
check(
    "exact closed-M5 lineage is centralized",
    "MatchesClosedBar(" in lineage_rule and
    "sourceCreatedM5 == traceClosedM5" in lineage_rule and
    "sourceBarOpenTimeUtcTicks == traceBarOpenTimeUtcTicks" in lineage_rule,
)
check(
    "recorder uses stable trace identity",
    "SignalTraceIdentityRule.Build(" in recorder and
    "SignalTraceId = traceId" in recorder,
)
check(
    "plan geometry requires exact bar and direction lineage",
    "_plan.Direction == decision.Direction" in recorder and
    "SignalTraceLineageRule.MatchesClosedBar(" in recorder and
    "SignalTraceLineageRule.CanJoinOutcome(" in recorder,
)
check(
    "preview geometry requires exact bar and direction lineage",
    "_setupPreview.Direction == decision.Direction" in recorder and
    'geometrySource = "PREVIEW"' in recorder,
)
check(
    "trace archive remains buffered and schema-versioned",
    'SignalTraceSchema = "CFIP-SIGNAL-TRACE,3"' in trace_archive and
    "_bufferedArchivePersistence.Enqueue(" in trace_archive and
    "SignalTraceId" in trace_archive,
)
check(
    "plan carries the source trace identity",
    "SignalBarOpenTimeUtcTicks" in plan and
    "SignalTraceId" in plan and
    "p.SignalTraceId = BuildSignalTraceId(closedM5)" in plan_builder,
)
check(
    "outcome carries source trace identity",
    "SignalTraceId" in outcome_model and
    "SignalTraceId" in outcome_engine,
)
check(
    "outcome archive is backward-compatible with legacy rows",
    'OutcomeArchiveSchema = "CFIP-OUTCOME-ARCHIVE,2"' in outcome_archive and
    'parts.Length >= 19' in outcome_archive and
    'parts[18]' in outcome_archive,
)
check(
    "offline analyzer accepts all trace schema versions and joins outcomes",
    "TRACE_SCHEMAS" in analyzer and
    "CFIP-SIGNAL-TRACE,3" in analyzer and
    "load_outcomes(" in analyzer and
    "outcome_linkage(" in analyzer and
    "join_is_research_only" in analyzer,
)
check(
    "runtime contract covers deterministic trace lineage",
    "VerifySignalTraceLineageSemantics();" in contracts and
    "SignalTraceIdentityRule.Build(" in contracts and
    "MatchesClosedBar(" in contracts,
)
check(
    "future outcomes do not enter the live trace recorder",
    "OutcomeObservation" not in recorder and
    "RealizedR" not in recorder and
    "Profitable" not in recorder,
)
check(
    "trace capture remains one row per canonical closed bar in memory",
    "_signalTraceMemoryKeys.Contains(key)" in recorder and
    "_signalTraceMemoryKeys.Add(key)" in recorder,
)

print("CR4.3 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)
print("CR4.3 STATIC GATE PASS")
