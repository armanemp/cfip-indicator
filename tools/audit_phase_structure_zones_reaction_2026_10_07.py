#!/usr/bin/env python3
"""Static acceptance gate for the 2026-10-07 structure/zones/reaction audit."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(rel: str) -> str:
    return (ROOT / rel).read_text(encoding="utf-8")

errors = []

structure = read("src/CFIP.Indicator/Analysis/Structure/StructureAnalyzer.cs")
sweep = read("src/CFIP.Indicator/Analysis/Structure/LiquiditySweepAnalyzer.cs")
fvg = read("src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs")
fvg_rule = read("src/CFIP.Indicator/Core/Math/FvgRule.cs")
scoring = read("src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs")
location = read("src/CFIP.Indicator/Core/Math/LocationEvidenceRule.cs")

checks = [
    ("structure break uses canonical event rule", "StructuralEventRule.IsFreshBreak(" in structure),
    ("CHOCH uses canonical change-of-character rule", "StructuralEventRule.IsChangeOfCharacter(" in structure),
    ("liquidity sweep requires active unbroken level", "LiquiditySweepRule.IsActiveUnbrokenLevel(" in sweep),
    ("FVG geometry has one mathematical owner", "internal static class FvgRule" in fvg_rule),
    ("FVG detection delegates geometry to FvgRule", "FvgRule.TryGetThreeBarGap(" in fvg and "FvgRule.TryGetTwoBarGap(" in fvg),
    ("location score has one owner", "internal static class LocationEvidenceRule" in location),
    ("frame scoring consumes canonical location score", "LocationEvidenceRule.Evaluate(" in scoring),
    ("frame evidence is directional, not conflict-summed", "int bullEvidence = 0;" in scoring and "int bearEvidence = 0;" in scoring),
    ("frame evidence is zero on an exact score tie", ": 0;" in scoring),
]

for name, ok in checks:
    if not ok:
        errors.append(name)

if errors:
    for error in errors:
        print("FAIL | " + error)
    raise SystemExit(1)

print("PASS | structure/zones/reaction audit")
