#!/usr/bin/env python3
"""CBOT-P2 static boundary audit: canonical read-only Indicator provider."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
INDICATOR = ROOT / "src" / "CFIP.Indicator"
CBOT = ROOT / "src" / "CFIP.cBot"
PROVIDER = INDICATOR / "Runtime" / "Provider" / "CFIPReadOnlyProvider.cs"
INDICATOR_CSPROJ = INDICATOR / "CFIP.Indicator.csproj"
CBOT_CSPROJ = CBOT / "CFIP.cBot.csproj"
CBOT_SOURCE = CBOT / "CFIPExecutionBot.cs"
CALC = INDICATOR / "Runtime" / "Calculation" / "CalculationCycle.cs"
STAGES = INDICATOR / "Runtime" / "Calculation" / "CalculationStageIsolation.cs"
SEED = INDICATOR / "Runtime" / "Calculation" / "CalculationStartupSeed.cs"
INTENT = INDICATOR / "Planning" / "Execution" / "ExecutionIntentBuilder.cs"

errors = []

def read(path):
    if not path.exists():
        errors.append("missing: " + path.relative_to(ROOT).as_posix())
        return ""
    return path.read_text(encoding="utf-8")

provider = read(PROVIDER)
indicator_csproj = read(INDICATOR_CSPROJ)
cbot_csproj = read(CBOT_CSPROJ)
cbot = read(CBOT_SOURCE)
calc = read(CALC)
stages = read(STAGES)
seed = read(SEED)
intent = read(INTENT)

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

check(
    "Indicator references platform-neutral Contracts",
    "..\\CFIP.Contracts\\CFIP.Contracts.csproj" in indicator_csproj
)

check(
    "cBot references Indicator and Contracts projects",
    "..\\CFIP.Indicator\\CFIP.Indicator.csproj" in cbot_csproj and
    "..\\CFIP.Contracts\\CFIP.Contracts.csproj" in cbot_csproj
)

check(
    "provider exposes canonical immutable SignalEnvelope read-only",
    "public SignalEnvelope LatestSignalEnvelope =>" in provider and
    "public long ProviderRevision =>" in provider and
    "public bool ProviderReady =>" in provider
)

check(
    "provider exposes invisible heartbeat output for deterministic lazy evaluation",
    '[Output(' in provider and
    '"CFIP Provider Heartbeat"' in provider and
    'LineColor = "Transparent"' in provider
)

check(
    "provider uses canonical identity/version/revision",
    "ContractVersion.Current" in provider and
    "ContractIdentity" in provider and
    "_cfipProviderRevision" in provider
)

check(
    "provider maps execution intent without broker mutation",
    "BuildCanonicalExecutionIntent(" in provider and
    "ExecutionAction.PendingStop" in provider and
    "ExecutionAction.PendingLimit" in provider and
    "ExecutionAction.Aggressive" in provider and
    "ExecuteMarketOrder(" not in provider and
    "PlaceStopOrder(" not in provider and
    "PlaceLimitOrder(" not in provider and
    "ModifyPosition(" not in provider
)

check(
    "execution intent builder captures canonical-provider source intent",
    "CaptureProviderExecutionIntent(intent);" in intent
)

check(
    "provider publishes only after the existing calculation/presentation pipeline",
    '"CBOT READ-ONLY PROVIDER"' in stages and
    "RefreshReadOnlyProvider(" in stages and
    stages.index('"PRESENTATION"') < stages.index('"CBOT READ-ONLY PROVIDER"')
)

check(
    "startup seed also publishes the provider snapshot",
    "RefreshReadOnlyProvider(" in seed
)

check(
    "heartbeat is updated from the normal Calculate lifecycle",
    "PublishProviderHeartbeatValue(index);" in calc
)

check(
    "cBot initializes CFIP through supported GetIndicator mechanism",
    "Indicators.GetIndicator<CFIPIndicator>(" in cbot
)

check(
    "cBot forces referenced indicator evaluation through output",
    "_indicator.ProviderHeartbeat.LastValue" in cbot
)

check(
    "cBot uses read-only provider only",
    "LatestSignalEnvelope" in cbot and
    "ExecuteMarketOrder(" not in cbot and
    "PlaceStopOrder(" not in cbot and
    "PlaceLimitOrder(" not in cbot and
    "ModifyPosition(" not in cbot and
    "ClosePosition(" not in cbot and
    "CancelPendingOrder(" not in cbot
)

check(
    "cBot instance is explicitly disarmed for execution",
    "EnableAutoTrading = false" in cbot and
    "EnableAutomaticOrders = false" in cbot and
    "EnableAggressiveAutoEntry = false" in cbot and
    "AutoProtectBrokerPositions = false" in cbot and
    "EnableLiveExitManagement = false" in cbot
)

for forbidden in ("System.Reflection", "GetType(", "Invoke(", "ChartObjects", "Chart.Draw", "File.", "LocalStorage", "HttpClient", "WebSocket"):
    check(
        f"no unsupported transport/mechanism token: {forbidden}",
        forbidden not in provider and forbidden not in cbot
    )

print("CBOT-P2 READ-ONLY INDICATOR PROVIDER SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("CBOT-P2 STATIC PROVIDER GATE PASS")
