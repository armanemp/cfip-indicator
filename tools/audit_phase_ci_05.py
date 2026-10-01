#!/usr/bin/env python3
"""Static acceptance gate for CI-05 FVG lifecycle semantics."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append("missing file: " + relative)
        return ""
    return path.read_text(encoding="utf-8")

def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)

fvg_rule = read("src/CFIP.Indicator/Core/Math/FvgRule.cs")
lifecycle_rule = read("src/CFIP.Indicator/Core/Math/FvgLifecycleRule.cs")
detection = read("src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs")
lifecycle = read("src/CFIP.Indicator/Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs")
mitigation = read("src/CFIP.Indicator/Analysis/Structure/Zones/FvgMitigationEvaluator.cs")
quality = read("src/CFIP.Indicator/Analysis/Structure/Zones/FvgZoneQualityCalculator.cs")
predictive = read("src/CFIP.Indicator/Planning/Execution/PredictivePendingZoneCollector.cs")
ob_confluence = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockConfluenceAnalyzer.cs")
obstacle = read("src/CFIP.Indicator/Trading/Validation/RewardPathZoneObstacleScanner.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
verify = read("tools/verify_architecture.py")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")

production_files = [
    p for p in (ROOT / "src/CFIP.Indicator").rglob("*.cs")
]

check(
    "canonical FVG geometry remains in FvgRule",
    all(token in fvg_rule for token in (
        "TryGetThreeBarGap(",
        "TryGetTwoBarGap(",
        "MeetsMinimumGap(",
        "IsOverlapInclusive(",
        "IsFullyFilled(",
        "TryApplyPartialMitigation(",
        "Identity(",
        "low < high",
    ))
)

check(
    "FVG lifecycle has one platform-neutral owner",
    "internal static class FvgLifecycleRule" in lifecycle_rule and
    "IsAgeValid(" in lifecycle_rule and
    "ResolveFvgMitigationProbe(" in lifecycle_rule and
    "TryApplyMitigationStep(" in lifecycle_rule
)

check(
    "lifecycle delegates geometry transitions to the canonical FVG rule",
    "FvgRule.IsFullyFilled(" in lifecycle_rule and
    "FvgRule.TryApplyPartialMitigation(" in lifecycle_rule
)

check(
    "full-fill disabled invalidation preserves source geometry",
    "return !invalidateOnFullFill;" in lifecycle_rule and
    "disabled full-fill invalidation retains canonical source geometry" in runtime
)

check(
    "source age is enforced at the managed-zone boundary",
    "FvgLifecycleRule.IsAgeValid(" in lifecycle and
    "MaximumZoneAgeBars" in lifecycle
)

check(
    "detector uses creation-bar ATR and canonical gap geometry",
    all(token in detection for token in (
        "double creationAtr",
        "FvgRule.TryGetThreeBarGap(",
        "FvgRule.TryGetTwoBarGap(",
        "FvgRule.MeetsMinimumGap(",
        "FvgRule.IsOverlapInclusive(",
        "FvgLookback",
        "MaximumZoneAgeBars",
        "PassesCurrentFvgRetest(",
    ))
)

check(
    "detector current retest is post-creation only",
    "index <= zone.CreatedIndex" in detection and
    "bars.LowPrices[index] - tolerance" in detection and
    "bars.HighPrices[index] + tolerance" in detection
)

mitigation_main_end = mitigation.find("    private bool IsZoneFullyMitigated(")
mitigation_main = (
    mitigation[:mitigation_main_end]
    if mitigation_main_end >= 0
    else mitigation
)

check(
    "mitigation uses the canonical lifecycle owner",
    "FvgLifecycleRule.ResolveFvgMitigationProbe(" in mitigation_main and
    "FvgLifecycleRule.TryApplyMitigationStep(" in mitigation_main and
    "FvgBreakByWicks" in mitigation_main and
    "FvgInvalidateOnFullFill" in mitigation_main
)

check(
    "mitigation main loop no longer duplicates body/wick probe arithmetic",
    "Math.Min(" not in mitigation_main and
    "Math.Max(" not in mitigation_main
)

check(
    "FVG quality consumes remaining geometry and bounded age",
    "gap /" in quality and
    "MaximumZoneAgeBars" in quality and
    "remainingRatio" in quality
)

check(
    "predictive pending FVGs consume the managed lifecycle",
    "BuildManagedFvgZone(" in predictive and
    "FvgRule.TryGetThreeBarGap(" in predictive and
    "FvgRule.TryGetTwoBarGap(" in predictive
)

check(
    "OB/FVG confluence uses canonical FVG lifecycle",
    "BuildManagedFvgZone(" in ob_confluence and
    "FvgRule.IsOverlapInclusive(" in ob_confluence
)

check(
    "reward-path FVG obstacles use managed lifecycle rather than raw zones",
    "BuildManagedFvgZone(" in obstacle and
    "FvgRule.TryGetThreeBarGap(" in obstacle and
    "FvgRule.MeetsMinimumGap(" in obstacle
)

check(
    "FVG lifecycle contracts are registered",
    "VerifyFvgLifecycleSemantics();" in runtime and
    "FvgLifecycleRule.cs" in runtime_project and
    "CI-05 FVG lifecycle semantics contracts PASS" in runtime
)

check(
    "legacy architecture verifier knows the lifecycle owner",
    "FvgLifecycleRule.cs" in verify and
    "FvgLifecycleRule" in verify
)

check(
    "CI-05 accumulated audit is wired after CI-04",
    "audit_phase_ci_04.py" in workflow and
    "audit_phase_ci_05.py" in workflow and
    workflow.index("audit_phase_ci_05.py") > workflow.index("audit_phase_ci_04.py")
)

check(
    "CI-05 is the active continuation phase",
    "**CI-05 — FVG lifecycle" in roadmap and
    "**CI-05 — FVG lifecycle" in continuation
)

# No second production FVG lifecycle class is permitted.
lifecycle_declarations = []
for path in production_files:
    text = path.read_text(encoding="utf-8")
    if "class FvgLifecycleRule" in text:
        lifecycle_declarations.append(path.as_posix())

check(
    "exactly one production FVG lifecycle owner exists",
    len(lifecycle_declarations) == 1 and
    lifecycle_declarations[0].endswith("Core/Math/FvgLifecycleRule.cs")
)

print("CI-05 FVG LIFECYCLE SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-05 STATIC GATE PASS")
