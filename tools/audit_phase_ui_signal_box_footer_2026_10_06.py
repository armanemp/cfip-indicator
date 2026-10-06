#!/usr/bin/env python3
"""CFIP UI signal-box and footer data-status single-owner audit."""

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
    ("footer state is owned by PanelFactory", all(x in factory for x in ("_panelDataStatusText", "_panelFooterActions")) and "_panelDataStatusText" not in state),
    ("footer has a dedicated data status control", "_panelDataStatusBars" in factory and "_panelDataStatusBarControls" in factory),
    ("footer separates action controls from data status", "_panelFooterActions" in factory),
    ("data status is based on native tick volume", "TickVolumes" in footer and "ResolveDataBarLevel" in footer),
    ("data status uses M1/M5/M15/H1/H4 only", all(x in footer for x in ("M1", "M5", "M15", "H1", "H4")) and "M2" not in footer),
    ("data status is placed in the fixed footer", "_buttonStack.AddChild" in factory and "_panelDataStatusText" in factory),
    ("footer geometry reserves the data-status row", "PanelDataStatusRowHeight" in layout and "PanelDataStatusRowHeight" in constants),
    ("footer remains outside the ScrollViewer", "_panelStack.AddChild" in factory and "_buttonStack" in factory),
]

for label, ok in checks:
    if not ok:
        errors.append(label)

if len(re.findall(r'Parameter\("Show Signal Arrow"', display)) != 1:
    errors.append("Show Signal Arrow must have exactly one public declaration")

if errors:
    print("CFIP UI SIGNAL BOX / FOOTER DATA STATUS AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CFIP UI SIGNAL BOX / FOOTER DATA STATUS AUDIT: PASS")
