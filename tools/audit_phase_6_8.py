#!/usr/bin/env python3
"""Static acceptance gate for CR6.8 / F9 target-obstacle scan caching."""

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


validator = read("src/CFIP.Indicator/Trading/Validation/TargetObstacleValidator.cs")
builder = read("src/CFIP.Indicator/Trading/Validation/TargetObstacleScanSnapshotBuilder.cs")
cache = read("src/CFIP.Indicator/Trading/Validation/TargetObstacleScanCache.cs")
policy = read("src/CFIP.Indicator/Core/Math/TargetObstacleCachePolicy.cs")
selector = read("src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs")
plan_builder = read("src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs")
preview_builder = read("src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs")
plan_targets = read("src/CFIP.Indicator/Planning/TradePlan/PlanTargetPreparation.cs")
contracts = read("tools/CFIP.Planning.Contracts/Program.cs")
csproj = read("tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
benchmark = read("tools/benchmark_target_obstacle_cache.py")
roadmap = read("docs/CFIP-ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
review = read("docs/CLAUDE-REVIEW-REMEDIATION-ROADMAP.md")
phase = read("docs/PHASE-CR6-8-F9-TARGET-OBSTACLE-CACHE.md")


check(
    "F9 adds a dedicated bounded target-obstacle cache",
    "TargetObstacleScanCache" in cache and
    "MaximumEntries" in policy and
    "TargetObstacleScanCache _targetObstacleScanCache" in validator
)

check(
    "cache capacity is fixed and bounded",
    "MaximumEntries = 16" in policy and
    "new TargetObstacleScanCacheEntry[TargetObstacleCachePolicy.MaximumEntries]" in cache
)

check(
    "cache identity contains closed-bar and scan inputs",
    "BarCount" in policy and
    "OpenTimeTicks" in policy and
    "Index" in policy and
    "Direction" in policy and
    "SwingStrength" in policy and
    "TargetLookbackBars" in policy and
    "LiquidityLookback" in policy and
    "EqualityTolerance" in policy and
    "PipSize" in policy
)

check(
    "same-Bars new-bar/history contexts are invalidated",
    "IsObsoleteSameBars" in policy and
    "InvalidateObsoleteSameBars" in cache and
    "ReferenceEquals(entry.Bars, bars)" in cache and
    "InvalidateTargetObstacleBars" in cache
)

check(
    "HistoryLoaded and Reloaded invalidate the affected Bars cache",
    "HistoryLoaded +=" in cache and
    "Reloaded +=" in cache and
    "InvalidateTargetObstacleBars" in cache
)

check(
    "EvaluateTargetObstacle consumes a snapshot instead of rescanning equality levels per candidate",
    "GetTargetObstacleScanSnapshot(" in validator and
    "IsTargetObstacleSwing(" in builder and
    "FindEqualHigh(" not in validator and
    "FindEqualLow(" not in validator
)

check(
    "target-specific Entry/Target path geometry remains outside the cache",
    "level > entry" in validator and
    "level < target - clearance" in validator and
    "pair.ResolvedLevel < target - clearance" in validator and
    "pair.ResolvedLevel > target + clearance" in validator
)

check(
    "SelectTargets has one owner and canonical callers remain unchanged",
    selector.count("private List<Level> SelectTargets(") == 1 and
    "SelectTargets(" in plan_builder and
    "SelectTargets(" in preview_builder and
    "HasTargetObstacle(" in plan_targets
)

check(
    "F9 cache counters make reuse observable",
    "Hits { get; private set; }" in cache and
    "Misses { get; private set; }" in cache and
    "Builds { get; private set; }" in cache and
    "Evictions { get; private set; }" in cache
)

check(
    "deterministic planning contracts cover F9 identity/invalidation behavior",
    "VerifyTargetObstacleCachePolicyF9();" in contracts and
    "F9 identical cache key is stable" in contracts and
    "F9 new closed bar invalidates same-bars cache entries" in contracts
)

check(
    "planning contracts compile the Core F9 cache policy",
    "Core/Math/TargetObstacleCachePolicy.cs" in csproj
)

check(
    "platform-neutral reference benchmark is present",
    "CR6.8 / F9 reference benchmark" in benchmark and
    "structural reference model" in benchmark
)

check(
    "F9 audit is accumulated immediately after F8",
    "audit_phase_6_7.py" in workflow and
    "audit_phase_6_8.py" in workflow and
    workflow.index("audit_phase_6_8.py") > workflow.index("audit_phase_6_7.py")
)

check(
    "F9 documentation and continuation advance to F3",
    "CR6.8 / F9 closeout" in roadmap and
    "CR6.9 / F3" in roadmap and
    "CR6.8 / F9" in continuation and
    "CR6.9 / F3" in continuation and
    "CR6.8 / F9" in review and
    "CR6.9 / F3" in review and
    "CR6.8 / F9" in phase
)

check(
    "no new public target/risk parameter is introduced by F9 cache files",
    "[Parameter(" not in cache and
    "[Parameter(" not in policy and
    "[Parameter(" not in validator
)

print("CR6.8 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.8 STATIC GATE PASS")
