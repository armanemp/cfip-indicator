#!/usr/bin/env python3
"""Static acceptance gate for CI-13 TP source, obstacle and ladder integrity."""

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors = []


production_source = "\n".join(
    p.read_text(encoding="utf-8")
    for p in (ROOT / "src/CFIP.Indicator").rglob("*.cs")
)


def read(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    if not condition:
        errors.append(name)


selector = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetSelector.cs"
)
stage_builder = read(
    "src/CFIP.Indicator/Planning/TradePlan/TargetLadderStageCandidateBuilder.cs"
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
canonical_path_builder = read(
    "src/CFIP.Indicator/Planning/TradePlan/CanonicalTradePathGeometryBuilder.cs"
)
execution_rebuild = read(
    "src/CFIP.Indicator/Trading/Execution/ExecutionPlanPreparation.cs"
)
live_fill_reconcile = read(
    "src/CFIP.Indicator/Trading/Lifecycle/LiveFillExitReconciler.cs"
)
live_enrichment = read(
    "src/CFIP.Indicator/Trading/Lifecycle/LivePlanTargetEnrichment.cs"
)
live_target_candidate = read(
    "src/CFIP.Indicator/Trading/LiveManagement/LiveTargetCandidateEvaluator.cs"
)
live_target_progression = read(
    "src/CFIP.Indicator/Trading/LiveManagement/TargetProgression.cs"
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
    "TryBuildTargetLadderStageOptions(" in selector,
)

check(
    "ladder stage builder owns target candidate collection",
    "TryScoreTargetCandidate(" in stage_builder and
    "new TargetLadderOption(" in stage_builder,
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
    "no legacy TP metadata owner or caller survives in production",
    "ApplyTargetMeta(" not in production_source and
    "private double FindImprovedLiveTarget(" not in production_source,
)

check(
    "target metadata has one resolver and no proximity-based production owner",
    metadata.count("ApplyResolvedTargetMeta(") >= 2 and
    "ApplyTargetMeta(" not in metadata and
    "List<Level> candidates" not in metadata and
    "atr * 0.15" not in metadata,
)

check(
    "execution rebuild binds TP metadata to selected ladder provenance",
    "ApplySelectedTargetMeta(" in execution_rebuild and
    "selected" in execution_rebuild and
    "ApplyTargetMeta(" not in execution_rebuild,
)

check(
    "live-fill reconciliation preserves prior provenance or uses selected source",
    "ApplyResolvedTargetMeta(" in live_fill_reconcile and
    "referencePlan?.Tp1Source" in live_fill_reconcile and
    "ApplyTargetMeta(" not in live_fill_reconcile,
)

check(
    "live target enrichment preserves prior provenance or binds selected source",
    "ApplyResolvedTargetMeta(" in live_enrichment and
    "existingTp2Source" in live_enrichment and
    "existingTp3Source" in live_enrichment and
    "existingTp4Source" in live_enrichment and
    "ApplyTargetMeta(" not in live_enrichment,
)

check(
    "live target candidate evaluator returns the exact selected source object",
    "private Level FindImprovedLiveTarget(" in live_target_candidate and
    "best = level;" in live_target_candidate,
)

check(
    "live target progression records exact source provenance without legacy inference",
    "Level bestLevel" in live_target_progression and
    "ApplyExactTargetMeta(" in live_target_progression and
    "ApplyTargetMeta(" not in live_target_progression,
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
    "TargetLadderSelectionRule.SelectBestPath(" in contracts and
    "VerifyTargetLadderSelection();" in contracts,
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
    "roadmap records the CI-13 completed closeout",
    "### CI-13 implementation record" in roadmap and
    "Status: **VERIFIED COMPLETE — PR #168 merged to `main`." in roadmap,
)

check(
    "roadmap records CI-14 historical closeout and current certification state",
    "CI-14" in roadmap and
    "## 2.0.1 — Current certification state" in roadmap and
    ("CI-17A" in roadmap or "CI-17" in roadmap)
)

if errors:
    print("CI-13 SOURCE / ARCHITECTURE AUDIT FAILED")
    for error in errors:
        print(" - " + error)
    raise SystemExit(1)

print("CI-13 SOURCE / ARCHITECTURE AUDIT PASS")
