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
    "multi-scenario capacity is owned and bounded by the cBot",
    "EffectiveConcurrentScenarioLimit" in bot and
    "Math.Max(" in bot and
    "Max Concurrent Scenarios" in bot and
    'DefaultValue = 3' in bot and
    "maximumOpenPositions != 1" in settings
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
    "cBot signal handoff is not dependent on the next broker tick",
    "Timer.Start(" in bot and
    "TimeSpan.FromMilliseconds(100)" in bot and
    "protected override void OnTimer()" in bot and
    "ShouldProcessRealtimeTimerEnvelope(" in bot and
    "ReloadSignalStore(true)" in bot
)

check(
    "tiny opportunity magnitude is enforced on current and future paths",
    "OpportunityMagnitudeRule.IsMeaningful(" in actionability and
    "OpportunityMagnitudeRule.IsMeaningful(" in future and
    "class OpportunityMagnitudeRule" in magnitude
)

check(
    "range-market RR floor was strengthened",
    "input.Tp1RR < 2.25" in range_rule
)

check(
    "all timeframe analysis remains wired into the decision stack",
    "request.M1Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "request.M5Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "request.M15Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "request.M30Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "request.H1Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "request.H4Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "request.D1Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "request.W1Frame" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "context.W1" in read("src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs")
)

check(
    "M15 is tuning/reference while lower timeframes refine entry",
    "canonical multi-timeframe architecture" in read("src/CFIP.Indicator/Core/Math/ExecutionTimeframePolicy.cs") and
    "M15 is the canonical signal-tuning/reference layer" in read("src/CFIP.Indicator/Core/Math/ExecutionTimeframePolicy.cs") and
    "M5/M1 refine the live entry" in read("src/CFIP.Indicator/Core/Math/ExecutionTimeframePolicy.cs")
)

check(
    "stop geometry consumes M1 through W1 structural candidates",
    "M1_MICRO_STRUCTURE_STOP" in read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs") and
    '"M15"' in read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs") and
    '"W1"' in read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs") and
    'candidate.Timeframe == "M1"' in read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
)

check(
    "target geometry consumes M1 plus M5/M15/M30/H1/H4/D1/W1",
    "M1_MICRO_SWING_TARGET" in read("src/CFIP.Indicator/Planning/TradePlan/Sources/M1MicroTargetSource.cs") and
    "AddM1MicroTargetContext(" in read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs") and
    "HTF_FVG" in read("src/CFIP.Indicator/Planning/TradePlan/Sources/HtfTargetSource.cs") and
    '"W1"' in read("src/CFIP.Indicator/Planning/TradePlan/Sources/HtfTargetSource.cs")
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
    "all-timeframe MTF early prediction fusion is explicit",
    "class MtfEarlyPredictionFusionRule" in read("src/CFIP.Indicator/Core/Math/MtfEarlyPredictionFusionRule.cs") and
    "MtfEarlyPredictionFusionRule.Evaluate(" in read("src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs") and
    "_w1Frame" in read("src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs")
)

check(
    "nine-level HTF-strength arrow stack is presentation-only and bounded",
    "class MtfTrendStrengthRule" in read("src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs") and
    "MtfTrendStrengthLevel" in read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshot.cs") and
    "MtfTrendStrengthRule.Evaluate(" in read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs") and
    "((level - 1) % 3) + 1" in read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs") and
    "levels 1..3" not in read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs")
)

check(
    "all eight timeframe frames are analyzed on each closed-M5 canonical cycle",
    "_m1Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs") and
    "_m5Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs") and
    "_m15Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs") and
    "_m30Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs") and
    "_h1Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs") and
    "_h4Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs") and
    "_d1Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs") and
    "_w1Frame" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
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
