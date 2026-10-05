#!/usr/bin/env python3
"""Static acceptance gate for CR2.3 unified indicator-quality thresholds."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

rule = read("src/CFIP.Indicator/Core/Math/IndicatorExecutionQualityRule.cs")
threshold_policy = read("src/CFIP.Indicator/Core/Math/ExecutionThresholdPolicy.cs")
market = ""
pending_submit = read("src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs")
pending = read("src/CFIP.Indicator/Trading/Pending/Policy/PendingOrderPolicy.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

required = {
    "canonical indicator-quality owner": (
        "internal static class IndicatorExecutionQualityRule" in rule and
        "EvaluateIndicatorExecutionQuality(" in rule
    ),
    "market thresholds centralized": (
        "AutomaticMarketQualityMinimum = 60" in rule and
        "AutomaticMarketConflictMaximum = 52" in rule
    ),
    "submission thresholds centralized": (
        "PendingSubmissionQualityMinimum = 58" in rule and
        "PendingSubmissionConflictMaximum = 55" in rule
    ),
    "pending setup quality centralized": (
        "PendingSetupQualityMinimum = 62" in rule and
        "PendingContinuationConflictMaximum = 48" in rule and
        "PendingReversalConflictMaximum = 50" in rule
    ),
    "pending continuation uses owner": (
        "IndicatorQualityGateStage.PendingContinuation" in pending and
        "IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(" in pending
    ),
    "pending reversal uses owner": (
        "IndicatorQualityGateStage.PendingReversal" in pending and
        "IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(" in pending
    ),
    "pending submission uses owner": (
        "IndicatorQualityGateStage.PendingSubmission" in pending_submit and
        "IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(" in pending_submit
    ),
    "legacy threshold owner no longer duplicates indicator constants": (
        "IndicatorConfluenceMinimum" not in threshold_policy and
        "IndicatorConflictMaximum" not in threshold_policy
    ),
    "deterministic contract present": "VerifyIndicatorExecutionQualitySemantics();" in contracts,
    "runtime project links canonical rule": "IndicatorExecutionQualityRule.cs" in contracts_project,
}

for name, ok in required.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

for path, content_text in (
    ("PendingSubmissionValidator.cs", pending_submit),
    ("PendingOrderPolicy.cs", pending),
):
    for forbidden in (
        "ExecutionThresholdPolicy.AutomaticMarketIndicator",
        "ExecutionThresholdPolicy.PendingSubmissionIndicator",
        "ExecutionThresholdPolicy.PendingContinuationIndicator",
        "ExecutionThresholdPolicy.PendingReversalIndicator",
    ):
        if forbidden in content_text:
            errors.append(
                f"{path} still owns a legacy indicator threshold reference: {forbidden}"
            )
            print(f"FAIL | {errors[-1]}")

print("CR2.3 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.3 STATIC GATE PASS")
