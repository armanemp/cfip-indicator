#!/usr/bin/env python3
"""CFIP smart separated signal-arrow audit.

The audit enforces a single production owner for directional presentation:
MtfTrendStrengthRule -> SignalVisualSnapshot -> SignalStackedArrowRenderer.
M1 remains a non-directional precision/confirmation layer; no competing chart
marker is rendered for it.
"""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append("missing: " + rel)
        return ""
    return path.read_text(encoding="utf-8")


def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)


trend = read("src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs")
stack = read("src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs")
snapshot = read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs")
builder = read("src/CFIP.Indicator/UI/Chart/MtfTrendStrengthSnapshotBuilder.cs")
signal = read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs")
presentation = read("src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs")
plan = read("src/CFIP.Indicator/UI/Chart/PlanRenderCoordinator.cs")
workflow = read(".github/workflows/source-check.yml")
phase = read("docs/PHASE-SMART-SEPARATED-SIGNAL-ARROWS-2026-10-04.md")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")

check(
    "one canonical smart-strength owner exists",
    "class MtfTrendStrengthRule" in trend and
    "ResolveCompositeScore" in trend and
    "ResolveNineLevel" in trend and
    "LevelMinimumScore = 35" in trend and
    "LevelBandSize = 5" in trend and
    "ResolveTier" in trend,
)

check(
    "the nine levels map to three real strength tiers",
    "score < LevelMinimumScore" in trend and
    "NumericGuards.ClampInt(" in trend and
    "Math.Min(9, level)" not in trend or "NumericGuards.ClampInt" in trend,
)

check(
    "strength uses existing multidimensional market evidence",
    "frame.Quality" in trend and
    "frame.Adx" in trend and
    "frame.EmaSpreadAtr" in trend and
    "frame.EmaSlopeAtr" in trend and
    "ResolveStructuralQuality(" in trend and
    "ResolveLocationQuality(" in trend and
    "frame.IndicatorIndependentEvidenceGroupCount" in trend and
    "frame.IndicatorConflict" in trend and
    "ResolveClosedPressureQuality(" in trend and
    "ResolveLivePressureQuality(" not in trend,
)

check(
    "canonical arrow renderer consumes snapshot strength only",
    "snapshot.MtfTrendStrengthLevel" in stack and
    "((strength - 1) % 3) + 1" in stack and
    "UpdateSignalArrowBox(" in stack and
    '"↑"' in stack and
    '"↓"' in stack,
)

check(
    "three arrows use deterministic fixed-box spacing",
    "Orientation.Horizontal" in stack and
    "_signalArrowBoxArrows" in stack and
    "new System.Collections.Generic.List<TextBlock>(3)" in stack and
    "Width = 18" in stack and
    "Height = 28" in stack and
    "Margin = new Thickness(1, 0, 1, 0)" in stack and
    "Width = 66" in stack and
    "Height = 66" in stack,
)

check(
    "the canonical direction owns strength resolution",
    "snapshot.AuthoritativeDirection =" in snapshot and
    "ApplyMtfTrendStrength(" in snapshot and
    "visualDirection);" in snapshot and
    snapshot.find("snapshot.AuthoritativeDirection =") < snapshot.find("ApplyMtfTrendStrength("),
)

check(
    "direction override is absent from the single strength evaluator",
    "preferredDirection" not in builder and
    "MtfTrendStrengthRule.Evaluate(" in builder,
)

check(
    "M1 precision marker no longer owns a competing visual marker",
    "P + " + chr(34) + "M1_TRIGGER" + chr(34) not in signal and
    "ChartIconType.Circle" not in signal and
    "UpdateSignalArrowBox(" in stack,
)

check(
    "trade-direction conflict downgrades rather than erases the authoritative signal",
    "AuthoritativeDirection" in stack and
    "decisionOwnsDirection" in stack and
    "mtfDirection != 0" in stack and
    "mtfDirection != direction" in stack and
    "strength = Math.Min(3, strength)" in stack,
)

check(
    "non-canonical event/history/opportunity visuals remain separate from canonical directional arrows",
    "ChartIconType.UpTriangle" in read("src/CFIP.Indicator/Trading/Alerts/ContextAlertEmitter.cs") and
    "ChartIconType.DownTriangle" in read("src/CFIP.Indicator/Trading/Alerts/ContextAlertEmitter.cs") and
    "ChartIconType.UpTriangle" in read("src/CFIP.Indicator/UI/Historical/HistoricalRenderer.cs") and
    "ChartIconType.DownTriangle" in read("src/CFIP.Indicator/UI/Historical/HistoricalRenderer.cs") and
    "ChartIconType.UpArrow" not in read("src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs") and
    "ChartIconType.DownArrow" not in read("src/CFIP.Indicator/UI/Chart/ParallelOpportunityRenderer.cs"),
)

check(
    "all production arrow call-sites use one calculation-lifecycle owner",
    "RenderCanonicalMtfTrendArrows(" in read("src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs") and
    "RenderStackedSignalArrows(" in stack and
    "ResolveSignalArrowState(" not in signal and
    "fallbackState" not in stack,
)

check(
    "stale duplicate renderers are removed from production source",
    not (ROOT / "src/CFIP.Indicator/Core/Math/HtfTrendArrowStrengthRule.cs").exists() and
    not (ROOT / "src/CFIP.Indicator/UI/Chart/MtfTrendArrowRenderer.cs").exists() and
    "HtfTrendArrowStrengthRule" not in stack and
    "RenderMtfTrendStrengthArrowStack" not in signal and
    "RenderMtfTrendStrengthArrowStack" not in presentation and
    "RenderMtfTrendStrengthArrowStack" not in plan,
)

check(
    "workflow accumulates the dedicated audit",
    "python tools/audit_phase_smart_separated_signal_arrows_2026_10_04.py" in workflow,
)

check(
    "phase documentation records the single-owner contract",
    "Smart Separated Signal Arrows" in phase and
    ("single-owner" in phase.lower() or "single owner" in phase.lower()) and
    ("fixed 66×66" in phase.lower() or "fixed 66x66" in phase.lower()) and
    "git pull --ff-only" in phase,
)

check(
    "phase and continuation state record the closeout",
    "2026-10-04" in phase and
    "smart separated signal arrows" in phase.lower() and
    "smart separated signal arrows" in continuation.lower(),
)

# Ensure the old identifiers are not reintroduced anywhere in production C#.
production_root = ROOT / "src"
for term in (
    "HtfTrendArrowStrengthRule",
    "MtfTrendArrowRenderer",
    "RenderMtfTrendStrengthArrowStack",
):
    matches = []
    for path in production_root.rglob("*.cs"):
        text = path.read_text(encoding="utf-8")
        if term in text:
            matches.append(str(path.relative_to(ROOT)))
    check(
        f"no production C# references obsolete owner: {term}",
        not matches,
    )

if errors:
    print("=" * 72)
    print("SMART SEPARATED SIGNAL ARROWS AUDIT: FAIL")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("=" * 72)
print("SMART SEPARATED SIGNAL ARROWS AUDIT: PASS")
