#!/usr/bin/env python3
"""Regression audit for the 2026-10-07 arrow/footer/cBot cutover."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(rel: str) -> str:
    p = ROOT / rel
    if not p.exists():
        errors.append("missing: " + rel)
        return ""
    return p.read_text(encoding="utf-8")


def check(name: str, ok: bool) -> None:
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)


arrow = read("src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs")
panel_factory = read("src/CFIP.Indicator/UI/Panel/PanelFactory.cs")
footer = read("src/CFIP.Indicator/UI/Panel/PanelFooterFactory.cs")
constants = read("src/CFIP.Indicator/UI/Panel/PanelConstants.cs")
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
gate = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
settings = read("src/CFIP.cBot/Execution/CbotExecutionSettings.cs")
market = read("src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs")
pending = read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs")
indicator = "\n".join(
    p.read_text(encoding="utf-8")
    for p in (ROOT / "src/CFIP.Indicator").rglob("*.cs")
)
workflow = read(".github/workflows/source-check.yml")

check(
    "footer uses real visible margin equal to the reserved gap",
    "PanelFooterActionGap = 8" in constants and
    "Margin =" in footer and
    "PanelFooterActionGap" in footer,
)

check(
    "arrow overlay owns its own creation and z-order lifecycle",
    "EnsureSignalArrowBox();" in arrow and
    "Chart.RemoveControl(_signalArrowBox);" in arrow and
    "Chart.AddControl(_signalArrowBox);" in arrow,
)

check(
    "arrow direction follows the canonical snapshot direction with MTF fallback",
    "snapshot.AuthoritativeDirection != 0" in arrow and
    "snapshot.MtfTrendDirection" in arrow.split("int direction", 1)[1].split("if (direction == 0)", 1)[0],
)

check(
    "arrow glyph remains the single thicker presentation owner",
    'FontFamily = "Segoe UI Symbol"' in arrow and
    "FontSize = 28" in arrow and
    "FontWeight = FontWeight.ExtraBold" in arrow and
    "LineHeight = 32" in arrow and
    "new System.Collections.Generic.List<TextBlock>(3)" in arrow,
)

check(
    "cBot demo execution is armed by default while live execution remains independently armed",
    'Enable Automatic Trading", Group = "Execution Policy", DefaultValue = true' in bot and
    'Enable Automatic Orders", Group = "Execution Policy", DefaultValue = true' in bot and
    '"Enable Demo Market Execution"' in bot and
    "DefaultValue = true" in bot[bot.find('"Enable Demo Market Execution"'):bot.find('"Enable Demo Market Execution"') + 180] and
    "EnableLiveMarketExecution" in bot and
    "DefaultValue = false" in bot[bot.find("Enable Live Market Execution"):bot.find("Enable Live Pending Stop Execution")],
)

check(
    "Indicator/cBot broker boundary is delegated to the canonical CBOT-0 gate",
    "CbotExecutionSettings" in bot and
    "DemoMarketExecutionCoordinator" in market and
    "DemoPendingOrderExecutionCoordinator" in pending,
)

check(
    "obsolete Indicator execution UI/reminder parameters are removed",
    "public bool EnableAutoTrading" not in read("src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs") and
    "public bool EnableAutomaticOrders" not in read("src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs") and
    "AutoTradingReminder" not in read("src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs") and
    "ShowTradeActionButtons" not in read("src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs") and
    "AlwaysShowSafetyButtons" not in read("src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs") and
    "ManagedActionsOnly" not in read("src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs") and
    not (ROOT / "src/CFIP.Indicator/Trading/Lifecycle/AutoTradingDisableReminder.cs").exists(),
)

check(
    "cBot market and pending coordinators remain the only broker mutation owners",
    "ExecuteMarketOrder(" in market and
    "PlaceStopOrder(" in pending and
    "PlaceLimitOrder(" in pending and
    "ExecuteMarketOrder(" not in indicator and
    "PlaceStopOrder(" not in indicator and
    "PlaceLimitOrder(" not in indicator,
)

check(
    "cBot market-hours gate uses the broker symbol schedule and no obsolete fixed UTC window",
    "robot.Symbol.MarketHours.IsOpened(nowUtc)" in gate and
    "SessionStartUtc" not in bot and
    "SessionEndUtc" not in bot and
    "SessionStartUtc" not in settings and
    "SessionEndUtc" not in settings and
    "IsInsideSession(" not in gate,
)

check(
    "execution settings remain cBot-owned",
    "CbotExecutionSettings.Create(" in bot and
    "CbotExecutionSettings" in settings and
    "ChartIndicator.Parameters" not in settings and
    "indicator.Parameters" not in settings,
)

check(
    "dead indicator execution remnants removed",
    not (ROOT / "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketRangeCalculator.cs").exists() and
    not (ROOT / "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketFillReconciliation.cs").exists() and
    not (ROOT / "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs").exists(),
)

check(
    "regression audit is part of Source/Architecture CI",
    "tools/audit_phase_cbot_arrow_footer_execution_2026_10_07.py" in workflow,
)

if errors:
    print("=" * 72)
    print("CBOT / ARROW / FOOTER CUTOVER AUDIT: FAIL")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("=" * 72)
print("CBOT / ARROW / FOOTER CUTOVER AUDIT: PASS")
