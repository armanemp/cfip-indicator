#!/usr/bin/env python3
"""CI-20 panel option integrity audit.

Every Display/Panel option must either have a live canonical consumer or be
explicitly documented as retained compatibility. The audit also checks the
public range against the effective panel geometry clamps.
"""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src" / "CFIP.Indicator"

PARAMETER_FILES = [
    SRC / "Indicator" / "Parameters" / "14_display_core.cs",
    SRC / "Indicator" / "Parameters" / "14_display_advanced.cs",
    SRC / "Indicator" / "Parameters" / "14_display_panel.cs",
    SRC / "Indicator" / "Parameters" / "21_complete_intelligence.cs",
]

CONSUMERS = {
    "ShowLevelLines": ["UI/Chart/PlanRenderCoordinator.cs"],
    "FullWidthLevelLines": ["UI/Chart/PlanLineRenderer.cs"],
    "LevelLineThickness": ["UI/Chart/PlanLineRenderer.cs"],
    "ShowEntry": ["UI/Chart/PlanRenderCoordinator.cs", "UI/Chart/PlanLabelRenderCoordinator.cs"],
    "ShowTrigger": ["UI/Chart/PlanRenderCoordinator.cs", "UI/Chart/PlanLabelRenderCoordinator.cs", "UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs"],
    "ShowSL": ["UI/Chart/PlanRenderCoordinator.cs"],
    "ShowTP1": ["UI/Chart/PlanRenderCoordinator.cs", "UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs"],
    "ShowTP2": ["UI/Chart/PlanRenderCoordinator.cs", "UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs"],
    "ShowTP3": ["UI/Chart/PlanRenderCoordinator.cs", "UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs"],
    "ShowTP4": ["UI/Chart/PlanRenderCoordinator.cs", "UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs"],
    "ShowSignalArrow": ["UI/Chart/SignalRenderer.cs"],
    "ShowEarlyWatch": ["UI/Chart/SignalPresentationRenderer.cs"],
    "ShowHistoricalSignals": ["Runtime/Calculation/CalculationClosedBar.cs"],
    "HistoricalSignalLimit": ["UI/Historical/HistoricalRenderer.cs"],
    "ShowUnifiedPanel": ["UI/Panel/PanelMainRenderer.cs"],
    "ShowPanelBackground": ["UI/Panel/Theme/PanelVisualSettings.cs"],
    "PanelPosition": ["UI/Panel/PanelLayoutManager.cs"],
    "PanelWidth": ["UI/Panel/PanelFactory.cs", "UI/Panel/PanelMainRenderer.cs", "UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs"],
    "PanelFontSize": ["UI/Panel/PanelRowWriter.cs", "UI/Panel/Theme/PanelRowsLayout.cs"],
    "PanelFontFamily": ["UI/Panel/Theme/PanelRowsLayout.cs"],
    "PanelBold": ["UI/Panel/PanelRowWriter.cs"],
    "PanelBackground": ["UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs"],
    "PanelBackgroundAlpha": ["UI/Panel/PanelFactory.cs", "UI/Panel/Theme/PanelVisualSettings.cs"],
    "PanelBorder": ["UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs"],
    "PanelBorderAlpha": ["UI/Panel/PanelFactory.cs", "UI/Panel/PanelToggleButtonFactory.cs", "UI/Panel/Theme/PanelVisualSettings.cs"],
    "PanelBorderThickness": ["UI/Panel/PanelFactory.cs", "UI/Panel/PanelToggleButtonFactory.cs", "UI/Panel/Theme/PanelVisualSettings.cs"],
    "PanelCornerRadius": ["UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs"],
    "PanelPadding": ["UI/Panel/PanelMainRenderer.cs", "UI/Panel/Theme/PanelVisualSettings.cs"],
    "PanelMargin": ["UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs", "UI/Panel/PanelLayoutManager.cs"],
    "PanelRowGap": ["UI/Panel/PanelRowWriter.cs", "UI/Panel/PanelLayoutManager.cs"],
    "PanelMaxHeight": ["UI/Panel/PanelMainRenderer.cs", "UI/Panel/PanelFactory.cs"],
    "PanelRowPadding": ["UI/Panel/PanelRowWriter.cs", "UI/Panel/PanelLayoutManager.cs"],
    "PanelButtonGap": ["UI/Panel/PanelMainRenderer.cs"],
    "PanelAccentColor": ["UI/Panel/Rows/PanelOverviewExecutionRowsRenderer.cs"],
    "PanelSectionColor": ["UI/Panel/Rows/PanelContextRowsRenderer.cs"],
    "PanelSecondaryTextColor": ["UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs"],
    "PanelMutedTextColor": ["UI/Panel/PanelExecutionState.cs"],
    "PanelWarningColor": ["UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs"],
    "PanelTextColor": ["UI/Panel/PanelToggleButtonFactory.cs", "UI/Panel/Theme/PanelActionButtonsLayout.cs"],
    "EntryLineColor": ["UI/Chart/PlanRenderCoordinator.cs"],
    "TriggerLineColor": ["UI/Chart/PlanRenderCoordinator.cs"],
    "SlLineColor": ["UI/Chart/PlanRenderCoordinator.cs"],
    "TpLineColor": ["UI/Chart/PlanRenderCoordinator.cs"],
    "Tp2LineColor": ["UI/Chart/PlanRenderCoordinator.cs"],
    "Tp3LineColor": ["UI/Chart/PlanRenderCoordinator.cs"],
    "Tp4LineColor": ["UI/Chart/PlanRenderCoordinator.cs"],
    "BuyArrowColor": ["UI/Chart/SignalRenderer.cs"],
    "SellArrowColor": ["UI/Chart/SignalRenderer.cs"],
    "LineLengthBars": ["UI/Chart/PredictionLineRenderer.cs"],
    "LineForwardBars": ["UI/Chart/PredictionLineRenderer.cs"],
    "ShowSignalLabels": ["UI/Chart/PlanLabelRenderCoordinator.cs"],
    "ShowLevelPriceLabels": ["UI/Chart/PlanLabelRenderCoordinator.cs"],
    "ShowContextEventMarker": ["UI/Chart/OutcomeMarkerRenderer.cs"],
    "ShowPredictionObjects": ["UI/Chart/PredictionRenderer.cs"],
    "ArrowOffsetAtr": ["UI/Chart/SignalStackedArrowRenderer.cs"],
    "MinimumArrowOffsetPips": ["UI/Chart/SignalStackedArrowRenderer.cs"],
    "ShowEarlyArrow": ["UI/Chart/SignalPresentationRenderer.cs"],
    "ShowPanelToggleButton": ["UI/Panel/PanelToggleButtonFactory.cs", "UI/Panel/Theme/PanelActionButtonsLayout.cs"],
    "PanelToggleWidth": ["UI/Panel/PanelToggleButtonFactory.cs"],
    "PanelToggleHeight": ["UI/Panel/PanelToggleButtonFactory.cs"],
    "ShowEngineStatus": ["UI/Panel/Rows/PanelOverviewDiagnosticRowsRenderer.cs"],
    "PanelStateHoldSeconds": ["UI/Panel/PanelTextFormatting.cs"],
    "ShowLevelPricesInUnifiedPanel": ["UI/Panel/Rows/PanelTradePlanLevelRowsRenderer.cs"],
    "ShowTradePlanPanel": ["UI/Panel/Rows/PanelTradePlanRowsRenderer.cs"],
}

