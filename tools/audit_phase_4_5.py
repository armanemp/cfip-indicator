#!/usr/bin/env python3
"""Static acceptance gate for CR4.5 per-timeframe regime semantics."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


analyzer = read("src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs")
frame_evidence = read(
    "src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs"
)
frame_scoring = read(
    "src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs"
)
frame_model = read(
    "src/CFIP.Indicator/Analysis/Market/Models/Frame.cs"
)
frame_cache = read(
    "src/CFIP.Indicator/Analysis/Market/MarketRegimeFrameCache.cs"
)
resolution_rule = read(
    "src/CFIP.Indicator/Core/Math/FrameRegimeResolutionRule.cs"
)
fusion = read(
    "src/CFIP.Indicator/Core/Math/IndicatorEvidenceFusionRule.cs"
)
calc = read(
    "src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs"
)
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")


check(
    "Frame carries explicit per-timeframe regime metadata",
    "public string Regime;" in frame_model
    and "public int RegimeQuality;" in frame_model
    and "public int RegimeStability;" in frame_model,
)
check(
    "every analyzed timeframe resolves regime through the shared analyzer",
    "AnalyzeMarketRegime(" in frame_evidence
    and "f.Regime =" in frame_evidence
    and "FrameRegimeResolutionRule.ResolveSnapshot(" in frame_evidence,
)
check(
    "non-M5 regime snapshots are independently cached",
    "_marketRegimeFrameCache.TryGetSnapshot(" in analyzer
    and "_marketRegimeFrameCache.StoreSnapshot(" in analyzer
    and "AnalyzeMarketRegimeCore(" in analyzer,
)
check(
    "M5 keeps its existing dedicated regime path",
    "GetM5RegimeCoreSnapshot(" in analyzer
    and "if (snapshot == null ||" in analyzer
    and "index <= 40" in analyzer,
)
check(
    "cache is bounded to the active MTF set",
    "private const int Capacity = 8;" in frame_cache
    and "new Entry[Capacity]" in frame_cache,
)
check(
    "cache invalidates on series replacement or fingerprint mutation",
    "!ReferenceEquals(entry.Bars, bars)" in frame_cache
    and "entry.First ==" in frame_cache
    and "entry.Last ==" in frame_cache,
)
check(
    "scoring consumes the frame's own normalized regime",
    "return FrameRegimeResolutionRule.Normalize(" in frame_scoring
    and "frame.Regime" in frame_scoring
    and "return "UNKNOWN";" not in frame_scoring,
)
check(
    "UNKNOWN regime is explicitly neutral",
    'public const string Unknown = "UNKNOWN";' in resolution_rule
    and "UNKNOWN is intentionally neutral" in fusion
    and "double trendWeight = 1.0;" in fusion
    and "double momentumWeight = 1.0;" in fusion
    and "double contextWeight = 1.0;" in fusion,
)
check(
    "recognized regime names are normalized symmetrically",
    "ToUpperInvariant()" in resolution_rule
    and "TREND" in resolution_rule
    and "EXPANSION" in resolution_rule
    and "RANGE" in resolution_rule
    and "TRANSITION" in resolution_rule
    and "HIGH_VOLATILITY" in resolution_rule
    and "COMPRESSION" in resolution_rule,
)
check(
    "all canonical MTF frames are analyzed independently",
    "_m5Frame = " in calc
    and "_m15Frame =" in calc
    and "_m30Frame =" in calc
    and "_h1Frame =" in calc
    and "_h4Frame =" in calc
    and "_d1Frame =" in calc
    and "_w1Frame =" in calc,
)
check(
    "runtime contract tests frame-regime direction independence and symmetry",
    "VerifyFrameRegimeSemantics();" in contracts
    and "FrameRegimeResolutionRule.ResolveSnapshot(bullTrend)" in contracts
    and "FrameRegimeResolutionRule.ResolveSnapshot(bearTrend)" in contracts
    and "BUY/SELL regime-weighted fusion is not symmetric." in contracts,
)
check(
    "runtime contract project includes the canonical resolution rule",
    "FrameRegimeResolutionRule.cs" in contracts_project,
)
check(
    "phase is recorded as complete and target-terminal boundary remains explicit",
    "CR4.5" in roadmap
    and "target-terminal" in roadmap.lower()
    and "CR4.5" in continuation
    and "Next phase" in continuation,
)

print("CR4.5 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)
print("CR4.5 STATIC GATE PASS")
