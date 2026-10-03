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
builder = (
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs") +
    read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityCandidateBuilder.cs")
)
stage = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs")
policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
registry = read("src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs")
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
    "execution candidates are not truncated by the display-only opportunity limit",
    "SelectScenariosForExecution(" in registry and
    "SelectScenariosForExecution(" in provider and
    "SelectScenariosForDisplay(" not in re.search(
        r"private string BuildScenarioBatchFingerprint[\s\S]*?private SignalScenarioBatch BuildScenarioBatch",
        provider
    ).group(0)
    if re.search(
        r"private string BuildScenarioBatchFingerprint[\s\S]*?private SignalScenarioBatch BuildScenarioBatch",
        provider
    ) else False
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

on_tick_match = re.search(
    r"protected override void OnTick\(\)[\\s\\S]*?(?=protected override void OnTimer\(\))",
    bot,
)
on_timer_match = re.search(
    r"protected override void OnTimer\(\)[\\s\\S]*?(?=private void ProcessSignalEnvelope\()",
    bot,
)

check(
    "cBot has one canonical realtime signal-consumption clock on OnTimer, not OnTick",
    on_tick_match is not None and
    on_timer_match is not None and
    "TryReadScenarioBatch(" not in on_tick_match.group(0) and
    "TryRead(" not in on_tick_match.group(0) and
    "ProcessSignalEnvelope(" not in on_tick_match.group(0) and
    "SweepScenarioProtectionStates(" not in on_tick_match.group(0) and
    "TryReadScenarioBatch(" in on_timer_match.group(0) and
    "for (int i = 0; i < scenarios.Length; i++)" in on_timer_match.group(0) and
    "MaxValue = 10" in bot
)

check(
    "cBot capacity remains scenario-scoped instead of a global single-position veto",
    "CountManagedScenarioObjects(" in env and
    "CONCURRENT SCENARIO CAPACITY BLOCKED" in env and
    "SINGLE-PLAN CAPACITY BLOCKED" not in env
)

check(
    "live trading is explicitly account-scoped and default-off",
    "Enable Live Market Execution" in bot and
    "Enable Live Pending Stop Execution" in bot and
    "Enable Live Pending Limit Execution" in bot and
    "Account.IsLive" in bot and
    "EffectiveMarketExecutionEnabled" in bot
)

check(
    "tiny stagnant opportunities are blocked before current/future execution",
    "OpportunityMagnitudeRule.IsMeaningful(" in read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs") and
    "OpportunityMagnitudeRule.IsMeaningful(" in future and
    "TP1 MOVE TOO SMALL FOR REGIME" in read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
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
