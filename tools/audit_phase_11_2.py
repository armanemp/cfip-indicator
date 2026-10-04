#!/usr/bin/env python3
"""Phase 11.2 contract audit: indicator identity, label separation, signal pipeline and execution freshness."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
HOST = ROOT / "src/CFIP.Indicator/Indicator/CFIPIndicator.cs"
STATE = ROOT / "src/CFIP.Indicator/Indicator/State.cs"
LINE = ROOT / "src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs"
LABEL = ROOT / "src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs"
PANEL_FACTORY = ROOT / "src/CFIP.Indicator/UI/Panel/PanelFactory.cs"
PANEL_LAYOUT = ROOT / "src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs"
PANEL_MAIN = ROOT / "src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs"
PANEL_PIPE = ROOT / "src/CFIP.Indicator/UI/Panel/Rows/PanelSignalPipelineRowsRenderer.cs"
PANEL_ROWS = ROOT / "src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs"
LAMP = ROOT / "src/CFIP.Indicator/UI/Panel/ProcessingHeartbeatLamp.cs"
AGG = ROOT / "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveBrokerExecution.cs"
AGG_FINAL = ROOT / "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs"
MARKET = ROOT / "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketSubmissionValidator.cs"
PENDING = ROOT / "src/CFIP.Indicator/Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs"
PENDING_FINAL = ROOT / "src/CFIP.Indicator/Trading/Pending/Placement/PendingSubmissionValidator.cs"

def require(path: Path, pattern: str, label: str) -> None:
    text = path.read_text(encoding="utf-8")
    if not re.search(pattern, text, re.MULTILINE | re.DOTALL):
        raise SystemExit(f"missing {label}")

host = HOST.read_text(encoding="utf-8")
if not (
    re.search(r'\[Indicator\(\s*"CFIP Smart Indicator"\s*,[\s\S]*?IsOverlay', host)
    or re.search(r"\[Indicator\(\s*\n\s*IsOverlay", host)
):
    raise SystemExit("indicator host must have a valid cTrader IndicatorAttribute registration")
if '[Indicator("CFIPIndicator"' in host:
    raise SystemExit("legacy string-based IndicatorAttribute registration remains")

state = STATE.read_text(encoding="utf-8")
if "_lastAutoPlanTriggerM1" in state:
    raise SystemExit("dead _lastAutoPlanTriggerM1 field remains")

require(
    LINE,
    r"return Bars\.Count - 1;",
    "latest-candle plan-line endpoint",
)
require(
    ROOT / "src/CFIP.Indicator/UI/Chart/PlanLabelRenderCoordinator.cs",
    r"GetCompactPlanLabelAnchorBar\(\)",
    "canonical compact label anchor owner",
)
require(
    LABEL,
    r"HorizontalAlignment\s*=\s*\n\s*HorizontalAlignment\.Right",
    "right-aligned level labels at the left-of-line anchor",
)
require(
    LABEL,
    r"double labelPrice\s*=\s*\n\s*NormalizePrice\(price\)",
    "exact-price level-label alignment",
)
require(LAMP, r'ForegroundColor\s*=\s*Color\.FromArgb', "processing lamp pulse color")
require(LAMP, r'_processingLampPulseIndex\s*=\s*\(_processingLampPulseIndex \+ 1\) % 6', "processing lamp pulse state")
require(PANEL_FACTORY, r"CreateProcessingHeartbeatLamp\(\)[\s\S]*?_panelHeaderStack\.AddChild", "lamp header ownership")
require(PANEL_LAYOUT, r"_panelHeaderTitle\.Width[\s\S]*?contentWidth[\s\S]*?-\s*36", "header room reserved for lamp")
require(PANEL_MAIN, r"UpdateProcessingHeartbeatLamp\(\)", "lamp refresh hook")
require(PANEL_PIPE, r"PIPELINE[\s\S]*?QUALITY[\s\S]*?PIPELINE REASON", "signal pipeline panel diagnostics")
require(PANEL_ROWS, r"RenderPanelSignalPipelineRows\(", "pipeline renderer wired to overview")
require(MARKET, r"CanRunAutomaticEntry\(\)[\s\S]*?RefreshLiveDecisionActionability\(\s*closedM5\s*\)[\s\S]*?PassesMarketSuitability\([\s\S]*?true\)", "final market runtime, actionability and suitability refresh")
if AGG.exists():
    raise SystemExit("legacy Indicator aggressive broker owner must remain removed after CBOT-P4A")

require(
    AGG_FINAL,
    r"RefreshLiveDecisionActionability\(\s*closedM5\s*\)[\s\S]*?PassesMarketSuitability\([\s\S]*?true",
    "aggressive final freshness and suitability guard",
)
require(
    PENDING,
    r"RefreshLiveDecisionActionability\(\s*closedM5\s*\)",
    "pending live decision refresh",
)
require(
    PENDING_FINAL,
    r"CanRunAutomaticEntry\(\)[\s\S]*?EnsureTradingPermission\(\)[\s\S]*?PassesMarketSuitability\([\s\S]*?true",
    "pending final runtime, permission and suitability gates",
)

print("Phase 11.2 contract audit OK")
