#!/usr/bin/env python3
"""CFIP cBot boundary audit for the bounded demo-market migration phase."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
INDICATOR = ROOT / "src" / "CFIP.Indicator"
CONTRACTS = ROOT / "src" / "CFIP.Contracts"
CBOT = ROOT / "src" / "CFIP.cBot"
SOLUTION = ROOT / "CFIP.Indicator.sln"

MUTATIONS = (
    "ExecuteMarketOrder(",
    "ExecuteMarketRangeOrder(",
    "PlaceStopOrder(",
    "PlaceLimitOrder(",
    "ModifyPosition(",
    "ModifyPendingOrder(",
    "ClosePosition(",
    "CancelPendingOrder(",
    "ModifyStopLossPrice(",
    "ModifyTakeProfitPrice(",
    "ModifyTakeProfitPips(",
    "ModifyTakeProfit(",
)

ALLOWED_CBOT_MUTATION_OWNER = "Execution/DemoMarketExecutionCoordinator.cs"

errors = []

for path, label in ((CONTRACTS, "Contracts"), (CBOT, "cBot")):
    if not path.exists():
        errors.append(f"missing {label} project directory")

if "CFIP.Contracts" not in SOLUTION.read_text(encoding="utf-8"):
    errors.append("solution does not register CFIP.Contracts")
if "CFIP.cBot" not in SOLUTION.read_text(encoding="utf-8"):
    errors.append("solution does not register CFIP.cBot")

for path in CONTRACTS.rglob("*.cs"):
    source = path.read_text(encoding="utf-8")
    if "cAlgo" in source or "cTrader.Automate" in source:
        errors.append(f"platform dependency in Contracts: {path.relative_to(ROOT)}")

cbot_project = CBOT / "CFIP.cBot.csproj"
project_text = cbot_project.read_text(encoding="utf-8") if cbot_project.exists() else ""
if "CFIP.Indicator.csproj" in project_text:
    errors.append("cBot must not reference the Indicator project in the demo migration phase")

mutation_hits = []
for path in CBOT.rglob("*.cs"):
    source = path.read_text(encoding="utf-8")
    for token in MUTATIONS:
        if re.search(r"" + re.escape(token[:-1]) + r"s*(", source):
            mutation_hits.append((path.relative_to(CBOT).as_posix(), token))

if not any(path == ALLOWED_CBOT_MUTATION_OWNER for path, _ in mutation_hits):
    errors.append("cBot demo market mutation owner is missing")
for path, token in mutation_hits:
    if path != ALLOWED_CBOT_MUTATION_OWNER:
        errors.append(f"cBot broker mutation escaped single owner: {path}::{token}")

bot = (CBOT / "CFIPExecutionBot.cs").read_text(encoding="utf-8")
if '"CFIP Smart Execution Bot"' not in bot:
    errors.append("stable cBot display name is missing")
if 'DefaultTimeFrame = "M5"' not in bot:
    errors.append("M5 default host timeframe is missing")
if 'DefaultValue = false)]' not in bot or "EnableDemoMarketExecution" not in bot:
    errors.append("demo execution must default DISARMED")
if "Account.IsLive" not in bot:
    errors.append("demo-only live-account guard is missing")
if "CfipIndicatorChartBinding.TryFind(" not in bot:
    errors.append("exact chart Indicator binding is missing")
if "CfipDeviceSignalTransport.TryRead(" not in bot:
    errors.append("canonical device signal transport is missing")
if "_shadow.Observe(" not in bot:
    errors.append("shadow broker-safety validation is missing")

indicator = (INDICATOR / "Indicator" / "CFIPIndicator.cs").read_text(encoding="utf-8")
if "CS0612" not in indicator:
    errors.append("documented SDK obsolete warning suppression is missing")

if errors:
    print("CBOT DEMO MARKET BOUNDARY AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT DEMO MARKET BOUNDARY AUDIT: PASS")
print("cBot broker mutation owner: 1")
print(f"Allowed owner: {ALLOWED_CBOT_MUTATION_OWNER}")
print("Demo live-account guard: PASS")
print("Indicator project reference: 0")
