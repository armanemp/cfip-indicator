#!/usr/bin/env python3
"""Live/realtime cBot handoff, attachment, audio and stagnant-market hardening gate."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(path):
    p = ROOT / path
    if not p.exists():
        errors.append("missing: " + path)
        return ""
    return p.read_text(encoding="utf-8")

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
gate = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
binding = read("src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs")
audio = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
parallel = read("src/CFIP.Indicator/Analysis/Market/ParallelOpportunityBuilder.cs")
future = read("src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs")
ranking = read("src/CFIP.Indicator/Analysis/Market/OpportunityIntelligenceRanking.cs")
candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
quality = read("src/CFIP.Indicator/Core/Math/TradeOpportunityQualityRule.cs")
registry = read("src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs")
policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
floor = read("src/CFIP.Indicator/Core/Math/RegimeAdaptiveRewardFloorRule.cs")
provider_plan = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
decision_alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs")
workflow = read(".github/workflows/source-check.yml")

check("100 ms signal-store handoff cadence",
      "SignalReloadIntervalMilliseconds = 100" in bot and
      "SignalReloadIntervalMilliseconds" in bot)

live_opportunity = read("src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs")

check("100 ms live opportunity refresh cadence",
      "LiveOpportunityRefreshIntervalMilliseconds = 100" in live_opportunity and
      "LiveOpportunityRefreshIntervalMilliseconds" in live_opportunity)

reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")

check("200 ms cBot state visibility cadence and case-insensitive discovery",
      "CbotStateReadIntervalMilliseconds = 200" in reader and
      "OrdinalIgnoreCase" in reader and
      "ChartRobots" in reader)

check("live-unarmed cBot stays attached and publishes blocked truth",
      "_liveExecutionDisarmed" in bot and
      "LIVE DISARMED" in bot and
      "Stop();" not in bot.split("if (_liveExecutionDisarmed)", 1)[1].split("else", 1)[0])

check("indicator execution switches no longer veto cBot broker execution",
      "AUTO TRADING DISABLED IN INDICATOR" not in gate and
      "AUTOMATIC ORDERS DISABLED IN INDICATOR" not in gate and
      "liveExecutionEnabled" in gate)

check("effective panel execution state is cBot-owned",
      "EffectiveAutoTradingEnabled =\n                    cbotMarketArmed" in publisher and
      "EffectiveAutomaticOrdersEnabled =\n                    cbotOrdersArmed" in publisher and
      "INDICATOR SETTING OFF" not in panel)

check("CFIP binding uses custom chart indicators plus stable type/name diagnostics",
      "ChartIndicators.Custom" in binding and
      "OrdinalIgnoreCase" in binding and
      "seen=" in binding and
      "TypeName" in binding)

check("cBot presence publishes the actual chart-instance identity",
      "robot.InstanceId ?? string.Empty" in publisher)

check("indicator can prove attachment from a fresh bound cBot presence",
      "HasFreshCbotPresenceForCurrentIndicator()" in reader and
      "CBOT CONNECTED • ATTACHED" in reader)

check("multiple matching cBots resolve by exact cBot InstanceId",
      "CbotInstanceId" in reader and
      "candidate.InstanceId" in reader)

calculation_cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
initialization = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")

check("sound delivery is restricted to indicator realtime Calculate/last-bar execution",
      "ProcessQueuedAlertSoundDelivery()" in audio and
      "Notifications.PlaySound" in audio and
      "IsLastBar" in audio and
      "ProcessQueuedAlertSoundDelivery();" in calculation_cycle and
      "ProcessQueuedAlertPresentation();" in initialization)

check("critical alert queue never evicts an existing critical event",
      "Keep already-buffered critical alerts intact" in read("src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs"))

check("adaptive stagnant-market reward floor is canonical and strengthened",
      "RegimeAdaptiveRewardFloorRule" in floor and
      "MarketRegimeIdentity.Compression" in floor and
      "Math.Max(baseFloor, 1.20)" in floor and
      "Math.Max(baseFloor, 1.00)" in floor and
      "Math.Max(baseFloor, 0.85)" in floor)

check("history + forecast are explicit ranking evidence",
      "ForecastAlignmentScore" in candidate and
      "HistoricalSupportScore" in candidate and
      "HistoricalCalibrationSamples" in candidate and
      "GetEmpiricalCalibrationSnapshot(" in ranking and
      "CalculateExecutionPriorityScore(" in ranking and
      "CalculateExecutionPriorityScore(" in quality and
      "ExecutionPriorityScore" in registry)

check("current and future opportunity builders consume the same floor",
      "RegimeAdaptiveRewardFloorRule.Resolve" in parallel and
      "RegimeAdaptiveRewardFloorRule.Resolve" in future and
      "RegimeAdaptiveRewardFloorRule.Resolve" in policy)

check("hardening audit is accumulated in Source/Architecture CI",
      "python tools/audit_phase_cbot_live_realtime_execution_hardening_2026_10_03.py" in workflow)

if errors:
    print("=" * 72)
    print("FAIL | CBOT LIVE REALTIME EXECUTION HARDENING")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("=" * 72)
print("CBOT LIVE REALTIME EXECUTION HARDENING: PASS")
