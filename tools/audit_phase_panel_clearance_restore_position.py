#!/usr/bin/env python3
"""Static acceptance gate for panel bottom-clearance and hidden restore-button positioning."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]

def read(rel):
    p = ROOT / rel
    if not p.exists():
        raise SystemExit("Panel spacing audit failed: missing " + rel)
    return p.read_text(encoding="utf-8")

def require(cond, msg):
    if not cond:
        raise SystemExit("Panel spacing audit failed: " + msg)

constants = read("src/CFIP.Indicator/UI/Panel/PanelConstants.cs")
surface = read("src/CFIP.Indicator/UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs")
layout = read("src/CFIP.Indicator/UI/Panel/PanelLayoutManager.cs")
restore = read("src/CFIP.Indicator/UI/Panel/Theme/PanelRestoreButtonLayout.cs")
workflow = read(".github/workflows/source-check.yml")
phase = read("docs/PHASE-PANEL-CLEARANCE-RESTORE-POSITION.md")

require(
    "private const int PanelBottomClearance = 50;" in constants,
    "panel bottom clearance must be exactly 50px",
)

require(
    "PanelBottomClearance" in surface and
    "bottomMargin" in surface and
    "new Thickness(" in surface,
    "50px panel clearance must remain an internal bottom margin",
)

require(
    "private const int PanelRestoreBottomClearance = 50;" in constants,
    "hidden restore button bottom clearance must be 50px",
)

require(
    "PanelRestoreBottomClearance" in layout and
    "bottomPosition" in layout and
    "bottomMargin" in layout and
    "_panelRestoreButton.Margin" in layout,
    "bottom restore button must use the dedicated lifted bottom margin",
)

require(
    "SetPanelRestoreAlignment();" in restore and
    "_panelRestoreButton.IsVisible" in restore,
    "restore button layout must keep visibility/alignment synchronized",
)

require(
    "python tools/audit_phase_mtf_primary_panel.py" in workflow and
    "python tools/audit_phase_panel_clearance_restore_position.py" in workflow,
    "panel spacing audit must accumulate with the existing P1 audit",
)

parameter_count = sum(
    len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8")))
    for p in (ROOT / "src/CFIP.Indicator/Indicator/Parameters").glob("*.cs")
)
require(parameter_count == 547, f"public parameter contract changed: found {parameter_count}")

require(
    "No public parameter" in phase and
    "50px" in phase and
    "restore" in phase.lower(),
    "phase record must document non-change of public parameters and the new geometry",
)

print("Panel clearance / hidden restore-button audit PASS")
