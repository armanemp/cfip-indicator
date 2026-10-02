#!/usr/bin/env python3
"""Focused audit for the demo-only Market / Market-Range execution bridge."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
BOT = ROOT / "src/CFIP.cBot" / "CFIPExecutionBot.cs"
COORD = ROOT / "src/CFIP.cBot" / "Execution" / "DemoMarketExecutionCoordinator.cs"
INDICATOR = ROOT / "src/CFIP.Indicator" / "Indicator" / "CFIPIndicator.cs"

errors = []
bot = BOT.read_text(encoding="utf-8")
coord = COORD.read_text(encoding="utf-8")
indicator = INDICATOR.read_text(encoding="utf-8")

for token in (
    '"CFIP Smart Execution Bot"',
    'DefaultTimeFrame = "M5"',
    "EnableDemoMarketExecution",
    "Account.IsLive",
    "CfipIndicatorChartBinding.TryFind(",
    "CfipDeviceSignalTransport.TryRead(",
    "_shadow.Observe(",
):
    if token not in bot:
        errors.append("missing cBot token: " + token)

if 'DefaultValue = false)]' not in bot:
    errors.append("demo execution must default to false")

if "ExecuteMarketOrder(" not in coord:
    errors.append("demo market coordinator has no Market mutation")

if "ExecuteMarketRangeOrder(" not in coord:
    errors.append("demo market coordinator has no Market-Range mutation")

if "CFIP DEMO" not in coord:
    errors.append("demo coordinator must use explicit demo comment")

if "CS0612" not in indicator:
    errors.append("Indicator SDK warning suppression is missing")

if errors:
    print("CBOT DEMO LIVE MARKET AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT DEMO LIVE MARKET AUDIT: PASS")
print("Market / Market-Range mutation owner: DemoMarketExecutionCoordinator")
print("Demo-only guard: PASS")
print("Default execution arm: OFF")
