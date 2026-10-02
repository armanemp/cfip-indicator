#!/usr/bin/env python3
"""Regression audit for the mode-aware RetestMarket trigger path."""

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


policy = read("src/CFIP.Indicator/Core/Math/EntryActionabilityPolicy.cs")
trigger = read("src/CFIP.Indicator/Planning/Execution/TriggerGate.cs")
plan_gate = read("src/CFIP.Indicator/Trading/Validation/PlanCreationEligibility.cs")
scenario = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
actionability = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
closed_bar = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
panel_status = read("src/CFIP.Indicator/UI/Panel/PanelCanonicalSignalStatus.cs")

check(
    "one canonical mode-aware trigger policy exists",
    "public static bool RequiresConfirmedTrigger(" in policy and
    "return mode != ExecutionMode.RetestMarket;" in policy,
)

check(
    "Retest does not require generic M5 confirmed trigger",
    "EntryActionabilityPolicy.RequiresConfirmedTrigger(" in trigger and
    "EntryActionabilityPolicy.RequiresConfirmedTrigger(" in plan_gate and
    "ExecutionMode.RetestMarket" not in trigger.split(
        "private bool IsActionabilityTriggerReady(", 1
    )[1].split("private string ExecutionModeText(", 1)[0],
)

check(
    "plan creation no longer has a global TriggerReady veto",
    "if (!_decision.TriggerReady)" not in plan_gate and
    "RequiresConfirmedTrigger(" in plan_gate,
)

check(
    "scenario authorization uses the same mode-aware trigger rule",
    "RequiresConfirmedTrigger(" in scenario and
    "if (!decision.TriggerReady)" not in scenario and
    "!decision.TriggerReady" not in scenario.split(
        "internal static bool TryResolveDirectionScenario(", 1
    )[1],
)

check(
    "live actionability still enforces mode-specific trigger semantics",
    "if (!IsActionabilityTriggerReady(" in actionability and
    "ExecutionMode.RetestMarket" in actionability,
)

check(
    "new-bar plan gating sees the current execution mode, not the prior bar",
    "UpdateExecutionModel(" in closed_bar and
    "TryEnsureAutomaticPlan(" in closed_bar and
    closed_bar.index("UpdateExecutionModel(") <
    closed_bar.index("TryEnsureAutomaticPlan("),
)

check(
    "panel presents Actionable before generic trigger-watch state",
    panel_status.index("if (_decision.ActionableNow)") <
    panel_status.index("if (!_decision.TriggerReady)"),
)

check(
    "runtime contract suite executes Retest trigger semantics",
    "VerifyRetestTriggerModeSemantics();" in runtime and
    "RetestMarket remains zone-driven" in runtime,
)

check(
    "accumulated CI contains this regression audit",
    "audit_phase_retest_trigger_path.py" in workflow,
)

check(
    "roadmap records the current root-cause fix",
    "RetestMarket trigger-path hardening" in roadmap,
)

check(
    "continuation state records the same architectural decision",
    "RetestMarket" in continuation and
    "trigger" in continuation.lower(),
)

check(
    "review roadmap no longer states a global Retest TriggerReady requirement",
    "canonical plan still requires" not in review.lower(),
)

if errors:
    print("RETEST TRIGGER PATH AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("RETEST TRIGGER PATH AUDIT: PASS")
