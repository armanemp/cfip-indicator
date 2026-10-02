#!/usr/bin/env python3
"""Static acceptance gate for CR6.2 / F2 aggressive risk and fill-plan semantics."""

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


guard = read(
    "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs"
)
broker = read(
    "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveExecutionPreparation.cs"
)
fill = read(
    "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs"
)
closed = read(
    "src/CFIP.Indicator/Trading/Lifecycle/PositionClosedHandler.cs"
)
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")

check(
    "final aggressive guard receives prepared ATR/SL/TP geometry",
    "double atr" in guard and
    "double stop" in guard and
    "double target" in guard,
)

check(
    "unreachable _plan dependency is removed from the final aggressive guard",
    "if (_plan != null)" not in guard,
)

check(
    "canonical reward-risk rule owns aggressive pre-submission RR/risk validation",
    "PlanRewardRiskQualityRule.Evaluate(" in guard and
    "StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(" in guard,
)

check(
    "reaction and requested trade direction must remain aligned",
    "AGGRESSIVE • REACTION/TRADE DIRECTION MISMATCH" in guard and
    "int expectedDirection" in guard,
)

check(
    "explicit Smart Agreement policy remains the only decision/reaction direction policy",
    "AggressiveRequireSmartAgreement" in guard and
    "_decision.Direction != _reaction.Direction" in guard,
)

check(
    "aggressive preparation retains the exact geometry contract before cBot migration",
    "BuildStructuralStop(" in broker and
    "stop" in broker and
    "target" in broker,
)

check(
    "managed plan is seeded from actual-fill exit geometry",
    "result.Position.EntryPrice," in fill and
    "actualStop," in fill and
    "actualTarget," in fill,
)

check(
    "post-fill reconciliation result is authoritative and fail-closed",
    "bool fillPlanReconciled" in fill and
    "AGGRESSIVE POST-FILL RECONCILIATION FAILED" in fill and
    "if (!fillPlanReconciled)" in fill,
)

check(
    "post-fill managed geometry is revalidated before live-state adoption",
    "AGGRESSIVE POST-FILL PLAN GEOMETRY INVALID" in fill and
    "LifecycleState.LivePosition" in fill and
    "EnrichLivePlanTargets" in fill,
)

check(
    "aggressive accepted-fill validation remains owned by the pre-existing canonical validation chain",
    "ValidateActualMarketFill(" in fill and
    "AGGRESSIVE FILL OUTSIDE EXECUTION ENVELOPE" in fill and
    "maximumFillDistance" not in fill and
    "Math.Abs(" not in fill,
)

check(
    "position close cleanup clears live plan and broker state",
    "_plan = null" in closed and
    "_activeBrokerStop = 0" in closed and
    "_activeBrokerTarget = 0" in closed and
    "_executionModel = null" in closed,
)

check(
    "deterministic F2 runtime contract is wired",
    "VerifyAggressiveRiskAndFillSemantics();" in runtime and
    "private static void VerifyAggressiveRiskAndFillSemantics()" in runtime and
    "REWARD TOO LOW" in runtime and
    "STOP RISK TOO HIGH" in runtime,
)

check(
    "F2 audit is accumulated in Source/Architecture CI",
    "audit_phase_6_2.py" in workflow,
)

print("CR6.2 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.2 STATIC GATE PASS")
