#!/usr/bin/env python3
"""Static acceptance gate for CR3.1 live invalidation and false-signal semantics."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

evaluation = read("src/CFIP.Indicator/Trading/LiveManagement/ActivePlanEvaluation.cs")
market = read("src/CFIP.Indicator/Trading/LiveManagement/ActivePlanMarketState.cs")
structural = read("src/CFIP.Indicator/Trading/LiveManagement/StructuralSetupInvalidationExit.cs")
false_signal = read("src/CFIP.Indicator/Trading/LiveManagement/ActivePlanFalseSignalGuard.cs")
coordinator = read("src/CFIP.Indicator/Trading/Lifecycle/LivePlanExitCoordinator.cs")
stop_accessor = read("src/CFIP.Indicator/Trading/Lifecycle/ActiveBrokerStopAccessor.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
param = read("src/CFIP.Indicator/Indicator/Parameters/16_accuracy.cs")
rule = read("src/CFIP.Indicator/Core/Math/LiveInvalidationRule.cs")
threshold_rule = read("src/CFIP.Indicator/Core/Math/FalseSignalAdverseRRule.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

checks = {
    "live invalidation is once per canonical closed M5 bar": (
        "LiveInvalidationRule.ShouldEvaluateClosedBar(" in evaluation and
        "_lastInvalidationEvaluationM5" in state
    ),
    "invalidation market is the closed M5 close, not the live quote": (
        "TryGetClosedM5InvalidationMarket(" in evaluation and
        "_m5Bars.ClosePrices[closedM5]" in market
    ),
    "directional closed-bar move is centralized": (
        "LiveInvalidationRule.TryCalculateDirectionalMove(" in evaluation
    ),
    "structural invalidation uses confirmed swing candidates": (
        "TryFindLatestSwingLow(" in structural and
        "TryFindLatestSwingHigh(" in structural and
        "double swingHigh" not in structural and
        "double swingLow" not in structural
    ),
    "rolling min/max swing scan was removed": (
        "swingLookback" not in structural and
        "swingHigh" not in structural and
        "swingLow" not in structural
    ),
    "structural exit records success only after broker mutation success": (
        "bool closeAccepted" in structural and
        "if (!closeAccepted)" in structural and
        "LiveInvalidationRule.RecordExitM5(" in structural
    ),
    "generic live-plan exit cannot advance bookkeeping on rejection": (
        "if (!closeStatus.IsAccepted())" in coordinator and
        "LiveInvalidationRule.RecordExitM5(" in coordinator
    ),
    "explicit soft adverse-R flag preserves current default": (
        'Parameter("Enable Soft Adverse-R Invalidation"' in param and
        "DefaultValue = true" in param
    ),
    "false-signal threshold has one semantic owner": (
        "FalseSignalAdverseRRule.TryResolveThresholds(" in false_signal and
        "MinimumHardAdverseR" in threshold_rule and
        "MaximumConfiguredAdverseR" in threshold_rule
    ),
    "false-signal threshold cannot silently sit behind protected SL": (
        "GetActiveBrokerStopPrice()" in false_signal and
        "_plan.Stop" in false_signal and
        "Math.Min(" in threshold_rule
    ),
    "false-signal inputs validate non-finite and parameter bounds": (
        "double.IsNaN(configuredAdverseR)" in threshold_rule and
        "double.IsInfinity(configuredAdverseR)" in threshold_rule and
        "configuredAdverseR > MaximumConfiguredAdverseR" in threshold_rule
    ),
    "soft/hard adverse-R remains symmetric": (
        "direction == 1" in threshold_rule and
        "entry - protectedStop" in threshold_rule and
        "protectedStop - entry" in threshold_rule
    ),
    "runtime contracts cover CR3.1": (
        "VerifyLiveInvalidationSemantics();" in contracts and
        "VerifyFalseSignalAdverseRSemantics();" in contracts and
        "RecordExitM5(" in contracts
    ),
    "runtime project links CR3.1 rules": (
        "LiveInvalidationRule.cs" in project and
        "FalseSignalAdverseRRule.cs" in project
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# C1: successful-exit bookkeeping must be downstream of a successful broker mutation.
if "_lastExitM5 = closedM5" in structural:
    errors.append("structural invalidation still advances _lastExitM5 unconditionally")
    print("FAIL | structural invalidation still advances _lastExitM5 unconditionally")

# C1/C2: the false-signal stage must not consume live currentMove directly.
if "ProcessActivePlanFalseSignalRisk(" in evaluation and "currentMove);" in evaluation:
    errors.append("false-signal guard still consumes live-tick currentMove")
    print("FAIL | false-signal guard still consumes live-tick currentMove")

# C2: do not add a second hard-coded FalseSignalAdverseR threshold.
for forbidden in (
    "Math.Max(0.25, FalseSignalAdverseR)",
    "Math.Max(1.0, FalseSignalAdverseR)",
):
    if forbidden in false_signal:
        errors.append("raw FalseSignalAdverseR clamp remains outside semantic owner")
        print("FAIL | raw FalseSignalAdverseR clamp remains outside semantic owner")
        break

print("CR3.1 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR3.1 STATIC GATE PASS")
