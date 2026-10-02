#!/usr/bin/env python3
"""Static acceptance gate for CFIP naming, cBot launch metadata and MTF panel bias display."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]

def read(rel):
    p = ROOT / rel
    if not p.exists():
        raise SystemExit("CFIP launch/panel audit failed: missing " + rel)
    return p.read_text(encoding="utf-8")

def require(cond, msg):
    if not cond:
        raise SystemExit("CFIP launch/panel audit failed: " + msg)

indicator = read("src/CFIP.Indicator/Indicator/CFIPIndicator.cs")
cbot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
frame_rule = read("src/CFIP.Indicator/Core/Math/PanelFrameDirectionRule.cs")
panel_format = read("src/CFIP.Indicator/UI/Panel/PanelTextFormatting.cs")
panel_context = read("src/CFIP.Indicator/UI/Panel/Rows/PanelContextRowsRenderer.cs")
panel_main = read("src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_csproj = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
phase = read("docs/PHASE-INDICATOR-NAME-CBOT-LAUNCH-MTF-PANEL.md")

require(
    '"CFIP Smart Indicator"' in indicator and
    "[Indicator(" in indicator,
    "Indicator must expose the requested stable cTrader display name",
)
require(
    '"CFIP Smart Execution Bot"' in cbot and
    'DefaultTimeFrame = "M5"' in cbot and
    "[Robot(" in cbot,
    "cBot must expose stable launch name and M5 default host timeframe",
)
require(
    "PanelFrameDirectionRule.ResolveDisplayDirection" in panel_format and
    "PanelFrameDirectionRule.ResolveLabel" in panel_format,
    "panel frame text must use the shared directional-bias presentation rule",
)
require(
    "BULL BIAS" in frame_rule and
    "BEAR BIAS" in frame_rule and
    "NEUTRAL" in frame_rule,
    "panel rule must distinguish directional bias from true neutral",
)
require(
    "FrameDirection(_m15Frame)" in panel_context and
    "FrameDirection(_h1Frame)" in panel_context,
    "MTF context rows must use the unified display-direction path",
)
require(
    "QuickExecutionRowHeight" not in panel_main and
    "SyncQuickExecutionControls();" not in panel_main,
    "panel render path must not reserve obsolete quick-execution UI height",
)
require(
    "VerifyPanelFrameDirectionPresentation();" in runtime and
    "PanelFrameDirectionRule.cs" in runtime_csproj,
    "runtime contracts must cover the panel directional-bias rule",
)
require(
    "tools/audit_phase_indicator_name_cbot_launch_mtf_panel.py" in workflow and
    "tools/audit_phase_mtf_primary_provider_identity.py" in workflow,
    "new audit must accumulate on top of the existing MTF/provider audits",
)
parameter_count = sum(
    len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8")))
    for p in (ROOT / "src/CFIP.Indicator/Indicator/Parameters").glob("*.cs")
)
require(parameter_count == 568, f"public parameter contract changed: found {parameter_count}")

require(
    "No strategy or threshold" in phase and
    "BULL BIAS" in phase and
    "CFIP Smart Indicator" in phase and
    "CFIP Smart Execution Bot" in phase,
    "phase record must document the presentation/name scope",
)

print("CFIP naming / cBot launch / MTF panel bias audit PASS")
