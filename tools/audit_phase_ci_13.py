#!/usr/bin/env python3
"""Static acceptance gate for CI-13 TP source, obstacle and ladder integrity."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    if not condition:
        errors.append(name)


selector = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs"
)
evaluator = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetCandidateEvaluator.cs"
)
merger = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetLevelMerger.cs"
)
metadata = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetMetadataEnricher.cs"
)
materialization = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanMaterialization.cs"
)
builder = read(
    "src/CFIP.Indicator/Planning/TradePlan/PlanBuilder.cs"
)
target_builder = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetLevelBuilder.cs"
)
htf_source = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/HtfTargetSource.cs"
)
smart_source = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/SmartExtraTargetSource.cs"
)
liquidity_source = read(
    "src/CFIP.Indicator/Planning/TradePlan/Sources/SupplyDemandLiquidityTargetSource.cs"
)
contracts = read("tools/CFIP.Planning.Contracts/Program.cs")
contract_project = read(
    "tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj"
)
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")

check(
    "TargetSelector delegates coherent path selection",
    "TargetLadderSelectionRule.SelectBestPath(" in selector and
    "new TargetLadderOption(" in selector,
)

check(
    "TargetSelector does not own candidate constraint mathematics",
    "TargetCandidateConstraintRule" not in selector and
    "!IsValidTarget(" not in selector,
)

check(
    "candidate evaluation has no selected-path mutable state",
    "List<Level> selected" not in evaluator and
    "selected.Any(" not in evaluator,
)

check(
    "candidate evaluator retains canonical obstacle validation",
    "EvaluateTargetObstacle(" in evaluator and
    "HasOpposingZonePathObstacle(" in evaluator and
    "HasHigherTfZonePathObstacle(" in evaluator,
)

check(
    "target clustering never fabricates a price",
    "match.Price =" not in merger and
    "NormalizePrice(" not in merger[merger.find("if (match == null)"):]
    if "if (match == null)" in merger else False,
)

check(
    "target clustering preserves representative provenance",
    "Preserve match.Price, Kind, Timeframe, Age and" in merger and
    "match.Timeframe =" not in merger and
    "match.SourceAgeMinutes =" not in merger and
    "match.Age =" not in merger and
    "match.Kind =" not in merger,
)

check(
    "authoritative plan metadata binds to selected source",
    "ApplySelectedTargetMeta(" in metadata and
    "SYNTHETIC_RR" in metadata,
)

check(
    "plan metadata no longer infers source from all candidates",
    "ApplyTargetMeta(" not in materialization and
    "List<Level> candidates" not in materialization,
)

check(
    "PlanBuilder passes selected ladder source objects",
    "EnrichPlanTargetMetadata(" in builder and
    "                p,\n                selected);" in builder,
)

for token in (
    "AddSupplyDemandAndLiquidityLevels(",
    "AddDailyPivotLevels(",
    "AddHtfTargets(",
    "AddPreviousPeriodLevels(",
    "AddSmartExtraTargetLevels(",
):
    check(
        f"target source family retained: {token}",
        token in target_builder,
    )

check(
    "HTF source records source age",
    "TargetAgeSemanticsRule.ElapsedMinutes(" in htf_source and
    "sourceAgeMinutes" in htf_source,
)

check(
    "extended liquidity keeps canonical active/unbroken rule",
    "LiquiditySweepRule.IsActiveUnbrokenLevel(" in
    read(
        "src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityAboveTargetSource.cs"
    ) and
    "LiquiditySweepRule.IsActiveUnbrokenLevel(" in
    read(
        "src/CFIP.Indicator/Planning/TradePlan/Sources/LiquidityBelowTargetSource.cs"
    ),
)

check(
    "synthetic fallback remains explicit",
    "AllowSyntheticTargetFallback" in
    read("src/CFIP.Indicator/Planning/TradePlan/TargetStageSelector.cs") and
    "SYNTHETIC_RR" in metadata,
)

check(
    "CI-13 ladder contracts are executable",
    "VerifyTargetLadderSelection();" in contracts and
    "TargetLadderSelectionRule.SelectBestPath(" in contracts,
)

check(
    "CI-13 ladder contracts are compiled",
    "Core/Math/TargetLadderOption.cs" in contract_project and
    "Core/Math/TargetLadderSelectionRule.cs" in contract_project,
)

check(
    "CI-13 source audit is wired into accumulated gate",
    "audit_phase_ci_13.py" in workflow,
)

check(
    "roadmap contains CI-13 and CI-14 continuation markers",
    "CI-13" in roadmap and
    "CI-14" in roadmap,
)

if errors:
    print("CI-13 SOURCE / ARCHITECTURE AUDIT FAILED")
    for error in errors:
        print(" - " + error)
    raise SystemExit(1)

print("CI-13 SOURCE / ARCHITECTURE AUDIT PASS")
