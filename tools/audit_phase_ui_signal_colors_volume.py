#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"

def read(rel):
    return (IND / rel).read_text(encoding="utf-8")

policy = read("UI/Panel/SignalPresentationColorPolicy.cs")
stacked = read("UI/Chart/SignalStackedArrowRenderer.cs")
timeframe = read("UI/Panel/PanelTimeframePresentationState.cs")
signal_state = read("UI/Panel/PanelSignalState.cs")
context = read("UI/Panel/Rows/PanelContextRowsRenderer.cs")
formatting = read("UI/Panel/PanelTextFormatting.cs")
params = read("Indicator/Parameters/14_display_core.cs")

checks = [
    ("canonical signal color policy exists", "SignalPresentationColorPolicy" in policy),
    ("policy owns strong/confirmed/caution BUY/SELL colors", all(x in policy for x in (
        "StrongBuyArrowColor", "ConfirmedBuyArrowColor", "CautionBuyArrowColor",
        "StrongSellArrowColor", "ConfirmedSellArrowColor", "CautionSellArrowColor",
    ))),
    ("stacked arrows consume canonical color policy", "SignalPresentationColorPolicy.Resolve(" in stacked),
    ("MTF lamp state consumes canonical color policy", "SignalPresentationColorPolicy.Resolve(" in timeframe),
    ("panel direction text consumes canonical color policy", "SignalPresentationColorPolicy.Resolve(" in signal_state),
    ("volume row is rendered", "GetMarketTickVolumeBarsText()" in context),
    ("volume presentation uses tick-volume semantics", "TICK VOL" in formatting and "TickVolumes" in formatting),
    ("legacy duplicate BUY arrow parameter removed", "BuyArrowColor" not in params),
    ("legacy duplicate SELL arrow parameter removed", "SellArrowColor" not in params),
]

errors = [name for name, ok in checks if not ok]

for path in ROOT.rglob("*.cs"):
    text = path.read_text(encoding="utf-8")
    if "BuyArrowColor" in text or "SellArrowColor" in text:
        errors.append("legacy directional color token remains in " + str(path.relative_to(ROOT)).replace("\\", "/"))

if errors:
    print("UI SIGNAL COLOR + VOLUME BAR AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    raise SystemExit(1)

print("UI SIGNAL COLOR + VOLUME BAR AUDIT: PASS")
print("Directional chart/panel/lamp colors have one canonical owner.")
print("Visible panel market-volume bars use cTrader TickVolumes.")
