#!/usr/bin/env python3
"""Static acceptance gate for CR2.1 structural semantics."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

structure = read("src/CFIP.Indicator/Analysis/Structure/StructureAnalyzer.cs")
sequence = read("src/CFIP.Indicator/Trading/Intelligence/StructuralSequenceAnalyzer.cs")
sweep = read("src/CFIP.Indicator/Analysis/Structure/LiquiditySweepAnalyzer.cs")
swing = read("src/CFIP.Indicator/Analysis/Structure/SwingPointAnalyzer.cs")
resolver = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateEvaluator.cs")
classifier = read("src/CFIP.Indicator/Planning/TradePlan/HtfTimeframeClassifier.cs")
divergence = read("src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs")
divergence_model = read("src/CFIP.Indicator/Core/Models/DivergenceResult.cs")
frame_evidence = read("src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")

required = {
    "Structure fresh-break owner": "StructuralEventRule.IsFreshBreak(" in structure,
    "CHOCH prior-structure owner": "StructuralEventRule.IsChangeOfCharacter(" in structure,
    "Sequence canonical structural event": "StructuralEvidenceRule.CanonicalEventCount(" in sequence,
    "Active liquidity owner": "LiquiditySweepRule.IsActiveUnbrokenLevel(" in sweep,
    "Canonical swing identity helpers": (
        "TryFindLatestSwingHigh(" in swing and
        "TryFindLatestSwingLow(" in swing
    ),
    "Unknown timeframe fails closed": (
        'case "W1":' in resolver and
        "return null;" in resolver
    ),
    "Canonical timeframe owner": "StructuralTimeframeRule.IsHigherThanM5(" in classifier,
    "Divergence conflict factory": "DivergenceResult.CreateConflict(" in divergence,
    "Divergence conflict is non-directional": (
        "CreateConflict(" in divergence_model and
        '"CONFLICT"' in divergence_model and
        "0," in divergence_model
    ),
    "Canonical rejection owner": "RejectionRule.IsRejection(" in structure,
    "Closed-bar structure inputs": "f.StructureBull =" in frame_evidence,
}

for name, ok in required.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# The old tautological CHOCH rolling-window pattern must not return.
for forbidden in (
    "Highest(",
    "Lowest(",
):
    if forbidden in structure:
        errors.append(f"legacy CHOCH rolling-extreme call remains in StructureAnalyzer: {forbidden}")
        print(f"FAIL | {errors[-1]}")

# CR2.1 keeps structural freshness bounded by a closed-bar crossing, and the
# active-liquidity scan is bounded by the configured structural lookback.
if "previousClose <=" not in read("src/CFIP.Indicator/Core/Math/StructuralEventRule.cs"):
    errors.append("fresh-break rule is missing bullish previous-side crossing semantics")
if "previousClose >=" not in read("src/CFIP.Indicator/Core/Math/StructuralEventRule.cs"):
    errors.append("fresh-break rule is missing bearish previous-side crossing semantics")

if "i < currentIndex" not in read("src/CFIP.Indicator/Core/Math/LiquiditySweepRule.cs"):
    errors.append("active-liquidity validation is missing bounded prior-close scan")

# Real behavior contracts must cover every CR2.1 owner.
for token in (
    "VerifyStructuralEventSemantics();",
    "VerifyLiquiditySweepSemantics();",
    "VerifyRejectionSemantics();",
    "VerifyDivergenceConflictSemantics();",
    "VerifyStructuralTimeframeSemantics();",
):
    if token not in contracts:
        errors.append(f"runtime contract suite missing CR2.1 coverage: {token}")

print("CR2.1 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.1 STATIC GATE PASS")
