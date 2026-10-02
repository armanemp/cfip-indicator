#!/usr/bin/env python3
"""CFIP cBot boundary audit for bounded demo Market / Market-Range / Aggressive migration."""
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

ALLOWED_CBOT_MUTATION_OWNERS = {
    "Execution/DemoMarketExecutionCoordinator.cs",
    "Execution/DemoPendingOrderExecutionCoordinator.cs",
}
errors = []

if not CONTRACTS.exists():
    errors.append("missing Contracts project")
if not CBOT.exists():
    errors.append("missing cBot project")
solution = SOLUTION.read_text(encoding="utf-8")
if "CFIP.Contracts" not in solution:
    errors.append("solution does not register CFIP.Contracts")
if "CFIP.cBot" not in solution:
    errors.append("solution does not register CFIP.cBot")

for path in CONTRACTS.rglob("*.cs"):
    source = path.read_text(encoding="utf-8")
    if "cAlgo" in source or "cTrader.Automate" in source:
        errors.append(
            f"platform dependency in Contracts: {path.relative_to(ROOT).as_posix()}"
        )

project = CBOT / "CFIP.cBot.csproj"
project_text = project.read_text(encoding="utf-8") if project.exists() else ""
if "CFIP.Indicator.csproj" in project_text:
    errors.append("cBot must not reference the Indicator project")

mutation_hits = []
for path in CBOT.rglob("*.cs"):
    source = path.read_text(encoding="utf-8")
    for token in MUTATIONS:
        if re.search(r"\b" + re.escape(token[:-1]) + r"\s*\(", source):
            mutation_hits.append(
                (path.relative_to(CBOT).as_posix(), token)
            )

for owner in ALLOWED_CBOT_MUTATION_OWNERS:
    if not any(path == owner for path, _ in mutation_hits):
        errors.append("cBot broker mutation owner is missing: " + owner)

for path, token in mutation_hits:
    if path not in ALLOWED_CBOT_MUTATION_OWNERS:
        errors.append(
            f"cBot broker mutation escaped single owner: {path}::{token}"
        )

bot = (CBOT / "CFIPExecutionBot.cs").read_text(encoding="utf-8")
required_bot_tokens = (
    '"CFIP Smart Execution Bot"',
    'DefaultTimeFrame = "M15"',
    "EnableDemoMarketExecution",
    "EnableDemoAggressiveExecution",
    "EnableDemoPendingStopExecution",
    "MaxExecutionMarginUsagePercent",
    "ExecutionMarginBufferPercent",
    "Account.IsLive",
    "CfipIndicatorChartBinding.TryFind(",
    "CfipDeviceSignalTransport.TryRead(",
    "_shadow.Observe(",
)
for token in required_bot_tokens:
    if token not in bot:
        errors.append("cBot missing required token: " + token)

indicator = (
    INDICATOR / "Indicator" / "CFIPIndicator.cs"
).read_text(encoding="utf-8")
if "CS0612" not in indicator:
    errors.append("CS0612 suppression is missing from the documented SDK naming boundary")

if errors:
    print("CBOT DEMO MARKET BOUNDARY AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT DEMO MARKET BOUNDARY AUDIT: PASS")
print("cBot broker mutation owners: 2")
print("Allowed owners: " + ", ".join(sorted(ALLOWED_CBOT_MUTATION_OWNERS)))
print("Demo live-account guard: PASS")
print("Indicator project reference: 0")
