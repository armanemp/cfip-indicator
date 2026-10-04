#!/usr/bin/env python3
"""Static acceptance gate for CR4.6 frame-scoring constant ownership."""

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


constants = read("src/CFIP.Indicator/Core/Math/FrameScoringConstants.cs")
scoring = read(
    "src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs"
)
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
roadmap = read("docs/CFIP-ROADMAP.md")
historical_roadmap = read("docs/archive/ROADMAP-LEGACY-2026-10-04.md")
continuation = read("docs/CONTINUATION-STATE.md")


check(
    "one Core owner exists for frame-scoring constants",
    "internal static class FrameScoringConstants" in constants
    and "FrameScoringConstants." in scoring,
)
check(
    "direction thresholds are centrally owned",
    "DirectionMinimumScore = 35" in constants
    and "DirectionMinimumLead = 8" in constants
    and "bull >= FrameScoringConstants.DirectionMinimumScore" in scoring
    and "bear >= FrameScoringConstants.DirectionMinimumScore" in scoring,
)
check(
    "conflict penalty constants are centrally owned",
    "ConflictPenaltyThreshold = 45" in constants
    and "ConflictPenaltyBaseline = 40" in constants
    and "ConflictPenaltyCap = 6" in constants
    and "ConflictPenaltyDivisor = 10" in constants
    and "FrameScoringConstants.ConflictPenaltyCap" in scoring
    and "FrameScoringConstants.ConflictPenaltyBaseline" in scoring
    and "FrameScoringConstants.ConflictPenaltyDivisor" in scoring,
)
check(
    "RSI exhaustion thresholds and penalty are centrally owned",
    "RsiBullExhaustionThreshold = 75.0" in constants
    and "RsiBearExhaustionThreshold = 25.0" in constants
    and "RsiExhaustionPenalty = 5" in constants
    and "FrameScoringConstants.RsiBullExhaustionThreshold" in scoring
    and "FrameScoringConstants.RsiBearExhaustionThreshold" in scoring,
)
check(
    "event contribution constants are centrally owned",
    all(
        token in constants
        for token in (
            "StructureContribution = 16",
            "MssContribution = 12",
            "ChochContribution = 9",
            "DisplacementContribution = 10",
            "LiquidityContribution = 10",
            "EqualLevelContribution = 5",
        )
    )
    and "FrameScoringConstants.StructureContribution" in scoring
    and "FrameScoringConstants.MssContribution" in scoring
    and "FrameScoringConstants.ChochContribution" in scoring
    and "FrameScoringConstants.DisplacementContribution" in scoring
    and "FrameScoringConstants.LiquidityContribution" in scoring
    and "FrameScoringConstants.EqualLevelContribution" in scoring
    and "ref bear, ref evidence" in scoring,
)
check(
    "regime and quality composition constants are centrally owned",
    all(
        token in constants
        for token in (
            "ChoppyRegimeBase = 8.0",
            "ChoppyRegimeSlope = 0.30",
            "NonChoppyRegimeCap = 14.0",
            "NonChoppyAdxSlope = 0.35",
            "RangeEfficiencyWeight = 8.0",
            "EmaSpreadCap = 4.0",
            "EmaSpreadWeight = 2.0",
            "StrongestQualityWeight = 0.40",
            "AdxQualityScale = 1.45",
            "AdxQualityWeight = 0.13",
            "EvidenceQualityScale = 5.0",
            "EvidenceQualityWeight = 0.20",
            "RegimeQualityWeight = 0.15",
            "IndicatorQualityWeight = 0.12",
        )
    ),
)
check(
    "scoring service no longer owns the migrated literal contributions",
    "AddScore(true, 16," not in scoring
    and "AddScore(true, 12," not in scoring
    and "AddScore(true, 9," not in scoring
    and "AddScore(f.DisplacementBull, 10," not in scoring
    and "AddScore(f.LiquidityBull, 10," not in scoring
    and "bull >= 35" not in scoring
    and "bear >= 35" not in scoring
    and "f.Rsi >= 75" not in scoring
    and "f.Rsi <= 25" not in scoring,
)
check(
    "runtime contract compiles and validates the constant owner",
    "FrameScoringConstants.cs" in contracts_project
    and "VerifyFrameScoringConstants();" in contracts
    and "frame-scoring event contributions preserve" in contracts
    and "RSI exhaustion constants preserve" in contracts,
)
check(
    "phase documentation identifies CR4.6 and preserves the no-tuning boundary",
    "CR4.6" in historical_roadmap
    and "CR4.6" in continuation
    and "no" in continuation.lower()
    and "threshold" in continuation.lower(),
)

print("CR4.6 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR4.6 STATIC GATE PASS")
