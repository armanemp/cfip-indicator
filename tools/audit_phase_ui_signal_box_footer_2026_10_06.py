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
    ("chart arrow renderer owns the new box", "UpdateSignalArrowBox(" in arrows),
    ("legacy WATCH_ARROW chart objects are only cleanup, never draw targets", "DrawIcon(" not in arrows),
    ("no obsolete candle M1 trigger circle remains", "M1_TRIGGER" not in signal_renderer and "ChartIconType.Circle" not in signal_renderer),
    ("box state is owned by the arrow renderer", all(x in arrows for x in ("_signalArrowBox", "_signalArrowBoxArrows", "_signalArrowBoxStack")) and "_signalArrowBox" not in state),
    ("box is bottom-right aligned", "HorizontalAlignment.Right" in arrows and "VerticalAlignment.Bottom" in arrows),
    ("box is compact square", "Width = 66" in arrows and "Height = 66" in arrows),
    ("box is non-interactive", "IsHitTestVisible = false" in arrows),
    ("footer state is owned by PanelFooterFactory", all(x in footer_factory for x in (
        "_panelFlowPressureRail",
        "_panelBuyPressureRow",
        "_panelBuyPressureTrack",
        "_panelSellPressureTrack",
        "_panelBuyPressureFill",
        "_panelSellPressureFill",
        "_panelBuyPressureLabel",
        "_panelAggBuyFlowRow",
        "_panelAggBuyFlowTrack",
        "_panelAggBuyFlowFill",
        "_panelAggSellFlowFill",
        "_panelAggBuyFlowLabel"
    )) and "_panelDataStatus" not in footer_factory),
    ("BUY/SELL volumes share one physical bar per source", "CreateCombinedFlowPressureRow(" in footer_factory and
     '"DOM"' in footer_factory and
     '"FLOW TICKS"' in footer_factory and
     "Orientation = Orientation.Horizontal" in footer_factory and
     "segments.AddChild(buyFill)" in footer_factory and
     "segments.AddChild(sellFill)" in footer_factory and
     "ApplyCombinedFlowBar(" in footer_factory),

    ("pressure bars use canonical signal colors", "BuyArrowColor" in footer_factory and "SellArrowColor" in footer_factory),
    ("pressure calculation is realtime DOM plus bounded aggressive-flow tick proxy",
     "TryResolveCanonicalBuySellLiquidity(" in footer_factory and
     "_marketDepth.BidEntries" in footer_factory and
     "_marketDepth.AskEntries" in footer_factory and
     "GetAggressiveFlowSnapshot()" in footer_factory and
     "FLOW BUY TICKS" in footer_factory and
     "FLOW SELL TICKS" in footer_factory),
    ("realtime flow rows use the canonical bounded rolling tick window",
     "GetAggressiveFlowSnapshot()" in footer_factory and
     "flow.BuyTicks" in footer_factory and
     "flow.SellTicks" in footer_factory),
    ("each combined pressure row and track spans the full panel content width", "contentWidth" in footer_factory and
     "SetCombinedFlowRowWidth(" in footer_factory and
     "_panelBuyPressureRow" in footer_factory and
     "_panelAggBuyFlowRow" in footer_factory and
     "track.Width = width" in footer_factory and
     "row.Width = width" in footer_factory and
     "label.Width = width" in footer_factory and
     "Width = 1" not in footer_factory[footer_factory.index("private StackPanel CreateFlowPressureRow"):footer_factory.index("private void UpdatePanelFlowPressureRail")] and
     "buyFill.Width = buyWidth" in footer_factory and
     "sellFill.Width = sellWidth" in footer_factory and
     "track.Width" in footer_factory),
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
