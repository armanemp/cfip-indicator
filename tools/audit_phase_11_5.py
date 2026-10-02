#!/usr/bin/env python3
"""Static/source contract audit for Phase 11.5."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
POLICY = ROOT / "src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs"
CANDIDATES = ROOT / "src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs"
TF_SCENARIOS = ROOT / "src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs"
SUBMISSION_ID = ROOT / "src/CFIP.Indicator/Core/Execution/SubmissionAttemptIdentity.cs"
SUBMISSION_COORD = ROOT / "src/CFIP.Indicator/Trading/Execution/SubmissionGateCoordinator.cs"
SUBMISSION_GATE = ROOT / "src/CFIP.Indicator/Core/Execution/SubmissionGate.cs"
AUTO_MARKET = ROOT / "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketBrokerExecution.cs"
AGGRESSIVE = ROOT / "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveBrokerExecution.cs"
PENDING_STOP = ROOT / "src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs"
PENDING_LIMIT = ROOT / "src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs"
STATE = ROOT / "src/CFIP.Indicator/Indicator/State.cs"
PANEL = ROOT / "src/CFIP.Indicator/Trading/Execution/State/AutoTradingStateStore.cs"
RUNTIME_PROJECT = ROOT / "tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj"
RUNTIME_CONTRACTS = ROOT / "tools/CFIP.Runtime.Contracts/Program.cs"

ERRORS = []


def read(path):
    if not path.exists():
        ERRORS.append(f"missing file: {path}")
        return ""
    return path.read_text(encoding="utf-8")

def read_optional(path):
    return path.read_text(encoding="utf-8") if path.exists() else ""


policy = read(POLICY)
candidates = read(CANDIDATES)
tf_scenarios = read(TF_SCENARIOS)
submission_id = read(SUBMISSION_ID)
submission_coord = read(SUBMISSION_COORD)
submission_gate = read(SUBMISSION_GATE)
auto_market = read_optional(AUTO_MARKET)
aggressive = read_optional(AGGRESSIVE)
pending_stop = read(PENDING_STOP)
pending_limit = read(PENDING_LIMIT)
state = read(STATE)
panel = read(PANEL)
runtime_project = read(RUNTIME_PROJECT)
runtime_contracts = read(RUNTIME_CONTRACTS)

for token in (
    "class ScenarioExecutionPolicyRule",
    "Evaluate(",
    "TryResolvePlanScenario(",
    "TryResolveDirectionScenario(",
    "CanonicalScenarioId(",
    "ExecutionAuthorized",
):
    if token not in policy:
        ERRORS.append("scenario execution policy missing: " + token)

if "ExecutionPolicyAllowed =" not in candidates:
    ERRORS.append("canonical candidate execution-policy state is not attached")

if (
    'BasePlanTimeframe = "M5"' not in tf_scenarios or
    "ExecutionPolicyAllowed =" not in candidates or
    "OBSERVE-ONLY TF SCENARIO" not in policy
):
    ERRORS.append(
        "independent timeframe scenarios must remain observe-only"
    )

if "ExecutionAuthorized" not in policy:
    ERRORS.append(
        "scenario policy must consume explicit execution authorization"
    )

for path, label in (
    (auto_market, "automatic market"),
    (aggressive, "aggressive market"),
    (pending_stop, "pending stop"),
    (pending_limit, "pending limit"),
):
    if not path:
        continue
    if "executionScenarioId" not in path:
        ERRORS.append(label + " does not bind an execution scenario identity")
    if "TryAcquireSubmission(" not in path:
        ERRORS.append(label + " does not use the shared submission gate")

for token in (
    "ScenarioId { get; }",
    "CanonicalKey",
    "ScenarioId =",
):
    if token not in submission_id:
        ERRORS.append("submission identity missing scenario scope: " + token)

if "ScenarioId;" not in submission_coord or "ResolvePlanExecutionScenarioId(" not in submission_coord:
    ERRORS.append("submission coordinator missing plan scenario resolution")

if "ResolveDirectionExecutionScenarioId(" not in submission_coord:
    ERRORS.append("submission coordinator missing direction scenario resolution")

if "identity.ScenarioId" not in submission_coord:
    ERRORS.append("execution telemetry must retain scenario identity")

if "SCENARIO=" not in submission_coord:
    ERRORS.append("execution history telemetry must persist scenario identity")

if "private string _activeExecutionScenarioId" not in state:
    ERRORS.append("runtime state missing active execution scenario")

if "_activeExecutionScenarioId" not in state:
    ERRORS.append("runtime state must retain active execution scenario identity")


if "MaximumRetainedStates" not in submission_gate:
    ERRORS.append("shared submission gate ownership must remain intact")

if "Core/Math/ScenarioExecutionPolicyRule.cs" not in runtime_project:
    ERRORS.append("runtime contracts project does not include canonical scenario execution policy")

if "VerifyScenarioExecutionPolicy()" not in runtime_contracts:
    ERRORS.append("runtime contracts do not execute Phase 11.5 scenario checks")

if "SubmissionAttemptIdentity" not in runtime_contracts:
    ERRORS.append("runtime contracts must cover scenario-scoped submission identity")

# Guard the architectural boundary: Phase 11.5 must not silently advertise
# multi-position broker execution.
capacity_rule = ROOT / "src/CFIP.Indicator/Core/Math/ExecutionCapacityRule.cs"
capacity_code = read(capacity_rule)
if "SupportedMaximumOpenPositions = 1" not in capacity_code:
    ERRORS.append("single-position capacity boundary changed without an explicit new certification phase")

if ERRORS:
    print("Phase 11.5 audit FAILED")
    for error in ERRORS:
        print(" - " + error)
    raise SystemExit(1)

print("Phase 11.5 audit OK")
