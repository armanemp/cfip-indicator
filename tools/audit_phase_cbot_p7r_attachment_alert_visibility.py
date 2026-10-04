#!/usr/bin/env python3
"""CFIP P7R attachment/alert/parallel-presentation regression audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing " + rel)
        return ""
    return path.read_text(encoding="utf-8")


def require(condition, message):
    if not condition:
        errors.append(message)


reader = read("src/CFIP.Indicator/Runtime/Cbot/CbotExecutionStateReader.cs")
binding = read("src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs")
alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationDecisionAlerts.cs")
signal_renderer = (
    read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs") +
    read("src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs")
)
watch_renderer = read("src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs")
labels = read("src/CFIP.Indicator/UI/Chart/PlanLabelFormatting.cs")
label_renderer = read("src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs")
parallel_renderer = read("src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs")
plan_lines = read("src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs")
workflow = read(".github/workflows/source-check.yml")

# Attachment truth: use stable cBot/Indicator type identity as well as the
# visible chart instance name, so a renamed instance does not become "not attached".
for source, display, label in (
    (reader, "CbotIdentity.DisplayName", "cBot reader"),
    (binding, "DisplayName", "Indicator binding"),
):
    require("candidate.Name" in source, label + " must inspect chart instance name")
    require(\
        ("candidate.Type != null" in source or "candidate.Type == null" in source),\
        label + " must inspect chart object type safely",\
    )
    require("candidate.Type.Name" in source, label + " must support type-name matching")
    require(display in source, label + " identity constant missing")

# Alerts are analysis/presentation truth; cBot liveness must not suppress them.
require(
    "ProcessParallelOpportunityAlerts(" in alerts,
    "parallel opportunity alert stage missing",
)
require(
    "if (_decision == null)" in alerts,
    "decision alert stage must remain guarded by decision availability",
)
require(
    "!_brokerStateReconciledThisCycle" not in alerts,
    "analysis alert delivery must not depend on broker reconciliation",
)
require(
    '"ACTION|SCENARIO|' in alerts and
    '"EARLY|SCENARIO|' in alerts,
    "scenario alerts need distinct actionable/watch event keys",
)
require(
    "CFIP #" in alerts and
    "displayNumber" in alerts and
    "candidate.ScenarioId" in alerts,
    "parallel alerts must expose deterministic scenario numbering/identity",
)

# Direction arrow follows the live chart bar and remains visible whenever a
# valid direction exists; no direction means removal.
require(
    "safeBar" in signal_renderer and
    "Bars.Count - 1" in signal_renderer,
    "signal arrow renderer must use a bounded canonical chart-bar index",
)
require(
    "snapshot.PlanActive" in signal_renderer and
    "decisionOwnsDirection" in signal_renderer,
    "active-plan direction remains owned by the canonical arrow renderer",
)
watch_gate = signal_renderer
require(
    "decisionOwnsDirection" in watch_gate and
    "snapshot.ActionableNow" in watch_gate and
    "snapshot.DecisionEntryAllowed" in watch_gate,
    "directional WATCH arrow must use the canonical actionable decision state",
)
require(
    "visualDirection == 0" in signal_renderer and
    "WATCH_ARROW" in signal_renderer and
    "RemoveObject" in signal_renderer,
    "directional arrow must hide when no direction is available",
)

require(
    "snapshot.PlanActive" in watch_gate and
    "snapshot.ActionableNow" in watch_gate,
    "directional WATCH layer must remain subordinate to the canonical active/actionable state",
)

# Multiple scenario presentation receives stable #N prefixes.
require(
    "int displayNumber = 0;" in parallel_renderer and
    "displayNumber++;" in parallel_renderer,
    "parallel renderer must number visible scenarios",
)
require(
    "int displayNumber = 0" in labels or "displayNumber = 0" in labels,
    "scenario label formatter must support display numbers",
)
require(
    "displayNumber.ToString" in labels,
    "scenario label formatter must emit numeric identity",
)
require(
    parallel_renderer.count("displayNumber),") >= 6,
    "entry/SL/TP scenario labels must all consume the same scenario number",
)

# Existing compact line semantics remain: solid, finite and 40-bar by default.
require(
    "CompactPlanLineLengthBars" in plan_lines and
    "40" in plan_lines and
    "return LineStyle.Solid" in plan_lines and
    "ExtendToInfinity" in plan_lines and
    "false" in plan_lines,
    "compact plan line contract regressed",
)

require(
    "ResolveCanonicalPlanLineColor(" in plan_lines and
    "ResolveCanonicalPlanLineColor(" in label_renderer and
    "semanticColor" in label_renderer,
    "compact level labels must use the exact canonical semantic line color",
)
require(
    "CompactPlanLabelFontSize = 11.0" in label_renderer and
    "label.IsBold" in label_renderer and
    "Chart.DrawText(" in label_renderer and
    "Chart.DrawRectangle(" not in label_renderer and
    "CompactPlanLabelGapBars = 1" in read("src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs") and
    "GetCompactPlanLabelAnchorTime(" in label_renderer and
    "HorizontalAlignment.Right" in label_renderer and
    "GetCompactPlanLabelAnchorTime(" in read("src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs") and
    "GetCompactPlanLabelAnchorTime(" in read("src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs"),
    "compact level labels must retain the canonical text-only presentation with one-bar left clearance",
)

require(
    "python tools/audit_phase_cbot_p7r_attachment_alert_visibility.py" in workflow,
    "P7R audit must be accumulated in Source/Architecture CI",
)

if errors:
    print("CBOT-P7R ATTACHMENT/ALERT/VISIBILITY AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P7R ATTACHMENT/ALERT/VISIBILITY AUDIT: PASS")
print("cBot/Indicator type-aware attachment matching: PASS")
print("broker-independent analysis alerts: PASS")
print("parallel scenario numbering: PASS")
print("live directional arrow semantics: PASS")
print("40-bar solid-line contract preserved: PASS")
