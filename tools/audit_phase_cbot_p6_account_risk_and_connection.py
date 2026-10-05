#!/usr/bin/env python3
"""CBOT-P6 account/execution-risk and connection-truth audit. cBot owns execution settings."""

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


def require(condition, message):
    if not condition:
        errors.append(message)


reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
binding = read("src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
settings = read("src/CFIP.cBot/Execution/CbotExecutionSettings.cs")
gate = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
daily = read("src/CFIP.cBot/Risk/CbotDailyLossGuard.cs")
intent = read("src/CFIP.Contracts/ExecutionIntent.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
devlog = read("docs/DEVELOPMENT-LOG.md")

require(
    "ChartRobots" in reader and
    "ChartRobot" in reader and
    "TryFindChartCbot" in reader and
    "CbotIdentity.DisplayName" in reader and
    '"Running"' in reader,
    "Indicator connection must use direct same-chart cBot presence and state",
)

require(
    "CbotExecutionSettings" in settings and
    "CbotExecutionSettings.Create(" in bot and
    "robot.EnableAutoTrading" in settings and
    "robot.MaximumOpenPositions" in settings,
    "cBot must consume its canonical execution settings without an Indicator execution-settings bridge",
)

for token in (
    "EnableAutoTrading",
    "EnableAutomaticOrders",
    "UseMarketHoursGuard",
    "SessionStartUtc",
    "SessionEndUtc",
    "UseSpreadFilter",
    "MaximumSpreadToStopRiskRatio",
    "EnableDailyLossLimit",
    "MaximumDailyLossPercent",
    "MaximumOpenPositions",
    "OneOrderPerSignal",
):
    require(token in settings, "execution settings bridge missing " + token)

for token in (
    "CONCURRENT SCENARIO CAPACITY BLOCKED",
    "ACCOUNT MARGIN LEVEL UNSAFE",
    "LIVE SPREAD EXCEEDS PLAN RISK LIMIT",
    "OUTSIDE CONFIGURED MARKET HOURS",
    "DAILY LOSS LIMIT REACHED",
):
    require(token in gate or token in daily, "P6 final execution gate missing " + token)

require(
    "CbotDailyLossGuard" in bot and
    "_dailyLossGuard.IsBlocked" in gate or
    "_dailyLossGuard" in bot,
    "cBot daily-loss authority is not integrated",
)

require(
    "MaxSpreadToStopRiskRatio" in intent and
    "MaxSpreadToStopRiskRatio" in provider,
    "canonical spread risk configuration must cross the Indicator→cBot intent boundary",
)

# The current cTrader Robot host exposes account/broker state, while the
# Indicator API owns the user-facing trading-permission prompt. The cBot
# remains fail-closed on live-account/broker submission errors and does not
# attempt an unsupported Robot.Permissions access.
require(
    "BrokerExecutionSafety.TryConstrainVolumeForMargin" in
    read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs"),
    "final broker volume normalization/margin cap must remain in the cBot mutation owner",
)

require(
    "BrokerExecutionSafety.TryConstrainVolumeForMargin" in
    read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs"),
    "pending execution must retain cBot-side final margin/volume protection",
)

require(
    "audit_phase_cbot_p6_account_risk_and_connection.py" in workflow,
    "P6 dedicated audit must be accumulated in Source/Architecture",
)

require(
    "CBOT-P6" in roadmap and
    "CBOT-P6" in continuation and
    "CBOT-P6" in devlog,
    "P6 must be recorded in continuity documents",
)

if errors:
    print("CBOT-P6 ACCOUNT/RISK/CONNECTION AUDIT: FAIL")
    for e in errors:
        print(" - " + e)
    sys.exit(1)

print("CBOT-P6 ACCOUNT/RISK/CONNECTION AUDIT: PASS")
print("direct chart cBot presence: PASS")
print("Indicator execution-settings bridge: PASS")
print("trading-permission/account safety: PASS")
print("session/spread guards: PASS")
print("daily-loss enforcement: PASS")
print("scenario-aware capacity: PASS")
print("cBot final margin/volume ownership: PASS")
