#!/usr/bin/env python3
"""Dedicated audit for the 2026-10-03 realtime/live/signal-truth unification phase."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing: " + rel)
        return ""
    return path.read_text(encoding="utf-8")

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
gate = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
market = read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs")
pending = read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs")
settings = read("src/CFIP.cBot/Execution/CbotIndicatorExecutionSettings.cs")
snapshot = read("src/CFIP.Contracts/CbotExecutionStateSnapshot.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
registry = read("src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs")
actionability = read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs")
future = read("src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs")
magnitude = read("src/CFIP.Indicator/Core/Math/OpportunityMagnitudeRule.cs")
range_rule = read("src/CFIP.Indicator/Core/Math/RangeSignalQualityRule.cs")
alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs")
alert_engine = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")
alert_processor = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
phase = read("docs/PHASE-REALTIME-LIVE-SIGNAL-UNIFICATION-2026-10-03.md")

check(
    "live action arms exist and default off",
    all(x in bot for x in (
        "EnableLiveMarketExecution",
        "EnableLivePendingStopExecution",
        "EnableLivePendingLimitExecution",
        "EnableLiveAggressiveExecution",
        "EnableLiveManagementExecution",
        "DefaultValue = false",
    ))
)

check(
    "live account is not unconditionally stopped",
    'Print(\n                    "CFIP DEMO cBot BLOCKED | live account detected' not in bot and
    'this build is demo-only' not in bot and
    "if (robot.Account.IsLive &&" in gate
)

check(
    "live mutation is account-mode fail-closed at both mutation owners",
    "liveAccount != robot.Account.IsLive" in market and
    "liveAccount != robot.Account.IsLive" in pending and
    "CFIP LIVE" in market and
    "CFIP LIVE" in pending
)

check(
    "multi-scenario capacity is bounded by both cBot and Indicator",
    "EffectiveConcurrentScenarioLimit" in bot and
    "Math.Min(" in bot and
    "MaximumOpenPositions" in settings and
    "MaxValue = 10" in bot
)

check(
    "ScenarioId remains independently selectable for execution",
    "SelectScenariosForExecution()" in registry and
    "candidate.FutureOrderReady" in registry and
    "(current &&" not in registry
)

check(
    "realtime provider and current quote handoff remain intact",
    "RefreshReadOnlyProvider(" in provider and
    "BuildScenarioBatch(" in provider and
    "Account.IsLive && executionEnabled" in bot
)

check(
    "tiny opportunity magnitude is enforced on current and future paths",
    "OpportunityMagnitudeRule.IsMeaningful(" in actionability and
    "OpportunityMagnitudeRule.IsMeaningful(" in future and
    "class OpportunityMagnitudeRule" in magnitude
)

check(
    "range-market RR floor was strengthened",
    "input.Tp1RR < 2.00" in range_rule
)

check(
    "popup direction is reconciled against canonical chart direction",
    "canonicalSnapshot.AuthoritativeDirection" in alerts and
    "_reaction.Direction" in alerts and
    "_decision.Direction" in alerts
)

check(
    "audio transport is fully wired and blocked alerts stay silent",
    "_alertDeliveryQueue.Enqueue(" in alert_engine and
    "ProcessQueuedAlertDelivery();" in alert_processor or
    "Notifications.PlaySound(" in alert_processor
)

check(
    "cBot account mode is part of shared panel truth",
    "ExecutionAccountMode" in snapshot and
    "ExecutionAccountMode =" in publisher and
    "ExecutionAccountMode" in reader
)

check(
    "operator continuity docs contain this phase",
    "REALTIME LIVE EXECUTION + SIGNAL TRUTH UNIFICATION" in roadmap and
    "REALTIME LIVE EXECUTION + SIGNAL TRUTH UNIFICATION" in continuation and
    "PR #240" in continuation and
    "Realtime Live Execution + Signal Truth Unification" in phase
)

check(
    "dedicated audit is part of Source/Architecture CI",
    "python tools/audit_phase_realtime_live_signal_unification_2026_10_03.py" in workflow
)

if errors:
    print("=" * 72)
    print("REALTIME LIVE SIGNAL UNIFICATION AUDIT: FAIL")
    for e in errors:
        print("- " + e)
    sys.exit(1)

print("=" * 72)
print("REALTIME LIVE SIGNAL UNIFICATION AUDIT: PASS")
