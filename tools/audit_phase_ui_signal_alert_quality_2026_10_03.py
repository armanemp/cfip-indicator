from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"

def read(rel: str) -> str:
    path = IND / rel
    if not path.exists():
        raise SystemExit("missing " + rel)
    return path.read_text(encoding="utf-8")

def check(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)

state = read("UI/Panel/PanelTimeframePresentationState.cs")
rule = read("UI/Panel/PanelTimeframePresentationRule.cs")
text_format = read("UI/Panel/PanelTextFormatting.cs")
lamp = read("UI/Panel/PanelTrendTimeframeLampRow.cs")
render_key = read("UI/Panel/PanelRenderOptimization.cs")
constants = read("UI/Panel/PanelConstants.cs")
alert_rail = read("UI/Panel/PanelAlertMessageRenderer.cs")
alert_engine = read("Trading/Alerts/AlertEngine.cs")
signal = read("UI/Chart/SignalPresentationRenderer.cs")
line_rule = read("Core/Math/PlanLinePresentationRule.cs")
line_parameter = read("Indicator/Parameters/14_display_core.cs")
labels = read("UI/Chart/PlanLabelAnchorCalculator.cs")
label_renderer = read("UI/Chart/PlanLabelRenderer.cs")
candidate = read("Analysis/Market/ParallelOpportunityCandidateBuilder.cs")
alerts_calc = read("Runtime/Calculation/CalculationDecisionAlerts.cs")
csproj = (IND / "CFIP.Indicator.csproj").read_text(encoding="utf-8")
indicator = read("Indicator/CFIPIndicator.cs")

check(
    "internal readonly struct PanelTimeframePresentationState" in state and
    "internal static class PanelTimeframePresentationRule" in rule and
    "PanelTimeframePresentationRule" in text_format and
    ".Resolve(" in text_format and
    "PanelTimeframePresentationRule" in lamp and
    ".Resolve(" in lamp and
    "PanelTimeframePresentationRule" in render_key and
    ".Resolve(" in render_key,
    "timeframe status must have one canonical presentation owner",
)

check(
    "frame.BullScore" in render_key and
    "frame.BearScore" in render_key and
    "frame.TrendBull" in render_key and
    "frame.TrendBear" in render_key and
    "frame.FvgBullQuality" in render_key and
    "frame.ObBullQuality" in render_key and
    "presentation.Strength" in render_key,
    "panel render key must invalidate when canonical timeframe status inputs change",
)

check(
    "PanelFooterMinHeight = 36" in constants and
    "PanelAlertMessageRowHeight = 18" in alert_rail and
    "rows - 1" in alert_rail and
    "lastVisibleRow" in alert_rail,
    "footer/alert rail must use the reduced geometry without phantom final-row spacing",
)

check(
    "identity.SignalId" not in alert_engine[alert_engine.find("private string BuildAlertEventDedupKey("):alert_engine.find("private bool IsRememberedAlertEvent(")] and
    "identity.ScenarioId" in alert_engine and
    "identity.CreatedClosedM5" in alert_engine and
    "identity.Direction" in alert_engine,
    "alert event identity must stay stable across provider plan/trace rebuilds",
)

check(
    "snapshot.ActionableNow" in signal and
    "IsStrongWatchSnapshot(snapshot)" in signal and
    "IsSignalOpportunityVisuallyMeaningful(snapshot)" in signal,
    "chart signal arrows must not expose raw weak direction previews",
)

check(
    "MaximumThickness = 3" in line_rule and
    "return MinimumThickness;" in line_rule and
    "Solid" in read("UI/Chart/PlanLineRenderer.cs") and
    "HorizontalAlignment = HorizontalAlignment.Left" in label_renderer and
    "lineLeft - offset" in labels,
    "chart level presentation must be canonical, one-pixel, solid and left-anchored",
)

check(
    "RewardDistanceAtr <" in candidate and
    "candidate.Tp1RR <" in candidate and
    "candidate.PresentationOnly" in candidate,
    "trade-facing opportunity presentation must require concrete reward quality",
)

check(
    "candidate.IsPrimaryTimeframeSignal &&" in alerts_calc and
    "!candidate.PresentationOnly" in alerts_calc,
    "presentation-only primary fallbacks must not become user-facing trade alerts",
)

check(
    '<AssemblyName>CFIPIndicator</AssemblyName>' in csproj and
    '<AlgoName>CFIP Smart Indicator</AlgoName>' in csproj and
    '[Indicator(' in indicator and
    'Cloud' not in csproj,
    "indicator source identity must remain stable and local/cloud transport-free",
)

print("UI / signal / alert quality hardening audit PASS")
