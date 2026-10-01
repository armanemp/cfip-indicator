#!/usr/bin/env python3
"""Static acceptance gate for CI-06 Order Block lifecycle integrity."""
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

rule = read("src/CFIP.Indicator/Core/Math/OrderBlockRule.cs")
lifecycle = read("src/CFIP.Indicator/Core/Math/OrderBlockLifecycleRule.cs")
builder = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs")
evidence = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockEvidenceBuilder.cs")
mitigation = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockMitigationGuard.cs")
analyzer = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockAnalyzer.cs")
confluence = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockConfluenceAnalyzer.cs")
zone = read("src/CFIP.Indicator/Core/Models/Zone.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
execution_zone = read("src/CFIP.Indicator/Planning/Execution/ExecutionZoneCandidateSelector.cs")
targets = read("src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs")
stops = read("src/CFIP.Indicator/Planning/TradePlan/StructuralStopCandidateCollector.cs")
obstacles = read("src/CFIP.Indicator/Trading/Validation/RewardPathZoneObstacleScanner.cs")
predictive = read("src/CFIP.Indicator/Planning/Execution/PredictivePendingZoneCollector.cs")
alert_engine = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")
alert_processor = read("src/CFIP.Indicator/UI/Popup/AlertDeliveryProcessor.cs")

production_files = list((ROOT / "src/CFIP.Indicator").rglob("*.cs"))

check(
    "source geometry has one mathematical owner",
    all(token in rule for token in (
        "IsOppositeSourceCandle(",
        "TryGetZone(",
        "MeetsDisplacement(",
        "BreaksStructure(",
        "OrderBlockIdentity(",
    ))
)

check(
    "lifecycle has one dedicated owner",
    all(token in lifecycle for token in (
        "OrderBlockLifecycleState.Fresh",
        "OrderBlockLifecycleState.Mitigated",
        "OrderBlockLifecycleState.Broken",
        "MinimumRetainedRatio",
        "IsOrderBlockAgeValid(",
        "ResolveOrderBlockMitigationProbe(",
        "IsOrderBlockFullyMitigated(",
        "TryApplyOrderBlockPartialMitigation(",
        "ClassifyOrderBlockLifecycle(",
    ))
)

check(
    "OrderBlockRule contains no lifecycle implementation",
    all(token not in rule for token in (
        "OrderBlockLifecycleState.Fresh",
        "OrderBlockLifecycleState.Mitigated",
        "OrderBlockLifecycleState.Broken",
        "ResolveOrderBlockMitigationProbe(",
        "IsOrderBlockFullyMitigated(",
        "TryApplyOrderBlockPartialMitigation(",
        "ClassifyLifecycle(",
    ))
)

check(
    "candidate builder enforces canonical age before materialization",
    "OrderBlockLifecycleRule.IsOrderBlockAgeValid(" in builder and
    "MaximumZoneAgeBars" in builder and
    "currentIndex >= bars.Count" in builder
)

check(
    "candidate builder preserves one source identity",
    "OrderBlockRule.OrderBlockIdentity(" in builder and
    "Id =" in builder and
    "CreatedIndex =" in builder and
    "Age =" in builder and
    "OrderBlockLifecycle =" in builder
)

check(
    "evidence delegates displacement and structure-break math",
    "OrderBlockRule.MeetsDisplacement(" in evidence and
    "OrderBlockRule.BreaksStructure(" in evidence
)

check(
    "mitigation delegates all lifecycle transitions",
    "OrderBlockLifecycleRule.ResolveOrderBlockMitigationProbe(" in mitigation and
    "OrderBlockLifecycleRule.TryApplyOrderBlockPartialMitigation(" in mitigation and
    "OrderBlockLifecycleRule.ClassifyOrderBlockLifecycle(" in mitigation and
    "OrderBlockLifecycleRule.MinimumRetainedRatio" in mitigation
)

check(
    "analyzer has no duplicated opposite-source-candle rule",
    "bool opposite =" not in analyzer and
    "BuildOrderBlockCandidate(" in analyzer and
    "IsOnCorrectMarketSide(" in analyzer
)

check(
    "FVG confluence remains canonical",
    "FvgRule.TryGetThreeBarGap(" in confluence and
    "FvgRule.TryGetTwoBarGap(" in confluence and
    "FvgRule.MeetsMinimumGap(" in confluence and
    "FvgRule.IsOverlapInclusive(" in confluence
)

check(
    "Zone carries the canonical OB identity/lifecycle state",
    "string Id" in zone and
    "int CreatedIndex" in zone and
    "int Age" in zone and
    "OrderBlockLifecycleState? OrderBlockLifecycle" in zone
)

check(
    "execution zone consumes the exact OB geometry",
    "FindNearestOrderBlockForExecution(" in execution_zone and
    "m5Ob.Low" in execution_zone and
    "m5Ob.High" in execution_zone
)

check(
    "target planning consumes the exact OB geometry",
    "FindNearestOrderBlock(" in targets and
    "ob.Low" in targets and
    "ob.High" in targets
)

check(
    "structural stop consumes the exact OB geometry",
    "FindNearestOrderBlock(" in stops and
    "supportOb.Low" in stops and
    "supportOb.High" in stops
)

check(
    "reward-path obstacles use the same managed OB builder",
    "BuildOrderBlockCandidate(" in obstacles and
    "oppositeOb.OrderBlockLifecycle" in obstacles
)

check(
    "predictive pending uses the same managed OB builder",
    "BuildOrderBlockCandidate(" in predictive
)

check(
    "runtime contract project includes lifecycle owner",
    "OrderBlockLifecycleRule.cs" in runtime_project
)

check(
    "runtime contracts exercise lifecycle states and stale-age boundary",
    "OrderBlockLifecycleRule.ResolveOrderBlockMitigationProbe(" in contracts and
    "OrderBlockLifecycleRule.TryApplyOrderBlockPartialMitigation(" in contracts and
    "OrderBlockLifecycleRule.ClassifyOrderBlockLifecycle(" in contracts and
    "OrderBlockLifecycleRule.IsOrderBlockAgeValid(" in contracts
)

check(
    "CI-06 runtime method emits a dedicated pass marker",
    "CI-06 Order Block lifecycle contracts PASS" in contracts
)

check(
    "CI-06 is accumulated after CI-05",
    "audit_phase_ci_05.py" in workflow and
    "audit_phase_ci_06.py" in workflow and
    workflow.index("audit_phase_ci_06.py") > workflow.index("audit_phase_ci_05.py")
)

check(
    "audio delivery remains single-owned and centralized",
    "Notifications.PlaySound(" not in alert_engine and
    alert_processor.count("Notifications.PlaySound(") == 3
)

check(
    "exactly one production Order Block lifecycle owner exists",
    sum("class OrderBlockLifecycleRule" in p.read_text(encoding="utf-8") for p in production_files) == 1 and
    sum("enum OrderBlockLifecycleState" in p.read_text(encoding="utf-8") for p in production_files) == 1
)

check(
    "no second production Order Block definition owner exists",
    sum("class OrderBlockRule" in p.read_text(encoding="utf-8") for p in production_files) == 1 and
    sum("OrderBlockRule.Calculate(" in p.read_text(encoding="utf-8") for p in production_files) == 0
)

print("CI-06 ORDER BLOCK LIFECYCLE SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-06 STATIC GATE PASS")
