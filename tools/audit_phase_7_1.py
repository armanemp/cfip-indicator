#!/usr/bin/env python3
"""Static acceptance gate for CR7.1 / G1 broker protection risk non-expansion."""

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


policy = read("src/CFIP.Indicator/Core/Math/ManagedStopProtectionRule.cs")
validation = read("src/CFIP.Indicator/Trading/Validation/PriceProtectionValidation.cs")
coordinator = read("src/CFIP.Indicator/Trading/Execution/BrokerProtectionCoordinator.cs")
bound = read("src/CFIP.Indicator/Trading/Execution/Aggressive/BoundPlanProtection.cs")
state = read("src/CFIP.Indicator/Trading/Lifecycle/BrokerProtectionStateEvaluator.cs")
progression = read("src/CFIP.Indicator/Core/Math/ProtectionProgressionRule.cs")
cache_policy = read("src/CFIP.Indicator/Core/Math/TargetObstacleCachePolicy.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

check(
    "existing-stop health has a dedicated Core owner",
    "IsExistingStopHealthy(" in policy and
    "? stop < entry" in policy and
    ": stop > entry;" in policy
)

check(
    "new-stop acceptance still uses market/minimum-distance validation",
    "public static bool Validate(" in policy and
    "market - minimumDistance" in policy and
    "market + minimumDistance" in policy
)

check(
    "indicator exposes explicit existing-stop health semantics",
    "IsExistingManagedStopHealthy(" in validation and
    "ManagedStopProtectionRule.IsExistingStopHealthy(" in validation
)

check(
    "coordinator preserves an existing protective SL when it is too close to market",
    "bool currentStopValid" in coordinator and
    "IsExistingManagedStopHealthy(" in coordinator and
    "else if (currentStopValid &&" in coordinator
)

check(
    "coordinator uses protective-only progression before replacing a healthy SL",
    "ProtectionProgressionRule.ShouldAdvanceStop(" in coordinator
)

check(
    "bound-plan protection keeps an existing healthy SL authoritative",
    "bool brokerStopValid" in bound and
    "IsExistingManagedStopHealthy(" in bound and
    "ProtectionProgressionRule.ShouldAdvanceStop(" in bound
)

check(
    "broker state evaluation does not equate near-market SL with missing protection",
    "IsExistingManagedStopHealthy(" in state and
    "position.StopLoss.Value" in state
)

check(
    "equal cache keys now have a GetHashCode implementation",
    "public override int GetHashCode()" in cache_policy and
    "EqualityTolerance.GetHashCode()" in cache_policy and
    "PipSize.GetHashCode()" in cache_policy
)

check(
    "runtime G1 and hash contracts are wired",
    "VerifyTargetObstacleCacheKeyHashSemantics();" in contracts and
    "VerifyBrokerProtectionG1();" in contracts and
    "G1 BUY near-market existing stop" in contracts
)

check(
    "runtime contract project compiles TargetObstacleCacheKey",
    "Core/Math/TargetObstacleCachePolicy.cs" in contracts_project
)

check(
    "G1 source audit is wired after F3",
    "audit_phase_6_9.py" in workflow and
    "audit_phase_7_1.py" in workflow and
    workflow.index("audit_phase_7_1.py") > workflow.index("audit_phase_6_9.py")
)

print("CR7.1 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR7.1 STATIC GATE PASS")
