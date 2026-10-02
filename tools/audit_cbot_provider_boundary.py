#!/usr/bin/env python3
"""CBOT provider-boundary audit for the current Device-scope handoff architecture."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
INDICATOR = ROOT / "src" / "CFIP.Indicator"
CBOT = ROOT / "src" / "CFIP.cBot"
INDICATOR_CSPROJ = INDICATOR / "CFIP.Indicator.csproj"
CBOT_CSPROJ = CBOT / "CFIP.cBot.csproj"
CBOT_SOURCE = CBOT / "CFIPExecutionBot.cs"
CALC = INDICATOR / "Runtime" / "Calculation" / "CalculationCycle.cs"
STAGES = INDICATOR / "Runtime" / "Calculation" / "CalculationStageIsolation.cs"
SEED = INDICATOR / "Runtime" / "Calculation" / "CalculationStartupSeed.cs"
PROVIDER_DIR = INDICATOR / "Runtime" / "Provider"

errors = []

def read(path):
    if not path.exists():
        errors.append("missing: " + path.relative_to(ROOT).as_posix())
        return ""
    return path.read_text(encoding="utf-8")

provider_paths = sorted(PROVIDER_DIR.glob("CFIPReadOnlyProvider*.cs"))
provider = "\n".join(read(path) for path in provider_paths)
indicator_csproj = read(INDICATOR_CSPROJ)
cbot_csproj = read(CBOT_CSPROJ)
cbot = read(CBOT_SOURCE)
calc = read(CALC)
stages = read(STAGES)
seed = read(SEED)
binding = read(CBOT / "Binding" / "CfipIndicatorChartBinding.cs")
transport = read(CBOT / "Binding" / "CfipDeviceSignalTransport.cs")

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

check(
    "required provider partial owners exist",
    len(provider_paths) == 5 and all(path.exists() for path in provider_paths),
)

check(
    "Indicator references platform-neutral Contracts",
    "..\\CFIP.Contracts\\CFIP.Contracts.csproj" in indicator_csproj,
)

check(
    "cBot references Contracts but not Indicator project",
    "..\\CFIP.Contracts\\CFIP.Contracts.csproj" in cbot_csproj and
    "..\\CFIP.Indicator\\CFIP.Indicator.csproj" not in cbot_csproj,
)

check(
    "provider exposes canonical immutable SignalEnvelope read-only",
    "public SignalEnvelope LatestSignalEnvelope =>" in provider and
    "public long ProviderRevision =>" in provider and
    "public bool ProviderReady =>" in provider,
)

check(
    "provider exposes invisible heartbeat output",
    '[Output(' in provider and
    '"CFIP Provider Heartbeat"' in provider and
    'LineColor = "Transparent"' in provider,
)

check(
    "provider uses canonical identity/version/revision",
    "ContractVersion.Current" in provider and
    "ContractIdentity" in provider and
    "_cfipProviderRevision" in provider,
)

check(
    "provider maps execution intent without broker mutation",
    "BuildCanonicalExecutionIntent(" in provider and
    "ExecuteMarketOrder(" not in provider and
    "PlaceStopOrder(" not in provider and
    "PlaceLimitOrder(" not in provider and
    "ModifyPosition(" not in provider,
)

check(
    "provider is published after presentation in the calculation pipeline",
    '"PRESENTATION"' in stages and
    "RefreshReadOnlyProvider(" in stages and
    stages.index('"PRESENTATION"') < stages.index("RefreshReadOnlyProvider("),
)

check(
    "startup seed publishes provider snapshot",
    "RefreshReadOnlyProvider(" in seed,
)

check(
    "normal calculation updates provider heartbeat",
    "PublishProviderHeartbeatValue(index);" in calc,
)

check(
    "cBot binds to the exact visible chart Indicator",
    "CfipIndicatorChartBinding.TryFind(" in cbot and
    "CFIP Smart Indicator" in binding and
    "Indicators.GetIndicator<CFIPIndicator>(" not in cbot,
)

check(
    "cBot consumes Device-scope SignalEnvelope transport",
    "CfipDeviceSignalTransport.TryRead(" in cbot and
    "CfipDeviceSignalTransport.Reload(" in cbot and
    "SignalBusKey.ForInstance(" in transport and
    "SignalBusKey.ForScenarioBatch(" in transport and
    "SignalEnvelopeCodec.TryDeserialize(" in transport and
    "SignalScenarioBatchCodec.TryDeserialize(" in transport,
)

check(
    "cBot execution arm is explicit and defaults OFF",
    "EnableDemoMarketExecution" in cbot and
    "DefaultValue = false" in cbot and
    "Account.IsLive" in cbot,
)

check(
    "cBot main host is broker-mutation-free",
    "ExecuteMarketOrder(" not in cbot and
    "PlaceStopOrder(" not in cbot and
    "PlaceLimitOrder(" not in cbot and
    "ModifyPosition(" not in cbot and
    "ClosePosition(" not in cbot and
    "CancelPendingOrder(" not in cbot,
)

for forbidden in ("System.Reflection", "GetType(", "Invoke(", "ChartObjects", "Chart.Draw", "HttpClient", "WebSocket"):
    check(
        "provider has no unsupported mechanism: " + forbidden,
        forbidden not in provider,
    )

if errors:
    print("CBOT-P2 PROVIDER BOUNDARY SUMMARY")
    print("=" * 72)
    print(f"Failures: {len(errors)}")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CBOT-P2 PROVIDER BOUNDARY SUMMARY")
print("=" * 72)
print("Transport: Device LocalStorage + exact chart Indicator instance")
print("Execution in cBot host: demo-arm gated; mutation isolated to DemoMarketExecutionCoordinator")
print("CBOT-P2 STATIC PROVIDER GATE PASS")
