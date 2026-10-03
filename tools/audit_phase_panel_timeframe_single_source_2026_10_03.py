from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"

def read(rel: str) -> str:
    return (IND / rel).read_text(encoding="utf-8")

def check(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)

state = read("UI/Panel/PanelTimeframePresentationState.cs")
lamp = read("UI/Panel/PanelTrendTimeframeLampRow.cs")
text_format = read("UI/Panel/PanelTextFormatting.cs")
header = read("UI/Panel/PanelHeaderLiveState.cs")
calc = read("Runtime/Calculation/CalculationClosedBar.cs")
optimization = read("UI/Panel/PanelRenderOptimization.cs")

check(
    "ResolvePanelTimeframeState(" in state and
    "PanelFrameDirectionRule.ResolveDisplayDirection(" in state and
    "ResolveLabel(" in state and
    "NativeIndicatorsReady" in state,
    "panel timeframe state must have one canonical direction/readiness/label resolver",
)

check(
    "ResolvePanelTimeframeState(frame)" in lamp and
    "FrameDirection(frame)" not in lamp and
    "ResolveFrameTrendStrength(" not in lamp,
    "timeframe lamps must consume the canonical presentation state without local direction/strength logic",
)

check(
    "ResolvePanelTimeframeState(frame)" in text_format and
    "PanelFrameDirectionRule.ResolveDisplayDirection(" not in text_format,
    "timeframe text must consume the same canonical presentation state as the lamp",
)

check(
    "ResolvePanelTimeframeState(_m15Frame)" in header and
    "ResolvePanelTimeframeState(_h1Frame)" in header,
    "live header timeframe status must consume the canonical presentation state",
)

check(
    "_m15Frame =" in calc and
    "_h1Frame =" in calc and
    "AnalyzeFrameCached(" in calc,
    "canonical timeframe state must be sourced from the existing cached frame calculation path",
)

check(
    "FramePresentationKey(_m15Frame)" in optimization and
    "FramePresentationKey(_h1Frame)" in optimization,
    "panel render optimization must remain keyed by canonical timeframe frame state",
)

print("Panel timeframe single-source-of-truth audit PASS")
