#!/usr/bin/env python3
"""Static acceptance gate for CI-10 trigger and trigger-lifecycle integrity."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append("missing file: " + relative)
        return ""
    return path.read_text(encoding="utf-8")

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

threshold = read("src/CFIP.Indicator/Core/Math/TriggerThresholdRule.cs")
lifecycle = read("src/CFIP.Indicator/Core/Math/TriggerLifecycleRule.cs")
m1_rule = read("src/CFIP.Indicator/Core/Math/M1TriggerRule.cs")
m5_trigger = read("src/CFIP.Indicator/Planning/Entry/ClosedBarTriggerReadyEvaluator.cs")
m1_eval = read("src/CFIP.Indicator/Planning/Entry/M1TriggerReadyEvaluator.cs")
m1_runtime = read("src/CFIP.Indicator/Planning/Entry/M1TriggerRuntimeUpdater.cs")
runtime_state = read("src/CFIP.Indicator/Planning/Entry/TriggerRuntimeState.cs")
fresh = read("src/CFIP.Indicator/Trading/Intelligence/FreshTriggerEvidenceAnalyzer.cs")
decision_eval = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvaluator.cs")
orchestration = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
runtime_contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
historical_roadmap = read("docs/archive/ROADMAP-LEGACY-2026-10-04.md")
continuation = read("docs/CONTINUATION-STATE.md")

check(
    "one canonical trigger threshold owner exists",
    "class TriggerThresholdRule" in threshold and
    "ResolveRequiredScore(" in threshold and
    "IsScoreReady(" in threshold
)

check(
    "M5 and M1 trigger paths share the canonical threshold resolver",
    m5_trigger.count("TriggerThresholdRule.ResolveRequiredScore(") == 1 and
    m1_eval.count("TriggerThresholdRule.ResolveRequiredScore(") == 1 and
    m1_runtime.count("TriggerThresholdRule.ResolveRequiredScore(") == 1 and
    "TriggerThresholdRule.IsScoreReady(" in m1_rule
)

check(
    "legacy duplicated precision/live threshold ternary is absent from trigger evaluators",
    "UsePrecisionExecutionModel\n                                    ? Math.Max(" not in m5_trigger and
    "UsePrecisionExecutionModel\n                ? Math.Max(" not in m1_eval and
    "UsePrecisionExecutionModel\n                ? Math.Max(" not in m1_runtime
)

check(
    "invalid trigger score inputs fail closed",
    "liveTriggerScore < 1" in threshold and
    "liveTriggerScore > 6" in threshold and
    "triggerScore < 0" in threshold and
    "requiredTrigger < 1" in threshold
)

check(
    "one canonical trigger lifecycle owner exists",
    "class TriggerLifecycleRule" in lifecycle and
    "ShouldReset(" in lifecycle and
    "CanEvaluateLiveM1Confirmation(" in lifecycle and
    "ShouldRecordNewConfirmation(" in lifecycle and
    "IsConfirmed(" in lifecycle
)

check(
    "live M1 confirmation is restricted to the currently forming M5 after the decision M5",
    "liveM5 <= decisionM5" in lifecycle and
    "parentM5 != liveM5" in lifecycle and
    "m1Open" in lifecycle and
    "m1NextOpen" in lifecycle
)

check(
    "runtime no longer accepts an old closed-M5 parent as a live M1 trigger",
    "TriggerLifecycleRule.CanEvaluateLiveM1Confirmation(" in m1_runtime and
    "(parentM5 != closedM5" not in m1_runtime and
    "parentM5 != liveM5" not in m1_runtime
)

check(
    "M1 confirmation revision is explicit and monotonic across M5 resets",
    "ConfirmationRevision" in runtime_state and
    "_triggerRuntime.ConfirmationRevision++" in m1_runtime and
    "ConfirmationRevision = 0" not in runtime_state
)

check(
    "new M1 confirmation is latched only once per causal M1 bar",
    "ShouldRecordNewConfirmation(" in m1_runtime and
    "candidateM1 != confirmedM1" in lifecycle and
    "_triggerRuntime.ConfirmedM1 =" in m1_runtime
)

check(
    "trigger readiness is propagated through one lifecycle contract",
    "TriggerLifecycleRule.IsConfirmed(" in m1_runtime and
    "closedM5TriggerReady &&" in decision_eval and
    "!input.UseM1Trigger || m1TriggerReady" in decision_eval
)

check(
    "fresh-trigger evidence remains prior-bar evidence only",
    "Highest(" in fresh and
    "index - 1" in fresh and
    "Lowest(" in fresh and
    "MinimumFreshTriggerEvidence" in m5_trigger
)

check(
    "direct displacement override remains scoped to fresh-M5 evidence bypass",
    "AllowDirectDisplacementOverride" in m5_trigger and
    "FreshTriggerEvidence(" in m5_trigger and
    "overrideOk" in m5_trigger and
    "TriggerThresholdRule.IsScoreReady(" in m5_trigger and
    "breakReady" in m5_trigger
)

check(
    "M1 runtime executes before plan synchronization",
    "UpdateM1TriggerRuntime(" in stage and
    "SynchronizePreTradePlanWithDecision(" in stage and
    stage.index("UpdateM1TriggerRuntime(") < stage.index("SynchronizePreTradePlanWithDecision(")
)

check(
    "decision remains the sole directional authority",
    "input.M1Frame.Direction" not in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreCalculator.cs") and
    "M1TriggerReady(" in orchestration
)

check(
    "runtime contracts compile the new Core trigger owners and runtime state",
    "Core/Math/TriggerThresholdRule.cs" in runtime_project and
    "Core/Math/TriggerLifecycleRule.cs" in runtime_project and
    "Planning/Entry/TriggerRuntimeState.cs" in runtime_project
)

check(
    "deterministic CI-10 runtime contracts cover threshold, window, expiry and latch boundaries",
    "VerifyCi10TriggerLifecycle();" in runtime_contracts and
    "trigger lifecycle contracts PASS" in runtime_contracts and
    "CanEvaluateLiveM1Confirmation(" in runtime_contracts and
    "ShouldReset(" in runtime_contracts and
    "ShouldRecordNewConfirmation(" in runtime_contracts
)

check(
    "CI-10 static audit is wired immediately after CI-09",
    "audit_phase_ci_09.py" in workflow and
    "audit_phase_ci_10.py" in workflow and
    workflow.index("audit_phase_ci_10.py") > workflow.index("audit_phase_ci_09.py")
)

check(
    "CI-10 continuity and phase record are documented",
    "CI-10 — Trigger and trigger-lifecycle audit" in historical_roadmap and
    "CI-10 implementation record" in continuation and
    "PHASE-CI-10-TRIGGER-LIFECYCLE.md" in historical_roadmap
)

print("CI-10 TRIGGER / TRIGGER-LIFECYCLE INTEGRITY SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-10 STATIC GATE PASS")
