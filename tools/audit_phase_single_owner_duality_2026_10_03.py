#!/usr/bin/env python3
"""Single-owner / no-duality audit for cBot lifecycle audio and chart signal presentation.

F2 extends the guard to prevent dead/no-op parallel-renderer helpers from becoming
shadow ownership points during future visual changes."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(path):
    return (ROOT / path).read_text(encoding="utf-8")

def require(ok, message):
    if not ok:
        raise SystemExit("FAIL: " + message)

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
audio = read("src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs")
line = read("src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs")
pending = read("src/CFIP.Indicator/UI/Chart/PendingOrderRenderer.cs")
parallel = read("src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs")
label = read("src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs")
label_coord = read("src/CFIP.Indicator/UI/Chart/PlanLabelRenderCoordinator.cs")
pending_label = pending
parallel_label = parallel
formatting = read("src/CFIP.Indicator/UI/Chart/PlanLabelFormatting.cs")
anchor = read("src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs")
prediction_label = read("src/CFIP.Indicator/UI/Chart/PredictionLabelsRenderer.cs")

require(
    audio.count("public void PlayStarted(") == 1 and
    bot.count("_audio.PlayStarted(") == 1 and
    "_audio.PlayLiveDisarmed(" not in bot and
    "public void PlayLiveDisarmed(" not in audio and
    "Notifications.PlaySound(" in audio and
    "Notifications.PlaySound(" not in bot,
    "cBot startup must have exactly one lifecycle audio owner and one startup cue",
)

require(
    "_audio.PlayStopped(" in bot and
    "_audio.PlayBlocked(" in bot and
    "_audio.PlayExecutionConfirmed(" in bot and
    "_audio.PlayExecutionRejected(" in bot,
    "cBot lifecycle/execution outcomes must remain behind the same audio owner",
)

require(
    "private const int CompactPlanLineLengthBars = 40;" in line and
    "return Bars.Count - 1;" in line and
    "GetPlanLineRightBar() -" in line and
    "if (FullWidthLevelLines)" not in line,
    "canonical signal line geometry must remain exactly 40 bars from the latest candle",
)

require(
    "LineStyle.Solid;" in line and
    "PlanLinePresentationRule.ResolveThickness(" in line and
    "line.ExtendToInfinity =\n                    false;" in line,
    "canonical signal lines must remain solid, fixed-thickness and finite",
)

require(
    "DrawPlanLine(" in pending and
    "Chart.DrawTrendLine(" not in pending and
    "DrawPlanLine(" in parallel and
    "Chart.DrawTrendLine(" not in parallel,
    "pending and parallel signal lines must delegate to the canonical line renderer",
)

require(
    "Chart.DrawText(" in label and
    "Chart.DrawRectangle(" not in label and
    "Chart.DrawIcon(" not in label and
    "HorizontalAlignment.Right" in label and
    "CompactPlanLabelFontSize = 11.0" in label and
    'Chart.RemoveObject(name + "_BOX")' in label and
    'Chart.RemoveObject(name + "_ANCHOR")' in label and
    "ResolveCanonicalPlanLineColor(" in line and
    "ResolveCanonicalPlanLineColor(" in label and
    "semanticColor" in label and
    "GetCompactPlanLabelAnchorTime(" in label,
    "canonical chart labels must be native ChartText, background-free and separated one bar left of the line",
)

require(
    "GetCompactPlanLabelAnchorTime(" in label_coord and
    "GetCompactPlanLabelAnchorTime(" in pending_label and
    "GetCompactPlanLabelAnchorTime(" in parallel_label and
    "GetCompactPlanLabelAnchorTime(" in anchor and
    "Chart.BarIndexToX(" in anchor and
    "targetX = lineX -" in anchor and
    "Chart.XToTime(" in anchor and
    "CompactPlanLabelGapBars = 1" in anchor,
    "all standard signal label paths must reuse one canonical left-of-line anchor with a deterministic one-bar gap",
)

prediction_line = read("src/CFIP.Indicator/UI/Chart/PredictionLineRenderer.cs")
prediction_label = read("src/CFIP.Indicator/UI/Chart/PredictionLabelsRenderer.cs")
require(
    "GetPredictionLineLeftBar(" in prediction_line and
    "GetPredictionLineLeftBar(" in prediction_label and
    "DrawPlanLabel(" in prediction_label,
    "prediction labels must consume the prediction line owner's exact left edge before applying the shared label gap",
)

require(
    "private string BuildCanonicalLevelLabel(" in formatting and
    formatting.count("private string BuildCanonicalLevelLabel(") == 1 and
    "ScenarioTimeframeTag(candidate)" in formatting and
    "ScenarioTimeframeTag(candidate)" not in formatting[formatting.find("private string ScenarioLabelPrefix("):formatting.find("private string BuildCanonicalLevelLabel(")],
    "plan and scenario labels must share one formatter and append the source timeframe exactly once",
)

require(
    "RenderPlanLabels(" in label_coord and
    "RenderCompactPlanLabel(" in label_coord and
    "RenderCompactPlanLabel(" in pending_label and
    "RenderCompactPlanLabel(" in parallel_label and
    "DrawPlanLabel(" in prediction_label,
    "all level-label surfaces must converge on the canonical label renderer",
)

require(
    "Chart.DrawText(" not in label_coord and
    "Chart.DrawText(" not in pending_label and
    "Chart.DrawText(" not in parallel_label,
    "secondary label coordinators must not create competing chart text objects",
)

require(
    "private string LaneLabel(" not in parallel and
    "private void HashSetCurrentOpportunityVisuals(" not in parallel and
    "HashSetCurrentOpportunityVisuals();" not in parallel,
    "parallel opportunity renderer must not retain dead/no-op presentation helpers",
)


require(
    "IsSameAsCanonicalPlan(" in parallel and
    "IsSameAsLivePlan(" not in parallel and
    "!_plan.IsLivePosition" not in parallel and
    "candidate.Tp4" in parallel and
    "SameCanonicalLevel(" in parallel,
    "parallel opportunity visuals must not duplicate the canonical plan before or during a live position",
)

print("CFIP SINGLE-OWNER / NO-DUALITY AUDIT: PASS")
print("cBot startup audio: one owner / one cue")
print("Signal/plan line geometry: one owner / 40 bars / Solid / 1px")
print("Pending + parallel lines: delegated to canonical line owner")
print("Chart labels: one renderer / native ChartText / exact price / no rectangle / tiny anchor marker")
print("Label formatting + source timeframe: one canonical formatter")
