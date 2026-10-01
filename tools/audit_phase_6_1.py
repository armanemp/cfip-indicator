#!/usr/bin/env python3
"""Static acceptance gate for CR6.1 / F1 opposing-zone reward-path semantics."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


scanner = read(
    "src/CFIP.Indicator/Trading/Validation/RewardPathZoneObstacleScanner.cs"
)
cache = read(
    "src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookupHotCache.cs"
)
core = read(
    "src/CFIP.Indicator/Core/Math/RewardPathGeometryRule.cs"
)
runtime_contracts = read(
    "tools/CFIP.Runtime.Contracts/Program.cs"
)
runtime_project = read(
    "tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj"
)
workflow = read(".github/workflows/source-check.yml")

check(
    "reward-path geometry is owned by Core",
    "internal static class RewardPathGeometryRule" in core and
    "RewardPathGeometryRule.cs" in runtime_project,
)

check(
    "legacy Trading reward-path geometry owner is removed",
    not (
        ROOT /
        "src/CFIP.Indicator/Trading/Validation/RewardPathGeometryRule.cs"
    ).exists(),
)

check(
    "opposing direction is computed once and used by FVG and OB builders",
    "int opposingDirection =
                -direction;" in scanner and
    "FvgRule.TryGetThreeBarGap(
                            opposingDirection," in scanner and
    "BuildManagedFvgZone(
                                bars,
                                i,
                                index,
                                opposingDirection," in scanner and
    "BuildOrderBlockCandidate(
                            bars,
                            i,
                            index,
                            opposingDirection," in scanner,
)

check(
    "canonical FVG mitigation owner is used for obstacle candidates",
    "BuildManagedFvgZone(" in scanner and
    "TryGetThreeBarGap(" in scanner and
    "FvgRule.MeetsMinimumGap(" in scanner,
)

check(
    "raw pre-mitigation FVG path scan was removed",
    "bars.LowPrices[i] -
                        bars.HighPrices[i - 2]" not in scanner and
    "bars.LowPrices[i - 2] -
                        bars.HighPrices[i]" not in scanner,
)

check(
    "only opposing zones reach reward-path geometry",
    "RewardPathGeometryRule.IsOpposingZoneDirection(" in scanner and
    "obstacle.Direction" in scanner,
)

check(
    "obstacle scan is cached per closed Bars/index context and direction",
    "_cachedOpposingZonePathObstacles" in cache and
    "_cachedOpposingZonePathObstacles.Clear()" in cache and
    "TryGetCachedOpposingZonePathObstacles(" in scanner and
    "StoreCachedOpposingZonePathObstacles(" in scanner,
)

check(
    "target-specific path geometry is evaluated after cache retrieval",
    "RewardPathGeometryRule.BlocksRewardPath(" in scanner and
    "entry,
                        target,
                        clearance" in scanner,
)

check(
    "deterministic F1 contracts are wired",
    "VerifyOpposingZonePathF1();" in runtime_contracts and
    "FvgRule.IsFullyFilled(" in runtime_contracts and
    "RewardPathGeometryRule.IsOpposingZoneDirection(" in runtime_contracts and
    "RewardPathGeometryRule.BlocksRewardPath(" in runtime_contracts,
)

check(
    "F1 runtime contract source is included in CI project",
    "Core/Math/RewardPathGeometryRule.cs" in runtime_project,
)

check(
    "F1 accumulated static gate is wired into Source/Architecture CI",
    "audit_phase_6_1.py" in workflow,
)

print("CR6.1 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.1 STATIC GATE PASS")
