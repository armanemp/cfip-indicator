#!/usr/bin/env python3
"""CFIP cBot/Indicator broker-boundary audit.

The extraction track now allows broker mutation only in the explicitly migrated
cBot owner. Indicator mutation owners remain frozen for the not-yet-extracted
paths, while already-extracted paths must be physically absent from Indicator.
"""
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

errors = []

for path, label in ((CONTRACTS, "Contracts"), (CBOT, "cBot")):
    if not path.exists():
        errors.append(f"missing {label} project directory: {path.relative_to(ROOT)}")

if not (CONTRACTS / "CFIP.Contracts.csproj").exists():
    errors.append("Contracts project file is missing")
if not (CBOT / "CFIP.cBot.csproj").exists():
    errors.append("cBot project file is missing")
if "CFIP.Contracts" not in SOLUTION.read_text(encoding="utf-8"):
    errors.append("solution does not register CFIP.Contracts")
if "CFIP.cBot" not in SOLUTION.read_text(encoding="utf-8"):
    errors.append("solution does not register CFIP.cBot")

contracts_sources = list(CONTRACTS.rglob("*.cs"))
for path in contracts_sources:
    source = path.read_text(encoding="utf-8")
    if "cAlgo" in source or "cTrader.Automate" in source:
        errors.append(
            f"Contracts must remain platform-neutral: {path.relative_to(ROOT)}"
        )

cbot_sources = list(CBOT.rglob("*.cs"))
if not cbot_sources:
    errors.append("cBot production source tree is empty")

cbot_market_owner = CBOT / "Execution" / "MarketBrokerMutation.cs"
for path in cbot_sources:
    source = path.read_text(encoding="utf-8")
    for token in MUTATIONS:
        if token in source and path != cbot_market_owner:
            errors.append(
                f"cBot broker mutation escaped explicit owner: "
                f"{path.relative_to(ROOT)} contains {token}"
            )

if not cbot_market_owner.exists():
    errors.append("P4A cBot market mutation owner is missing")

indicator_sources = list(INDICATOR.rglob("*.cs"))
owner_hits = []
for path in indicator_sources:
    source = path.read_text(encoding="utf-8")
    for token in MUTATIONS:
        if re.search(r"\b" + re.escape(token[:-1]) + r"\s*\(", source):
            owner_hits.append(path.relative_to(INDICATOR).as_posix())

allowed = {
    "Trading/Execution/BrokerPendingOrderPlacement.cs",
    "Trading/Execution/BrokerLimitOrderPlacement.cs",
    "Trading/Execution/BrokerPendingOrderCancellation.cs",
    "Trading/Execution/BrokerPositionCloseMutation.cs",
    "Trading/Execution/BrokerStopLossMutation.cs",
    "Trading/Execution/BrokerTakeProfitMutation.cs",
}
for path in sorted(set(owner_hits)):
    if path not in allowed:
        errors.append(f"Indicator broker mutation escaped frozen owner set: {path}")

if errors:
    print("CBOT-P0 STRUCTURAL BOUNDARY AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P0 STRUCTURAL BOUNDARY AUDIT: PASS")
print(f"Contracts C# files: {len(contracts_sources)}")
print(f"cBot C# files:      {len(cbot_sources)}")
print("cBot market mutation ownership: explicit")
print("Indicator market mutation: 0")
print("Remaining Indicator mutation ownership: frozen")
