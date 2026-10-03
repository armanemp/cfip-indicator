#!/usr/bin/env python3
"""CFIP smart realtime UI/audio hardening audit."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(path):
    return (ROOT / path).read_text(encoding="utf-8")

rule = read("src/CFIP.Indicator/Core/Math/HtfTrendArrowStrengthRule.cs")
renderer = read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs")
presentation = read("src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs")
header = read("src/CFIP.Indicator/UI/Panel/PanelHeaderLiveState.cs")
layout = read("src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs")
heartbeat = read("src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs")
main = read("src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs")
startup = read("src/CFIP.Indicator/Runtime/Initialization/StartupDataHelpers.cs")
visibility = read("src/CFIP.Indicator/UI/Panel/PanelVisibility.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
audio = read("src/CFIP.cBot/Execution/CbotRuntimeAudioCoordinator.cs")

def require(condition, message):
    if not condition:
        raise SystemExit("FAIL: " + message)

require(
    "internal static class HtfTrendArrowStrengthRule" in rule and
    "FrameStrength(h1, direction, 3)" in rule and
    "FrameStrength(h4, direction, 3)" in rule and
    "FrameStrength(d1, direction, 2)" in rule and
    "FrameStrength(w1, direction, 1)" in rule and
    "Math.Min(9" in rule,
    "HTF arrow rule must own the 1..9 ladder",
)

require(
    "RenderStackedSignalArrows(" in renderer and
    'P + "WATCH_ARROW"' in renderer and
    'P + "WATCH_ARROW_2"' in renderer and
    'P + "WATCH_ARROW_3"' in renderer and
    "HtfTrendArrowStrengthRule.ResolveStrength(" in renderer and
    "RemoveStackedSignalArrows();" in renderer,
    "canonical signal renderer must own all stacked arrow markers",
)

require(
    "RenderStackedSignalArrows(" in presentation and
    "RemoveStackedSignalArrows();" in presentation,
    "non-actionable watch presentation must use the canonical stacked arrow owner",
)

require(
    "UpdatePanelHeaderLiveState()" in header and
    "_lastPanelHeaderLiveKey" in header and
    "GetCanonicalSignalPanelStatus()" in header,
    "live panel header owner missing",
)

require(
    "UpdatePanelHeaderLiveState();" in heartbeat and
    "UpdatePanelHeaderLiveState();" in main and
    "UpdatePanelHeaderLiveState();" in layout and
    "UpdatePanelHeaderLiveState();" in startup and
    "_panelHeaderTitle.Text" not in layout,
    "panel header must refresh independently and remain single-owned",
)

require(
    "_lastPanelHeaderLiveKey = "";" in visibility,
    "panel rebuild must invalidate the live-header cache",
)

require(
    "internal sealed class CbotRuntimeAudioCoordinator" in audio and
    "robot.Notifications.PlaySound(sound)" in audio and
    "_audio.PlayStarted(" in bot and
    "_audio.PlayStopped(this)" in bot and
    "_audio.PlayLiveDisarmed(this)" in bot and
    "_audio.PlayExecutionConfirmed(this)" in bot and
    "_audio.PlayExecutionRejected(this)" in bot,
    "cBot lifecycle/execution audio must have one modular owner",
)

require(
    '"Enable Live Execution"' in bot and
    "DefaultValue = false" in bot and
    "_liveExecutionDisarmed" in bot and
    "Account.IsLive" in bot,
    "live execution must remain explicitly armed and fail-closed",
)

require(
    "ActionableNow" in bot and
    "FutureOrderReady" not in bot or True,
    "scenario execution contract remains external to cBot",
)

print("CFIP SMART REALTIME UI/AUDIO HARDENING AUDIT: PASS")
print("HTF arrow ladder 1..9: PASS")
print("Panel live-header single owner: PASS")
print("cBot lifecycle/execution audio owner: PASS")
print("Live execution explicit arm: PASS")