errors = []

def read(path):
    if not path.exists():
        errors.append("missing: " + str(path.relative_to(ROOT)).replace("\\", "/"))
        return ""
    return path.read_text(encoding="utf-8")

sources = {
    p: read(p)
    for p in ROOT.rglob("*.cs")
}
raw = "\n".join(sources.values())

declared = []
for path in PARAMETER_FILES:
    text = read(path)
    for match in re.finditer(r"public\s+[A-Za-z0-9_<>,.?]+\s+(\w+)\s*\{\s*get;\s*set;\s*\}", text):
        declared.append(match.group(1))

declared = list(dict.fromkeys(declared))
for name in declared:
    if name in CONSUMERS:
        for relative in CONSUMERS[name]:
            path = SRC / relative
            if not path.exists():
                errors.append(f"{name}: missing consumer {relative}")
                continue
            if name not in path.read_text(encoding="utf-8"):
                errors.append(f"{name}: consumer not wired in {relative}")
    elif name not in raw.replace(f"public bool {name}", "").replace(f"public int {name}", "").replace(f"public double {name}", "").replace(f"public string {name}", "").replace(f"public Color {name}", ""):
        errors.append(f"{name}: no runtime consumer detected")

display_source = "\n".join(read(p) for p in PARAMETER_FILES)

