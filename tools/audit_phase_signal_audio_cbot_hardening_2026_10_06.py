#!/usr/bin/env python3
"""2026-10-06 cBot/signal/color/audio hardening regression audit."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    p = ROOT / rel
    if not p.exists():
        errors.append("missing " + rel)
        return ""
    return p.read_text(encoding="utf-8")

def require(ok, message):
    if not ok:
        errors.append(message)

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
settings = read("src/CFIP.cBot/Execution/CbotExecutionSettings.cs")
old_settings = ROOT / "src/CFIP.cBot/Execution/CbotIndicatorExecutionSettings.cs"
environment = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
publisher = read("src/CFIP.cBot/Execution/CbotExecutionStatePublisher.cs")
panel = read("src/CFIP.Indicator/UI/Panel/PanelExecutionState.cs")
m_tf = read("src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs")
m_tf_builder = read("src/CFIP.Indicator/UI/Chart/MtfTrendStrengthSnapshotBuilder.cs")
arrows = read("src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs")
colors = read("src/CFIP.Indicator/UI/Chart/SignalPresentationColorRule.cs")
panel_tf = read("src/CFIP.Indicator/UI/Panel/PanelTimeframePresentationState.cs")
snapshot = read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs")
alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs")
reaction_rule = read("src/CFIP.Indicator/Core/Math/WatchReactionAlertRule.cs")
processor = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
alert_engine = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")

require(not old_settings.exists(), "legacy Indicator-owned cBot settings reader still exists")
require("class CbotExecutionSettings" in settings and "CbotExecutionSettings.Create(" in bot, "cBot execution policy owner is missing")
require("ChartIndicator.Parameters" not in settings and "indicator.Parameters" not in settings, "cBot execution settings must not read Indicator parameters")
require(
    "public bool EnableAutoTrading { get; set; }" in bot and
    "DefaultValue = true" in bot[bot.find("Enable Automatic Trading") - 20:bot.find("Enable Automatic Trading") + 160] and
    "Enable Live Market Execution" in bot and
    "DefaultValue = false" in bot[bot.find("Enable Live Market Execution"):bot.find("Enable Live Market Execution") + 180] and
    "Enable Live Aggressive Execution" in bot and
    "DefaultValue = false" in bot[bot.find("Enable Live Aggressive Execution"):bot.find("Enable Live Aggressive Execution") + 180],
    "cBot demo master arm may default ON, but live market/aggressive mutation must remain explicitly OFF",
)
require("Enable Demo Market Execution" in bot and "DefaultValue = true" in bot[bot.find("Enable Demo Market Execution"):bot.find("Enable Demo Market Execution") + 180], "demo market capability must default ON so only the explicit master arm blocks it")
require("CbotExecutionSettings" in environment and "CbotExecutionSettings" in publisher, "cBot consumers must use one execution-settings type")
require("EffectiveAutoTradingEnabled" in panel and "INDICATOR SETTING OFF" not in panel, "panel must not expose legacy Indicator execution state")
require("ResolveClosedPressureQuality(" in m_tf and "ResolveLivePressureQuality(" not in m_tf, "MTF visual strength must use closed-frame pressure, not the live quote")
require("frame.Bars.ClosePrices[frame.Index]" in m_tf, "closed-frame pressure source is missing")
require(
    "AuthoritativeDirection" in arrows and
    "RemoveStackedSignalArrows();" in arrows and
    "UpdateSignalArrowBox(" in arrows,
    "canonical arrow direction owner must remain explicit",
)
require("SignalPresentationColorRule.Resolve(" in arrows and "SignalPresentationColorRule.Resolve(" in panel_tf, "arrows and timeframe panel must share one color owner")
require("class SignalPresentationColorRule" in colors, "canonical signal palette owner missing")
require("bool reactionReady = false;" in snapshot, "live reaction must not become a user-facing trade signal while M5 is open")
require("Reaction is calculated from the currently forming M5 candle." in alerts and "never a user-facing signal" in alerts, "intrabar reaction must not emit a user-facing signal/alert")
require("bool reactionReady" in reaction_rule and "!reactionReady" in reaction_rule, "reaction alert eligibility must be explicitly trigger/readiness based")
require("new AlertDeliveryQueue(32)" in state, "sound delivery queue must retain bounded burst capacity")
require("if (!DeliverAlertSound(next))" in processor and "ALERT SOUND RETRY QUEUED" in processor, "failed sound playback must requeue the same canonical event")
require("Notifications.PlaySound(" not in alert_engine and processor.count("Notifications.PlaySound(") == 3, "sound playback must remain single-owned in AlertDeliveryProcessor")
require("SoundGroupKey" in processor and "_rememberedSignalSoundGroups" in processor, "semantic sound-group dedup must remain bounded and centralized")

if errors:
    print("SIGNAL/CBOT/AUDIO HARDENING AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("SIGNAL/CBOT/AUDIO HARDENING AUDIT: PASS")
print("cBot ownership: PASS")
print("stable MTF visual direction/strength: PASS")
print("shared signal color semantics: PASS")
print("closed-bar trade signal / intrabar reaction separation: PASS")
print("retryable sound delivery: PASS")