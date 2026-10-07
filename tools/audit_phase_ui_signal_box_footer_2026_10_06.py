#!/usr/bin/env python3
"""CFIP UI signal-box and buy/sell pressure footer single-owner audit."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src" / "CFIP.Indicator"
errors = []

def read(rel):
    path = SRC / rel
    if not path.exists():
        errors.append("missing: " + rel)
        return ""
    return path.read_text(encoding="utf-8")

arrows = read("UI/Chart/SignalStackedArrowRenderer.cs")
state = read("Indicator/State.cs")
factory = read("UI/Panel/PanelFactory.cs")
footer_factory = read("UI/Panel/PanelFooterFactory.cs")
footer = read("UI/Panel/PanelAlertMessageRenderer.cs")
signal_renderer = read("UI/Chart/SignalRenderer.cs")
layout = read("UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs")
constants = read("UI/Panel/PanelConstants.cs")
display = read("Indicator/Parameters/14_display_core.cs")

checks = [
    ("ShowSignalArrow parameter remains enabled by default", 'Parameter("Show Signal Arrow"' in display and "DefaultValue = true" in display),
    ("chart arrow renderer owns the canonical chart objects", "Chart.DrawIcon(" in arrows and "ChartIconType.UpArrow" in arrows and "ChartIconType.DownArrow" in arrows),
    ("legacy arrow names are cleanup-only", "WATCH_ARROW" in arrows and "RemoveStackedSignalArrows" in arrows),
    ("no obsolete candle M1 trigger circle remains", "M1_TRIGGER" not in signal_renderer and "ChartIconType.Circle" not in signal_renderer),
    ("arrow state is owned by the chart renderer", "CanonicalTrendArrowPrefix" in arrows and "_signalArrowBox" not in arrows and "_signalArrowBox" not in state),
    ("arrows are placed outside the candle by canonical renderer", "ResolveArrowBarIndex(" in arrows and "ResolveArrowAtr(" in arrows and "basePrice" in arrows),
    ("arrow strength is deterministic 1..3 objects", "arrowCount" in arrows and "for (int level = 0" in arrows),
    ("chart arrows are non-interactive", "IsInteractive = false" in arrows),
    ("footer state is owned by PanelFooterFactory", all(x in footer_factory for x in (
        "_panelFlowPressureRail",
        "_panelBuyPressureRow",
        "_panelSellPressureRow",
        "_panelBuyPressureTrack",
        "_panelSellPressureTrack",
        "_panelBuyPressureFill",
        "_panelSellPressureFill",
        "_panelBuyPressureLabel",
        "_panelSellPressureLabel"
    )) and "_panelDataStatus" not in footer_factory),
    ("footer contains exactly two stacked pressure rows", "CreateFlowPressureRow(" in footer_factory and
     'CreateFlowPressureRow(\n                    "BUY"' in footer_factory and
     'CreateFlowPressureRow(\n                    "SELL"' in footer_factory and
     "Orientation = Orientation.Vertical" in footer_factory),
    ("pressure bars use canonical signal colors", "BuyArrowColor" in footer_factory and "SellArrowColor" in footer_factory),
    ("pressure calculation is realtime-M15 and tick-volume based", "_m15Bars" in footer_factory and
     "int latestBar =\n                bars.Count - 1" in footer_factory and
     "bars.TickVolumes[i]" in footer_factory and
     "(close - low)" in footer_factory and
     "(1.0 - buyShare)" in footer_factory),
    ("pressure uses the latest three M15 candles including the active bar", "latestBar - 2" in footer_factory and
     "first" in footer_factory and
     "buyVolume +=" in footer_factory and
     "sellVolume +=" in footer_factory),
    ("each pressure row and track spans the full panel content width", "contentWidth" in footer_factory and
     "_panelBuyPressureRow.Width" in footer_factory and
     "_panelSellPressureRow.Width" in footer_factory and
     "_panelBuyPressureTrack.Width =\n                contentWidth" in footer_factory and
     "_panelSellPressureTrack.Width =\n                contentWidth" in footer_factory and
     "Width = 1" not in footer_factory[footer_factory.index("private StackPanel CreateFlowPressureRow"):footer_factory.index("private void UpdatePanelFlowPressureRail")] and
     "_panelBuyPressureFill.Width" in footer_factory and
     "_panelSellPressureFill.Width" in footer_factory),
    ("footer has no legacy M1/M5/H1 status strip", "_panelDataStatus" not in footer_factory and "M2" not in footer_factory),
    ("pressure rail is placed directly below the timeframe lamps", "_buttonStack.AddChild" in footer_factory and "_panelFlowPressureRail" in footer_factory),
    ("footer geometry reserves the pressure rail", "PanelFlowPressureRailHeight" in layout and "PanelFlowPressureRailHeight" in constants and "PanelFooterActionGap" in constants),
    ("pressure rail refreshes on the bounded live panel cadence", "UpdatePanelFlowPressureRail();" in read("UI/Panel/PanelContentRefresh.cs") and "PanelContentRefreshMilliseconds = 500" in read("UI/Panel/PanelContentRefresh.cs")),
    ("footer remains outside the ScrollViewer", "_panelStack.AddChild" in factory and "_buttonStack" in factory),
]

for label, ok in checks:
    if not ok:
        errors.append(label)

if len(re.findall(r'Parameter\("Show Signal Arrow"', display)) != 1:
    errors.append("Show Signal Arrow must have exactly one public declaration")

if errors:
    print("CFIP UI SIGNAL BOX / BUY-SELL PRESSURE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CFIP UI SIGNAL BOX / BUY-SELL PRESSURE AUDIT: PASS")
