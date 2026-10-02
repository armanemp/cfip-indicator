#!/usr/bin/env python3
"""Static acceptance gate for CR7.2 / G2 Retest adverse-momentum semantics."""

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


policy = read("src/CFIP.Indicator/Core/Math/EntryTrapRiskPolicy.cs")
action_policy = read("src/CFIP.Indicator/Core/Math/EntryActionabilityPolicy.cs")
risk_rule = read("src/CFIP.Indicator/Core/Math/EntryTrapRiskRule.cs")
evaluator = (
    read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs") +
    read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityPreparation.cs") +
    read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityGateEvaluation.cs")
)
retest_context = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityRetestContext.cs")
decision = read("src/CFIP.Indicator/Core/Models/Decision.cs")
orchestration = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
reason_builder = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionReasonBuilder.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
decision_project = read("tools/CFIP.Decision.Contracts/CFIP.Decision.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

check(
    "trap thresholds have one Core owner",
    "public const double AdverseM5BlockAtr = 0.30;" in policy and
    "public const double AdverseM1BlockAtr = 0.45;" in policy and
    "public const double StrongAdverseM5Atr = 0.45;" in policy and
    "public const double StrongAdverseM1Atr = 0.40;" in policy and
    "public const int StrongAdverseRiskFloor = 75;" in policy and
    "AdverseM5BlockAtr = EntryTrapRiskPolicy.AdverseM5BlockAtr" in action_policy and
    "AdverseM1BlockAtr = EntryTrapRiskPolicy.AdverseM1BlockAtr" in action_policy and
    "StrongAdverseM5Atr = EntryTrapRiskPolicy.StrongAdverseM5Atr" in action_policy and
    "StrongAdverseM1Atr = EntryTrapRiskPolicy.StrongAdverseM1Atr" in action_policy
)

check(
    "all required Trap rejection reason codes are centralized",
    "TRAP_ADVERSE_M5" in policy and
    "TRAP_ADVERSE_M1" in policy and
    "TRAP_EXTREME" in policy and
    "TRAP_DIVERGENCE" in policy
)

check(
    "EntryTrapRiskRule owns no duplicate numeric trap thresholds",
    "EntryTrapRiskPolicy.AdverseM5BlockAtr" in risk_rule and
    "EntryTrapRiskPolicy.AdverseM1BlockAtr" in risk_rule and
    "EntryTrapRiskPolicy.StrongAdverseM5Atr" in risk_rule and
    "EntryTrapRiskPolicy.StrongAdverseM1Atr" in risk_rule and
    "risk >= EntryTrapRiskPolicy.StrongAdverseRiskFloor" in risk_rule
)

check(
    "Retest context is diagnostic-only and does not remove the existing trap gate",
    "bool block =" in risk_rule and
    "m5 >= EntryTrapRiskPolicy.AdverseM5BlockAtr" in risk_rule and
    "m1 >= EntryTrapRiskPolicy.AdverseM1BlockAtr" in risk_rule and
    "EntryActionabilityPolicy.ShouldBlockTrapRisk(" in evaluator
)

check(
    "Retest pre-zone classifier is bounded to the recent adverse window",
    "IsAdverseWindowPreZone(" in retest_context and
    "index - lookbackBars" in retest_context and
    "barHigh >= low" in retest_context and
    "barLow <= high" in retest_context
)

check(
    "M5/M1 pre-zone flags are only active for an inside-zone Retest",
    "RetestTrapContext =" in evaluator and
    "LiveMode == ExecutionMode.RetestMarket" in evaluator and
    "M5AdversePreZone" in evaluator and
    "M1AdversePreZone" in evaluator
)

check(
    "ActionabilityReason remains the single panel-visible diagnostic channel",
    "public string ActionabilityReason;" in decision and
    "actionability.Reason" in orchestration and
    "_decision.ActionabilityReason" in panel
)

check(
    "Decision reason builder carries actionability reason into the composed reason",
    "decision.ActionabilityReason" in reason_builder and
    "decision.ActionabilityReason;" in reason_builder
)

check(
    "runtime contracts cover M5/M1/Extreme/Divergence reason taxonomy and Retest context",
    "VerifyEntryTrapRiskG2();" in runtime and
    "TRAP_ADVERSE_M5" in runtime and
    "TRAP_ADVERSE_M1" in runtime and
    "TRAP_EXTREME" in runtime and
    "TRAP_DIVERGENCE" in runtime and
    "RETEST PRE-ZONE M5" in runtime and
    "RETEST POST-ZONE/REACTION" in runtime
)

check(
    "runtime and decision contract projects compile the canonical trap policy",
    "Core/Math/EntryTrapRiskPolicy.cs" in runtime_project and
    "Core/Math/EntryTrapRiskPolicy.cs" in decision_project
)

check(
    "G2 static audit is accumulated after G1",
    "audit_phase_7_1.py" in workflow and
    "audit_phase_7_2.py" in workflow and
    workflow.index("audit_phase_7_2.py") > workflow.index("audit_phase_7_1.py")
)

print("CR7.2 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR7.2 STATIC GATE PASS")
