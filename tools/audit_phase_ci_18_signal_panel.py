#!/usr/bin/env python3
"""CI-18 signal / panel coherence audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    p = ROOT / rel
    if not p.exists():
        errors.append("missing " + rel)
        return ""
    return p.read_text(encoding="utf-8")

snapshot = read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs")
signal_state = read("src/CFIP.Indicator/UI/Panel/PanelSignalState.cs")
overview = read("src/CFIP.Indicator/UI/Panel/Rows/PanelOverviewStateRowsRenderer.cs")
context = read("src/CFIP.Indicator/UI/Panel/Rows/PanelContextRowsRenderer.cs")
pipeline = read("src/CFIP.Indicator/UI/Panel/Rows/PanelSignalPipelineRowsRenderer.cs")
canonical = read("src/CFIP.Indicator/UI/Panel/PanelCanonicalSignalStatus.cs")
confirmation = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionConfirmationGates.cs")
actionability = (
    read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityEvaluator.cs") +
    read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityPreparation.cs") +
    read("src/CFIP.Indicator/Trading/Validation/TradeActionabilityGateEvaluation.cs")
)
geometry = read("src/CFIP.Indicator/Core/Math/EntryGeometryRule.cs")
contracts = read("tools/CFIP.Decision.Contracts/Program.cs")

def require(cond, msg):
    if not cond:
        errors.append(msg)

require(
    "if (_decision != null &&" in snapshot and
    "_decision.Direction != 0" in snapshot and
    "return _decision.Direction;" in snapshot,
    "visual direction must retain a directional Decision even before actionability",
)
require(
    "private int GetMarketBiasDirection()" in signal_state and
    "private string GetMarketBiasText()" in signal_state,
    "panel must expose market bias separately from trade readiness",
)
require(
    "GetMarketBiasText()" in overview,
    "overview must render market bias explicitly",
)
require(
    "FrameDirection(_m15Frame)" in context and
    "FrameDirection(_h1Frame)" in context,
    "primary M15/H1 panel state must use shared frame-display direction",
)
require(
    "TRIGGER  " in pipeline and
    "_triggerRuntime.Score" in pipeline and
    "_triggerRuntime.RequiredScore" in pipeline,
    "panel must expose trigger runtime details",
)
require(
    "FrameDirection(_m15Frame)" in canonical and
    "FrameDirection(_h1Frame)" in canonical,
    "canonical signal status must use shared MTF display direction",
)
require(
    "M5OnlyConfirmedTrigger &&" not in confirmation,
    "decision EntryAllowed must not hard-block all modes on M5 trigger",
)
require(
    "!execution.Ready" in actionability and
    "IsActionabilityTriggerReady(" in actionability and
    "IsActionabilityTriggerReady(" in read("src/CFIP.Indicator/Planning/Execution/TriggerGate.cs"),
    "mode-specific trigger enforcement must live in the canonical trigger owner",
)
require(
    geometry.find("else if (insideZone)") < geometry.find("else if (continuation)"),
    "in-zone Retest must take precedence over continuation waiting",
)
require(
    "continuationRetest.Mode == ExecutionMode.RetestMarket" in contracts and
    "continuation context must not hide an in-zone BUY retest" in contracts,
    "continuation/in-zone Retest regression contract is missing",
)

if errors:
    print("CI-18 SIGNAL / PANEL COHERENCE AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CI-18 SIGNAL / PANEL COHERENCE AUDIT: PASS")
print("Directional thesis remains visible before trade actionability: PASS")
print("Market bias and trade readiness are separate panel states: PASS")
print("Primary M15/H1 panel direction is canonical: PASS")
print("Trigger lifecycle is observable in panel: PASS")
print("Retest is not hidden by continuation waiting: PASS")
print("Mode-specific trigger gate is preserved: PASS")