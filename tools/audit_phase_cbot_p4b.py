#!/usr/bin/env python3
"""CBOT-P4B + panel geometry hardening audit."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
INDICATOR = ROOT / "src" / "CFIP.Indicator"
CBOT = ROOT / "src" / "CFIP.cBot"
errors = []

main_renderer = (INDICATOR / "UI" / "Panel" / "PanelMainRenderer.cs").read_text(encoding="utf-8")
layout = (INDICATOR / "UI" / "Panel" / "PanelLayoutManager.cs").read_text(encoding="utf-8")
factory = (INDICATOR / "UI" / "Panel" / "PanelFactory.cs").read_text(encoding="utf-8")
visual = (INDICATOR / "UI" / "Panel" / "Theme" / "PanelVisualSettings.cs").read_text(encoding="utf-8")

if "ResolvePanelMaximumHeight(" not in main_renderer or "ResolvePanelMaximumHeight(" not in layout:
    errors.append("panel renderer must use the decoupled maximum-height resolver")
if "availableChartHeight" in main_renderer:
    errors.append("panel renderer must not size itself from the transient Chart.Height block")
if "_panelStack.Height" not in visual:
    errors.append("panel stack height must track final outer panel height")
if "CapturePanelGeometryBaseline()" not in factory or "_panelGeometryBaselineChartHeight" not in factory:
    errors.append("panel geometry baseline must be captured before Chart.AddControl")
if "Chart.AddControl(" not in factory:
    errors.append("panel must keep the supported chart-control API")

coord = (CBOT / "Execution" / "DemoMarketExecutionCoordinator.cs").read_text(encoding="utf-8")
bot = (CBOT / "CFIPExecutionBot.cs").read_text(encoding="utf-8")
contracts = (ROOT / "src" / "CFIP.Contracts" / "ContractEnums.cs").read_text(encoding="utf-8")

for token in ("ExecutionAction.Aggressive", "ExecuteMarketOrder(", "BrokerAction.SubmitAggressive"):
    if token not in coord:
        errors.append("cBot Aggressive mutation contract missing: " + token)
if "EnableDemoAggressiveExecution" not in bot:
    errors.append("cBot explicit demo Aggressive switch is missing")
if "Account.IsLive" not in bot:
    errors.append("cBot live-account guard is missing")
if "Aggressive = 2" not in contracts:
    errors.append("Contracts Aggressive execution action is missing")

agg_root = INDICATOR / "Trading" / "Execution" / "Aggressive"
for path in sorted(agg_root.rglob("*.cs")):
    code = path.read_text(encoding="utf-8")
    for token in ("ExecuteMarketOrder(", "ExecuteMarketRangeOrder(", "PlaceStopOrder(", "PlaceLimitOrder("):
        if token in code:
            errors.append("Indicator Aggressive broker mutation remains: " + path.relative_to(INDICATOR).as_posix() + "::" + token)

if errors:
    print("CBOT-P4B PANEL/AGGRESSIVE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P4B PANEL/AGGRESSIVE AUDIT: PASS")
print("Panel stack/final-height synchronization: PASS")
print("Panel Chart.Height decoupling: PASS")
print("Aggressive cBot mutation owner: PASS")
print("Indicator Aggressive direct broker mutation count: 0")