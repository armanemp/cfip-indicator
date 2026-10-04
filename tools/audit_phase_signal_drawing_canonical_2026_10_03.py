from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

line = (ROOT / "src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs").read_text(encoding="utf-8")
line_rule = (ROOT / "src/CFIP.Indicator/Core/Math/PlanLinePresentationRule.cs").read_text(encoding="utf-8")
labels = (ROOT / "src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs").read_text(encoding="utf-8")
anchor = (ROOT / "src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs").read_text(encoding="utf-8")
plan = (ROOT / "src/CFIP.Indicator/UI/Chart/PlanRenderCoordinator.cs").read_text(encoding="utf-8")
arrows = (ROOT / "src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs").read_text(encoding="utf-8")
clearer = (ROOT / "src/CFIP.Indicator/UI/Chart/PlanObjectClearer.cs").read_text(encoding="utf-8")
alert_marker = (ROOT / "src/CFIP.Indicator/UI/Chart/AlertSignalRenderer.cs").read_text(encoding="utf-8")

errors = []

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

check("plan lines are solid and canonical", "return LineStyle.Solid;" in line)
check("plan line geometry is fixed to 40 bars from latest candle", "CompactPlanLineLengthBars = 40" in line and "GetPlanLineRightBar()" in line and "GetPlanLineRightBar() -" in line and "if (FullWidthLevelLines)" not in line.split("private int GetPlanLineLeftBar", 1)[1])
check("plan thickness contract is one pixel", "return MinimumThickness;" in line_rule and "MinimumThickness = 1" in line_rule)
check("plan labels use canonical line-owned color", "Chart.DrawText(" in labels and
    "Chart.DrawRectangle(" not in labels and
    "ResolveCanonicalPlanLineColor(" in line and
    "ResolveCanonicalPlanLineColor(" in labels and
    "semanticColor" in labels)
check("plan labels use the canonical readable font, one-bar left clearance and follow the moving line anchor",
    "CompactPlanLabelFontSize = 11.0" in labels and
    "CompactPlanLabelGapBars = 1" in anchor and
    "GetCompactPlanLabelAnchorBar(" in anchor and
    "HorizontalAlignment.Right" in labels and
    "Chart.DrawText(" in labels and
    "canonicalLabelBar" in labels and
    "label.Time != expectedTime" in labels and
    "label.HorizontalAlignment" in labels and
    "label.VerticalAlignment" in labels and
    "GetCompactPlanLabelAnchorBar(" in labels)
check("labels share exact normalized price and canonical bar-index anchor", "NormalizePrice(price)" in labels and
    "GetPlanLineLeftBar" in anchor and
    "GetCompactPlanLabelAnchorBar(" in anchor and
    "canonicalLineLeftBar -\n                CompactPlanLabelGapBars" in anchor and
    "GetCompactPlanLabelAnchorBar(" in anchor)
check("active plan does not create a second arrow lifecycle", "RenderStackedSignalArrows(" not in plan and 'P + "ARROW"' not in plan)
check("legacy active arrow is cleaned", 'P + "ARROW"' in arrows and 'P + "ARROW"' in clearer)
check("directional arrows are explicit UpArrow/DownArrow", "ChartIconType.UpArrow" in arrows and "ChartIconType.DownArrow" in arrows)
check("directional marker lifecycle has one owner", "RenderCanonicalMtfTrendArrows(" in (ROOT / "src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs").read_text(encoding="utf-8") and "RenderStackedSignalArrows(" in arrows)
check("legacy alert mirror cannot create second signal marker", 'P + "ALERT_SIGNAL"' in alert_marker and "Chart.DrawIcon" not in alert_marker)

if errors:
    raise SystemExit("\n".join(errors))

check("canonical labels and line share the exact normalized Y price with centered text",
    "double labelPrice =\n                    NormalizePrice(price)" in labels and
    "label.Y =\n                    labelPrice" in labels and
    "label.VerticalAlignment =\n                    VerticalAlignment.Center" in labels and
    "line.Y1 =\n                    normalized" in line and
    "line.Y2 =\n                    normalized" in line
)

if errors:
    raise SystemExit("\n".join(errors))
