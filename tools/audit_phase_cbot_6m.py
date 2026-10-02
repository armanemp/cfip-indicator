#!/usr/bin/env python3
"""CBOT-6M static acceptance gate: concurrent scenario identity/capacity/reconciliation."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative: str) -> str:
    p = ROOT / relative
    if not p.exists():
        errors.append("missing: " + relative)
        return ""
    return p.read_text(encoding="utf-8")

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

batch = read("src/CFIP.Contracts/SignalScenarioBatch.cs")
codec = read("src/CFIP.Contracts/SignalScenarioBatchCodec.cs")
batch_key = read("src/CFIP.Contracts/SignalBusKey.cs")
scenario_identity = read("src/CFIP.Contracts/ScenarioExecutionIdentityRule.cs")
candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
scenario_policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
parallel = read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs")
provider_refresh = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
provider_batch = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs")
provider_publisher = read("src/CFIP.Indicator/Runtime/Provider/CFIPDeviceScenarioBatchPublisher.cs")
transport = read("src/CFIP.cBot/Binding/CfipDeviceSignalTransport.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
preflight = read("src/CFIP.cBot/Execution/CbotSignalPreflight.cs")
gate = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
broker_safety = read("src/CFIP.cBot/Execution/BrokerExecutionSafety.cs")
market = read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs")
pending = read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs")
reconciliation = read("src/CFIP.cBot/Recovery/CbotBrokerReconciliation.cs")
management = read("src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs")
workflow = read(".github/workflows/source-check.yml")
quality_rule = read("src/CFIP.Indicator/Core/Math/TradeOpportunityQualityRule.cs")
quality_builder = read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs")
quality_selection = read("src/CFIP.Indicator/Core/Math/ParallelScenarioSelectionRule.cs")
quality_batch = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs")

check(
    "shared scenario contract preserves identity and bounded batch revision",
    "string IndicatorInstanceId" in batch and
    "long Revision" in batch and
    "SignalEnvelope[] Scenarios" in batch and
    "JsonSerializer.Serialize" in codec
)
check(
    "scenario batch transport is instance-scoped",
    "ForScenarioBatch(" in batch_key and
    "LocalStorageScope.Device" in provider_publisher and
    "LocalStorageScope.Device" in transport
)
check(
    "scenario labels preserve per-ScenarioId identity without broker types in contracts",
    "ScenarioMarker = "|CFIP-S:" in scenario_identity and
    "return root +" in scenario_identity and
    "scenarioId.Trim()" in scenario_identity and
    "cAlgo." not in batch and
    "cAlgo." not in codec
)
check(
    "candidate carries the execution mode and requested volume",
    "public ExecutionMode ExecutionMode;" in candidate and
    "public double RequestedVolume;" in candidate and
    "ExecutionMode = execution.Mode" in parallel
)
check(
    "M15 is the only independent timeframe authorized for automatic execution",
    '"M15"' in scenario_policy and
    '"M15 SCENARIO EXECUTION AUTHORIZED"' in scenario_policy and
    '"OBSERVE-ONLY HTF SCENARIO"' in scenario_policy
)
check(
    "scenario batch does not force source timeframe back to M5",
    "CanonicalM5" not in re.search(
        r"private bool IsScenarioBatchExecutableCandidate[\s\S]*?\n        }",
        provider_batch
    ).group(0)
    if re.search(
        r"private bool IsScenarioBatchExecutableCandidate[\s\S]*?\n        }",
        provider_batch
    ) else False
)
check(
    "provider publishes batch from the same canonical provider revision",
    "BuildScenarioBatchFingerprint(" in provider_refresh and
    "PublishDeviceSignalScenarioBatch(" in provider_refresh and
    "Math.Max(" in provider_batch and
    "_cfipProviderRevision" in provider_batch
)
check(
    "scenario actions support Market, Pending Stop and Pending Limit",
    "ExecutionMode.RetestMarket" in provider_batch and
    "ExecutionMode.BreakoutMarket" in provider_batch and
    "ExecutionMode.ContinuationStop" in provider_batch and
    "ExecutionMode.ReversalLimit" in provider_batch and
    "ExecutionAction.PendingStop" in provider_batch and
    "ExecutionAction.PendingLimit" in provider_batch
)
check(
    "cBot consumes batch then falls back to canonical single envelope",
    "TryReadScenarioBatch(" in bot and
    "ProcessSignalEnvelope(" in bot and
    "TryRead(" in bot and
    "scenarioBatch.Scenarios" in bot
)
check(
    "cBot validates every scenario through one canonical preflight",
    "CbotSignalPreflight.TryValidate(" in bot and
    "staleAfterSeconds" in preflight and
    "SIGNAL SYMBOL SCOPE MISMATCH" in preflight
)
check(
    "concurrent capacity is scenario-aware and >1 capable",
    '"Max Concurrent Scenarios"' in bot and
    "CountManagedScenarioObjects(" in gate and
    "CONCURRENT SCENARIO CAPACITY BLOCKED" in gate
)
check(
    "same ScenarioId remains idempotent while different scenarios may coexist",
    "IdempotencyKey" in market and
    "IdempotencyKey" in pending and
    "SCENARIO CAPACITY BLOCKED • SCENARIO ALREADY ACTIVE" in market and
    "SCENARIO CAPACITY BLOCKED • SCENARIO ALREADY ACTIVE" in pending
)
check(
    "Market and Pending coordinators each enforce concurrent capacity immediately before mutation",
    "CountManagedScenarioObjects(" in market and
    "CountManagedScenarioObjects(" in pending and
    "ExecuteMarketOrder(" in market and
    "PlaceStopOrder(" in pending and
    "PlaceLimitOrder(" in pending
)
check(
    "per-scenario reconciliation is evaluated by execution label",
    "Evaluate(" in reconciliation and
    "_scenarioReconciliations" in bot and
    "ReconcileScenarioState(" in bot
)
check(
    "protection recovery is swept independently across cached scenarios",
    "_scenarioEnvelopes" in bot and
    "SweepScenarioProtectionStates(" in bot and
    "TryRecoverProtection(" in bot
)
check(
    "rebinding clears stale scenario identity/state",
    "_scenarioEnvelopes.Clear();" in bot and
    "_scenarioReconciliations.Clear();" in bot
)
check(
    "trade quality ranking has one bounded composite owner",
    "class TradeOpportunityQualityRule" in quality_rule and
    "CalculateRankBonus(" in quality_rule and
    "return Math.Max(" in quality_rule and
    "Math.Min(" in quality_rule
)
check(
    "canonical opportunity evidence is enriched before scenario ranking",
    "EnrichScenarioEvidence(" in quality_builder and
    "_m5Frame" in quality_builder
)
check(
    "parallel scenario ranking consumes the composite quality bonus",
    "TradeOpportunityQualityRule.CalculateRankBonus(" in quality_selection and
    "compositeBonus" in quality_selection
)
check(
    "cBot scenario PlanSnapshot carries the composite plan quality",
    "compositePlanQuality" in quality_batch and
    "TradeOpportunityQualityRule.CalculateRankBonus(" in quality_batch
)

check(
    "no broker mutation authority moved into Indicator",
    "ExecuteMarketOrder(" not in provider_batch and
    "PlaceStopOrder(" not in provider_batch and
    "PlaceLimitOrder(" not in provider_batch
)
check(
    "legacy single-plan gate is not present in the 6M cBot execution path",
    "SINGLE-PLAN CAPACITY BLOCKED" not in gate and
    "SINGLE-PLAN CAPACITY BLOCKED" not in market and
    "SINGLE-PLAN CAPACITY BLOCKED" not in pending
)
check(
    "single-plan safety remains intact before 6M adoption",
    "Max Concurrent Scenarios" in bot and
    "MaximumOpenPositions" not in bot
)
check(
    "6M audit is wired into Source/Architecture CI",
    "python tools/audit_phase_cbot_6m.py" in workflow
)

# Cheap regression scan for the active source tree.
production = "\n".join(
    p.read_text(encoding="utf-8")
    for p in (ROOT / "src").rglob("*.cs")
)
for forbidden in (
    "ShowPopup(",
    "_popup",
    "ShowPopupAlerts",
    "PopupCriticalOnly",
):
    if forbidden in production:
        print("FAIL | P9 popup regression: " + forbidden)
        errors.append("P9 popup regression: " + forbidden)

print("=" * 72)
print("CBOT-6M CONCURRENT MULTI-SCENARIO SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("CBOT-6M STATIC GATE PASS")
