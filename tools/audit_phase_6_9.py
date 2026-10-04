#!/usr/bin/env python3
"""Static acceptance gate for CR6.9 / F3 orphan managed-position protection."""

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

orphan = read("src/CFIP.Indicator/Trading/Execution/Aggressive/OrphanManagedProtection.cs")
caller = read("src/CFIP.Indicator/Trading/Execution/Aggressive/BrokerProtectionExecution.cs")
policy = read("src/CFIP.Indicator/Core/Math/OrphanManagedProtectionRule.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
historical_roadmap = read("docs/archive/ROADMAP-LEGACY-2026-10-04.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
phase = read("docs/PHASE-CR6-9-F3-ORPHAN-MANAGED-POSITION-PROTECTION.md")

check(
    "Core F3 success owner is direction-safe and fail-closed",
    "if (direction != 1 && direction != -1)" in policy and
    "return stopCandidateValid &&" in policy and
    "brokerProtectionConfirmed;" in policy
)

invalid_block = orphan.rsplit("if (!IsValidStop(", 1)[1].split("double target", 1)[0] if "if (!IsValidStop(" in orphan else ""
check(
    "invalid computed/fallback stop cannot report success",
    "ORPHAN-PROTECTION-FAILED" in invalid_block and
    "CanReportSuccess(" in invalid_block and
    "return true;" not in invalid_block
)

check(
    "orphan fallback uses canonical structural geometry",
    "StructuralStopGeometryRule.EvaluateFallback(" in orphan and
    "StructuralStopRiskRule.IsWithinPlanningRiskEnvelope(" in orphan
)

check(
    "broker confirmation remains part of the orphan success invariant",
    "bool brokerProtectionConfirmed" in orphan and
    "EnsureBrokerProtectionForPosition(" in orphan and
    "brokerProtectionConfirmed);" in orphan
)

failure_block = (
    caller.split("if (!ProtectOrphanManagedPosition(", 1)[1]
    .split("_lastBrokerModifyUtc", 1)[0]
    if "if (!ProtectOrphanManagedPosition(" in caller
    else ""
)
check(
    "caller enters RecoveryRequired on orphan protection failure",
    "_brokerProtectionRecoveryRequired = true;" in caller and
    "LifecycleState.RecoveryRequired" in caller and
    "ORPHAN MANAGED POSITION • PROTECTION FAILED • RETRY" in caller
)

check(
    "failure path does not update broker-modify timestamp before retry",
    bool(failure_block) and
    "_lastBrokerModifyUtc =" not in failure_block
)

check(
    "successful orphan path retains broker-modify timestamp update",
    caller.count("_lastBrokerModifyUtc =") >= 2
)

check(
    "BUY/SELL fallback stop construction remains symmetric through the canonical owner",
    "direction == 1" in orphan and
    "StructuralStopGeometryRule.EvaluateFallback(" in orphan and
    "FallbackSlAtr" in orphan
)

check(
    "runtime F3 contract is wired",
    "VerifyOrphanManagedProtectionF3();" in contracts and
    "F3 BUY invalid computed stop can never report protection success" in contracts and
    "F3 SELL invalid computed stop can never report protection success" in contracts
)

check(
    "runtime contract project compiles the F3 Core owner",
    "Core/Math/OrphanManagedProtectionRule.cs" in contracts_project
)

check(
    "F3 audit is accumulated immediately after F9",
    "audit_phase_6_8.py" in workflow and
    "audit_phase_6_9.py" in workflow and
    workflow.index("audit_phase_6_9.py") > workflow.index("audit_phase_6_8.py")
)

check(
    "F3 docs advance to Prompt 7 G1",
    "CR6.9 / F3 closeout" in historical_roadmap and
    "CR7.1 / G1" in historical_roadmap and
    "CR7.1 / G1" in continuation and
    "CR7.1 / G1" in review and
    "Next phase: CR7.1 / G1" in phase
)

check(
    "F3 adds no public parameter",
    "[Parameter(" not in policy
)

print("CR6.9 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.9 STATIC GATE PASS")
