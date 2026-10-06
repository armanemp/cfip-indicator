#!/usr/bin/env python3
"""CFIP aggressive-flow, no-runtime-timeout and XAU risk-sizing audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"
BOT = ROOT / "src" / "CFIP.cBot"
errors = []

def read(path):
    if not path.exists():
        errors.append("missing: " + str(path.relative_to(ROOT)))
        return ""
    return path.read_text(encoding="utf-8")

flow = read(IND / "Analysis/Market/AggressiveFlowAnalyzer.cs")
flow_runtime = read(IND / "Runtime/AggressiveFlowRuntime.cs")
runtime = read(IND / "Runtime/Initialization/RuntimeInitialization.cs")
footer = read(IND / "UI/Panel/PanelFooterFactory.cs")
constants = read(IND / "UI/Panel/PanelConstants.cs")
volume = read(IND / "Trading/Risk/VolumeSizer.cs")
aggressive_volume = read(IND / "Trading/Risk/AggressiveVolumeSizer.cs")
margin = read(IND / "Trading/Risk/MarginSafetyCalculator.cs")
broker = read(BOT / "Execution/BrokerExecutionSafety.cs")
market = read(BOT / "Execution/DemoMarketExecutionCoordinator.cs")

checks = [
    ("flow has one owner", "class AggressiveFlowAnalyzer" in flow and "AggressiveFlowSnapshot" in flow),
    ("flow consumes cTrader ticks", "Ticks" in flow and "Tick" in flow and "Bid" in flow and "Ask" in flow),
    ("flow is explicitly not trade-size volume", "does not expose executed trade size" in flow and "never labels tick count as" in flow),
    ("flow has bounded memory without wall-clock expiry", "MaximumRetainedTicks = 4096" in flow and "DateTime" not in flow),
    ("runtime attaches the flow owner", "MarketData.GetTicks(Symbol.Name)" in flow_runtime and "AggressiveFlowAnalyzer" in flow_runtime),
    ("indicator startup has no 30-second data timeout", "ASYNC MARKET DATA INITIALIZATION TIMEOUT" not in runtime and "TotalSeconds >=
                    30" not in runtime),
    ("indicator detaches flow on shutdown", "StopAggressiveFlowRuntime();" in runtime),
    ("UI keeps depth and flow separate", "TryResolveCanonicalBuySellLiquidity(" in footer and "GetAggressiveFlowSnapshot()" in footer),
    ("UI labels flow as ticks, not units", '"ticks"' in footer and "AGG BUY TICKS" in footer and "AGG SELL TICKS" in footer),
    ("footer geometry reserves the four-row rail", "PanelFlowPressureRailHeight = 84" in constants and "PanelFooterMinHeight = 112" in constants),
    ("normal risk sizing uses broker symbol risk primitive", "VolumeForFixedRisk(" in volume and "NormalizeVolumeInUnits(" in volume),
    ("aggressive risk sizing uses broker symbol risk primitive", "VolumeForFixedRisk(" in aggressive_volume and "NormalizeVolumeInUnits(" in aggressive_volume),
    ("indicator margin guard uses broker estimate", "GetEstimatedMargin(" in margin and "NormalizeVolumeInUnits(" in margin),
    ("cBot final margin owner remains unique", "TryConstrainVolumeForMargin(" in broker and "GetEstimatedMargin(" in broker),
    ("cBot normalizes final broker volume", "NormalizeVolumeInUnits(" in broker),
    ("cBot market path applies the final constrained volume", "TryConstrainVolumeForMargin(" in market and "ExecuteMarketOrder(" in market),
]

for label, ok in checks:
    if not ok:
        errors.append(label)

if errors:
    print("CFIP AGGRESSIVE FLOW / XAU RISK AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CFIP AGGRESSIVE FLOW / XAU RISK AUDIT: PASS")
