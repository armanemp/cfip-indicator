#!/usr/bin/env python3
"""Cross-panel semantic and visual consistency audit."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"

def read(rel: str) -> str:
    return (IND / rel).read_text(encoding="utf-8")

def check(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)

signal_state = read("UI/Panel/PanelSignalState.cs")
canonical = read("UI/Panel/PanelCanonicalSignalStatus.cs")
header = read("UI/Panel/PanelHeaderLiveState.cs")
context = read("UI/Panel/Rows/PanelContextRowsRenderer.cs")
decision = read("UI/Panel/Rows/PanelDecisionRowsRenderer.cs")
wave = read("UI/Panel/Rows/PanelWaveTrendAndOpportunityRowsRenderer.cs")

check(
    "m15State.DirectionLabel" in signal_state and
    "h1State.DirectionLabel" in signal_state and
    "m5State.DirectionLabel" in signal_state and
    '"BULL BIAS"' in signal_state and
    '"BEAR BIAS"' in signal_state,
    "market-bias row must consume canonical timeframe labels",
)

check(
    "m15State.DirectionLabel" in canonical and
    "h1State.DirectionLabel" in canonical and
    "m15 == 0 && h1 == 0" in canonical,
    "primary timeframe status must consume canonical timeframe labels",
)

check(
    "m15State.DirectionLabel" in header and
    "h1State.DirectionLabel" in header,
    "live header must consume canonical timeframe labels",
)

check(
    "m15FrameState.DirectionLabel" in context and
    "h1FrameState.DirectionLabel" in context and
    "DirectionText(primaryM15Direction)" not in context and
    "DirectionText(primaryH1Direction)" not in context,
    "primary M15/H1 alignment must not re-encode canonical labels",
)

check(
    "DirectionText(_decision.HtfAnchorDirection)" in decision and
    "DirectionText(_decision.MidframeDirection)" in decision and
    "_decision.EntryFrameAlignment" in decision and
    "HTF " in decision and
    "MID " in decision and
    "ENTRY/A" in decision,
    "top-down panel must use textual directions and explicit alignment fields",
)

check(
    "_decision.HtfAnchorDirection +" not in decision and
    "_decision.MidframeDirection +" not in decision,
    "top-down panel must not render raw numeric direction fields",
)

check(
    "waveTrendConflict" in wave and
    'waveTrendState += " • CONFLICT"' in wave and
    "Color waveTrendColor" in wave and
    "PanelDirectionColor(waveTrendDirection)" in wave and
    "waveTrendDirection != direction" in wave,
    "WaveTrend text/color must share evidence direction with explicit conflict state",
)

print("Panel semantic and visual consistency audit PASS")
