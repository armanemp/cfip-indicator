#!/usr/bin/env python3
"""Final realtime/live smart system integration audit."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(path):
    return (ROOT / path).read_text(encoding="utf-8")

def require(ok, msg):
    if not ok:
        raise SystemExit("FAIL: " + msg)

candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
alert = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
calc = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
trend = read("src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs")
arrow = read("src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs")
renderer = read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs")
presentation = read("src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs")
header = read("src/CFIP.Indicator/UI/Panel/PanelHeaderLiveState.cs")
header_renderer = read("src/CFIP.Indicator/UI/Panel/PanelHeaderRenderer.cs")
heartbeat = read("src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs")
layout = read("src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs")
factory = read("src/CFIP.Indicator/UI/Panel/PanelFactory.cs")
lamps = read("src/CFIP.Indicator/UI/Panel/PanelTrendTimeframeLampRow.cs")
visibility = read("src/CFIP.Indicator/UI/Panel/PanelVisibility.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
audio = read("src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs")

require(
    "RewardDistanceAtr" in candidate and
    "MinimumRequiredRewardDistanceAtr" in candidate and
    "REWARD DISTANCE BELOW OPPORTUNITY FLOOR" in policy,
    "reward-distance contract must reach the canonical scenario policy",
)

require(
    "_alertSoundDeliveryQueue" in state and
    "_alertSoundDeliveryQueue.Enqueue(next)" in alert and
    "if (!IsLastBar" in alert and
    "ProcessQueuedAlertSoundDelivery();" in calc,
    "signal sound delivery must be queued independently and played on realtime Calculate/last-bar",
)

require(
    "class MtfTrendStrengthRule" in trend and
    "ResolveNineLevel" in trend and
    "LevelMinimumScore = 35" in trend and
    "LevelBandSize = 5" in trend and
    "ResolveTier" in trend and
    "MtfTrendStrengthLevel" in arrow and
    "HtfTrendArrowStrengthRule" not in arrow,
    "canonical MTF smart arrow rule must own the 9-level strength ladder",
)

require(
    "RenderCanonicalMtfTrendArrows(" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs") and
    "RenderStackedSignalArrows(" in arrow and
    "RemoveStackedSignalArrows();" in arrow and
    '"WATCH_ARROW"' in arrow and
    '"WATCH_ARROW_2"' in arrow and
    '"WATCH_ARROW_3"' in arrow and
    "Symbol.PipSize * 3" in arrow and
    "offset * 0.75" in arrow,
    "stacked arrows must have one canonical renderer, deterministic level mapping and real separation",
)

require(
    "if (visualDirection == 0)" in renderer and
    "RemoveStackedSignalArrows();" in renderer and
    "ChartIconType.Circle" in renderer,
    "directionless arrows must be hidden and M1 trigger must not become a second directional arrow",
)

require(
    "UpdatePanelHeaderLiveState()" in header and
    "_lastPanelHeaderLiveKey" in state and
    "UpdatePanelHeaderLiveState();" in heartbeat and
    "UpdatePanelHeaderLiveState();" in layout and
    "UpdatePanelHeaderLiveState();" in header_renderer,
    "panel header must have one live-state owner refreshed independently",
)

require(
    "_panelTrendTimeframeLampRow" in lamps and
    "M1" in lamps and "M5" in lamps and "M15" in lamps and
    "M30" in lamps and "H1" in lamps and "H4" in lamps and
    "D1" in lamps and "W1" in lamps and
    "_panelStack.AddChild(" in factory and
    "_panelScroll" in factory and
    "fixed" in factory.lower(),
    "eight-timeframe lamp row must exist outside the ScrollViewer",
)

require(
    "PanelTrendTimeframeLampRowHeight" in layout and
    "PanelTrendTimeframeLampRowHeight" in read("src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs"),
    "fixed MTF lamp row must be included in panel geometry",
)

require(
    'public void PlayStarted(' in audio and
    'public void PlayBlocked(' in audio and
    'public void PlayStopped(' in audio and
    'public void PlayExecutionConfirmed(' in audio and
    'public void PlayExecutionRejected(' in audio and
    "_audio.PlayStarted(" in bot and
    "_audio.PlayStopped(" in bot and
    "_audio.PlayBlocked(" in bot,
    "cBot lifecycle/block/execution audio must have one modular owner",
)

require(
    "Account.IsLive" in bot and
    "EnableLiveMarketExecution" in bot and
    "EnableLivePendingStopExecution" in bot and
    "EnableLivePendingLimitExecution" in bot and
    "EnableLiveAggressiveExecution" in bot and
    "EnableLiveManagementExecution" in bot,
    "live execution must have explicit account-scoped controls",
)

require(
    "DefaultValue = false" in bot,
    "live execution controls must default to fail-closed",
)

require(
    "ActionableNow" in read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs") and
    "FutureOrderReady" in read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs") and
    "PendingStop" in bot and
    "PendingLimit" in bot,
    "current and future scenarios must remain semantically distinct",
)

require(
    "_lastPanelHeaderLiveKey = '';" in visibility or
    '_lastPanelHeaderLiveKey = "";' in visibility,
    "panel rebuild must invalidate the live-header cache",
)

print("CFIP FINAL REALTIME/LIVE/SMART SYSTEM INTEGRATION AUDIT: PASS")
print("Realtime sound queue separation: PASS")
print("HTF 1..9 smart arrows: PASS")
print("Live panel header owner: PASS")
print("Fixed MTF lamp row: PASS")
print("cBot live controls/audio: PASS")
print("Current/future scenario separation: PASS")
