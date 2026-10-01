#!/usr/bin/env python3
"""Static acceptance gate for CR6.1 / F1 opposing FVG/OB target-path semantics."""

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


scanner = read("src/CFIP.Indicator/Trading/Validation/RewardPathZoneObstacleScanner.cs")
zone_lookup = read("src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookup.cs")
fvg_lifecycle = read("src/CFIP.Indicator/Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs")
fvg_mitigation = read("src/CFIP.Indicator/Analysis/Structure/Zones/FvgMitigationEvaluator.cs")
ob_builder = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs")
ob_mitigation = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockMitigationGuard.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
phase_doc = read("docs/PHASE-CR6-1-OPPOSING-ZONE-PATH.md")
core = read("src/CFIP.Indicator/Core/Math/RewardPathObstacleRule.cs")


check(
    "Core owns opposite-direction resolution",
    "class RewardPathObstacleRule" in core and
    "RewardPathObstacleRule.OpposingDirection(" in scanner and
    "RewardPathObstacleRule.OpposingDirection(" in zone_lookup,
)

check(
    "FVG obstacle scan uses opposing managed candidates",
    "FindNearestFvg(" in scanner and
    "false," in scanner and
    "TryGetCachedFvgCandidates(" in scanner and
    "candidate.Direction" in scanner and
    "IsOpposingZone(" in scanner,
)

check(
    "OB obstacle scan uses opposing managed candidates and lifecycle",
    "FindNearestOrderBlock(" in scanner and
    "TryGetCachedObCandidates(" in scanner and
    "OrderBlockLifecycleState.Broken" in scanner and
    "OrderBlockLifecycleState.Mitigated" not in scanner,
)

check(
    "raw FVG candle-gap scanning is absent from obstacle scanner",
    "FvgRule.TryGetThreeBarGap(" not in scanner and
    "bars.LowPrices[i] -" not in scanner,
)

check(
    "canonical FVG mitigation remains in the managed FVG builder",
    "TryApplyFvgMitigation(" in fvg_lifecycle and
    "TryApplyFvgMitigation(" in fvg_mitigation,
)

check(
    "canonical OB mitigation remains in the managed OB builder",
    "TryApplyOrderBlockMitigation(" in ob_builder and
    "TryApplyOrderBlockPartialMitigation(" in ob_mitigation,
)

check(
    "runtime contracts cover F1 direction and managed-zone mitigation",
    "VerifyRewardPathObstacleSemantics();" in runtime and
    "RewardPathObstacleRule.OpposingDirection(1)" in runtime and
    "FvgRule.TryApplyPartialMitigation(" in runtime and
    "OrderBlockRule.TryApplyOrderBlockPartialMitigation(" in runtime,
)

check(
    "runtime project compiles the Core rule",
    "RewardPathObstacleRule.cs" in runtime_project,
)

check(
    "F1 accumulated audit is wired after E8",
    "audit_phase_5_8.py" in workflow and
    "audit_phase_6_1.py" in workflow and
    workflow.index("audit_phase_6_1.py") >
    workflow.index("audit_phase_5_8.py"),
)

check(
    "continuity records transition from CR6.1 to CR6.2",
    "CR6.1 / F1" in roadmap and
    "CR6.2 / F2" in roadmap and
    "CR6.1 / F1" in continuation and
    "CR6.2 / F2" in continuation and
    "CR6.1 / F1" in phase_doc,
)

print("CR6.1 SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)
print("CR6.1 STATIC GATE PASS")
