#!/usr/bin/env python3
"""Static gate for the no-trade cTrader CBOT-Preflight kit."""
from pathlib import Path
import re
import sys
ROOT = Path(__file__).resolve().parents[1]
BOT = ROOT / "preflight" / "CFIPPreflightBot.cs"
PROBE = ROOT / "preflight" / "CFIPPreflightProbeIndicator.cs"
MUTATIONS = ("ExecuteMarketOrder","ExecuteMarketRangeOrder","PlaceStopOrder","PlaceLimitOrder","ModifyPosition","ModifyPendingOrder","ClosePosition","CancelPendingOrder","ModifyStopLossPrice","ModifyTakeProfitPrice","ModifyTakeProfitPips","ModifyTakeProfit")
errors = []
for path in (BOT, PROBE):
    if not path.exists(): errors.append(f"missing preflight source: {path.relative_to(ROOT)}")
bot = BOT.read_text(encoding="utf-8") if BOT.exists() else ""
probe = PROBE.read_text(encoding="utf-8") if PROBE.exists() else ""
for api in MUTATIONS:
    if re.search(r"\b" + re.escape(api) + r"\s*\(", bot): errors.append(f"preflight cBot contains forbidden broker mutation API: {api}")
    if re.search(r"\b" + re.escape(api) + r"\s*\(", probe): errors.append(f"preflight probe contains forbidden broker mutation API: {api}")
for source_name, source in (("CFIPPreflightBot.cs", bot), ("CFIPPreflightProbeIndicator.cs", probe)):
    for token in ("System.Reflection","GetType(","Invoke(","ChartObjects","Chart.Draw","File.","LocalStorage","HttpClient","WebSocket","static "):
        if token in source: errors.append(f"{source_name}: forbidden transport/mechanism token: {token}")
for token in ("Indicators.GetIndicator<CFIPPreflightProbeIndicator>()","Indicators.GetIndicator<CFIPIndicator>(","_probe.Probe.LastValue","EnableAutoTrading = false","EnableAutomaticOrders = false","EnableAggressiveAutoEntry = false","AutoProtectBrokerPositions = false","CFIP PREFLIGHT RESULT"):
    if token not in bot: errors.append(f"CFIPPreflightBot.cs missing required token: {token}")
for token in ("[Output(\"Probe\")]","public int Revision","public string Scope","public DateTime LastCalculatedUtc","public DateTime FirstCalculatedUtc"):
    if token not in probe: errors.append(f"CFIPPreflightProbeIndicator.cs missing required token: {token}")
print("CBOT-PREFLIGHT STATIC GATE")
print("=" * 72)
print(f"Bot source:   {BOT.relative_to(ROOT)}")
print(f"Probe source: {PROBE.relative_to(ROOT)}")
print(f"Failures:     {len(errors)}")
if errors:
    for error in errors: print(f"- {error}")
    sys.exit(1)
print("PASS — no-trade, non-reflection, non-scraping capability kit.")