#!/usr/bin/env python3
"""Audit the local cTrader Indicator/cBot lifecycle boundary."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(rel: str) -> str:
    path = ROOT / rel
    if not path.exists():
        raise SystemExit(f"missing {rel}")
    return path.read_text(encoding="utf-8")

def check(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
binding = read("src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs")
indicator = read("src/CFIP.Indicator/Indicator/CFIPIndicator.cs")
indicator_project = read("src/CFIP.Indicator/CFIP.Indicator.csproj")
root_build_props = read("Directory.Build.props")
cbot_project = read("src/CFIP.cBot/CFIP.cBot.csproj")
identity = read("src/CFIP.Contracts/IndicatorIdentity.cs")
cbot_identity = read("src/CFIP.Contracts/CbotIdentity.cs")

check(
    "ChartIndicators.Add(" not in bot,
    "cBot must not auto-add/resolve the custom Indicator during lifecycle refresh",
)
check(
    "_indicatorAutoAttachAttempted" not in bot,
    "obsolete cBot auto-attach state must be removed",
)
check(
    "ChartIndicators.IndicatorAdded += OnChartIndicatorAdded" in bot and
    "ChartIndicators.IndicatorRemoved += OnChartIndicatorRemoved" in bot and
    "ChartIndicators.IndicatorModified += OnChartIndicatorModified" in bot,
    "cBot must keep event-driven chart Indicator lifecycle observation",
)
check(
    "CFIP ANALYSIS BIND | unresolved" in bot and
    "ATTACH LOCAL INDICATOR" in bot,
    "missing Indicator must fail closed with an explicit local-attachment action",
)
check(
    "CFIP SMART INDICATOR NOT ATTACHED TO THIS CHART" in binding,
    "binding must own the precise missing-Indicator diagnostic",
)
check(
    "ChartIndicators.Custom" in binding and
    "candidate.Type.Name" in binding and
    "IndicatorIdentity.TypeName" in binding,
    "cBot must bind only to an already attached canonical Indicator instance",
)
check(
    'TypeName = "CFIPIndicator"' in identity and
    'DisplayName = "CFIP Smart Execution Bot"' in cbot_identity and
    'TypeName = "CFIPExecutionBot"' in cbot_identity,
    "shared stable type identities are required",
)
check(
    '"CFIP Smart Indicator"' in indicator and
    "[Indicator(" in indicator and
    "AccessRights = AccessRights.None" in indicator,
    "Indicator host identity and restricted access rights must remain stable",
)
check(
    "<AssemblyName>CFIPIndicator</AssemblyName>" in indicator_project and
    "<AlgoName>CFIP Smart Indicator</AlgoName>" in indicator_project and
    "<Deterministic>true</Deterministic>" in root_build_props,
    "Indicator build identity must remain stable and deterministic",
)
check(
    "<AssemblyName>CFIPExecutionBot</AssemblyName>" in cbot_project and
    "<AlgoName>CFIP Smart Execution Bot</AlgoName>" in cbot_project and
    "<Deterministic>true</Deterministic>" in root_build_props,
    "cBot build identity must remain stable and deterministic",
)
check(
    'DefaultTimeFrame = "M5"' in bot and
    "execTimeframe=M15" in bot and
    "Bars.TimeFrame != TimeFrame.Minute15" not in bot,
    "cBot launch timeframe remains host-only while M15 stays the execution clock",
)

print("CBOT local/cloud lifecycle audit PASS")