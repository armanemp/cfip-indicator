#!/usr/bin/env python3
"""Static acceptance gate for the MTF-P1 primary-signal/panel separation phase."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"MTF-P1 audit failed: missing {relative}")
    return path.read_text(encoding="utf-8")

rule = read("src/CFIP.Indicator/Core/Math/PrimaryTimeframeSignalRule.cs")
builder = read("src/CFIP.Indicator/Analysis/Market/TimeframeScenarioBuilder.cs")
candidate = read("src/CFIP.Indicator/Core/Models/TradeOpportunityCandidate.cs")
selection = read("src/CFIP.Indicator/Core/Math/ParallelScenarioSelectionRule.cs")
labels = read("src/CFIP.Indicator/UI/Chart/PlanLabelFormatting.cs")
panel_factory = read("src/CFIP.Indicator/UI/Panel/PanelFactory.cs")
panel_visual = read("src/CFIP.Indicator/UI/Panel/Theme/PanelVisualSettings.cs")
panel_surface = read("src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs")
panel_params = read("src/CFIP.Indicator/Indicator/Parameters/14_display_core.cs")
workflow = read(".github/workflows/source-check.yml")

def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit("MTF-P1 audit failed: " + message)

policy = read("src/CFIP.Indicator/Core/Math/ExecutionTimeframePolicy.cs")

require(
    ("M15" in policy and "H1" in policy and
     "ExecutionTimeframePolicy.PrimaryExecution" in rule),
    "primary source rule must recognize M15/H1 through the canonical timeframe policy",
)
require(
    "m5Direction == sourceDirection" in rule and
    "m1Direction == sourceDirection" in rule,
    "M5 and M1 tuning must remain directional to the source signal",
)
require(
    "M15" in builder and
    "H1" in builder and
    "_m15Frame" in builder and
    "_h1Frame" in builder and
    "_m5Frame" in builder and
    "M1TriggerReady(" in builder and
    "PrimaryTimeframeSignalRule.Evaluate(" in builder,
    "builder must create primary M15/H1 candidates and tune them with M5/M1 evidence",
)
require(
    'public bool IsPrimaryTimeframeSignal;' in candidate and
    'public bool M5TuningAligned;' in candidate and
    'public bool M1TuningConfirmed;' in candidate,
    "candidate must retain explicit primary/tuning state",
)
require(
    "candidate.IsPrimaryTimeframeSignal" in selection and
    "LocationConfluenceScore" in selection,
    "primary candidates and source location evidence must participate in display ordering",
)
require(
    'candidate.IsPrimaryTimeframeSignal' in labels and
    '"PRIMARY"' in labels,
    "chart labels must identify primary timeframe signals",
)
require(
    "CreateQuickExecutionControls();" not in panel_factory and
    "_quickExecutionStack" not in panel_factory,
    "chart panel must not construct or add AUTO TRADE/AUTO ORDERS quick controls",
)
require(
    "PanelBottomClearance =" in read("src/CFIP.Indicator/UI/Panel/PanelConstants.cs") and
    "PanelBottomClearance" in panel_surface and
    "new Thickness(" in panel_surface,
    "bottom panel clearance must be an internal layout constant applied as bottom margin",
)
require(
    "python tools/audit_phase_mtf_primary_panel.py" in workflow,
    "MTF-P1 static audit must be accumulated in source-check workflow",
)

print("MTF-P1 audit PASS")
print("Primary signals: M15 + H1")
print("Tuning: closed M5 alignment + closed M1 confirmation")
print("Panel: execution quick buttons removed; bottom clearance applied")