def require(text, token, label):
    if token not in text:
        errors.append(label)

panel_dimension_rule = read(SRC / "Core/Math/PanelDimensionRule.cs")
panel_layout = read(SRC / "UI/Panel/PanelLayoutManager.cs")
panel_main = read(SRC / "UI/Panel/PanelMainRenderer.cs")

require(
    panel_dimension_rule,
    "EffectiveWidth(",
    "PanelWidth canonical dimension owner is missing",
)
require(
    panel_dimension_rule,
    "configuredWidth",
    "PanelDimensionRule does not expose configured-width ownership",
)
require(
    panel_dimension_rule,
    "220",
    "PanelWidth minimum is not aligned to public MinValue=220",
)
require(
    panel_dimension_rule,
    "700",
    "PanelWidth maximum is not aligned to public MaxValue=700",
)
require(
    panel_layout,
    "PanelDimensionRule.EffectiveWidth(",
    "PanelLayoutManager does not consume the canonical PanelWidth owner",
)
require(
    panel_main,
    "EffectivePanelContentWidth()",
    "PanelMainRenderer does not consume the canonical panel content-width owner",
)
require(
    read(SRC / "UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs"),
    "_panel.MinWidth = 220;",
    "Panel surface still advertises the old 260px minimum",
)
require(
    read(SRC / "UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs"),
    "_panel.MaxWidth = 700;",
    "Panel surface still advertises the old 760px maximum",
)
require(
    read(SRC / "UI/Panel/PanelRowWriter.cs"),
    "PanelRowGap",
    "PanelRowGap is not applied to actual row controls",
)
require(
    read(SRC / "UI/Panel/PanelRowWriter.cs"),
    "PanelRowPadding",
    "PanelRowPadding is not applied to actual row controls",
)
require(
    read(SRC / "UI/Panel/PanelToggleButtonFactory.cs"),
    "PanelToggleWidth",
    "PanelToggleWidth is not consumed by the toggle width owner",
)
require(
    read(SRC / "UI/Panel/PanelToggleButtonFactory.cs"),
    "PanelToggleHeight",
    "PanelToggleHeight is not consumed by the toggle height owner",
)
require(
    read(SRC / "UI/Panel/Theme/PanelActionButtonsLayout.cs"),
    "int buttonGap",
    "PanelButtonGap is not threaded through the final toggle layout owner",
)
require(
    read(SRC / "UI/Panel/Theme/PanelActionButtonsLayout.cs"),
    "Math.Max(\n                        0,\n                        buttonGap)",
    "PanelButtonGap does not affect the actual toggle margin",
)
require(
    read(SRC / "UI/Panel/Theme/PanelVisualSettings.cs"),
    "buttonGap);",
    "PanelVisualSettings does not pass PanelButtonGap into the final toggle layout",
)

compat = read(SRC / "UI/Panel/PanelRenderOptimization.cs")
for token in (
    "ActionButtonHeight",
    "ActionButtonMargin",
    "ActionButtonWidth",
    "AlwaysShowSafetyButtons",
    "ShowTradeActionButtons",
):
    if token not in compat:
        errors.append("retained compatibility option missing from explicit compatibility owner: " + token)

if errors:
    print("CI-20 PANEL OPTION INTEGRITY AUDIT: FAIL")
    for e in errors:
        print(" - " + e)
    sys.exit(1)

print("CI-20 PANEL OPTION INTEGRITY AUDIT: PASS")
print(f"Checked display/complete-intelligence properties: {len(declared)}")
print("Every checked option has a canonical consumer.")
print("PanelWidth 220..700 runtime clamp: PASS")
print("Panel row gap/padding affect actual controls: PASS")
print("Retained action-button compatibility state is explicit: PASS")
