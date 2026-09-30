#!/usr/bin/env python3
"""Static acceptance gate for CR2.8 historical rendering semantics and cost."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

rule = read("src/CFIP.Indicator/Core/Math/HistoricalRenderingRule.cs")
renderer = read("src/CFIP.Indicator/UI/Historical/HistoricalRenderer.cs")
cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
init = read("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

checks = {
    "fixed historical scan budget is explicit": (
        "MaximumScanBars = 500" in rule and
        "ResolveOldestScannedIndex" in rule and
        "lastClosed -" in rule
    ),
    "historical renderer consumes the bounded rule": (
        "HistoricalRenderingRule.ResolveOldestScannedIndex" in renderer and
        "HistoricalRenderingRule.ResolveLastClosedIndex" in renderer
    ),
    "only closed analyzable bars are evaluated": (
        "IsClosedAnalyzableIndex" in renderer and
        "index < barCount - 1" in rule
    ),
    "per closed-bar result cache is keyed by timestamp": (
        "Dictionary<DateTime, HistoricalSignalPresentation>" in renderer and
        "TryGetValue(" in renderer and
        "Bars.OpenTimes[i]" in renderer
    ),
    "cached results are bounded to the render window": (
        "TrimHistoricalSignalCache" in renderer and
        "staleKeys" in renderer
    ),
    "chart object identity is timestamp based": (
        "ObjectIdentity" in rule and
        "HistoricalRenderingRule.ObjectIdentity" in renderer and
        "openTime.Ticks" in rule
    ),
    "old index identity is removed": (
        "H +\n                    i" not in renderer and
        "PRESENTATION_" in rule
    ),
    "history lifecycle invalidates presentation cache": (
        "Bars.HistoryLoaded" in renderer and
        "Bars.Reloaded" in renderer and
        "InvalidateHistoricalRenderingCache" in renderer and
        "HookHistoricalBarsEvents" in init and
        "UnhookHistoricalBarsEvents" in init
    ),
    "same host-bar state does not rerun historical analysis": (
        "_lastHistoricalHostBar" in cycle and
        "if (_lastHistoricalHostBar !=" in cycle and
        "RenderHistoricalSignals();" in cycle
    ),
    "historical renderer stays presentation-only": (
        "presentation-only" in renderer.lower() and
        "SendUnifiedAlert" not in renderer and
        "EnsureSignalPlan" not in renderer and
        "Execute" not in renderer
    ),
    "runtime contracts cover CR2.8": (
        "VerifyHistoricalRenderingSemantics();" in contracts and
        "HistoricalRenderingRule.MaximumScanBars" in contracts
    ),
    "runtime project links CR2.8 rule": (
        "HistoricalRenderingRule.cs" in project
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

matches = re.findall(
    r"for \(int i = lastClosed;\s+i >= oldest;\s+i--",
    renderer,
    flags=re.S,
)
if len(matches) != 1:
    errors.append("historical loop must be one bounded descending scan")
    print("FAIL | historical loop must be one bounded descending scan")
else:
    print("PASS | historical loop is a single bounded descending scan")

print("CR2.8 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.8 STATIC GATE PASS")
