#!/usr/bin/env python3
"""Realtime multi-scenario opportunity / future-order regression gate."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append("missing: " + relative)
        return ""
    return path.read_text(encoding="utf-8")

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
future = read("src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs")
builder = read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs")
stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs")
policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
intent_builder = read("src/CFIP.Indicator/Planning/Execution/ExecutionIntentBuilder.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
env = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
workflow = read(".github/workflows/source-check.yml")

check(
    "candidate carries live intrabar and future-order state",
    "public bool FutureOrderReady;" in candidate and
    "public double FutureOrderDistanceAtr;" in candidate and
    "public string FutureOrderSource;" in candidate and
    "public double ZoneLow;" in candidate and
    "public double ZoneHigh;" in candidate
)

check(
    "future pending builder reuses canonical preparation rather than duplicating broker geometry",
    "TryPrepareContinuationStop(" in future and
    "TryPrepareReversalLimit(" in future and
    "RiskRewardMathRule.Evaluate(" in future and
    "IsValidPendingEntry(" in future
)

check(
    "future discovery can build both continuation-stop and reversal-limit scenarios",
    "ExecutionMode.ContinuationStop" in future and
    "ExecutionMode.ReversalLimit" in future and
    "PendingModeAllowsStop()" in future and
    "PendingModeAllowsLimit()" in future
)

check(
    "future discovery does not overwrite the canonical current-market provider intent",
    "_suppressProviderIntentCapture = true" in future and
    "_suppressProviderIntentCapture = false" in future and
    "_suppressProviderIntentCapture" in intent_builder
)

check(
    "parallel structural candidates remain closed-M5 based while live state is refreshed separately",
    "if (_lastOpportunityCandidatesM5 == closedM5)" in builder and
    "RefreshLiveParallelOpportunityStates(" in stage
)

check(
    "live candidate refresh is bounded to a 200ms minimum interval unless the M5 changes",
    "TotalMilliseconds < 200" in future and
    "_lastLiveOpportunityRefreshUtc" in state and
    "_lastLiveOpportunityRefreshM5" in state
)

check(
    "live opportunity refresh runs after M1 timing and before provider publication",
    stage.index("UpdateM1TriggerRuntime(") <
    stage.index("RefreshLiveParallelOpportunityStates(") <
    stage.index("RefreshReadOnlyProvider(")
)

check(
    "future pending candidates are executable without current ActionableNow",
    "bool futurePending =" in policy and
    "FUTURE PENDING SCENARIO EXECUTION AUTHORIZED" in policy and
    "(candidate.ActionableNow || futurePending)" in provider
)

check(
    "provider still routes future stop/limit to the correct cBot action",
    "ExecutionMode.ContinuationStop" in provider and
    "ExecutionAction.PendingStop" in provider and
    "ExecutionMode.ReversalLimit" in provider and
    "ExecutionAction.PendingLimit" in provider
)

check(
    "cBot consumes the scenario batch on every incoming tick with bounded concurrency",
    "protected override void OnTick()" in bot and
    "TryReadScenarioBatch(" in bot and
    "for (int scenarioIndex = 0;" in bot and
    "MaxValue = 10" in bot
)

check(
    "cBot capacity remains scenario-scoped instead of a global single-position veto",
    "CountManagedScenarioObjects(" in env and
    "CONCURRENT SCENARIO CAPACITY BLOCKED" in env and
    "SINGLE-PLAN CAPACITY BLOCKED" not in env
)

check(
    "live trading remains fail-closed while realtime demo execution is available",
    "if (Account.IsLive)" in bot and
    "this build is demo-only" in bot and
    "Enable Demo Market Execution" in bot
)

check(
    "realtime/future audit is accumulated into Source/Architecture CI",
    "python tools/audit_phase_realtime_multiscenario_opportunity_engine_2026_10_03.py" in workflow
)

check(
    "Indicator remains broker-mutation-free in the new realtime path",
    "ExecuteMarketOrder(" not in future and
    "PlaceStopOrder(" not in future and
    "PlaceLimitOrder(" not in future and
    "ClosePosition(" not in future
)

if errors:
    print("=" * 72)
    print("REALTIME MULTI-SCENARIO OPPORTUNITY ENGINE: FAIL")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("=" * 72)
print("REALTIME MULTI-SCENARIO OPPORTUNITY ENGINE: PASS")
