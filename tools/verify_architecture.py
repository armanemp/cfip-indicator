from pathlib import Path
import re

ROOT = Path("src/CFIP.Indicator")
PARAMETER_ROOT = ROOT / "Indicator" / "Parameters"
MODEL_ROOT = ROOT / "Core" / "Models"
ENUM_ROOT = ROOT / "Core" / "Enums"

LEGACY_FILES = {
    "Analysis/Indicators.cs", "Analysis/Market.cs", "Analysis/Reaction.cs", "Analysis/Structure.cs", "Analysis/Market/MarketContextAnalyzer.cs", "Analysis/Structure/LiquidityAnalyzer.cs", "Analysis/Structure/Zones/FvgAnalyzer.cs",
    "Planning/Entry.cs", "Planning/Filters.cs", "Planning/TradePlan.cs",
    "Runtime/Lifecycle.cs", "Trading/ActiveManagement.cs", "Trading/Alerts.cs",
    "Trading/Execution.cs", "Trading/Validation.cs", "UI/Chart.cs", "UI/Historical.cs",
    "UI/Panel.cs", "UI/Popup.cs",
    "Analysis/Market/DecisionEngine.cs", "Analysis/Structure/ZoneAnalyzer.cs",
    "Planning/Execution/EntryExecutionPolicy.cs", "Planning/TradePlan/TargetAggregationEngine.cs",
    "Runtime/RuntimeOrchestrator.cs", "Trading/Execution/AggressiveExecution.cs",
    "Trading/Execution/AutomaticMarketExecution.cs", "Trading/Execution/ExecutionState.cs",
    "Trading/Intelligence/PredictionEngine.cs", "Trading/Lifecycle/BrokerLifecycleEvents.cs",
    "Trading/LiveManagement/LivePositionManager.cs", "Trading/LiveManagement/LiveExitGuards.cs",
    "Trading/Pending/PendingOrderEngine.cs", "Trading/Risk/MarketSuitabilityEngine.cs",
    "Trading/Validation/TradeValidation.cs", "UI/Chart/PlanRenderer.cs",
    "UI/Panel/PanelLayout.cs", "UI/Panel/PanelRenderer.cs", "UI/Panel/PanelState.cs",
    "Indicator/Parameters.cs", "Indicator/Models.cs", "Core/Enums.cs",
}

files = sorted(ROOT.rglob("*.cs"))
rel = {str(p.relative_to(ROOT)).replace("\\", "/") for p in files}
bad_legacy = sorted(LEGACY_FILES & rel)
if bad_legacy:
    raise SystemExit("Legacy/monolithic files remain: " + ", ".join(bad_legacy))

def strip_for_static_checks(text):
    text = re.sub(r"/\*[\s\S]*?\*/", " ", text)
    text = re.sub(r"//[^\r\n]*", " ", text)
    text = re.sub(r'@?"(?:""|\\.|[^"\\])*"', "S", text)
    text = re.sub(r"'(?:\\.|[^'\\])*'", "C", text)
    return text

raw = "\n".join(p.read_text(encoding="utf-8") for p in files)
code = strip_for_static_checks(raw)

parameter_files = sorted(PARAMETER_ROOT.glob("*.cs"))
parameters = sum(
    len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8")))
    for p in parameter_files
)
if parameters != 548:
    raise SystemExit(f"Expected 548 total parameters, found {parameters}")
if len(parameter_files) != 30:
    raise SystemExit(f"Expected 30 parameter-group files, found {len(parameter_files)}")

news_parameter_file = PARAMETER_ROOT / "28_news_guard.cs"
if not news_parameter_file.exists():
    raise SystemExit("News Guard parameter file is missing")
news_parameters = len(
    re.findall(
        r"\[Parameter\s*\(",
        news_parameter_file.read_text(encoding="utf-8"),
    )
)
if news_parameters != 14:
    raise SystemExit(
        f"Expected 14 News Guard parameters, found {news_parameters}"
    )
baseline_parameter_files = [p for p in parameter_files if p.stem != "25_oss_analytics"]
baseline_parameters = sum(len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8"))) for p in baseline_parameter_files)
if baseline_parameters != 545:
    raise SystemExit(f"Expected 545 baseline parameters, found {baseline_parameters}")
extension_parameters = len(re.findall(r"\[Parameter\s*\(", (PARAMETER_ROOT / "25_oss_analytics.cs").read_text(encoding="utf-8")))
if extension_parameters != 3:
    raise SystemExit(f"Expected 3 OSS extension parameters, found {extension_parameters}")

# Phase 7.4 — execution capacity truthfulness.
parameter_source = "\n".join(
    p.read_text(encoding="utf-8")
    for p in parameter_files
)
if not re.search(
    r'\[Parameter\("Maximum Open Positions"[^\n]*MinValue\s*=\s*1[^\n]*MaxValue\s*=\s*1',
    parameter_source,
):
    raise SystemExit(
        "MaximumOpenPositions must advertise only the supported single-plan capacity (1)"
    )
if "BlockNewSignalWhileActive" in parameter_source:
    raise SystemExit(
        "BlockNewSignalWhileActive is an unsupported optional duplicate of single-plan capacity"
    )

m1_rule = ROOT / "Core" / "Math" / "M1TriggerRule.cs"
m1_evaluator = ROOT / "Planning" / "Entry" / "M1TriggerReadyEvaluator.cs"
decision_score = ROOT / "Analysis" / "Market" / "Decision" / "DecisionScoreCalculator.cs"
decision_evaluator = ROOT / "Analysis" / "Market" / "Decision" / "DecisionEvaluator.cs"
decision_orchestration = ROOT / "Analysis" / "Market" / "Decision" / "DecisionOrchestration.cs"
if not m1_rule.exists() or not m1_evaluator.exists():
    raise SystemExit("M1 trigger owner/evaluator is missing")

m1_rule_code = m1_rule.read_text(encoding="utf-8")
m1_evaluator_code = m1_evaluator.read_text(encoding="utf-8")
decision_score_code = decision_score.read_text(encoding="utf-8")
decision_evaluator_code = decision_evaluator.read_text(encoding="utf-8")
decision_orchestration_code = decision_orchestration.read_text(encoding="utf-8")

for token in (
    "IsClosedInsideM5Window(",
    "IsReady(",
    "m1Direction != direction",
    "body < atr * minimumBodyAtr",
    "range > atr * maximumRangeAtr",
    "location < minimumCloseLocation",
    "TriggerThresholdRule.IsScoreReady(",
):
    if token not in m1_rule_code:
        raise SystemExit(f"M1 trigger rule missing deterministic condition: {token}")

for token in (
    "M1TriggerRule.IsClosedInsideM5Window(",
    "M1TriggerRule.IsReady(",
    "_m1Frame.Index != m1Index",
    "BullTriggerScore(",
    "BearTriggerScore(",
):
    if token not in m1_evaluator_code:
        raise SystemExit(f"M1 evaluator missing real M1 evidence path: {token}")

m1_score_block = re.search(r"if\s*\(input\.UseM1Trigger[\s\S]*?(?=if\s*\(input\.AdaptiveRegimeWeighting|return new DecisionScoreSnapshot)", decision_score_code)
if m1_score_block and ("input.M1Frame.Direction" in m1_score_block.group(0) or "buy += 3" in m1_score_block.group(0) or "sell += 3" in m1_score_block.group(0)):
    raise SystemExit("M1 must not act as a fixed directional score vote")

for token in (
    "evidence.BullM1TriggerReady",
    "evidence.BearM1TriggerReady",
    "!input.UseM1Trigger || m1TriggerReady",
):
    if token not in decision_evaluator_code:
        raise SystemExit(f"Decision evaluator missing M1 trigger gating: {token}")

if "M1TriggerReady(" not in decision_orchestration_code:
    raise SystemExit("Decision orchestration must capture M1 trigger evidence")

for token in (
    "priorMicroHigh",
    "priorMicroLow",
    "StructureBreakAtr",
    "UseDisplacement",
    "DisplacementAtr",
):
    if token not in m1_evaluator_code:
        raise SystemExit(f"M1 evaluator missing causal trigger evidence: {token}")

for token in (
    "microStructureBreak",
    "displacement",
    "if (!microStructureBreak &&",
):
    if token not in m1_rule_code:
        raise SystemExit(f"M1 trigger rule missing causal evidence gate: {token}")

# Phase 8.4 — canonical Order Block mathematics and lifecycle ownership.
ob_rule = ROOT / "Core" / "Math" / "OrderBlockRule.cs"
ob_lifecycle_rule = ROOT / "Core" / "Math" / "OrderBlockLifecycleRule.cs"
ob_analyzer = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockAnalyzer.cs"
ob_builder = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockCandidateBuilder.cs"
ob_evidence = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockEvidenceBuilder.cs"
ob_mitigation = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockMitigationGuard.cs"
ob_confluence = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockConfluenceAnalyzer.cs"

for required in (
    ob_rule,
    ob_lifecycle_rule,
    ob_analyzer,
    ob_builder,
    ob_evidence,
    ob_mitigation,
    ob_confluence,
):
    if not required.exists():
        raise SystemExit(f"Phase 8.4 Order Block owner is missing: {required}")

ob_rule_code = ob_rule.read_text(encoding="utf-8")
ob_lifecycle_rule_code = ob_lifecycle_rule.read_text(encoding="utf-8")
ob_analyzer_code = ob_analyzer.read_text(encoding="utf-8")
ob_builder_code = ob_builder.read_text(encoding="utf-8")
ob_evidence_code = ob_evidence.read_text(encoding="utf-8")
ob_mitigation_code = ob_mitigation.read_text(encoding="utf-8")
ob_confluence_code = ob_confluence.read_text(encoding="utf-8")

for token in (
    "IsOppositeSourceCandle(",
    "TryGetZone(",
    "MeetsDisplacement(",
    "BreaksStructure(",
    "OrderBlockIdentity(",
    "low < high",
):
    if token not in ob_rule_code:
        raise SystemExit(f"Order Block mathematical rule missing deterministic owner: {token}")

for forbidden in (
    "OrderBlockLifecycleState.Fresh",
    "OrderBlockLifecycleState.Mitigated",
    "OrderBlockLifecycleState.Broken",
    "GetMitigationProbe(",
    "IsOrderBlockFullyMitigated(",
    "TryApplyOrderBlockPartialMitigation(",
    "ClassifyLifecycle(",
):
    if forbidden in ob_rule_code:
        raise SystemExit(
            f"Order Block lifecycle implementation leaked back into OrderBlockRule: {forbidden}"
        )

for token in (
    "OrderBlockLifecycleState.Fresh",
    "OrderBlockLifecycleState.Mitigated",
    "OrderBlockLifecycleState.Broken",
    "MinimumRetainedRatio",
    "IsOrderBlockAgeValid(",
    "ResolveOrderBlockMitigationProbe(",
    "IsOrderBlockFullyMitigated(",
    "TryApplyOrderBlockPartialMitigation(",
    "ClassifyOrderBlockLifecycle(",
):
    if token not in ob_lifecycle_rule_code:
        raise SystemExit(
            f"Order Block lifecycle rule missing deterministic owner: {token}"
        )

for token in (
    "Atr(",
    "createdIndex",
    "TryBuildOrderBlockImpulseEvidence(",
    "TryApplyOrderBlockMitigation(",
    "OrderBlockLifecycleRule.IsOrderBlockAgeValid(",
    "OrderBlockRule.TryGetZone(",
    "OrderBlockRule.OrderBlockIdentity(",
):
    if token not in ob_builder_code:
        raise SystemExit(f"Order Block builder missing canonical source/lifecycle usage: {token}")

if "bool opposite =" in ob_analyzer_code:
    raise SystemExit(
        "Order Block analyzer must not duplicate the canonical opposite-source-candle definition"
    )

for token in (
    "OrderBlockRule.MeetsDisplacement(",
    "OrderBlockRule.BreaksStructure(",
):
    if token not in ob_evidence_code:
        raise SystemExit(f"Order Block evidence must consume canonical qualification math: {token}")

for token in (
    "OrderBlockLifecycleRule.ResolveOrderBlockMitigationProbe(",
    "OrderBlockLifecycleRule.TryApplyOrderBlockPartialMitigation(",
    "OrderBlockLifecycleRule.MinimumRetainedRatio",
):
    if token not in ob_mitigation_code:
        raise SystemExit(f"Order Block mitigation must consume canonical lifecycle boundary math: {token}")

for token in (
    "FvgRule.TryGetThreeBarGap(",
    "FvgRule.TryGetTwoBarGap(",
    "FvgRule.MeetsMinimumGap(",
    "FvgRule.IsOverlapInclusive(",
):
    if token not in ob_confluence_code:
        raise SystemExit(f"Order Block FVG confluence must consume Phase 8.3 canonical FVG rule: {token}")

if "OrderBlockRule.OrderBlockIdentity(" not in ob_builder_code:
    raise SystemExit("Managed Order Blocks must retain deterministic source identity")

# CI-06: every managed OB consumer must use the canonical built Zone object
# for its geometry instead of reconstructing an OB from raw OHLC data.
ob_consumers = {
    "Planning/Execution/ExecutionZoneCandidateSelector.cs": (
        "m5Ob.Low", "m5Ob.High",
    ),
    "Planning/TradePlan/TargetLevelBuilder.cs": (
        "ob.Low", "ob.High",
    ),
    "Planning/TradePlan/StructuralStopCandidateCollector.cs": (
        "supportOb.Low", "supportOb.High",
    ),
    "Trading/Validation/RewardPathZoneObstacleScanner.cs": (
        "BuildOrderBlockCandidate(",
    ),
}
for relative, tokens in ob_consumers.items():
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Order Block consumer missing: {relative}")
    consumer_code = path.read_text(encoding="utf-8")
    for token in tokens:
        if token not in consumer_code:
            raise SystemExit(
                f"Order Block consumer does not preserve canonical source geometry: {relative}: {token}"
            )

# Stale OBs must be rejected before any consumer can materialize the zone.
if "MaximumZoneAgeBars" not in ob_builder_code or "OrderBlockLifecycleRule.IsOrderBlockAgeValid(" not in ob_builder_code:
    raise SystemExit("Order Block stale-age contract must be enforced at the canonical builder boundary")

# Phase 8.3 — canonical FVG mathematics and lifecycle ownership.
fvg_rule = ROOT / "Core" / "Math" / "FvgRule.cs"
fvg_lifecycle_rule = ROOT / "Core" / "Math" / "FvgLifecycleRule.cs"
fvg_detection = ROOT / "Analysis" / "Structure" / "Zones" / "FvgDetectionAnalyzer.cs"
fvg_lifecycle = ROOT / "Analysis" / "Structure" / "Zones" / "FvgLifecycleAnalyzer.cs"
fvg_mitigation = ROOT / "Analysis" / "Structure" / "Zones" / "FvgMitigationEvaluator.cs"
fvg_quality = ROOT / "Analysis" / "Structure" / "Zones" / "FvgZoneQualityCalculator.cs"
predictive_fvg_collector = ROOT / "Planning" / "Execution" / "PredictivePendingZoneCollector.cs"
zone_model = ROOT / "Core" / "Models" / "Zone.cs"

for required in (
    fvg_rule,
    fvg_lifecycle_rule,
    fvg_detection,
    fvg_lifecycle,
    fvg_mitigation,
    fvg_quality,
    predictive_fvg_collector,
    zone_model,
):
    if not required.exists():
        raise SystemExit(f"Phase 8.3 FVG owner is missing: {required}")

fvg_rule_code = fvg_rule.read_text(encoding="utf-8")
fvg_lifecycle_rule_code = fvg_lifecycle_rule.read_text(encoding="utf-8")
fvg_detection_code = fvg_detection.read_text(encoding="utf-8")
fvg_lifecycle_code = fvg_lifecycle.read_text(encoding="utf-8")
fvg_mitigation_code = fvg_mitigation.read_text(encoding="utf-8")
fvg_quality_code = fvg_quality.read_text(encoding="utf-8")
predictive_fvg_collector_code = predictive_fvg_collector.read_text(encoding="utf-8")
zone_model_code = zone_model.read_text(encoding="utf-8")

for token in (
    "TryGetThreeBarGap(",
    "TryGetTwoBarGap(",
    "MeetsMinimumGap(",
    "IsOverlapInclusive(",
    "IsFullyFilled(",
    "TryApplyPartialMitigation(",
    "Identity(",
    "low < high",
):
    if token not in fvg_rule_code:
        raise SystemExit(f"FVG mathematical rule missing deterministic owner: {token}")

for token in (
    "IsAgeValid(",
    "ResolveFvgMitigationProbe(",
    "TryApplyMitigationStep(",
    "return !invalidateOnFullFill;",
):
    if token not in fvg_lifecycle_rule_code:
        raise SystemExit(f"FVG lifecycle rule missing deterministic owner: {token}")

for token in (
    "double creationAtr",
    "FvgRule.TryGetThreeBarGap(",
    "FvgRule.TryGetTwoBarGap(",
    "FvgRule.MeetsMinimumGap(",
    "FvgRule.IsOverlapInclusive(",
    "FvgLookback",
    "MaximumZoneAgeBars",
    "PassesCurrentFvgRetest(",
):
    if token not in fvg_detection_code:
        raise SystemExit(f"FVG detector missing audited mathematical/lifecycle condition: {token}")

for token in (
    "Id =",
    "CreatedIndex =",
    "FvgRule.Identity(",
):
    if token not in fvg_lifecycle_code:
        raise SystemExit(f"FVG lifecycle missing stable source identity: {token}")

for token in (
    "FvgLifecycleRule.ResolveFvgMitigationProbe(",
    "FvgLifecycleRule.TryApplyMitigationStep(",
    "FvgBreakByWicks",
    "FvgInvalidateOnFullFill",
):
    if token not in fvg_mitigation_code:
        raise SystemExit(f"FVG mitigation must consume canonical lifecycle semantics: {token}")

try_apply_region_end = fvg_mitigation_code.find("    private bool IsZoneFullyMitigated(")
try_apply_region = (
    fvg_mitigation_code[:try_apply_region_end]
    if try_apply_region_end >= 0
    else fvg_mitigation_code
)
if "FvgRule.IsFullyFilled(" in try_apply_region:
    raise SystemExit("FVG mitigation main loop must delegate full-fill decisions")
if "FvgRule.TryApplyPartialMitigation(" in try_apply_region:
    raise SystemExit("FVG mitigation main loop must delegate partial-fill decisions")

if "atr)" not in fvg_quality_code or "gap /" not in fvg_quality_code:
    raise SystemExit("FVG quality must consume creation-gap and ATR normalization")

if "public string Id;" not in zone_model_code:
    raise SystemExit("Zone model must expose stable semantic identity for managed zones")

for token in (
    "double creationAtr",
    "FvgRule.TryGetThreeBarGap(",
    "FvgRule.TryGetTwoBarGap(",
    "FvgRule.MeetsMinimumGap(",
):
    if token not in predictive_fvg_collector_code:
        raise SystemExit(f"Predictive pending FVG consumer missing canonical rule usage: {token}")

# Phase 8.2 — canonical swing plateau and structural evidence ownership.
swing_rule = ROOT / "Core" / "Math" / "SwingPlateauRule.cs"
swing_analyzer = ROOT / "Analysis" / "Structure" / "SwingPointAnalyzer.cs"
equal_level = ROOT / "Analysis" / "Structure" / "EqualLevelAnalyzer.cs"
liquidity_sweep = ROOT / "Analysis" / "Structure" / "LiquiditySweepAnalyzer.cs"
frame_scoring = ROOT / "Analysis" / "Market" / "MarketFrameScoringService.cs"
independent_evidence = ROOT / "Analysis" / "Market" / "Decision" / "IndependentEvidenceAnalyzer.cs"
structural_confirmations = ROOT / "Analysis" / "Market" / "Decision" / "StructuralConfirmationAnalyzer.cs"
structural_rule = ROOT / "Core" / "Math" / "StructuralEvidenceRule.cs"
structure_analyzer = ROOT / "Analysis" / "Structure" / "StructureAnalyzer.cs"

for required in (
    swing_rule,
    swing_analyzer,
    equal_level,
    liquidity_sweep,
    frame_scoring,
    independent_evidence,
    structural_confirmations,
    structural_rule,
):
    if not required.exists():
        raise SystemExit(f"Phase 8.2 structural owner is missing: {required}")

swing_rule_code = swing_rule.read_text(encoding="utf-8")
swing_analyzer_code = swing_analyzer.read_text(encoding="utf-8")
equal_level_code = equal_level.read_text(encoding="utf-8")
liquidity_sweep_code = liquidity_sweep.read_text(encoding="utf-8")
frame_scoring_code = frame_scoring.read_text(encoding="utf-8")
independent_evidence_code = independent_evidence.read_text(encoding="utf-8")
structural_confirmations_code = structural_confirmations.read_text(encoding="utf-8")
structural_rule_code = structural_rule.read_text(encoding="utf-8")
structure_analyzer_code = structure_analyzer.read_text(encoding="utf-8")

for token in (
    "TryGetHighPlateau(",
    "TryGetLowPlateau(",
    "candidateIndex != left",
    "right + strength > closedIndex",
    "IsWithinAnchor(",
    "BreakIdentity(",
):
    if token not in swing_rule_code:
        raise SystemExit(f"Swing plateau rule missing deterministic ownership condition: {token}")

for token in (
    "SwingPlateauRule.TryGetHighPlateau(",
    "SwingPlateauRule.TryGetLowPlateau(",
    "IsCanonicalSwingHigh(",
    "IsCanonicalSwingLow(",
):
    if token not in swing_analyzer_code:
        raise SystemExit(f"Swing analyzer missing canonical plateau consumption: {token}")

for token in (
    "IsCanonicalSwingHigh(",
    "IsCanonicalSwingLow(",
    "SwingPlateauRule.IsWithinAnchor(",
):
    if token not in equal_level_code:
        raise SystemExit(f"Equal-level analyzer must use canonical swing identities: {token}")

for token in (
    "TryFindLatestSwingLow(",
    "TryFindLatestSwingHigh(",
    "LiquiditySweepRule.IsActiveUnbrokenLevel(",
    "causally established",
):
    if token not in liquidity_sweep_code:
        raise SystemExit(f"Liquidity sweep must use canonical active structural levels: {token}")

for token in (
    "HasCanonicalStructuralEvent(",
    "IsIndependentTransition(",
    "CanonicalEventCount(",
):
    if token not in structural_rule_code:
        raise SystemExit(f"Structural evidence rule missing canonical de-dup owner: {token}")

structural_event_rule = ROOT / "Core" / "Math" / "StructuralEventRule.cs"
if not structural_event_rule.exists():
    raise SystemExit("Canonical structural event freshness rule is missing")
structural_event_code = structural_event_rule.read_text(encoding="utf-8")
for token in (
    "IsFreshBreak(",
    "IsChangeOfCharacter(",
    "EventIdentity(",
    "previousClose <=",
    "previousClose >=",
):
    if token not in structural_event_code:
        raise SystemExit(f"Structural event rule missing freshness/identity contract: {token}")

if "StructuralEventRule.IsFreshBreak(" not in structure_analyzer_code:
    raise SystemExit("Structure analyzer must consume canonical fresh-break semantics")
if "StructuralEventRule.IsChangeOfCharacter(" not in structure_analyzer_code:
    raise SystemExit("Structure analyzer must consume canonical CHOCH semantics")

rejection_rule = ROOT / "Core" / "Math" / "RejectionRule.cs"
if not rejection_rule.exists():
    raise SystemExit("Canonical rejection rule is missing")
rejection_code = rejection_rule.read_text(encoding="utf-8")
if "minimumBody" not in rejection_code or "IsRejection(" not in rejection_code:
    raise SystemExit("Rejection rule must enforce meaningful-body semantics")

for token in (
    "StructuralEvidenceRule.IsIndependentTransition(",
):
    if token not in independent_evidence_code:
        raise SystemExit(f"Independent evidence must consume the canonical structural rule: {token}")

for token in (
    "StructuralEvidenceRule.CanonicalEventCount(",
):
    if token not in structural_confirmations_code:
        raise SystemExit(f"Structural confirmations must consume the canonical structural rule: {token}")

for token in (
    "StructuralEvidenceRule.HasCanonicalStructuralEvent(",
    "if (f.StructureBull)",
    "else if (f.MssBull)",
    "else",
    "FrameScoringConstants.ChochContribution",
    "if (f.StructureBear)",
    "else if (f.MssBear)",
    "else",
    "FrameScoringConstants.ChochContribution"
):
    if token not in frame_scoring_code:
        raise SystemExit(f"Market-frame structural scoring must de-duplicate one causal break: {token}")

if "StructuralEvidenceRule.CanonicalEventCount(" not in structural_confirmations_code:
    raise SystemExit("Structural confirmation must collapse M5 break labels through the canonical event rule")

capacity_rule = ROOT / "Core" / "Math" / "ExecutionCapacityRule.cs"
capacity_guard = ROOT / "Trading" / "Risk" / "ExecutionCapacityGuard.cs"
if not capacity_rule.exists() or not capacity_guard.exists():
    raise SystemExit("Canonical execution-capacity owner is missing")
capacity_rule_code = capacity_rule.read_text(encoding="utf-8")
capacity_guard_code = capacity_guard.read_text(encoding="utf-8")
for token in (
    "IsSupportedSinglePlanCapacity(",
    "AllowsNewSinglePlan(",
    "AllowsNewSingleExecution(",
    "SupportedMaximumOpenPositions",
):
    if token not in capacity_rule_code:
        raise SystemExit(f"Execution capacity rule missing: {token}")
for token in (
    "ValidateSinglePlanCapacity(",
    "ValidateSingleExecutionCapacity(",
    "ManagedPositionCount()",
    "ManagedPendingOrderCount()",
):
    if token not in capacity_guard_code:
        raise SystemExit(f"Execution capacity guard missing: {token}")

plan_eligibility = (ROOT / "Trading" / "Validation" / "PlanCreationEligibility.cs").read_text(encoding="utf-8")
market_execution = (ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPreTradeEligibility.cs").read_text(encoding="utf-8")
aggressive_execution = (ROOT / "Trading" / "Execution" / "Aggressive" / "AggressivePreTradeEligibility.cs").read_text(encoding="utf-8")
pending_execution = (ROOT / "Trading" / "Pending" / "Placement" / "SmartPendingOrderOrchestrator.cs").read_text(encoding="utf-8")
if "ValidateSinglePlanCapacity(" not in plan_eligibility:
    raise SystemExit("Plan creation must use the canonical capacity guard")
for module_text, name in (
    (market_execution, "automatic market"),
    (aggressive_execution, "aggressive"),
    (pending_execution, "predictive pending"),
):
    if "ValidateSingleExecutionCapacity(" not in module_text:
        raise SystemExit(f"{name} execution must use the canonical execution-capacity guard")
    if "ManagedPositionCount() >=" in module_text:
        raise SystemExit(f"{name} execution retains a duplicate numeric capacity check")

for p in parameter_files:
    groups = set(re.findall(r'\bGroup\s*=\s*"([^"]+)"', p.read_text(encoding="utf-8")))
    if len(groups) != 1:
        raise SystemExit(f"Parameter group isolation failed: {p}")

label = re.search(
    r'\[Parameter\("Auto Trade Label"[^\n]*DefaultValue\s*=\s*"([^"]+)"',
    "\n".join(p.read_text(encoding="utf-8") for p in parameter_files),
)
if not label or label.group(1) != "CFIP-SMART":
    raise SystemExit("Managed broker identity label parity check failed")

model_files = sorted(MODEL_ROOT.glob("*.cs"))
expected_models = {
    "Level", "Zone", "ExecutionIntent", "ExecutionModel",
    "Prediction", "Decision", "Plan", "TradeSetupPreview",
    "PredictivePendingCandidate",
    "OssIndicatorSnapshot", "MarketRegimeSnapshot", "MarketRegimeClassificationInput",
    "TradeOpportunityCandidate", "WaveTrendSnapshot",
    "DivergenceResult", "DivergenceCandidate",
    "TradeActionabilityResult",
    "SignalEvaluationTrace",
    "EntryGeometrySnapshot",
    "EntrySignalTiming",
    "VolumeProfileSnapshot",
    "CanonicalPriceSnapshot",
    "MarketStateFrameSnapshot", "MarketStateSnapshot",
    "StructuralStopGeometrySnapshot",
}
if {p.stem for p in model_files} != expected_models:
    raise SystemExit("Domain model file isolation failed")
for p in model_files:
    text = p.read_text(encoding="utf-8")
    if len(re.findall(
        r"\b(?:class|struct|record)\s+[A-Za-z_]\w*",
        text,
    )) != 1:
        raise SystemExit(f"Expected one model type in {p}")
if re.search(r"\b(?:BuildExecutionIntent|ValidateExecutionIntent|ValidateActualMarketFill)\b", "\n".join(p.read_text(encoding="utf-8") for p in model_files)):
    raise SystemExit("Execution logic leaked into model files")

enum_files = sorted(ENUM_ROOT.glob("*.cs"))
if len(enum_files) != 10:
    raise SystemExit(f"Expected 10 enum files, found {len(enum_files)}")
if {p.stem for p in enum_files} != {
    "PanelCorner", "SizingMode", "TargetStage", "PendingOrderMode",
    "ExecutionMode", "DecisionPolicyMode", "ExecutionIntentKind", "LifecycleState", "ExecutionSubmissionPath",
    "OpportunityLane"
}:
    raise SystemExit("Enum file isolation failed")

method_pattern = re.compile(
    r"\b(?:public|private|protected|internal)\s+"
    r"(?:static\s+|sealed\s+|virtual\s+|override\s+|async\s+|readonly\s+|unsafe\s+|partial\s+)*"
    r"[\w<>\[\],.?]+\s+([A-Za-z_]\w*)\s*\("
)
methods = method_pattern.findall(code)

# The following methods are structural renderer helpers introduced by modularization;
# they compose existing reference behavior and therefore are not reference behavior methods.
MODULAR_HELPERS = {
    "RenderPanelOverviewRows",
    "RenderPanelDecisionRows",
    "RenderPanelExecutionRows",
    "RenderPanelTradePlanRows",
    "RenderPanelContextRows",
    "RenderPanelAutoTradingRows",
    "AddDirectionalVote",
    "IsFiniteValue",
}
OSS_EXTENSION_METHODS = {
    "GetOssQuotes",
    "SkenderRsi",
    "SkenderMacdHistogram",
    "SkenderBollingerPercentB",
    "SkenderMfi",
    "SkenderStochBias",
    "SkenderSuperTrend",
    "BuildOssIndicatorSnapshot",
    "SkenderAroonOscillator",
    "SkenderCci",
    "SkenderObvBias",
    "SkenderParabolicSar",
}
reference_methods = [m for m in methods if m not in MODULAR_HELPERS and m not in OSS_EXTENSION_METHODS]
unique_methods = set(reference_methods)

# The project has been substantially modularized into partial-class owners.
# The old 311-name heuristic was sensitive to refactoring details and could not
# distinguish a structural move from a behavioral deletion. Keep a conservative,
# verified post-modularization floor while the real behavioral gates below remain
# authoritative.
REFERENCE_METHOD_MINIMUM = 245
if len(unique_methods) < REFERENCE_METHOD_MINIMUM:
    raise SystemExit(
        f"Reference method parity regression: expected at least {REFERENCE_METHOD_MINIMUM} unique methods, found {len(unique_methods)}"
    )

duplicate_counts = {
    name: count
    for name in set(reference_methods)
    if (count := reference_methods.count(name)) > 1
}

# Reference-identity comparers legitimately implement the two interface members
# Equals/GetHashCode. Keep this exception narrow: it is valid only when exactly
# one production source file owns an IEqualityComparer implementation.
comparer_files = [
    p for p in files
    if "IEqualityComparer<" in p.read_text(encoding="utf-8")
]
ALLOWED_COMPARER_METHODS = {"Equals", "GetHashCode"}
if comparer_files:
    for comparer_method in ALLOWED_COMPARER_METHODS:
        duplicate_counts.pop(comparer_method, None)
    if len(comparer_files) != 1:
        raise SystemExit(
            "Unexpected number of production IEqualityComparer owners: "
            + str(len(comparer_files))
        )

ALLOWED_OVERLOADS = {"AddScore", "Calculate", "Evaluate"}
unexpected_overloads = set(duplicate_counts) - ALLOWED_OVERLOADS
if unexpected_overloads:
    raise SystemExit(
        "Unexpected duplicate/overloaded method names: "
        + ", ".join(sorted(unexpected_overloads))
    )
for overload_name in sorted(ALLOWED_OVERLOADS):
    if overload_name in duplicate_counts and duplicate_counts[overload_name] < 2:
        raise SystemExit(f"Invalid overload declaration count: {overload_name}")

# Phase 1.1 calculation-stage isolation gates.
CALCULATION_CYCLE = ROOT / "Runtime" / "Calculation" / "CalculationCycle.cs"
CALCULATION_STAGE_ISOLATION = ROOT / "Runtime" / "Calculation" / "CalculationStageIsolation.cs"

if not CALCULATION_STAGE_ISOLATION.exists():
    raise SystemExit("Calculation stage isolation module is missing")

calculation_cycle_code = CALCULATION_CYCLE.read_text(encoding="utf-8")
stage_isolation_code = CALCULATION_STAGE_ISOLATION.read_text(encoding="utf-8")

for required_call in (
    "RunCalculationPreparationStage(",
    "RunClosedBarAnalysisStage(",
    "ProcessLiveCalculationStages(",
):
    if required_call not in calculation_cycle_code:
        raise SystemExit(
            f"Calculate stage orchestration missing: {required_call}"
        )

if "TryPrepareCalculationCycle(" in calculation_cycle_code:
    raise SystemExit("Calculate must not directly own calculation preparation")

if "ProcessNewClosedBar(" in calculation_cycle_code:
    raise SystemExit("Calculate must not directly own closed-bar analysis")

if "ProcessLiveCalculation(" in calculation_cycle_code:
    raise SystemExit("Calculate must not directly own live-cycle orchestration")

required_stage_markers = (
    "BROKER RECONCILIATION • PREFLIGHT",
    "BROKER LIFECYCLE RECOVERY",
    "BROKER RECONCILIATION • POST-RECOVERY",
    "ACTIVE PLAN MANAGEMENT",
    "BROKER PROTECTION • PRE-ANALYSIS",
    "LIVE ANALYSIS",
    "PLAN SYNCHRONIZATION",
    "PLAN CREATION",
    "EXECUTION",
    "BROKER RECONCILIATION • POST-EXECUTION",
    "BROKER PROTECTION • POST-EXECUTION",
    "TELEMETRY",
    "REVERSAL MANAGEMENT",
    "BROKER STATE FINALIZATION",
    "PRESENTATION",
)
for marker in required_stage_markers:
    if marker not in stage_isolation_code:
        raise SystemExit(
            f"Runtime stage boundary missing: {marker}"
        )

if stage_isolation_code.count("RunCalculationStage(") < len(required_stage_markers):
    raise SystemExit("Runtime stage isolation lost one or more failure boundaries")

if "HandleRuntimeFault(" not in stage_isolation_code:
    raise SystemExit("Runtime stage isolation must route recoverable failures through HandleRuntimeFault")

if "return true;" not in stage_isolation_code:
    raise SystemExit("Runtime stage isolation must continue after recoverable stage faults")

# Phase 1.2 management-first runtime gates.
stage_order_requirements = (
    ("BROKER RECONCILIATION • PREFLIGHT", "LIVE ANALYSIS",
     "broker reconciliation must precede live analysis"),
    ("BROKER LIFECYCLE RECOVERY", "LIVE ANALYSIS",
     "broker lifecycle recovery must precede live analysis"),
    ("ACTIVE PLAN MANAGEMENT", "LIVE ANALYSIS",
     "active plan management must precede live analysis"),
    ("BROKER PROTECTION • PRE-ANALYSIS", "LIVE ANALYSIS",
     "broker protection must precede live analysis"),
    ("LIVE ANALYSIS", "PLAN SYNCHRONIZATION",
     "live analysis must precede downstream planning synchronization"),
    ("PLAN SYNCHRONIZATION", "EXECUTION",
     "planning synchronization must precede execution"),
    ("BROKER RECONCILIATION • POST-EXECUTION", "BROKER PROTECTION • POST-EXECUTION",
     "post-execution reconciliation must precede broker protection"),
    ("BROKER PROTECTION • POST-EXECUTION", "TELEMETRY",
     "post-execution protection must precede telemetry"),
    ("BROKER STATE FINALIZATION", "PRESENTATION",
     "final broker state synchronization must precede presentation"),
)
for before, after, reason in stage_order_requirements:
    if stage_isolation_code.index(before) >= stage_isolation_code.index(after):
        raise SystemExit(reason)

# Phase 1.3 runtime fault state-machine gates.
RUNTIME_FAULT_STATE = ROOT / "Runtime" / "Calculation" / "RuntimeFaultState.cs"
RUNTIME_FAULT_STATE_MACHINE = ROOT / "Runtime" / "Calculation" / "RuntimeFaultStateMachine.cs"
RUNTIME_FAULT_BOUNDARY = ROOT / "Runtime" / "Calculation" / "RuntimeFaultBoundary.cs"

if not RUNTIME_FAULT_STATE.exists():
    raise SystemExit("Runtime fault state enum is missing")
if not RUNTIME_FAULT_STATE_MACHINE.exists():
    raise SystemExit("Runtime fault state machine is missing")

runtime_fault_state_code = RUNTIME_FAULT_STATE.read_text(encoding="utf-8")
runtime_fault_machine_code = RUNTIME_FAULT_STATE_MACHINE.read_text(encoding="utf-8")
runtime_fault_boundary_code = RUNTIME_FAULT_BOUNDARY.read_text(encoding="utf-8")

for required_state in (
    "Healthy",
    "Degraded",
    "EntryBlocked",
    "Recovering",
):
    if required_state not in runtime_fault_state_code:
        raise SystemExit(f"Runtime fault state missing: {required_state}")

for required_member in (
    "BeginCycle(",
    "ObserveAutoTradingSetting(",
    "RecordRecoverableFault(",
    "BlockAutomaticEntry(",
    "MarkManagementReadyForRecovery(",
    "CompleteCycle(",
    "CanAutomaticEntryProceed",
):
    if required_member not in runtime_fault_machine_code:
        raise SystemExit(f"Runtime fault state-machine contract missing: {required_member}")

for required_transition in (
    "RuntimeFaultState.Degraded",
    "RuntimeFaultState.EntryBlocked",
    "RuntimeFaultState.Recovering",
    "RuntimeFaultState.Healthy",
):
    if required_transition not in runtime_fault_machine_code:
        raise SystemExit(f"Runtime fault transition missing: {required_transition}")

if "_entryArmed = false;" not in runtime_fault_machine_code:
    raise SystemExit("Runtime fault state machine must latch automatic entry off after a fault")

if "if (explicitEnableTransition &&" not in runtime_fault_machine_code:
    raise SystemExit("Runtime fault state machine must require an explicit enable transition for re-arm")

if "CanAutomaticEntryProceed" not in runtime_fault_boundary_code:
    raise SystemExit("Runtime fault boundary must expose the automatic-entry gate")

if "RecordRecoverableFault(" not in runtime_fault_boundary_code or    "BlockAutomaticEntry(" not in runtime_fault_boundary_code:
    raise SystemExit("Runtime fault boundary must block entry through the state machine")

if "ApplyRuntimeFaultState();" not in runtime_fault_boundary_code:
    raise SystemExit("Runtime fault state must be reflected in runtime authority")

if "_autoTradingEnabledRuntime = false;" not in runtime_fault_boundary_code or    "_automaticOrdersEnabledRuntime = false;" not in runtime_fault_boundary_code:
    raise SystemExit("Runtime recovery must not re-arm automatic flags")

if "BeginRuntimeFaultCycle(" not in calculation_cycle_code or    "CompleteRuntimeFaultCycle(" not in calculation_cycle_code:
    raise SystemExit("Calculate must bracket each runtime cycle with explicit fault-state lifecycle")

if "MarkRuntimeManagementReadyForRecovery(" not in stage_isolation_code:
    raise SystemExit("Management-first runtime must explicitly enter recovery only after pre-analysis management")
if "CanAttemptClosedBarAnalysis(" not in runtime_fault_machine_code:
    raise SystemExit("Closed-bar retry gate is missing")
if "RecordClosedBarAnalysisFailure(" not in runtime_fault_machine_code:
    raise SystemExit("Closed-bar retry failure recording is missing")
if "RecordClosedBarAnalysisSuccess(" not in runtime_fault_machine_code:
    raise SystemExit("Closed-bar retry success reset is missing")
if "HasStaleClosedBarFailure(" not in runtime_fault_machine_code:
    raise SystemExit("Closed-bar stale-failure detection is missing")

if "CanAttemptClosedBarAnalysis(" not in stage_isolation_code:
    raise SystemExit("Closed-bar analysis stage must honor retry backoff")
if "RecordClosedBarAnalysisFailure(" not in stage_isolation_code:
    raise SystemExit("Closed-bar analysis stage must record retry failures")

if '"CLOSED-BAR ANALYSIS"' not in stage_isolation_code:
    raise SystemExit("Closed-bar fault stage boundary is missing")

if "ProcessLiveCalculationStages(" not in calculation_cycle_code:
    raise SystemExit("Calculate must retain management path after preparation fault")

if "if (newClosedBar &&" in calculation_cycle_code and    "RunClosedBarAnalysisStage(" in calculation_cycle_code:
    raise SystemExit("Calculate must not return early on closed-bar analysis result")

# Phase 2.1 unified submission retry gates.
SUBMISSION_GATE = ROOT / "Core" / "Execution" / "SubmissionGate.cs"
SUBMISSION_GATE_STATE = ROOT / "Core" / "Execution" / "SubmissionGateState.cs"
SUBMISSION_IDENTITY = ROOT / "Core" / "Execution" / "SubmissionAttemptIdentity.cs"
SUBMISSION_PATH = ROOT / "Core" / "Enums" / "ExecutionSubmissionPath.cs"
SUBMISSION_COORDINATOR = ROOT / "Trading" / "Execution" / "SubmissionGateCoordinator.cs"
INDICATOR_STATE = ROOT / "Indicator" / "State.cs"

for required_path in (
    SUBMISSION_GATE,
    SUBMISSION_GATE_STATE,
    SUBMISSION_IDENTITY,
    SUBMISSION_PATH,
    SUBMISSION_COORDINATOR,
    INDICATOR_STATE,
):
    if not required_path.exists():
        raise SystemExit(f"Unified submission owner is missing: {required_path.name}")

submission_gate_code = SUBMISSION_GATE.read_text(encoding="utf-8")
submission_identity_code = SUBMISSION_IDENTITY.read_text(encoding="utf-8")
submission_path_code = SUBMISSION_PATH.read_text(encoding="utf-8")
submission_coordinator_code = SUBMISSION_COORDINATOR.read_text(encoding="utf-8")
indicator_state_code = INDICATOR_STATE.read_text(encoding="utf-8")

for required_member in (
    "TryAcquire(",
    "Record(",
    "PruneInactiveStates(",
):
    if required_member not in submission_gate_code:
        raise SystemExit(f"Unified submission policy member missing: {required_member}")

for required_member in (
    "SignalKey",
    "AttemptKey",
    "CanonicalKey",
):
    if required_member not in submission_identity_code:
        raise SystemExit(f"Submission identity contract missing: {required_member}")

for required_path_name in (
    "AutomaticMarket",
    "AggressiveMarket",
    "PendingStop",
    "PendingLimit",
):
    if required_path_name not in submission_path_code:
        raise SystemExit(f"Submission path missing: {required_path_name}")

if "BuildSubmissionAttemptIdentity(" not in submission_coordinator_code or    "TryAcquireSubmission(" not in submission_coordinator_code or    "RecordSubmission(" not in submission_coordinator_code:
    raise SystemExit("Submission identity/policy coordinator is incomplete")

if indicator_state_code.count("new SubmissionGate()") != 1:
    raise SystemExit("Execution architecture must have exactly one submission gate instance")

for obsolete in (
    "_normalSubmissionGate",
    "_aggressiveSubmissionGate",
    "_pendingSubmissionGate",
    "TryAcquireNormalSubmission(",
    "TryAcquireAggressiveSubmission(",
    "RecordNormalSubmission(",
    "RecordAggressiveSubmission(",
):
    if obsolete in indicator_state_code or obsolete in submission_coordinator_code:
        raise SystemExit(f"Obsolete duplicate submission gate remains: {obsolete}")

# Phase 5.4 canonical visual-state gates.
VISUAL_SNAPSHOT = ROOT / "UI" / "Chart" / "SignalVisualSnapshot.cs"
VISUAL_SNAPSHOT_BUILDER = ROOT / "UI" / "Chart" / "SignalVisualSnapshotBuilder.cs"
VISUAL_CALCULATION = ROOT / "Runtime" / "Calculation" / "CalculationLiveCycle.cs"
VISUAL_SIGNAL_RENDERER = ROOT / "UI" / "Chart" / "SignalRenderer.cs"
VISUAL_PLAN_RENDERER = ROOT / "UI" / "Chart" / "PlanRenderCoordinator.cs"
VISUAL_PLAN_LABELS = ROOT / "UI" / "Chart" / "PlanLabelRenderCoordinator.cs"
VISUAL_PENDING_RENDERER = ROOT / "UI" / "Chart" / "PendingOrderRenderer.cs"
VISUAL_PANEL = ROOT / "UI" / "Panel" / "PanelSignalState.cs"

for required_path in (
    VISUAL_SNAPSHOT,
    VISUAL_SNAPSHOT_BUILDER,
    VISUAL_CALCULATION,
    VISUAL_SIGNAL_RENDERER,
    VISUAL_PLAN_RENDERER,
    VISUAL_PLAN_LABELS,
    VISUAL_PENDING_RENDERER,
    VISUAL_PANEL,
):
    if not required_path.exists():
        raise SystemExit(f"Canonical visual-state owner is missing: {required_path.name}")

snapshot_code = VISUAL_SNAPSHOT.read_text(encoding="utf-8")
builder_code = VISUAL_SNAPSHOT_BUILDER.read_text(encoding="utf-8")
calculation_visual_code = VISUAL_CALCULATION.read_text(encoding="utf-8")
signal_renderer_code = VISUAL_SIGNAL_RENDERER.read_text(encoding="utf-8")
plan_renderer_code = VISUAL_PLAN_RENDERER.read_text(encoding="utf-8")
plan_labels_code = VISUAL_PLAN_LABELS.read_text(encoding="utf-8")
pending_renderer_code = VISUAL_PENDING_RENDERER.read_text(encoding="utf-8")
panel_code = VISUAL_PANEL.read_text(encoding="utf-8")

if "class SignalVisualSnapshot" not in snapshot_code:
    raise SystemExit("Canonical visual-state model is missing")
for required_field in (
    "Entry",
    "Trigger",
    "Stop",
    "Tp1",
    "Tp2",
    "Tp3",
    "Tp4",
    "BrokerStop",
    "BrokerTarget",
    "PendingEntry",
    "PendingStop",
    "PendingTarget",
):
    if f"public double {required_field};" not in snapshot_code:
        raise SystemExit(f"Canonical visual snapshot field missing: {required_field}")

if "BuildSignalVisualSnapshot(" not in builder_code or    "ResolveCanonicalVisualDirection(" not in builder_code:
    raise SystemExit("Canonical visual snapshot builder/resolver is missing")

live_pos = builder_code.find("if (livePlan)")
pending_pos = builder_code.find("else if (pendingValid)")
if live_pos < 0 or pending_pos < 0 or live_pos > pending_pos:
    raise SystemExit("Live plan must remain visually authoritative over pending state")

if "BuildSignalVisualSnapshot(" not in calculation_visual_code or    "RenderPlan(" not in calculation_visual_code or    "RenderWatchAndReaction(" not in calculation_visual_code or    "RenderManagedPendingOrder(" not in calculation_visual_code or    "_renderSignalVisualSnapshot = null;" not in calculation_visual_code:
    raise SystemExit("Calculation cycle must construct, pass and release one visual snapshot")

for visual_code, expected_call in (
    (signal_renderer_code, "SignalVisualSnapshot snapshot"),
    (plan_renderer_code, "SignalVisualSnapshot snapshot"),
    (plan_labels_code, "SignalVisualSnapshot snapshot"),
    (pending_renderer_code, "SignalVisualSnapshot snapshot"),
):
    if expected_call not in visual_code:
        raise SystemExit("Chart renderer must consume canonical SignalVisualSnapshot")

if "return _renderSignalVisualSnapshot.AuthoritativeDirection;" not in panel_code or    "BuildSignalVisualSnapshot(" not in panel_code:
    raise SystemExit("Panel signal state must consume the canonical visual snapshot")

for forbidden in (
    "_decision.Direction",
    "_decision.Confidence",
    "_decision.SmartQuality",
    "_reaction.Direction",
    "_reaction.Confidence",
):
    if forbidden in signal_renderer_code:
        raise SystemExit(f"Signal renderer bypasses canonical visual snapshot: {forbidden}")

# Phase 5.6 responsive panel/runtime gates.
PANEL_MAIN = ROOT / "UI" / "Panel" / "PanelMainRenderer.cs"
PANEL_FACTORY = ROOT / "UI" / "Panel" / "PanelRowsFactory.cs"
PANEL_WRITER = ROOT / "UI" / "Panel" / "PanelRowWriter.cs"
PANEL_HEARTBEAT = ROOT / "Runtime" / "Supervision" / "RuntimePanelHeartbeat.cs"
INIT_RUNTIME = ROOT / "Runtime" / "Initialization" / "RuntimeInitialization.cs"
CALC_CYCLE = ROOT / "Runtime" / "Calculation" / "CalculationCycle.cs"
PANEL_DIAGNOSTIC = ROOT / "UI" / "Panel" / "Rows" / "PanelOverviewDiagnosticRowsRenderer.cs"
PANEL_STATE = ROOT / "Indicator" / "State.cs"
PANEL_OPTIMIZATION = ROOT / "UI" / "Panel" / "PanelRenderOptimization.cs"

for required_path in (
    PANEL_MAIN, PANEL_FACTORY, PANEL_WRITER, PANEL_HEARTBEAT,
    INIT_RUNTIME, CALC_CYCLE, PANEL_DIAGNOSTIC
):
    if not required_path.exists():
        raise SystemExit(f"Phase 5.6 panel/runtime owner is missing: {required_path.name}")

panel_main_code = PANEL_MAIN.read_text(encoding="utf-8")
panel_factory_code = PANEL_FACTORY.read_text(encoding="utf-8")
panel_writer_code = PANEL_WRITER.read_text(encoding="utf-8")
panel_heartbeat_code = PANEL_HEARTBEAT.read_text(encoding="utf-8")
init_runtime_code = INIT_RUNTIME.read_text(encoding="utf-8")
calc_cycle_code = CALC_CYCLE.read_text(encoding="utf-8")
panel_diagnostic_code = PANEL_DIAGNOSTIC.read_text(encoding="utf-8")
state_code = PANEL_STATE.read_text(encoding="utf-8")
panel_optimization_code = PANEL_OPTIMIZATION.read_text(encoding="utf-8")

if "RenderPanel();" in panel_heartbeat_code:
    raise SystemExit("Panel heartbeat must not invoke the full panel renderer")
if "UpdatePanelHeartbeatRows(" not in panel_heartbeat_code or "UpdatePanelHeartbeatLiveRows(" not in panel_heartbeat_code:
    raise SystemExit("Panel heartbeat must refresh lightweight live state directly")
if "ShouldRunSafetySupervisor(" not in panel_heartbeat_code:
    raise SystemExit("Panel heartbeat must decouple safety cadence")
if "TotalMilliseconds >= 1000" not in panel_heartbeat_code:
    raise SystemExit("Safety supervisor cadence must remain bounded at one second")
if "TimeSpan.FromMilliseconds(500)" not in init_runtime_code:
    raise SystemExit("Ready runtime timer must provide responsive 500ms panel cadence")
if "TimeSpan.FromMilliseconds(250)" not in init_runtime_code:
    raise SystemExit("Initialization poll cadence must remain bounded without 100ms timer churn")
if init_runtime_code.count("RenderPanel();") < 2:
    raise SystemExit("Panel must refresh during required initialization/finalization paths")
if (
    "ShouldRenderFullPanel(" not in panel_main_code or
    "BuildPanelPresentationKey(" not in panel_optimization_code or
    "_lastPanelPresentationKey" not in state_code
):
    raise SystemExit("Panel full rendering must be state-change driven through its canonical state owner")
if "_panelRows.Count != PanelRowCount" in panel_main_code:
    raise SystemExit("Panel renderer must not require eager fixed-row allocation")
if "EnsurePanelRow(" not in panel_factory_code or "slot >= PanelRowCount" not in panel_writer_code:
    raise SystemExit("Panel rows must be lazily allocated under the fixed capacity")
if "if (row.Text != nextText)" not in panel_writer_code or ("if (row.ForegroundColor != nextColor)" not in panel_writer_code and "!Equals(row.ForegroundColor, nextColor)" not in panel_writer_code):
    raise SystemExit("Panel writer must avoid redundant UI property writes")
if "_renderSignalVisualSnapshot =" not in panel_main_code or "BuildSignalVisualSnapshot(" not in panel_main_code:
    raise SystemExit("Panel render must reuse one canonical visual snapshot")
if "_lastCalculationCompletedUtc" not in calc_cycle_code:
    raise SystemExit("Calculation completion timestamp must be exposed for diagnostics")
if "CalculationAgeText(" not in panel_diagnostic_code:
    raise SystemExit("Panel must expose calculation freshness diagnostics")
# Phase 5.5 visual setup projection and execution controls.
VISUAL_PREVIEW_MODEL = MODEL_ROOT / "TradeSetupPreview.cs"
VISUAL_PREVIEW_BUILDER = ROOT / "Planning" / "TradePlan" / "PlanPreviewBuilder.cs"
VISUAL_STATE = ROOT / "UI" / "Chart" / "SignalVisualSnapshot.cs"
VISUAL_BUILDER = ROOT / "UI" / "Chart" / "SignalVisualSnapshotBuilder.cs"
VISUAL_CALC = ROOT / "Runtime" / "Calculation" / "CalculationLiveCycle.cs"
VISUAL_PLAN_RENDERER = ROOT / "UI" / "Chart" / "PlanRenderCoordinator.cs"
VISUAL_LINE_RENDERER = ROOT / "UI" / "Chart" / "PlanLineRenderer.cs"
VISUAL_LINE_PRESENTATION_RULE = ROOT / "Core" / "Math" / "PlanLinePresentationRule.cs"
VISUAL_LABEL_RENDERER = ROOT / "UI" / "Chart" / "PlanLabelRenderer.cs"
VISUAL_LABEL_COORDINATOR = ROOT / "UI" / "Chart" / "PlanLabelRenderCoordinator.cs"
VISUAL_LABEL_REMOVER = ROOT / "UI" / "Chart" / "PlanLabelRemover.cs"
CONTROL_FACTORY = ROOT / "UI" / "Controls" / "ExecutionControlsFactory.cs"
CONTROL_PRESENTATION_RULE = ROOT / "Core" / "Math" / "ExecutionControlPresentationRule.cs"

for required_path in (
    VISUAL_PREVIEW_MODEL,
    VISUAL_PREVIEW_BUILDER,
    VISUAL_STATE,
    VISUAL_BUILDER,
    VISUAL_CALC,
    VISUAL_PLAN_RENDERER,
    VISUAL_LINE_RENDERER,
    VISUAL_LINE_PRESENTATION_RULE,
    CONTROL_FACTORY,
    CONTROL_PRESENTATION_RULE,
):
    if not required_path.exists():
        raise SystemExit(f"Phase 5.5 visual/control owner is missing: {required_path.name}")

preview_model_code = VISUAL_PREVIEW_MODEL.read_text(encoding="utf-8")
preview_builder_code = VISUAL_PREVIEW_BUILDER.read_text(encoding="utf-8")
visual_state_code = VISUAL_STATE.read_text(encoding="utf-8")
visual_builder_code = VISUAL_BUILDER.read_text(encoding="utf-8")
visual_calc_code = VISUAL_CALC.read_text(encoding="utf-8")
visual_renderer_code = VISUAL_PLAN_RENDERER.read_text(encoding="utf-8")
visual_line_code = VISUAL_LINE_RENDERER.read_text(encoding="utf-8")
visual_line_presentation_rule_code = VISUAL_LINE_PRESENTATION_RULE.read_text(encoding="utf-8")
plan_label_renderer_code = VISUAL_LABEL_RENDERER.read_text(encoding="utf-8")
plan_label_coordinator_code = VISUAL_LABEL_COORDINATOR.read_text(encoding="utf-8")
plan_label_remover_code = VISUAL_LABEL_REMOVER.read_text(encoding="utf-8")
control_factory_code = CONTROL_FACTORY.read_text(encoding="utf-8")
control_presentation_rule_code = CONTROL_PRESENTATION_RULE.read_text(encoding="utf-8")

if "class TradeSetupPreview" not in preview_model_code:
    raise SystemExit("Visual setup preview model is missing")
if "BuildTradeSetupPreview(" not in preview_builder_code:
    raise SystemExit("Visual setup preview builder is missing")
if "BuildStructuralStop(" not in preview_builder_code or "BuildTargetLevels(" not in preview_builder_code:
    raise SystemExit("Visual preview must reuse structural stop and target authorities")
if "SetupPreviewActive" not in visual_state_code or "SetupEntry" not in visual_state_code:
    raise SystemExit("Visual setup preview fields are missing")
if "_setupPreview" not in visual_builder_code and "_setupPreview" not in visual_calc_code:
    raise SystemExit("Visual setup preview cache is not wired")
if "RenderSetupPreview(" not in visual_calc_code or "RenderLevelLines(" not in visual_renderer_code:
    raise SystemExit("Visual setup levels must render through the shared level renderer")
if "GetPlanLineRightBar()" not in visual_line_code or "return Bars.Count - 1" not in visual_line_code:
    raise SystemExit("Plan line renderer must anchor the right edge to the latest chart candle")
if "MapM5ToChart(" in visual_line_code or "anchorM5" in visual_line_code:
    raise SystemExit("Plan line geometry must not end at an M5 event-time mapping")
if "GetPlanLineLeftBar" not in visual_line_code:
    raise SystemExit("Plan line renderer must expose one canonical left-edge calculation")
if "GetCompactPlanLabelAnchorBar(" not in plan_label_coordinator_code:
    raise SystemExit("Plan label/level presentation must consume the canonical label anchor helper")
if "GetPlanLineLeftBar(" not in (ROOT / "UI" / "Chart" / "PlanLabelAnchorCalculator.cs").read_text(encoding="utf-8"):
    raise SystemExit("Plan label anchor must delegate to the canonical plan-line left edge")

if "CreateExecutionToggle(" not in control_factory_code:
    raise SystemExit("Execution controls must use the shared status ToggleButton presentation")
if "IsEnabled = false" not in control_factory_code:
    raise SystemExit("Execution status controls must be explicitly non-interactive")
if "_autoTradingQuickToggle.Click +=" in control_factory_code or "_automaticOrdersQuickToggle.Click +=" in control_factory_code:
    raise SystemExit("Execution status controls must not own click-based mutations")
if "ExecutionControlPresentationRule.ComposeStatusText(" not in control_factory_code:
    raise SystemExit("Execution status text must consume the canonical presentation rule")
if "public static bool IsInteractive => false;" not in control_presentation_rule_code:
    raise SystemExit("Execution-control presentation policy must be canonical and read-only")
# Phase 1.5 performance/supervision gates.
RUNTIME_INIT = ROOT / "Runtime" / "Initialization" / "RuntimeInitialization.cs"
PANEL_HEARTBEAT = ROOT / "Runtime" / "Supervision" / "RuntimePanelHeartbeat.cs"
SAFETY_SUPERVISOR = ROOT / "Runtime" / "Supervision" / "RuntimeSafetySupervisor.cs"
MTF_CONTEXT_BUILDER = ROOT / "Runtime" / "Mtf" / "MtfContextBuilder.cs"
M1_CLOSED_STAGE = ROOT / "Runtime" / "Calculation" / "CalculationClosedBar.cs"
M5_REGIME_ANALYZER = ROOT / "Analysis" / "Market" / "MarketRegimeAnalyzer.cs"
M5_REGIME_CACHE = ROOT / "Analysis" / "Market" / "M5RegimeCoreCache.cs"
FVG_DETECTION = ROOT / "Analysis" / "Structure" / "Zones" / "FvgDetectionAnalyzer.cs"
FVG_MITIGATION = ROOT / "Analysis" / "Structure" / "Zones" / "FvgMitigationEvaluator.cs"
OB_DETECTION = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockAnalyzer.cs"
OB_MITIGATION = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockMitigationGuard.cs"
PROTECTION_MANAGER = ROOT / "Trading" / "LiveManagement" / "ProtectionManager.cs"

for required_path in (
    RUNTIME_INIT,
    PANEL_HEARTBEAT,
    SAFETY_SUPERVISOR,
    MTF_CONTEXT_BUILDER,
    M1_CLOSED_STAGE,
    M5_REGIME_ANALYZER,
    M5_REGIME_CACHE,
    FVG_DETECTION,
    FVG_MITIGATION,
    OB_DETECTION,
    OB_MITIGATION,
    PROTECTION_MANAGER,
):
    if not required_path.exists():
        raise SystemExit(f"Phase 1.5 performance owner is missing: {required_path.name}")

runtime_init_code = RUNTIME_INIT.read_text(encoding="utf-8")
heartbeat_code = PANEL_HEARTBEAT.read_text(encoding="utf-8")
supervisor_code = SAFETY_SUPERVISOR.read_text(encoding="utf-8")
mtf_code = MTF_CONTEXT_BUILDER.read_text(encoding="utf-8")
m1_code = M1_CLOSED_STAGE.read_text(encoding="utf-8")
regime_code = M5_REGIME_ANALYZER.read_text(encoding="utf-8")
regime_cache_code = M5_REGIME_CACHE.read_text(encoding="utf-8")
fvg_detection_code = FVG_DETECTION.read_text(encoding="utf-8")
fvg_mitigation_code = FVG_MITIGATION.read_text(encoding="utf-8")
ob_detection_code = OB_DETECTION.read_text(encoding="utf-8")
ob_mitigation_code = OB_MITIGATION.read_text(encoding="utf-8")
protection_code = PROTECTION_MANAGER.read_text(encoding="utf-8")

if "GetBarsAsync(" not in runtime_init_code:
    raise SystemExit("Startup must use asynchronous Bars acquisition")
if "_initializationPendingDataLoads" not in runtime_init_code:
    raise SystemExit("Async startup must track pending data loads")
if "StartAsyncBarsInitialization(" not in runtime_init_code:
    raise SystemExit("Async startup coordinator is missing")
if "if (HasEnoughData())" not in runtime_init_code:
    raise SystemExit("Startup must finalize as soon as core MTF data is ready")
if "QueueStartupCalculationSeed();" not in runtime_init_code:
    raise SystemExit("Startup must queue one lightweight calculation seed after the initial panel render")
finalize_start = runtime_init_code.find("private void FinalizeAsyncInitialization()")
finalize_end = runtime_init_code.find("protected override void Initialize()", finalize_start)
finalize_code = (
    runtime_init_code[finalize_start:finalize_end]
    if finalize_start >= 0 and finalize_end > finalize_start
    else ""
)
if "RunStartupCalculationSeed();" in finalize_code:
    raise SystemExit("Startup seed must not execute synchronously inside initialization finalization")

CALC_STARTUP_SEED = ROOT / "Runtime" / "Calculation" / "CalculationStartupSeed.cs"
if not CALC_STARTUP_SEED.exists():
    raise SystemExit("Lightweight startup calculation seed owner is missing")
startup_seed_code = CALC_STARTUP_SEED.read_text(encoding="utf-8")
for forbidden in (
    "TryAutoTrade(",
    "TryAggressiveAutoTrade(",
    "RefreshPendingExecutionIntent(",
    "ProcessLiveCalculationStages(",
):
    if forbidden in startup_seed_code:
        raise SystemExit(f"Startup calculation seed must not own live execution path: {forbidden}")

if "RunRuntimeSafetySupervisor(" not in heartbeat_code:
    raise SystemExit("Timer heartbeat must invoke the safety supervisor")
for required_step in (
    "SynchronizeLiveBrokerState(",
    "RecoverManagedLivePlan(",
    "ProtectBrokerPositions(",
    "CheckEndOfDayAlert(",
):
    if required_step not in supervisor_code:
        raise SystemExit(f"Safety supervisor step missing: {required_step}")
if "RunSafetySupervisorStep(" not in supervisor_code:
    raise SystemExit("Safety supervisor must isolate each safety step")

if "_mtfClosedContextCache.TryGetStableContext(" not in mtf_code or    "_mtfClosedContextCache.StoreStableContext(" not in mtf_code:
    raise SystemExit("Closed MTF context cache is not wired into the builder")

if "cachedM1Frame" not in m1_code or "ReferenceEquals(" not in m1_code:
    raise SystemExit("Closed M1 frame reuse guard is missing")

if "GetM5RegimeCoreSnapshot(" not in regime_code:
    raise SystemExit("M5 regime analysis must use the recent core snapshot cache")
if "M5RegimeCoreCache" not in regime_cache_code:
    raise SystemExit("M5 regime core cache owner is missing")

if "effectiveLookback" not in fvg_detection_code or "MaximumZoneAgeBars" not in fvg_detection_code:
    raise SystemExit("FVG scan must be bounded by effective zone age")
if "createdIndex +" not in fvg_mitigation_code or "MaximumZoneAgeBars" not in fvg_mitigation_code:
    raise SystemExit("FVG mitigation must be bounded by zone age")

if "effectiveLookback" not in ob_detection_code or "MaximumZoneAgeBars" not in ob_detection_code:
    raise SystemExit("Order-block scan must be bounded by effective zone age")
if "createdIndex +" not in ob_mitigation_code or "MaximumZoneAgeBars" not in ob_mitigation_code:
    raise SystemExit("Order-block mitigation must be bounded by zone age")

if "double atr =" not in protection_code or "_m5Frame.Atr" not in protection_code:
    raise SystemExit("Protection hot path must reuse cached M5 ATR when available")

# Signal/execution synchronization gates.
PLAN_RENDER = ROOT / "UI" / "Chart" / "PlanRenderCoordinator.cs"
PLAN_LABEL_RENDER = ROOT / "UI" / "Chart" / "PlanLabelRenderCoordinator.cs"
plan_render_code = PLAN_RENDER.read_text(encoding="utf-8")
plan_label_render_code = PLAN_LABEL_RENDER.read_text(encoding="utf-8")
if "SignalVisualSnapshot snapshot" not in plan_render_code:
    raise SystemExit("Plan rendering must consume the canonical visual snapshot")
if "RenderLevelLines(snapshot" not in plan_render_code:
    raise SystemExit("Plan rendering must project levels from the canonical visual snapshot")
if "SignalVisualSnapshot snapshot" not in plan_label_render_code:
    raise SystemExit("Plan label rendering must consume the canonical visual snapshot")
if "BuildPlanLevelVisualState(" not in plan_label_render_code:
    raise SystemExit("Plan label rendering must project label state from the canonical visual snapshot")

# Market/Aggressive broker reporting is owned by the cBot after CBOT-P4A.

# Pending Stop and Pending Limit are cBot-owned after CBOT-P4C/P4D.
# Indicator prepares immutable intent and lifecycle snapshots only.
continuation_stop_code = (
    ROOT / "Trading" / "Pending" / "Placement" / "ContinuationStopPlacement.cs"
).read_text(encoding="utf-8")
if "PrepareContinuationStopForCbot(" not in continuation_stop_code:
    raise SystemExit("Pending Stop intent-only preparation owner is missing")
if "PlaceStopOrder(" in continuation_stop_code:
    raise SystemExit("Pending Stop broker mutation leaked back into Indicator")

reversal_limit_path = ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPlacement.cs"
reversal_limit_code = reversal_limit_path.read_text(encoding="utf-8")
for token in (
    "PrepareReversalLimitForCbot(",
    "CapturePendingOrderPlanSnapshot(",
):
    if token not in reversal_limit_code:
        raise SystemExit(f"Pending Limit intent boundary missing: {token}")
if "PlaceLimitOrder(" in reversal_limit_code:
    raise SystemExit("Pending Limit broker mutation leaked back into Indicator")

PENDING_CBOT = ROOT.parent / "CFIP.cBot" / "Execution" / "DemoPendingOrderExecutionCoordinator.cs"
PENDING_CBOT_CODE = PENDING_CBOT.read_text(encoding="utf-8")
for token in (
    "ExecutionAction.PendingLimit",
    "PlaceLimitOrder(",
    "BrokerAction.SubmitPendingLimit",
):
    if token not in PENDING_CBOT_CODE:
        raise SystemExit(f"cBot Pending Limit mutation contract missing: {token}")

# Phase 6.1 closed-bar decision contract.
CLOSED_BAR_RULE = ROOT / "Core" / "Math" / "ClosedBarReferenceRule.cs"
CLOSED_BAR_INDEX = ROOT / "Analysis" / "Market" / "Math" / "IndexMath.cs"
CLOSED_CONTEXT = ROOT / "Runtime" / "Mtf" / "MtfClosedContext.cs"
DECISION_REQUEST = ROOT / "Analysis" / "Market" / "Decision" / "DecisionInputBuildRequest.cs"
DECISION_FACTORY = ROOT / "Analysis" / "Market" / "Decision" / "DecisionInputSnapshotFactory.cs"
DECISION_ORCHESTRATION = ROOT / "Analysis" / "Market" / "Decision" / "DecisionOrchestration.cs"
DECISION_TIMEFRAME = ROOT / "Analysis" / "Market" / "Decision" / "TimeframeAgreementAnalyzer.cs"
CLOSED_CALCULATION = ROOT / "Runtime" / "Calculation" / "CalculationClosedBar.cs"
RUNTIME_CONTRACT_PROJECT = ROOT.parent.parent / "tools" / "CFIP.Runtime.Contracts" / "CFIP.Runtime.Contracts.csproj"

for required_path in (
    CLOSED_BAR_RULE,
    CLOSED_BAR_INDEX,
    CLOSED_CONTEXT,
    DECISION_REQUEST,
    DECISION_FACTORY,
    DECISION_ORCHESTRATION,
    DECISION_TIMEFRAME,
    CLOSED_CALCULATION,
):
    if not required_path.exists():
        raise SystemExit(f"Phase 6.1 closed-bar owner is missing: {required_path.name}")

closed_bar_rule_code = CLOSED_BAR_RULE.read_text(encoding="utf-8")
closed_bar_index_code = CLOSED_BAR_INDEX.read_text(encoding="utf-8")
closed_context_code = CLOSED_CONTEXT.read_text(encoding="utf-8")
decision_request_code = DECISION_REQUEST.read_text(encoding="utf-8")
decision_factory_code = DECISION_FACTORY.read_text(encoding="utf-8")
decision_orchestration_code = DECISION_ORCHESTRATION.read_text(encoding="utf-8")
decision_timeframe_code = DECISION_TIMEFRAME.read_text(encoding="utf-8")
closed_calculation_code = CLOSED_CALCULATION.read_text(encoding="utf-8")

if "ResolveClosedIndex(" not in closed_bar_rule_code or "IsFullyClosed(" not in closed_bar_rule_code:
    raise SystemExit("Canonical closed-bar reference rule is incomplete")

if "ClosedBarReferenceRule.ResolveClosedIndex(" not in closed_bar_index_code:
    raise SystemExit("IndexMath must delegate closed-index ownership to the canonical rule")
if "GetIndexByTime(" in closed_bar_index_code:
    raise SystemExit("Closed-index calculation must not depend on ambiguous time-series lookup semantics")

if "public DateTime Reference" not in closed_context_code:
    raise SystemExit("Closed MTF context must carry the authoritative UTC reference")
for required_index in ("public int M5", "public int M1", "public int M15", "public int M30", "public int H1", "public int H4", "public int D1", "public int W1"):
    if required_index not in closed_context_code:
        raise SystemExit(f"Closed MTF context index missing: {required_index}")

if "public MtfClosedContext ClosedContext" not in decision_request_code:
    raise SystemExit("Decision input request must carry the canonical closed context")
if "ValidateClosedBarAlignment(" not in decision_factory_code:
    raise SystemExit("Decision input factory must validate closed-bar alignment")
for required_frame in ("M5Frame", "M15Frame", "M30Frame", "H1Frame", "H4Frame"):
    if f"ValidateRequiredFrame(" not in decision_factory_code or required_frame not in decision_factory_code:
        raise SystemExit(f"Required closed decision frame validation missing: {required_frame}")
for optional_frame in ("M1Frame", "D1Frame", "W1Frame"):
    if "ValidateOptionalFrame(" not in decision_factory_code or optional_frame not in decision_factory_code:
        raise SystemExit(f"Optional closed decision frame validation missing: {optional_frame}")

if "MtfClosedContext closedContext" not in decision_orchestration_code:
    raise SystemExit("Decision orchestration must receive the canonical closed context")
if "ClosedContext = closedContext" not in decision_orchestration_code:
    raise SystemExit("Decision input request must preserve the canonical closed context")
if "TimeframeAgreement(1, closedContext)" not in decision_orchestration_code or "TimeframeAgreement(-1, closedContext)" not in decision_orchestration_code:
    raise SystemExit("Decision timeframe evidence must use the canonical closed context")

if "int[] closedIndices" not in decision_timeframe_code or "closedIndices[i]" not in decision_timeframe_code:
    raise SystemExit("Timeframe agreement must consume canonical closed indices")

if "BuildDecision(" not in closed_calculation_code or "mtf);" not in closed_calculation_code:
    raise SystemExit("Closed-bar calculation must pass the canonical MTF context into decision construction")

if not RUNTIME_CONTRACT_PROJECT.exists() or "ClosedBarReferenceRule.cs" not in RUNTIME_CONTRACT_PROJECT.read_text(encoding="utf-8"):
    raise SystemExit("Runtime acceptance project must compile the closed-bar reference contract")

# Phase 6.3 — aggressive execution authority has moved out of the live Indicator path.
AGGRESSIVE_STAGE_CODE = (ROOT / "Runtime" / "Calculation" / "CalculationStageIsolation.cs").read_text(encoding="utf-8")
if "TryAggressiveAutoTrade(" in AGGRESSIVE_STAGE_CODE:
    raise SystemExit("Indicator calculation cycle must not retain aggressive broker execution")
# Phase 6.4 compact 40-bar plan-level visual contract.
if "CompactPlanLineLengthBars = 40" not in visual_line_code:
    raise SystemExit("Plan level renderer must use the fixed 40-bar compact span")
if "GetPlanLineRightBar()" not in visual_line_code or "return Bars.Count - 1" not in visual_line_code:
    raise SystemExit("Compact plan levels must terminate at the latest chart candle")
if "MapM5ToChart(" in visual_line_code:
    raise SystemExit("Compact plan levels must not use M5 time mapping for their right edge")
if "Chart.FirstVisibleBarIndex" in visual_line_code or "Chart.LastVisibleBarIndex" in visual_line_code:
    raise SystemExit("Compact plan levels must not use full-width visible-chart boundaries")
if "as ChartText" not in plan_label_renderer_code:
    raise SystemExit("Plan labels must reuse existing ChartText objects")
if 'name + "_BOX"' not in plan_label_renderer_code:
    raise SystemExit("Plan labels must retain legacy box cleanup compatibility")
if 'RemovePlanLabel(P + "ENTRY_LABEL")' not in plan_label_remover_code:
    raise SystemExit("Plan label remover must clean the compact label entry")
if 'RemovePlanLabel(P + "ACTIVE_TP_LABEL")' not in plan_label_remover_code:
    raise SystemExit("Plan label remover must clean the compact active-target label")
if "return LineStyle.Solid" not in visual_line_code:
    raise SystemExit("All compact signal/plan level lines must use Solid style")
if "ResolvePlanLineThickness(" not in visual_line_code:
    raise SystemExit("Plan-level thickness resolver is missing")
if "PlanLinePresentationRule.ResolveThickness(" not in visual_line_code:
    raise SystemExit("Plan-level thickness must consume the canonical presentation rule")
if "MinimumThickness = 1" not in visual_line_presentation_rule_code:
    raise SystemExit("Plan-line presentation rule must preserve minimum thickness 1")
if "return MinimumThickness;" not in visual_line_presentation_rule_code:
    raise SystemExit("Plan-line presentation rule must keep the one-pixel visual contract")
if "line.Thickness = 1" in visual_line_code:
    raise SystemExit("Plan-line renderer must consume the canonical one-pixel thickness rule")

# Runtime UI responsiveness hotfix contract.
PANEL_VISIBILITY = ROOT / "UI" / "Panel" / "PanelVisibility.cs"
panel_visibility_code = PANEL_VISIBILITY.read_text(encoding="utf-8")
toggle_start = panel_visibility_code.find("private void TogglePanel()")
toggle_end = panel_visibility_code.find("private void RemovePanel()", toggle_start)
toggle_code = (
    panel_visibility_code[toggle_start:toggle_end]
    if toggle_start >= 0 and toggle_end > toggle_start
    else ""
)
if "RenderPanel();" in toggle_code:
    raise SystemExit("Panel Hide/Show toggle must not synchronously invoke full RenderPanel()")
if "_panel.IsVisible" not in toggle_code or "_panelRestoreButton" not in toggle_code:
    raise SystemExit("Panel toggle must remain an immediate visibility-only mutation")

VERSION_RESIDUE_PATTERNS = (
    re.compile(r"\bv\d+\b", re.I),
    re.compile(r"\b(?:rev|release)[-_ ]?\d+\b", re.I),
    re.compile(r"\bClean\d+\b", re.I),
    re.compile(r"\bCFIPClean\d+\b", re.I),
    re.compile(r"CFIP_MTF_LiveEntryEngine_Clean", re.I),
    re.compile(r"\bCFIP[\s_-]*(?:SMART|AUTO)[\s_-]*\d+\b", re.I),
)
HISTORICAL_IDENTIFIER_PATTERN = re.compile(
    r"\b(?:Legacy|Compatibility|Compat|Deprecated|Versioned|Obsolete|Alias)\w*\b",
    re.I,
)
COMPATIBILITY_ALIAS_PATTERN = re.compile(
    r"^\s*using\s+\w*(?:Legacy|Compatibility|Compat|Deprecated|Versioned|Alias)\w*\s*=",
    re.I | re.MULTILINE,
)
EMPTY_CATCH_PATTERN = re.compile(
    r"\bcatch(?:\s*\([^)]*\))?\s*\{\s*\}",
    re.I,
)
GENERATED_DIR_NAMES = {"bin", "obj", ".vs", "TestResults"}
GENERATED_SUFFIXES = {".dll", ".pdb", ".exe", ".nupkg"}
hygiene_errors = []

for production_file in files:
    source_text = production_file.read_text(encoding="utf-8")
    static_code = strip_for_static_checks(source_text)

    for residue_pattern in VERSION_RESIDUE_PATTERNS:
        if residue_pattern.search(source_text):
            hygiene_errors.append(f"{production_file}: version/historical residue")
            break

    historical_match = HISTORICAL_IDENTIFIER_PATTERN.search(static_code)
    if historical_match:
        hygiene_errors.append(
            f"{production_file}: obsolete/historical identifier "
            f"({historical_match.group(0)})"
        )

    if COMPATIBILITY_ALIAS_PATTERN.search(static_code):
        hygiene_errors.append(f"{production_file}: compatibility alias")

    if EMPTY_CATCH_PATTERN.search(static_code):
        hygiene_errors.append(f"{production_file}: empty catch block")

    raw_bytes = production_file.read_bytes()
    without_crlf = raw_bytes.replace(b"\r\n", b"")
    if b"\r\n" in raw_bytes and b"\n" in without_crlf:
        hygiene_errors.append(f"{production_file}: mixed line endings")

for production_path in ROOT.rglob("*"):
    if not production_path.is_file():
        continue
    if production_path.name in GENERATED_DIR_NAMES:
        hygiene_errors.append(
            f"{production_path}: generated artifact directory"
        )
    if production_path.suffix.lower() in GENERATED_SUFFIXES:
        hygiene_errors.append(
            f"{production_path}: generated artifact"
        )

if hygiene_errors:
    raise SystemExit(
        "Production-source hygiene failures:\n- "
        + "\n- ".join(sorted(set(hygiene_errors)))
    )

if re.search(r"\b(?:Buy|Sell)\b.{0,100}\b(?:Button|ToggleButton)\b", code, re.I):
    raise SystemExit("Manual trade-entry controls detected")

oversized = [
    str(p.relative_to(ROOT))
    for p in files
    if p.stat().st_size > 20 * 1024
]
if oversized:
    raise SystemExit("Oversized production modules: " + ", ".join(sorted(oversized)))

indicator_files = {
    "ExponentialMovingAverage.cs": "Ema",
    "AverageTrueRange.cs": "Atr",
    "RelativeStrengthIndex.cs": "Rsi",
    "AverageDirectionalIndex.cs": "Adx",
    "DirectionalMovementIndex.cs": "DmiBias",
}
for filename, method in indicator_files.items():
    candidates = [p for p in files if p.name == filename]
    if len(candidates) != 1 or method not in candidates[0].read_text(encoding="utf-8"):
        raise SystemExit(f"Indicator isolation check failed: {filename}")

# Atomic analysis ownership checks. Each conceptual behavior remains in one file.
ATOMIC_METHOD_OWNERS = {
    "HasVolumeExpansion": ROOT / "Analysis" / "Market" / "VolumeExpansionAnalyzer.cs",
    "HasMacdBias": ROOT / "Analysis" / "Market" / "MacdBiasAnalyzer.cs",
    "HasVwapBias": ROOT / "Analysis" / "Market" / "VwapBiasAnalyzer.cs",
    "HasHealthyVolatility": ROOT / "Analysis" / "Market" / "HealthyVolatilityAnalyzer.cs",
    "PremiumDiscountBias": ROOT / "Analysis" / "Market" / "PremiumDiscountAnalyzer.cs",
    "LiveBias": ROOT / "Analysis" / "Market" / "LiveBiasAnalyzer.cs",
    "AddFrame": ROOT / "Analysis" / "Market" / "MarketFrameScoring.cs",
    "Format": ROOT / "Analysis" / "Market" / "Decision" / "DecisionReasonFormatter.cs",
    "BullLiquiditySweep": ROOT / "Analysis" / "Structure" / "LiquiditySweepAnalyzer.cs",
    "BearLiquiditySweep": ROOT / "Analysis" / "Structure" / "LiquiditySweepAnalyzer.cs",
    "FindSwingHigh": ROOT / "Analysis" / "Structure" / "SwingPointAnalyzer.cs",
    "FindSwingLow": ROOT / "Analysis" / "Structure" / "SwingPointAnalyzer.cs",
    "FindSwingHighAbove": ROOT / "Analysis" / "Structure" / "SwingPointAnalyzer.cs",
    "FindSwingLowBelow": ROOT / "Analysis" / "Structure" / "SwingPointAnalyzer.cs",
    "FindEqualHigh": ROOT / "Analysis" / "Structure" / "EqualLevelAnalyzer.cs",
    "FindEqualLow": ROOT / "Analysis" / "Structure" / "EqualLevelAnalyzer.cs",
    "FindNearestFvg": ROOT / "Analysis" / "Structure" / "Zones" / "FvgDetectionAnalyzer.cs",
    "SelectNearestFvg": ROOT / "Analysis" / "Structure" / "Zones" / "FvgDetectionAnalyzer.cs",
    "IsZoneFullyMitigated": ROOT / "Analysis" / "Structure" / "Zones" / "FvgMitigationEvaluator.cs",
    "HasZoneRetest": ROOT / "Analysis" / "Structure" / "Zones" / "FvgLifecycleAnalyzer.cs",
    "BuildManagedFvgZone": ROOT / "Analysis" / "Structure" / "Zones" / "FvgLifecycleAnalyzer.cs",
    "HasOrderBlockLiquiditySweep": ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockConfluenceAnalyzer.cs",
    "HasOrderBlockFvgConfluence": ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockConfluenceAnalyzer.cs",
}
for method, owner in ATOMIC_METHOD_OWNERS.items():
    if not owner.exists():
        raise SystemExit(f"Atomic analysis owner missing: {owner}")
    count = len(re.findall(r"^\s*(?:public|private|protected|internal)\b[^\r\n{;]*\b" + re.escape(method) + r"\s*\(", owner.read_text(encoding="utf-8"), re.MULTILINE))
    if count != 1:
        raise SystemExit(f"Atomic method ownership failed: {method} in {owner} (count={count})")

FRAME_ANALYZER = ROOT / "Analysis" / "Market" / "MarketFrameAnalyzer.cs"
FRAME_CODE = FRAME_ANALYZER.read_text(encoding="utf-8")
if "BuildReason(" in FRAME_CODE or "AddFrame(" in FRAME_CODE:
    raise SystemExit("MarketFrameAnalyzer retains secondary responsibilities")
SCORING = ROOT / "Analysis" / "Market" / "MarketFrameScoring.cs"
if len(re.findall(r"\bprivate void AddScore\s*\(", SCORING.read_text(encoding="utf-8"))) != 2:
    raise SystemExit("MarketFrameScoring must own both AddScore overloads")

POSITION_MODIFIED_HANDLER = ROOT / "Trading" / "Lifecycle" / "PositionModifiedHandler.cs"
POSITION_MODIFIED_CODE = POSITION_MODIFIED_HANDLER.read_text(encoding="utf-8")
if "boundToActivePlan" not in POSITION_MODIFIED_CODE:
    raise SystemExit("Position modification must be bound to the active plan")
if "if (!boundToActivePlan)" not in POSITION_MODIFIED_CODE:
    raise SystemExit("Unbound position modifications must be ignored")

POSITION_OPENED_HANDLER = ROOT / "Trading" / "Lifecycle" / "PositionOpenedHandler.cs"
POSITION_OPENED_CODE = POSITION_OPENED_HANDLER.read_text(encoding="utf-8")
if "boundToActivePlan" not in POSITION_OPENED_CODE:
    raise SystemExit("Position opened handling must use explicit plan identity")
if "_plan.IsLivePosition" not in POSITION_OPENED_CODE:
    raise SystemExit("Position opened handling must not bind a pre-trade plan by label alone")

BROKER_STATE_SNAPSHOT = ROOT / "Trading" / "Lifecycle" / "BrokerStateSnapshot.cs"
BROKER_STATE_CODE = BROKER_STATE_SNAPSHOT.read_text(encoding="utf-8")
if "BROKER STATE • BOUND POSITION NOT FOUND" not in BROKER_STATE_CODE:
    raise SystemExit("Broker state sync must clear stale live plans when bound position disappears")
if "GetManagedLivePositionForPlan()" not in BROKER_STATE_CODE:
    raise SystemExit("Broker state sync must resolve the authoritative bound position")

CAPACITY_RULE = ROOT / "Core" / "Math" / "ExecutionCapacityRule.cs"
CAPACITY_RULE_CODE = CAPACITY_RULE.read_text(encoding="utf-8")
if "AllowsNewSinglePlan(" not in CAPACITY_RULE_CODE:
    raise SystemExit("Pure single-plan capacity rule missing")
if "AllowsNewSingleExecution(" not in CAPACITY_RULE_CODE:
    raise SystemExit("New-execution capacity rule missing")
if "!hasActivePlan" not in CAPACITY_RULE_CODE:
    raise SystemExit("New-plan capacity rule must reject an active local plan")
if "managedPositionCount <" not in CAPACITY_RULE_CODE:
    raise SystemExit("Single-plan capacity rule must cap managed positions")
if "managedPendingOrderCount == 0" not in CAPACITY_RULE_CODE:
    raise SystemExit("Single-plan capacity rule must reject an existing managed pending order")

LIVE_RECOVERY_RULE = ROOT / "Core" / "Math" / "LivePlanRecoveryRule.cs"
LIVE_RECOVERY_RULE_CODE = LIVE_RECOVERY_RULE.read_text(encoding="utf-8")
if "ShouldClearStaleLivePlan" not in LIVE_RECOVERY_RULE_CODE:
    raise SystemExit("Pure stale live-plan recovery rule missing")

CAPACITY_GUARD = ROOT / "Trading" / "Risk" / "ExecutionCapacityGuard.cs"
CAPACITY_CODE = CAPACITY_GUARD.read_text(encoding="utf-8")
if "ExecutionCapacityRule.AllowsNewSinglePlan(" not in CAPACITY_CODE:
    raise SystemExit("Plan capacity guard must delegate to the pure rule")
if "ExecutionCapacityRule.AllowsNewSingleExecution(" not in CAPACITY_CODE:
    raise SystemExit("Execution capacity guard must delegate to the pure execution rule")
if "ValidateSinglePlanCapacity(" not in CAPACITY_CODE:
    raise SystemExit("Shared single-plan capacity guard missing")
if "ValidateSingleExecutionCapacity(" not in CAPACITY_CODE:
    raise SystemExit("Shared execution capacity guard missing")
plan_execution_path = ROOT / "Trading" / "Validation" / "PlanCreationEligibility.cs"
if "ValidateSinglePlanCapacity(" not in plan_execution_path.read_text(encoding="utf-8"):
    raise SystemExit("Plan creation must use the single-plan capacity guard")
for execution_path in [
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPreTradeEligibility.cs",
    ROOT / "Trading" / "Execution" / "Aggressive" / "AggressivePreTradeEligibility.cs",
    ROOT / "Trading" / "Pending" / "Placement" / "SmartPendingOrderOrchestrator.cs",
]:
    execution_code = execution_path.read_text(encoding="utf-8")
    if "ValidateSingleExecutionCapacity(" not in execution_code:
        raise SystemExit(f"Shared execution capacity guard missing in {execution_path.name}")
    if "ManagedPositionCount() >=" in execution_code:
        raise SystemExit(f"Duplicate position-capacity calculation remains in {execution_path.name}")

# Market/Aggressive broker state ownership moved to cBot in CBOT-P4A.

AUTO_STATE = ROOT / "Trading" / "Execution" / "State" / "AutoTradingStateStore.cs"
AUTO_STATE_CODE = AUTO_STATE.read_text(encoding="utf-8")
PANEL_EXECUTION_STATE_CODE = (
    ROOT / "UI" / "Panel" / "PanelExecutionState.cs"
).read_text(encoding="utf-8")
if (
    "RecoveryRequired" not in PANEL_EXECUTION_STATE_CODE or
    '"RECOVERY REQUIRED • "' not in PANEL_EXECUTION_STATE_CODE or
    "ExecutionPanelStateKind.RecoveryRequired" not in PANEL_EXECUTION_STATE_CODE
):
    raise SystemExit("Recovery state must remain visibly distinct in the auto-trading panel")

required_method_files = {
    "BuildExecutionIntent": ROOT / "Planning" / "Execution" / "ExecutionIntentBuilder.cs",
    "ValidateExecutionIntent": ROOT / "Planning" / "Execution" / "ExecutionIntentValidation.cs",
    "ValidateActualMarketFill": ROOT / "Planning" / "Execution" / "ExecutionIntentValidation.cs",
}
for method, path in required_method_files.items():
    if method not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Execution intent ownership check failed: {method}")

print(
    f"Architecture OK: {len(files)} C# files, {parameters} parameters, "
    f"{len(methods)} method declarations / {len(unique_methods)} unique baseline methods (minimum {REFERENCE_METHOD_MINIMUM})."
)

# Accepted terminal host compatibility guard.
# The production Indicator targets the installed LiteFinance cTrader runtime.
# Host APIs not verified against that runtime must not be introduced into the
# Indicator partial host merely because they exist in a newer public SDK.
HOST_UNVERIFIED_TOKENS = (
    r"\bOnException\s*\(",
    r"\bChartStaticText\b",
)
for p in files:
    source_text = strip_for_static_checks(p.read_text(encoding="utf-8"))
    for token in HOST_UNVERIFIED_TOKENS:
        if re.search(token, source_text):
            raise SystemExit(
                f"Unverified host API leaked into production Indicator: {p}"
            )

# Trade-plan construction boundary.
PLAN_BUILDER = ROOT / "Planning" / "TradePlan" / "PlanBuilder.cs"
PLAN_BUILDER_CODE = PLAN_BUILDER.read_text(encoding="utf-8")
if PLAN_BUILDER.stat().st_size > 4096:
    raise SystemExit("PlanBuilder.cs must remain a thin orchestration boundary")
for token in (
    "TryPreparePlanInputs(",
    "BuildTargetLevels(",
    "SelectTargets(",
    "TryBuildPlanTargets(",
    "CreatePlanFromInputs(",
    "EnrichPlanTargetMetadata(",
    "ValidatePlanIntegrity(",
):
    if token not in PLAN_BUILDER_CODE:
        raise SystemExit(f"PlanBuilder orchestration call missing: {token}")
for declaration in (
    "private double BuildStructuralStop(",
    "private List<Level> BuildTargetLevels(",
    "private double SelectTarget(",
    "private void ApplyTargetMeta(",
    "private bool HasTargetObstacle(",
):
    if declaration in PLAN_BUILDER_CODE:
        raise SystemExit(f"PlanBuilder retains extracted responsibility: {declaration}")
for required_path in (
    ROOT / "Planning" / "TradePlan" / "PlanInputPreparation.cs",
    ROOT / "Planning" / "TradePlan" / "PlanTargetPreparation.cs",
    ROOT / "Planning" / "TradePlan" / "PlanMaterialization.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Trade-plan construction owner missing: {required_path}")

# Target-selection boundary.
TARGET_SELECTOR = ROOT / "Planning" / "TradePlan" / "TargetSelector.cs"
TARGET_SELECTOR_CODE = TARGET_SELECTOR.read_text(encoding="utf-8")
if TARGET_SELECTOR.stat().st_size > 4096:
    raise SystemExit("TargetSelector.cs must remain a thin stage-orchestration boundary")
for token in (
    "BuildTargetSelectionRequiredRR(",
    "TargetLadderSelectionRule.SelectBestPath(",
    "TryBuildTargetLadderStageOptions("
):
    if token not in TARGET_SELECTOR_CODE:
        raise SystemExit(f"TargetSelector orchestration call missing: {token}")
for required_path in (
    ROOT / "Planning" / "TradePlan" / "TargetSelectionPolicy.cs",
    ROOT / "Planning" / "TradePlan" / "TargetCandidateEvaluator.cs",
    ROOT / "Planning" / "TradePlan" / "TargetLadderStageCandidateBuilder.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Target selection owner missing: {required_path}")
for declaration in (
    "private double[] BuildTargetSelectionRequiredRR(",
    "private double FindPreviousSelectedTargetPrice(",
    "private bool RequiresHtfRewardForTargetStage(",
    "private bool TryBuildTargetLadderStageOptions("
):
    if declaration in TARGET_SELECTOR_CODE:
        raise SystemExit(f"TargetSelector retains extracted responsibility: {declaration}")

# Order-block candidate construction boundary.
OB_BUILDER = ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockCandidateBuilder.cs"
OB_BUILDER_CODE = OB_BUILDER.read_text(encoding="utf-8")
if OB_BUILDER.stat().st_size > 4096:
    raise SystemExit("OrderBlockCandidateBuilder.cs must remain a thin candidate orchestration boundary")
for token in (
    "TryBuildOrderBlockImpulseEvidence(",
    "TryApplyOrderBlockMitigation(",
    "CalculateOrderBlockQuality(",
):
    if token not in OB_BUILDER_CODE:
        raise SystemExit(f"Order-block orchestration call missing: {token}")
for declaration in (
    "private bool TryBuildOrderBlockImpulseEvidence(",
    "private bool TryApplyOrderBlockMitigation(",
    "private int CalculateOrderBlockQuality(",
):
    if declaration in OB_BUILDER_CODE:
        raise SystemExit(f"OrderBlockCandidateBuilder retains extracted responsibility: {declaration}")
for required_path in (
    ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockEvidenceBuilder.cs",
    ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockMitigationGuard.cs",
    ROOT / "Analysis" / "Structure" / "Zones" / "OrderBlockQualityCalculator.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Order-block owner missing: {required_path}")

# FVG lifecycle ownership.
FVG_LIFECYCLE_RULE = ROOT / "Core" / "Math" / "FvgLifecycleRule.cs"
FVG_LIFECYCLE_RULE_CODE = FVG_LIFECYCLE_RULE.read_text(encoding="utf-8")
for token in (
    "IsAgeValid(",
    "ResolveFvgMitigationProbe(",
    "TryApplyMitigationStep(",
    "return !invalidateOnFullFill;",
):
    if token not in FVG_LIFECYCLE_RULE_CODE:
        raise SystemExit(f"Canonical FVG lifecycle rule missing: {token}")

FVG_LIFECYCLE = ROOT / "Analysis" / "Structure" / "Zones" / "FvgLifecycleAnalyzer.cs"
FVG_LIFECYCLE_CODE = FVG_LIFECYCLE.read_text(encoding="utf-8")
if FVG_LIFECYCLE.stat().st_size > 8192:
    raise SystemExit("FvgLifecycleAnalyzer.cs must remain an orchestration boundary")
for token in (
    "TryApplyFvgMitigation(",
    "CalculateFvgQuality(",
):
    if token not in FVG_LIFECYCLE_CODE:
        raise SystemExit(f"FVG lifecycle orchestration call missing: {token}")
for declaration in (
    "private bool TryApplyFvgMitigation(",
    "private int CalculateFvgQuality(",
):
    if declaration in FVG_LIFECYCLE_CODE:
        raise SystemExit(f"FvgLifecycleAnalyzer retains extracted responsibility: {declaration}")
for path, token in (
    (
        ROOT / "Analysis" / "Structure" / "Zones" / "FvgMitigationEvaluator.cs",
        "TryApplyFvgMitigation("
    ),
    (
        ROOT / "Analysis" / "Structure" / "Zones" / "FvgZoneQualityCalculator.cs",
        "CalculateFvgQuality("
    ),
):
    if not path.exists() or token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"FVG lifecycle owner missing: {path}")

# Structural-stop planning ownership.
STRUCTURAL_STOP_SELECTOR = ROOT / "Planning" / "TradePlan" / "StructuralStopCandidateSelector.cs"
STRUCTURAL_STOP_SELECTOR_CODE = STRUCTURAL_STOP_SELECTOR.read_text(encoding="utf-8")
if STRUCTURAL_STOP_SELECTOR.stat().st_size > 4096:
    raise SystemExit("StructuralStopCandidateSelector.cs must remain a thin orchestration boundary")
if "TrySelectBestStructuralStopCandidate(" not in STRUCTURAL_STOP_SELECTOR_CODE:
    raise SystemExit("Structural-stop selector orchestration call missing")
if "private bool TrySelectBestStructuralStopCandidate(" in STRUCTURAL_STOP_SELECTOR_CODE:
    raise SystemExit("StructuralStopCandidateSelector retains candidate evaluation")
if "MaterializeStructuralStop(" in STRUCTURAL_STOP_SELECTOR_CODE:
    raise SystemExit("StructuralStopCandidateSelector retains duplicate stop materialization")

STRUCTURAL_STOP_EVALUATOR = ROOT / "Planning" / "TradePlan" / "StructuralStopCandidateEvaluator.cs"
STRUCTURAL_STOP_EVALUATOR_CODE = STRUCTURAL_STOP_EVALUATOR.read_text(encoding="utf-8")
if "TrySelectBestStructuralStopCandidate(" not in STRUCTURAL_STOP_EVALUATOR_CODE:
    raise SystemExit("Structural-stop candidate evaluator missing")
if "StructuralStopGeometryRule.Evaluate(" not in STRUCTURAL_STOP_EVALUATOR_CODE:
    raise SystemExit("Structural-stop evaluator must consume canonical geometry owner")
if "IsValidStop(" not in STRUCTURAL_STOP_EVALUATOR_CODE:
    raise SystemExit("Structural-stop evaluator must retain broker-distance adapter validation")

STRUCTURAL_STOP_GEOMETRY = ROOT / "Core" / "Math" / "StructuralStopGeometryRule.cs"
STRUCTURAL_STOP_GEOMETRY_CODE = STRUCTURAL_STOP_GEOMETRY.read_text(encoding="utf-8")
if "class StructuralStopGeometryRule" not in STRUCTURAL_STOP_GEOMETRY_CODE:
    raise SystemExit("Canonical structural stop geometry owner missing")
for token in (
    "Evaluate(",
    "EvaluateFallback(",
    "ResolveBufferAtr(",
):
    if token not in STRUCTURAL_STOP_GEOMETRY_CODE:
        raise SystemExit(f"Structural-stop geometry owner missing: {token}")

STRUCTURAL_STOP_FINALIZER = ROOT / "Planning" / "TradePlan" / "StructuralStopFinalizer.cs"
if STRUCTURAL_STOP_FINALIZER.exists():
    raise SystemExit("Obsolete duplicate structural stop finalizer must remain removed")

# Plan-integrity ownership.
PLAN_INTEGRITY = ROOT / "Planning" / "TradePlan" / "PlanIntegrityValidator.cs"
PLAN_INTEGRITY_CODE = PLAN_INTEGRITY.read_text(encoding="utf-8")
if PLAN_INTEGRITY.stat().st_size > 4096:
    raise SystemExit("PlanIntegrityValidator.cs must remain a thin orchestration boundary")
for token in (
    "ValidatePlanProtectionAndEntry(",
    "ValidatePlanRewardStructure(",
    "ValidatePlanMarketConstraints(",
):
    if token not in PLAN_INTEGRITY_CODE:
        raise SystemExit(f"Plan-integrity orchestration call missing: {token}")
for declaration in (
    "private bool ValidatePlanProtectionAndEntry(",
    "private bool ValidatePlanRewardStructure(",
    "private bool ValidatePlanMarketConstraints(",
):
    if declaration in PLAN_INTEGRITY_CODE:
        raise SystemExit(f"PlanIntegrityValidator retains extracted responsibility: {declaration}")
for path, token in {
    ROOT / "Planning" / "TradePlan" / "PlanProtectionIntegrityValidator.cs": "ValidatePlanProtectionAndEntry(",
    ROOT / "Planning" / "TradePlan" / "PlanRewardIntegrityValidator.cs": "ValidatePlanRewardStructure(",
    ROOT / "Planning" / "TradePlan" / "PlanMarketConstraintValidator.cs": "ValidatePlanMarketConstraints(",
}.items():
    if not path.exists() or token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Plan-integrity owner missing: {path}")

# Signal/decision validation ownership.
SIGNAL_OWNER = ROOT / "Trading" / "Validation" / "SignalPlanCoordinator.cs"
PLAN_ELIGIBILITY = ROOT / "Trading" / "Validation" / "PlanCreationEligibility.cs"
BLOCK_POLICY = ROOT / "Trading" / "Validation" / "DecisionBlockReasonPolicy.cs"
LIVE_GATE_POLICY = ROOT / "Trading" / "Validation" / "LiveExecutionGateReasonPolicy.cs"
SMART_POLICY = ROOT / "Trading" / "Validation" / "SmartThresholdPolicy.cs"
for path, token in {
    SIGNAL_OWNER: "EnsureSignalPlan(",
    PLAN_ELIGIBILITY: "ShouldCreatePlan(",
    BLOCK_POLICY: "IsHardDecisionBlockReason(",
    LIVE_GATE_POLICY: "IsLiveExecutionGateReason(",
    SMART_POLICY: "GetAdaptiveSmartThresholds(",
}.items():
    if not path.exists() or token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Signal/decision validation owner missing: {path}")
if "SmartMinimumConsensusFloor(" not in SMART_POLICY.read_text(encoding="utf-8"):
    raise SystemExit("SmartThresholdPolicy must own the consensus floor")
for path in (
    ROOT / "Trading" / "Validation" / "DecisionGateValidation.cs",
):
    if path.exists():
        raise SystemExit(f"Obsolete aggregate validation module must remain removed: {path}")

# Final obsolete/dead-path sweep.
OBSOLETE_PRODUCTION_PATHS = (
    ROOT / "Trading" / "Execution" / "BrokerMutationCoordinator.cs",
    ROOT / "Trading" / "Validation" / "RewardPathValidation.cs",
    ROOT / "Trading" / "Validation" / "DecisionGateValidation.cs",
    ROOT / "Trading" / "Intelligence" / "DecisionFeatures.cs",
    ROOT / "Planning" / "TradePlan" / "StructuralStopFinalizer.cs",
)
for obsolete_path in OBSOLETE_PRODUCTION_PATHS:
    if obsolete_path.exists():
        raise SystemExit(f"Obsolete production path detected: {obsolete_path}")

# Execution-zone planning ownership.
EXECUTION_ZONE = ROOT / "Planning" / "Execution" / "ExecutionZoneBuilder.cs"
EXECUTION_ZONE_CODE = EXECUTION_ZONE.read_text(encoding="utf-8")
if EXECUTION_ZONE.stat().st_size > 4096:
    raise SystemExit("ExecutionZoneBuilder.cs must remain a planning orchestration boundary")
for token in (
    "TrySelectExecutionZoneCandidate(",
    "EvaluateExecutionZoneQuality(",
):
    if token not in EXECUTION_ZONE_CODE:
        raise SystemExit(f"Execution-zone orchestration call missing: {token}")
for declaration in (
    "private bool TrySelectExecutionZoneCandidate(",
    "private int EvaluateExecutionZoneQuality(",
):
    if declaration in EXECUTION_ZONE_CODE:
        raise SystemExit(f"ExecutionZoneBuilder retains extracted responsibility: {declaration}")
for path in (
    ROOT / "Planning" / "Execution" / "ExecutionZoneCandidateSelector.cs",
    ROOT / "Planning" / "Execution" / "ExecutionZoneQualityEvaluator.cs",
):
    if not path.exists():
        raise SystemExit(f"Execution-zone owner missing: {path}")

# Panel row renderer ownership.
PANEL_OVERVIEW = ROOT / "UI" / "Panel" / "Rows" / "PanelOverviewRowsRenderer.cs"
PANEL_OVERVIEW_CODE = PANEL_OVERVIEW.read_text(encoding="utf-8")
if PANEL_OVERVIEW.stat().st_size > 4096:
    raise SystemExit("PanelOverviewRowsRenderer.cs must remain a composition boundary")
for token in (
    "RenderPanelOverviewStateRows(",
    "RenderPanelOverviewExecutionRows(",
    "RenderPanelOverviewDiagnosticRows(",
):
    if token not in PANEL_OVERVIEW_CODE:
        raise SystemExit(f"Panel overview composition call missing: {token}")

PANEL_TRADE_PLAN = ROOT / "UI" / "Panel" / "Rows" / "PanelTradePlanRowsRenderer.cs"
PANEL_TRADE_PLAN_CODE = PANEL_TRADE_PLAN.read_text(encoding="utf-8")
if PANEL_TRADE_PLAN.stat().st_size > 4096:
    raise SystemExit("PanelTradePlanRowsRenderer.cs must remain a composition boundary")
for token in (
    "RenderPanelTradePlanLevelRows(",
    "RenderPanelTradePlanLiveRows(",
):
    if token not in PANEL_TRADE_PLAN_CODE:
        raise SystemExit(f"Panel trade-plan composition call missing: {token}")
for path in (
    ROOT / "UI" / "Panel" / "Rows" / "PanelOverviewStateRowsRenderer.cs",
    ROOT / "UI" / "Panel" / "Rows" / "PanelOverviewExecutionRowsRenderer.cs",
    ROOT / "UI" / "Panel" / "Rows" / "PanelOverviewDiagnosticRowsRenderer.cs",
    ROOT / "UI" / "Panel" / "Rows" / "PanelTradePlanLevelRowsRenderer.cs",
    ROOT / "UI" / "Panel" / "Rows" / "PanelTradePlanLiveRowsRenderer.cs",
):
    if not path.exists():
        raise SystemExit(f"Panel row owner missing: {path}")

# Broker protection state ownership.
BROKER_PROTECTION_STATE = ROOT / "Trading" / "Lifecycle" / "BrokerProtectionStateEvaluator.cs"
BROKER_PROTECTION_STATE_CODE = BROKER_PROTECTION_STATE.read_text(encoding="utf-8")
if "EvaluateBrokerProtection(" not in BROKER_PROTECTION_STATE_CODE:
    raise SystemExit("Broker protection state evaluator missing")
for consumer_name in (
    "BrokerStateSnapshot.cs",
    "PositionOpenedHandler.cs",
    "PositionModifiedHandler.cs",
):
    consumer = ROOT / "Trading" / "Lifecycle" / consumer_name
    code = consumer.read_text(encoding="utf-8")
    if "EvaluateBrokerProtection(" not in code:
        raise SystemExit(f"Broker protection state must use shared evaluator: {consumer_name}")
    for declaration in (
        "bool brokerStopValid =",
        "bool brokerTargetValid =",
    ):
        if declaration in code:
            raise SystemExit(f"Broker protection validation duplicated in {consumer_name}: {declaration}")

# Cross-path execution consistency for the remaining Indicator pending boundary.
CROSS_PATH_CONTRACTS = {
    "ContinuationStopPlacement.cs": (
        "PrepareContinuationStopForCbot(",
    ),
    "ReversalLimitPlacement.cs": (
        "TryPrepareReversalLimit(",
        "PrepareReversalLimitForCbot(",
        "CapturePendingOrderPlanSnapshot(",
    ),
}
for filename, tokens in CROSS_PATH_CONTRACTS.items():
    path = (
        ROOT / "Trading" / "Execution" / "AutomaticMarket" / filename
        if filename == "AutomaticMarketBrokerExecution.cs"
        else ROOT / "Trading" / "Execution" / "Aggressive" / filename
        if filename == "AggressiveBrokerExecution.cs"
        else ROOT / "Trading" / "Pending" / "Placement" / filename
    )
    code = path.read_text(encoding="utf-8")
    for token in tokens:
        if token not in code:
            raise SystemExit(f"Cross-path execution contract missing in {filename}: {token}")

for path, tokens in {
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketSubmissionValidator.cs": (
        "BuildExecutionIntent(",
        "ValidateExecutionIntent(",
    ),
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketFillReconciliation.cs": (
        "ReconcileLivePlanToActualFill(",
        "IsExecutableFillPrice(",
    ),
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPostFillTargetResolver.cs": (
        "AutoTarget(",
        "RequestLivePlanExit(",
    ),
}.items():
    code = path.read_text(encoding="utf-8")
    for token in tokens:
        if token not in code:
            raise SystemExit(f"Automatic-market owner contract missing: {path.name}: {token}")

# Pending placement boundary.
PENDING_STOP = ROOT / "Trading" / "Pending" / "Placement" / "ContinuationStopPlacement.cs"
PENDING_STOP_CODE = PENDING_STOP.read_text(encoding="utf-8")
if PENDING_STOP.stat().st_size > 4096:
    raise SystemExit("ContinuationStopPlacement.cs must remain a placement orchestration boundary")
for token in (
    "PrepareContinuationStopForCbot(",
):
    if token not in PENDING_STOP_CODE:
        raise SystemExit(f"Continuation stop intent owner missing: {token}")
if "PlaceStopOrder(" in PENDING_STOP_CODE:
    raise SystemExit("Continuation stop broker mutation remains in Indicator")

PENDING_STOP_PREP = ROOT / "Trading" / "Pending" / "Placement" / "ContinuationStopPreparation.cs"
if not PENDING_STOP_PREP.exists() or "BuildStructuralStop(" not in PENDING_STOP_PREP.read_text(encoding="utf-8"):
    raise SystemExit("Continuation stop preparation owner missing")

PENDING_LIMIT = ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPlacement.cs"
PENDING_LIMIT_CODE = PENDING_LIMIT.read_text(encoding="utf-8")
if PENDING_LIMIT.stat().st_size > 4096:
    raise SystemExit("ReversalLimitPlacement.cs must remain a placement orchestration boundary")
for token in (
    "TryPrepareReversalLimit(",
    "PrepareReversalLimitForCbot(",
    "CapturePendingOrderPlanSnapshot(",
):
    if token not in PENDING_LIMIT_CODE:
        raise SystemExit(f"Reversal limit ownership call missing: {token}")

PENDING_LIMIT_PREP = ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPreparation.cs"
if (
    not PENDING_LIMIT_PREP.exists() or
    "TrySelectPredictivePendingLevel(" not in
    PENDING_LIMIT_PREP.read_text(encoding="utf-8")
):
    raise SystemExit("Reversal limit preparation owner missing")

PENDING_CBOT = ROOT.parent / "CFIP.cBot" / "Execution" / "DemoPendingOrderExecutionCoordinator.cs"
if (
    not PENDING_CBOT.exists() or
    "PlaceLimitOrder(" not in PENDING_CBOT.read_text(encoding="utf-8")
):
    raise SystemExit("cBot Pending Limit broker mutation owner missing")

PENDING_SUBMISSION = ROOT / "Trading" / "Pending" / "Placement" / "PendingSubmissionValidator.cs"
if not PENDING_SUBMISSION.exists():
    raise SystemExit("Shared pending submission validator missing")
PENDING_SUBMISSION_CODE = PENDING_SUBMISSION.read_text(encoding="utf-8")
for token in (
    "ValidateExecutionIntent(",
    "PassesAutoTradeSafetyGuards(",
    "PendingExpiration(",
):
    if token not in PENDING_SUBMISSION_CODE:
        raise SystemExit(f"Shared pending submission ownership missing: {token}")

# Aggressive execution boundary.
AGGRESSIVE_PRETRADE = ROOT / "Trading" / "Execution" / "Aggressive" / "AggressivePreTradePreparation.cs"
AGGRESSIVE_PRETRADE_CODE = AGGRESSIVE_PRETRADE.read_text(encoding="utf-8")
if AGGRESSIVE_PRETRADE.stat().st_size > 4096:
    raise SystemExit("AggressivePreTradePreparation.cs must remain an orchestration boundary")
for token in (
    "PassAggressivePreTradeEligibility(",
    "TryPrepareAggressiveExecution(",
):
    if token not in AGGRESSIVE_PRETRADE_CODE:
        raise SystemExit(f"Aggressive pre-trade orchestration call missing: {token}")

for path, token in (
    (
        ROOT / "Trading" / "Execution" / "Aggressive" / "AggressivePreTradeEligibility.cs",
        "ValidateSingleExecutionCapacity("
    ),
    (
        ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveExecutionPreparation.cs",
        "BuildStructuralStop("
    ),
    (
        ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveAcceptedFillHandler.cs",
        "ValidateActualMarketFill("
    ),
):
    if not path.exists() or token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Aggressive execution owner missing or incomplete: {path}")

# Aggressive broker execution owner was removed from the Indicator in CBOT-P4A.
# Automatic-market broker execution owner was removed in the same cutover.
# Automatic Market broker mutation is cBot-owned after CBOT-P4A.
# Live target progression boundary.
TARGET_PROGRESSION = ROOT / "Trading" / "LiveManagement" / "TargetProgression.cs"
TARGET_PROGRESSION_CODE = TARGET_PROGRESSION.read_text(encoding="utf-8")
if TARGET_PROGRESSION.stat().st_size > 8192:
    raise SystemExit("TargetProgression.cs must remain a live target orchestration owner")
for token in (
    "BuildTargetLevels(",
    "FindImprovedLiveTarget(",
    "RecalculatePlanRR(",
):
    if token not in TARGET_PROGRESSION_CODE:
        raise SystemExit(f"TargetProgression orchestration call missing: {token}")
for declaration in (
    "private Level FindImprovedLiveTarget(",
    "private bool IsEligibleLiveTarget(",
    "private bool IsImprovedLiveTarget(",
    "private double CalculateLiveTargetScore(",
):
    if declaration in TARGET_PROGRESSION_CODE:
        raise SystemExit(f"TargetProgression retains extracted evaluation: {declaration}")
for required_path in (
    ROOT / "Trading" / "LiveManagement" / "LiveTargetCandidateEvaluator.cs",
    ROOT / "Trading" / "LiveManagement" / "PlanRiskRewardRecalculator.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Live target progression owner missing: {required_path}")

# Reward-path validation boundary.
REWARD_PATH_FILES = (
    ROOT / "Trading" / "Validation" / "RewardPathZoneObstacleScanner.cs",
    ROOT / "Core" / "Math" / "RewardPathGeometryRule.cs",
    ROOT / "Trading" / "Validation" / "TargetObstacleValidator.cs",
    ROOT / "Trading" / "Validation" / "HigherTfRewardPathValidator.cs",
    ROOT / "Trading" / "Validation" / "HtfTargetPresenceValidator.cs",
)
for required_path in REWARD_PATH_FILES:
    if not required_path.exists():
        raise SystemExit(f"Reward-path owner missing: {required_path}")
if (ROOT / "Trading" / "Validation" / "RewardPathGeometryRule.cs").exists():
    raise SystemExit("Legacy Trading reward-path geometry owner must not remain")
if (ROOT / "Trading" / "Validation" / "RewardPathValidation.cs").exists():
    raise SystemExit("Obsolete RewardPathValidation.cs must not return")
for path, declarations in {
    REWARD_PATH_FILES[0]: ("private bool HasOpposingZonePathObstacle(",),
    REWARD_PATH_FILES[1]: ("public static bool BlocksRewardPath(",),
    REWARD_PATH_FILES[2]: ("private bool HasTargetObstacle(",),
    REWARD_PATH_FILES[3]: ("private bool HasHigherTfZonePathObstacle(",),
    REWARD_PATH_FILES[4]: ("private bool HasAnyHtfTargetLevel(",),
}.items():
    code = path.read_text(encoding="utf-8")
    for declaration in declarations:
        if declaration not in code:
            raise SystemExit(f"Reward-path owner declaration missing: {declaration}")
    if path.stat().st_size > 8192:
        raise SystemExit(f"Reward-path owner is too large: {path}")

# Market-frame analysis boundary.
MARKET_FRAME_ANALYZER = ROOT / "Analysis" / "Market" / "MarketFrameAnalyzer.cs"
MARKET_FRAME_ANALYZER_CODE = MARKET_FRAME_ANALYZER.read_text(encoding="utf-8")
if MARKET_FRAME_ANALYZER.stat().st_size > 4096:
    raise SystemExit("MarketFrameAnalyzer.cs must remain a thin orchestration boundary")
if "BuildMarketFrameEvidence(" not in MARKET_FRAME_ANALYZER_CODE:
    raise SystemExit("MarketFrameAnalyzer must delegate evidence construction")
if "ScoreMarketFrame(" not in MARKET_FRAME_ANALYZER_CODE:
    raise SystemExit("MarketFrameAnalyzer must delegate frame scoring")
for required_path in (
    ROOT / "Analysis" / "Market" / "MarketFrameEvidence.cs",
    ROOT / "Analysis" / "Market" / "MarketFrameScoringService.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Market-frame owner missing: {required_path}")
for token in ("AddScore(", "BuildOssIndicatorSnapshot(", "BullStructure(", "FindNearestFvg("):
    if token in MARKET_FRAME_ANALYZER_CODE:
        raise SystemExit(f"MarketFrameAnalyzer retains extracted evidence/scoring responsibility: {token}")

# Calculation-cycle orchestration boundary.
CALCULATION_CYCLE = ROOT / "Runtime" / "Calculation" / "CalculationCycle.cs"
CALCULATION_CYCLE_CODE = CALCULATION_CYCLE.read_text(encoding="utf-8")
if CALCULATION_CYCLE.stat().st_size > 4096:
    raise SystemExit("CalculationCycle.cs must remain a thin orchestration boundary")
if len(re.findall(r"\bpublic\s+override\s+void\s+Calculate\s*\(", CALCULATION_CYCLE_CODE)) != 1:
    raise SystemExit("CalculationCycle must own exactly one Calculate override")
for token in (
    "RunCalculationPreparationStage(",
    "RunClosedBarAnalysisStage(",
    "ProcessLiveCalculationStages(",
):
    if token not in CALCULATION_CYCLE_CODE:
        raise SystemExit(f"CalculationCycle stage orchestration call missing: {token}")

for forbidden in (
    "TryPrepareCalculationCycle(",
    "ProcessNewClosedBar(",
    "ProcessLiveCalculation(",
):
    if forbidden in CALCULATION_CYCLE_CODE:
        raise SystemExit(f"CalculationCycle must not directly own extracted stage: {forbidden}")
for forbidden in (
    "AnalyzeFrame(",
    "BuildDecision(",
    "BuildEarlyPrediction(",
    "TryAutoTrade(",
    "TryAggressiveAutoTrade(",
    "TrySmartPendingOrders(",
    "ProtectBrokerPositions(",
    "MonitorOutcome(",
    "RenderPanel(",
):
    if forbidden in CALCULATION_CYCLE_CODE:
        raise SystemExit(f"CalculationCycle retains extracted responsibility: {forbidden}")
for required_path in (
    ROOT / "Runtime" / "Calculation" / "CalculationPreparation.cs",
    ROOT / "Runtime" / "Calculation" / "CalculationClosedBar.cs",
    ROOT / "Runtime" / "Calculation" / "CalculationDecisionAlerts.cs",
    ROOT / "Runtime" / "Calculation" / "CalculationLiveCycle.cs",
    ROOT / "Runtime" / "Calculation" / "CalculationStageIsolation.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Calculation-cycle owner missing: {required_path}")

# Phase 8 contract/source invariants.
CONTRACTS = {
    "decision": Path("tools/CFIP.Decision.Contracts/Program.cs"),
    "planning": Path("tools/CFIP.Planning.Contracts/Program.cs"),
    "execution": Path("tools/CFIP.Execution.Contracts/Program.cs"),
}
for name, path in CONTRACTS.items():
    if not path.exists():
        raise SystemExit(f"Contract fixture missing: {path}")

for token, path in {
    "VerifyConsensusSymmetry": CONTRACTS["decision"],
    "VerifyConfidenceDeterminism": CONTRACTS["decision"],
    "VerifyStopTargetProtection": CONTRACTS["planning"],
    "VerifyLifecycleTransitions": CONTRACTS["planning"],
    "VerifyLifecycleEventIdempotency": CONTRACTS["execution"],
}.items():
    if token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Contract fixture missing: {token}")

for required_file in (
    ROOT / "Core" / "Math" / "PriceProtectionRule.cs",
    ROOT / "Core" / "Math" / "ProtectionProgressionRule.cs",
    ROOT / "Trading" / "Lifecycle" / "LifecycleTransitionPolicy.cs",
    ROOT / "Trading" / "Lifecycle" / "LifecycleEventIdempotencyGuard.cs",
    ROOT / "Trading" / "Lifecycle" / "LifecycleEventState.cs",
):
    if not required_file.exists():
        raise SystemExit(f"Phase 8 invariant owner missing: {required_file}")

if "PriceProtectionRule.ValidateStop(" not in (ROOT / "Trading" / "Validation" / "PriceProtectionValidation.cs").read_text(encoding="utf-8"):
    raise SystemExit("Production stop validation is not delegated to PriceProtectionRule")
if "PriceProtectionRule.ValidateTarget(" not in (ROOT / "Trading" / "Validation" / "PriceProtectionValidation.cs").read_text(encoding="utf-8"):
    raise SystemExit("Production target validation is not delegated to PriceProtectionRule")

event_handlers = {
    "POSITION_OPENED": ROOT / "Trading" / "Lifecycle" / "PositionOpenedHandler.cs",
    "PENDING_CREATED": ROOT / "Trading" / "Lifecycle" / "PendingCreatedHandler.cs",
    "PENDING_FILLED": ROOT / "Trading" / "Lifecycle" / "PendingFilledHandler.cs",
    "PENDING_CANCELLED": ROOT / "Trading" / "Lifecycle" / "PendingCancelledHandler.cs",
    "POSITION_CLOSED": ROOT / "Trading" / "Lifecycle" / "PositionClosedHandler.cs",
}
for event_type, path in event_handlers.items():
    text_module = path.read_text(encoding="utf-8")
    if "_lifecycleEventGuard.TryBegin(" not in text_module or event_type not in text_module:
        raise SystemExit(f"Lifecycle idempotency guard missing from {path}")

# Phase 9 runtime acceptance/source-path invariants.
RUNTIME_CONTRACT = Path("tools/CFIP.Runtime.Contracts/Program.cs")
if not RUNTIME_CONTRACT.exists():
    raise SystemExit("Phase 9 runtime acceptance contract project is missing")

for token in (
    "VerifyMtfContextIntegrity",
    "VerifyMarketExecutionAcceptance",
    "VerifyPendingOrderAcceptance",
    "VerifyRejectedMutationHandling",
    "VerifyFillEnvelopeSymmetry",
    "VerifyInitialProtectionDirectionality",
    "VerifyManagedBreakEvenDirectionality",
    "VerifyProtectionProgression",
    "VerifyTargetProgression",
    "VerifyLifecycleFlows",
    "VerifyLifecycleIdempotency",
):
    if token not in RUNTIME_CONTRACT.read_text(encoding="utf-8"):
        raise SystemExit(f"Runtime acceptance contract missing: {token}")

MTF_BUILDER = ROOT / "Runtime" / "Mtf" / "MtfContextBuilder.cs"
MTF_BUILDER_CODE = MTF_BUILDER.read_text(encoding="utf-8")
if len(re.findall(r"\bClosedIndex\s*\(", MTF_BUILDER_CODE)) != 8:
    raise SystemExit("MTF builder must resolve exactly eight closed-bar series indices")
if "BuildMtfClosedContext(" not in MTF_BUILDER_CODE:
    raise SystemExit("MTF closed-context owner missing")

PARTIAL_EXECUTOR = ROOT / "Trading" / "LiveManagement" / "PartialTakeProfitExecutor.cs"
PARTIAL_CODE = PARTIAL_EXECUTOR.read_text(encoding="utf-8")
if "GetManagedLivePositionForPlan()" not in PARTIAL_CODE:
    raise SystemExit("Partial close must bind to the active live plan position")
if "PARTIAL CLOSE • BREAK-EVEN REJECTED" not in PARTIAL_CODE:
    raise SystemExit("Partial break-even rejection recovery path is missing")

BROKER_PROTECTION_EXECUTION = ROOT / "Trading" / "Execution" / "Aggressive" / "BrokerProtectionExecution.cs"
BROKER_PROTECTION_CODE = BROKER_PROTECTION_EXECUTION.read_text(encoding="utf-8")
BOUND_PLAN_PROTECTION = ROOT / "Trading" / "Execution" / "Aggressive" / "BoundPlanProtection.cs"
BOUND_PLAN_CODE = BOUND_PLAN_PROTECTION.read_text(encoding="utf-8")
ORPHAN_PROTECTION = ROOT / "Trading" / "Execution" / "Aggressive" / "OrphanManagedProtection.cs"
ORPHAN_PROTECTION_CODE = ORPHAN_PROTECTION.read_text(encoding="utf-8")

if "Never apply its stop/target to another position" not in BROKER_PROTECTION_CODE:
    raise SystemExit("Broker protection must document and enforce plan-position identity")
if 'position.Id == _plan.PositionId' not in BROKER_PROTECTION_CODE:
    raise SystemExit("Plan protection lookup must be bound to the plan PositionId")
if "_plan.Stop" not in BOUND_PLAN_CODE:
    raise SystemExit("Bound plan protection must consume the active plan stop")
if "_plan.PositionId" not in BROKER_PROTECTION_CODE:
    raise SystemExit("Plan position identity must remain in the protection orchestrator")
if "ORPHAN MANAGED POSITION" not in ORPHAN_PROTECTION_CODE:
    raise SystemExit("Orphan broker protection must use its own protection context")

# Aggressive accepted-fill authority moves with the cBot execution phase.

PROTECTION_RUNTIME = ROOT / "Trading" / "Execution" / "Aggressive" / "BrokerProtectionExecution.cs"
PROTECTION_CODE = PROTECTION_RUNTIME.read_text(encoding="utf-8")
BOUND_PROTECTION_CODE = (ROOT / "Trading" / "Execution" / "Aggressive" / "BoundPlanProtection.cs").read_text(encoding="utf-8")
if "IsValidManagedStop(" not in BOUND_PROTECTION_CODE:
    raise SystemExit("Live broker protection must validate managed stops against market price")
if "ProtectionProgressionRule.ShouldAdvanceStop(" not in BOUND_PROTECTION_CODE:
    raise SystemExit("Live broker protection must enforce monotonic SL progression")
if "ProtectionProgressionRule.ShouldAdvanceTarget(" not in BOUND_PROTECTION_CODE:
    raise SystemExit("Live broker protection must enforce configured TP progression")
if "brokerStopValid" not in BOUND_PROTECTION_CODE:
    raise SystemExit("Live broker protection must inspect actual broker SL validity before clearing recovery")
if "brokerTargetValid" not in BOUND_PROTECTION_CODE:
    raise SystemExit("Live broker protection must inspect actual broker TP validity before clearing recovery")

BROKER_STATE = ROOT / "Trading" / "Lifecycle" / "BrokerStateSnapshot.cs"
BROKER_STATE_CODE = BROKER_STATE.read_text(encoding="utf-8")
BROKER_PROTECTION_STATE = ROOT / "Trading" / "Lifecycle" / "BrokerProtectionStateEvaluator.cs"
BROKER_PROTECTION_STATE_CODE = BROKER_PROTECTION_STATE.read_text(encoding="utf-8")
if "EvaluateBrokerProtection(" not in BROKER_STATE_CODE:
    raise SystemExit("Broker state snapshot must consume shared protection evaluation")
if "IsExistingManagedStopHealthy(" not in BROKER_PROTECTION_STATE_CODE:
    raise SystemExit("Shared broker protection state must validate existing SL directionally")
if "IsValidTarget(" not in BROKER_PROTECTION_STATE_CODE:
    raise SystemExit("Shared broker protection state must validate TP directionality")
if "LifecycleState.RecoveryRequired" not in BROKER_STATE_CODE:
    raise SystemExit("Invalid broker protection must enter explicit recovery")

ACTIVE_STOP = ROOT / "Trading" / "Lifecycle" / "ActiveBrokerStopAccessor.cs"
ACTIVE_STOP_CODE = ACTIVE_STOP.read_text(encoding="utf-8")
if "return _plan.Stop" in ACTIVE_STOP_CODE:
    raise SystemExit("Active broker SL accessor must not fall back to the desired plan stop")
if "return 0;" not in ACTIVE_STOP_CODE:
    raise SystemExit("Active broker SL accessor must expose zero when no confirmed broker SL exists")

ACTIVE_PLAN = ROOT / "Trading" / "LiveManagement" / "ActivePlanEvaluation.cs"
ACTIVE_PLAN_CODE = ACTIVE_PLAN.read_text(encoding="utf-8")
ACTIVE_PLAN_LEVELS = ROOT / "Trading" / "LiveManagement" / "ActivePlanLevelExitHandler.cs"
ACTIVE_PLAN_LEVEL_CODE = ACTIVE_PLAN_LEVELS.read_text(encoding="utf-8")
if "IsFinitePositive(liveStop)" not in ACTIVE_PLAN_LEVEL_CODE:
    raise SystemExit("Live SL exit must require a confirmed broker stop")

for handler_name in ("PositionOpenedHandler.cs", "PositionModifiedHandler.cs"):
    handler_path = ROOT / "Trading" / "Lifecycle" / handler_name
    handler_code = handler_path.read_text(encoding="utf-8")
    if "EvaluateBrokerProtection(" not in handler_code:
        raise SystemExit(f"{handler_name} must consume shared broker protection evaluation")

PROTECTION_COORDINATOR = ROOT / "Trading" / "Execution" / "BrokerProtectionCoordinator.cs"
PROTECTION_COORDINATOR_CODE = PROTECTION_COORDINATOR.read_text(encoding="utf-8")
for token in ("currentStopValid", "currentTargetValid", "desiredStopValid", "desiredTargetValid"):
    if token not in PROTECTION_COORDINATOR_CODE:
        raise SystemExit(f"Broker protection coordinator missing reconciliation guard: {token}")

EMPTY_MUTATION_STUB = ROOT / "Trading" / "Execution" / "BrokerMutationCoordinator.cs"
if EMPTY_MUTATION_STUB.exists():
    raise SystemExit("Empty broker mutation stub must not return to production")


INITIALIZATION = ROOT / "Runtime" / "Initialization" / "RuntimeInitialization.cs"
INITIALIZATION_CODE = INITIALIZATION.read_text(encoding="utf-8")
for token in (
    "Positions.Opened += OnPositionOpened",
    "Positions.Closed += OnPositionClosed",
    "PendingOrders.Filled += OnPendingOrderFilled",
    "Positions.Opened -= OnPositionOpened",
    "PendingOrders.Filled -= OnPendingOrderFilled",
):
    if token not in INITIALIZATION_CODE:
        raise SystemExit(f"Runtime lifecycle wiring missing: {token}")

for forbidden in (
    "ExecuteMarketOrder(",
    "PlaceStopOrder(",
    "PlaceLimitOrder(",
    "ClosePosition(",
    "CancelPendingOrder(",
):
    for root in (ROOT / "Analysis", ROOT / "Planning"):
        for p in sorted(root.rglob("*.cs")):
            if forbidden in strip_for_static_checks(p.read_text(encoding="utf-8")):
                raise SystemExit(f"Broker mutation escaped analysis/planning: {p} -> {forbidden}")


# Decision services must remain free of cTrader host dependencies.
DECISION_SERVICE_ROOT = ROOT / "Analysis" / "Market" / "Decision"
DECISION_SERVICE_FILES = {
    "DecisionEvidenceSnapshot.cs",
    "DecisionScoreSnapshot.cs",
    "DecisionScoreCalculator.cs",
    "DecisionConsensusSnapshot.cs",
    "DecisionConsensusCalculator.cs",
    "DecisionQualityCalculator.cs",
    "DecisionConfidenceCalculator.cs",
    "EmpiricalConfidenceCalibrator.cs",
    "DecisionFilterResult.cs",
    "DecisionThresholdFilterInput.cs",
    "DecisionThresholdFilterEvaluator.cs",
    "DecisionSmartConsensusFilterInput.cs",
    "DecisionSmartConsensusFilterEvaluator.cs",
    "DecisionReasonFormatter.cs",
}
for filename in DECISION_SERVICE_FILES:
    path = DECISION_SERVICE_ROOT / filename
    if not path.exists():
        raise SystemExit(f"Decision service missing: {filename}")
    text_module = path.read_text(encoding="utf-8")
    if re.search(r"\bcAlgo\.API(?:\.Indicators|\.Internals)?\b", text_module):
        raise SystemExit(f"cTrader dependency leaked into pure decision service: {filename}")

SNAPSHOT = DECISION_SERVICE_ROOT / "DecisionInputSnapshot.cs"
SNAPSHOT_CODE = SNAPSHOT.read_text(encoding="utf-8")
if "Func<" in SNAPSHOT_CODE:
    raise SystemExit("DecisionInputSnapshot contains executable callbacks")
REQUEST = DECISION_SERVICE_ROOT / "DecisionInputBuildRequest.cs"
if "Func<" in REQUEST.read_text(encoding="utf-8"):
    raise SystemExit("DecisionInputBuildRequest contains executable callbacks")
if not (DECISION_SERVICE_ROOT / "DecisionEvidenceSnapshot.cs").exists():
    raise SystemExit("Immutable decision evidence snapshot is missing")
if (ROOT / "Trading" / "Intelligence" / "DecisionFeatures.cs").exists():
    raise SystemExit("DecisionFeatures.cs must remain removed after intelligence split")

FILTER_PIPELINE = ROOT / "Analysis" / "Market" / "Decision" / "DecisionFilters.cs"
FILTER_CODE = FILTER_PIPELINE.read_text(encoding="utf-8")
if len(re.findall(r"^\s*private\s+bool\s+PassesDecisionFilters\s*\(", FILTER_CODE, re.MULTILINE)) != 1:
    raise SystemExit("DecisionFilters must own exactly one pipeline entry point")
for helper_name in (
    "EvaluateDecisionConfirmationGates",
    "EvaluateDecisionSmartGates",
    "EvaluateDecisionStructureGates",
    "EvaluateDecisionMarketGates",
    "EvaluateDecisionLifecycleGates",
):
    declaration_pattern = re.compile(
        r"^\s*(?:public|private|protected|internal)\b[^\r\n{;]*\b"
        + re.escape(helper_name)
        + r"\s*\(",
        re.MULTILINE,
    )
    owners = [
        p for p in files
        if len(declaration_pattern.findall(p.read_text(encoding="utf-8"))) == 1
    ]
    if len(owners) != 1:
        raise SystemExit(f"Decision gate ownership failed: {helper_name}")

# Presentation/UI boundary checks.
UI_ROOT = ROOT / "UI"
UI_FORBIDDEN_TOKENS = (
    "ExecuteMarketOrder(",
    "PlaceStopOrder(",
    "PlaceLimitOrder(",
    "ModifyStopLossPrice(",
    "ModifyTakeProfitPrice(",
    "ModifyPendingOrder(",
    "CancelPendingOrder(",
    "ClosePosition(",
    "PassesDecisionFilters(",
    "EvaluateDecision(",
    "EvaluateDecisionConfirmationGates(",
    "EvaluateDecisionSmartGates(",
    "EvaluateDecisionStructureGates(",
    "EvaluateDecisionMarketGates(",
    "EvaluateDecisionLifecycleGates(",
)
for p in sorted(UI_ROOT.rglob("*.cs")):
    relative = p.relative_to(ROOT)
    ui_text = strip_for_static_checks(p.read_text(encoding="utf-8"))
    for token in UI_FORBIDDEN_TOKENS:
        if token in ui_text:
            raise SystemExit(f"UI authority violation: {relative} -> {token}")

# Broker mutation boundary checks.
PRODUCTION_ROOT = ROOT
BROKER_MUTATION_PATTERNS = (
    r"(?<!Try)ExecuteMarketOrder\s*\(",
    r"(?<!Try)PlaceStopOrder\s*\(",
    r"(?<!Try)PlaceLimitOrder\s*\(",
    r"(?<!Try)ModifyStopLossPrice\s*\(",
    r"(?<!Try)ModifyTakeProfitPrice\s*\(",
    r"(?<!Try)ModifyPendingOrder\s*\(",
    r"(?<!Try)CancelPendingOrder\s*\(",
    r"(?<!Try)ClosePosition\s*\(",
)
for p in sorted(PRODUCTION_ROOT.rglob("*.cs")):
    relative = p.relative_to(ROOT)
    text_module = strip_for_static_checks(p.read_text(encoding="utf-8"))
    direct = [
        pattern for pattern in BROKER_MUTATION_PATTERNS
        if re.search(pattern, text_module)
    ]
    if direct:
        raise SystemExit(
            f"Indicator direct broker mutation remains: {relative}"
        )

CBOT_MANAGEMENT_OWNER = (
    ROOT.parent /
    "CFIP.cBot" /
    "Execution" /
    "ManagementExecutionCoordinator.cs"
)
if not CBOT_MANAGEMENT_OWNER.exists():
    raise SystemExit("cBot ManagementExecutionCoordinator is missing after P4E")

EXECUTION_PLAN = ROOT / "Trading" / "Execution" / "ExecutionPlanPreparation.cs"
if any(re.search(pattern, strip_for_static_checks(EXECUTION_PLAN.read_text(encoding="utf-8")))
       for pattern in BROKER_MUTATION_PATTERNS):
    raise SystemExit("Broker mutation leaked into execution plan preparation")

# Phase 7.1 — hidden-clamp parameter semantics.
# These checks protect user-facing parameters from being silently overridden by
# tighter hard-coded bounds than their declared cTrader MinValue/MaxValue.
TRIGGER_PARAMETER_FILE = PARAMETER_ROOT / "15_control_advanced.cs"
TRIGGER_PARAMETER_CODE = TRIGGER_PARAMETER_FILE.read_text(encoding="utf-8")
TARGET_UPDATE_PARAMETER = PARAMETER_ROOT / "15_control_advanced.cs"

if not re.search(
    r'\[Parameter\("Live Trigger Score"[^\n]*MinValue\s*=\s*1[^\n]*MaxValue\s*=\s*6',
    TRIGGER_PARAMETER_CODE,
):
    raise SystemExit("Phase 7.1: Live Trigger Score parameter bounds changed unexpectedly")

if not re.search(
    r'\[Parameter\("Target Update Step ATR"[^\n]*MinValue\s*=\s*0\.02[^\n]*MaxValue\s*=\s*2',
    TRIGGER_PARAMETER_CODE,
):
    raise SystemExit("Phase 7.1: Target Update Step ATR parameter bounds changed unexpectedly")

TRIGGER_READY = ROOT / "Planning" / "Entry" / "ClosedBarTriggerReadyEvaluator.cs"
TRIGGER_READY_CODE = TRIGGER_READY.read_text(encoding="utf-8")
if re.search(
    r'Math\.Max\(\s*4\s*,\s*requiredTrigger\s*\)',
    TRIGGER_READY_CODE,
    re.DOTALL,
):
    raise SystemExit("Phase 7.1: Live/precision trigger threshold is hidden behind a hard floor of 4")
if "TriggerThresholdRule.IsScoreReady(" not in TRIGGER_READY_CODE:
    raise SystemExit("Phase 7.1: trigger threshold must consume the canonical effective user-facing requiredTrigger")

LIVE_TARGET = ROOT / "Trading" / "LiveManagement" / "LiveTargetCandidateEvaluator.cs"
LIVE_TARGET_CODE = LIVE_TARGET.read_text(encoding="utf-8")
if re.search(
    r'Math\.Max\(\s*0\.05\s*,\s*TargetUpdateStepAtr\s*\)',
    LIVE_TARGET_CODE,
    re.DOTALL,
):
    raise SystemExit("Phase 7.1: TargetUpdateStepAtr is hidden behind a hard floor of 0.05 ATR")
if "atr *\n                TargetUpdateStepAtr" not in LIVE_TARGET_CODE:
    raise SystemExit("Phase 7.1: target update step must consume TargetUpdateStepAtr directly")

# Runtime UI, execution-priority and protection hotfix contracts.
EXECUTION_CONTROLS_FACTORY = ROOT / "UI" / "Controls" / "ExecutionControlsFactory.cs"
EXECUTION_CONTROLS_FACTORY_CODE = EXECUTION_CONTROLS_FACTORY.read_text(encoding="utf-8")
if "CreateExecutionToggle(" not in EXECUTION_CONTROLS_FACTORY_CODE:
    raise SystemExit("Panel execution controls must use the shared status ToggleButton surfaces")
if "IsEnabled = false" not in EXECUTION_CONTROLS_FACTORY_CODE:
    raise SystemExit("Panel execution controls must remain status-only")
for token in (
    "_autoTradingQuickToggle.Click +=",
    "_automaticOrdersQuickToggle.Click +=",
):
    if token in EXECUTION_CONTROLS_FACTORY_CODE:
        raise SystemExit(f"Panel execution control must not register a mutation handler: {token}")

EXECUTION_CONTROL_PRESENTATION = ROOT / "Core" / "Math" / "ExecutionControlPresentationRule.cs"
EXECUTION_CONTROL_PRESENTATION_CODE = EXECUTION_CONTROL_PRESENTATION.read_text(encoding="utf-8")
if "public static bool IsInteractive => false;" not in EXECUTION_CONTROL_PRESENTATION_CODE:
    raise SystemExit("Execution-control presentation policy must be canonical and read-only")

EXECUTION_CONTROLS_SYNC = ROOT / "UI" / "Controls" / "ExecutionControlsSynchronizer.cs"
EXECUTION_CONTROLS_SYNC_CODE = EXECUTION_CONTROLS_SYNC.read_text(encoding="utf-8")
if "_executionToggleSyncing = true" not in EXECUTION_CONTROLS_SYNC_CODE:
    raise SystemExit("Execution toggle visual synchronization must be guarded")

CALC_STAGE = ROOT / "Runtime" / "Calculation" / "CalculationStageIsolation.cs"
CALC_STAGE_CODE = CALC_STAGE.read_text(encoding="utf-8")
pending_idx = CALC_STAGE_CODE.find('"PENDING INTENT PREPARATION"')
plan_idx = CALC_STAGE_CODE.find('"PLAN CREATION"')
if min(pending_idx, plan_idx) < 0 or not pending_idx < plan_idx:
    raise SystemExit("Remaining Indicator execution stage ordering must keep pending before plan materialization")
if "AGGRESSIVE AUTO EXECUTION" in CALC_STAGE_CODE:
    raise SystemExit("Aggressive broker execution must not remain in Indicator calculation stages")
if "AUTOMATIC MARKET EXECUTION" in CALC_STAGE_CODE:
    raise SystemExit("Automatic Market broker execution must not remain in Indicator calculation stages")

LIVE_CYCLE = ROOT / "Runtime" / "Calculation" / "CalculationLiveCycle.cs"
LIVE_CYCLE_CODE = LIVE_CYCLE.read_text(encoding="utf-8")
if "GetManagedPendingOrder() != null" not in LIVE_CYCLE_CODE:
    raise SystemExit("Automatic plan creation must defer while a managed pending order exists")

PLAN_RENDERER = ROOT / "UI" / "Chart" / "PlanRenderCoordinator.cs"
PLAN_RENDERER_CODE = PLAN_RENDERER.read_text(encoding="utf-8")
if "RenderPlanLabels(snapshot, true)" not in PLAN_RENDERER_CODE:
    raise SystemExit("Setup preview must render compact level labels")
if "RemovePlanLabels();" in PLAN_RENDERER_CODE and "RenderPlanLabels(snapshot, true)" not in PLAN_RENDERER_CODE:
    raise SystemExit("Setup preview must not be label-less")

PLAN_LABEL_COORDINATOR = ROOT / "UI" / "Chart" / "PlanLabelRenderCoordinator.cs"
PLAN_LABEL_COORDINATOR_CODE = PLAN_LABEL_COORDINATOR.read_text(encoding="utf-8")
if "bool preview" not in PLAN_LABEL_COORDINATOR_CODE:
    raise SystemExit("Plan label renderer must accept preview context")
preview_level_contract = PLAN_RENDERER_CODE[PLAN_RENDERER_CODE.find("private PlanLevelVisualState BuildPlanLevelVisualState("):]
for required in (
    "preview ? snapshot.SetupEntry : snapshot.Entry",
    "preview ? snapshot.SetupIdealEntry : snapshot.IdealEntry",
    "preview ? snapshot.SetupTrigger : snapshot.Trigger",
    "preview ? snapshot.SetupStop : snapshot.Stop",
    "preview ? snapshot.SetupTp1 : snapshot.Tp1",
):
    if required not in preview_level_contract:
        raise SystemExit("Preview compact labels must use canonical preview level values")

PLAN_LABEL_RENDERER = ROOT / "UI" / "Chart" / "PlanLabelRenderer.cs"
PLAN_LABEL_RENDERER_CODE = PLAN_LABEL_RENDERER.read_text(encoding="utf-8")
if "GetReadableLabelTextColor(" not in PLAN_LABEL_RENDERER_CODE:
    raise SystemExit("Compact plan labels must resolve a canonical semantic text color")
compact_label_start = PLAN_LABEL_RENDERER_CODE.find("private void DrawCompactPlanLabel(")
compact_label_code = PLAN_LABEL_RENDERER_CODE[compact_label_start:] if compact_label_start >= 0 else ""
if compact_label_start < 0:
    raise SystemExit("Compact plan label renderer method is missing")
if "Chart.DrawRectangle(" in compact_label_code:
    raise SystemExit("Compact plan labels must remain background-free")
if "Chart.DrawText(" not in compact_label_code:
    raise SystemExit("Compact plan labels must own their native ChartText object")
if "GetReadableLabelTextColor(" not in compact_label_code or "return Color.White;" not in compact_label_code:
    raise SystemExit("Compact plan labels must use the canonical white text resolver")
if "CompactPlanLabelFontSize = 10.0" not in PLAN_LABEL_RENDERER_CODE:
    raise SystemExit("Compact plan labels must use the canonical readable font size")
if "CompactPlanLabelGapBars = 1" not in PLAN_LABEL_RENDERER_CODE:
    raise SystemExit("Compact plan labels must keep exactly one chart-bar left clearance")
if "HorizontalAlignment.Right" not in compact_label_code:
    raise SystemExit("Compact plan labels must terminate at the left-of-line anchor")
if "Chart.RemoveObject(" not in compact_label_code:
    raise SystemExit("Compact plan labels must clean legacy chart objects")

PROTECTION_MANAGER = ROOT / "Trading" / "LiveManagement" / "ProtectionManager.cs"
PROTECTION_MANAGER_CODE = PROTECTION_MANAGER.read_text(encoding="utf-8")
INTELLIGENT_PROTECTION_RULE = ROOT / "Core" / "Math" / "IntelligentProtectionRule.cs"
INTELLIGENT_PROTECTION_RULE_CODE = INTELLIGENT_PROTECTION_RULE.read_text(encoding="utf-8")
if "market - minimumDistance" in PROTECTION_MANAGER_CODE or "market + minimumDistance" in PROTECTION_MANAGER_CODE:
    raise SystemExit("Structural trailing must not chase raw market price via minimum-distance clamping")
if "structuralUpdate" not in INTELLIGENT_PROTECTION_RULE_CODE or "IsStructuralFarEnough(" not in INTELLIGENT_PROTECTION_RULE_CODE:
    raise SystemExit("Smart trailing progression must be structurally gated by the canonical protection rule")
if "IntelligentProtectionRule.Evaluate(" not in PROTECTION_MANAGER_CODE:
    raise SystemExit("ProtectionManager must delegate to the canonical intelligent protection owner")

# Phase 7.2 — predictive pending, frozen setup geometry and alert/visual parity.
PREDICTIVE_PENDING_MODEL = MODEL_ROOT / "PredictivePendingCandidate.cs"
PREDICTIVE_PENDING_SELECTOR = ROOT / "Planning" / "Execution" / "PredictivePendingLevelSelector.cs"
REVERSAL_LIMIT_PREP = ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPreparation.cs"
SETUP_PREVIEW = ROOT / "Planning" / "TradePlan" / "PlanPreviewBuilder.cs"
if not PREDICTIVE_PENDING_MODEL.exists():
    raise SystemExit("Predictive pending candidate model is missing")
if not PREDICTIVE_PENDING_SELECTOR.exists():
    raise SystemExit("Predictive pending level selector is missing")
if not REVERSAL_LIMIT_PREP.exists():
    raise SystemExit("Reversal limit preparation is missing")

predictive_selector_code = PREDICTIVE_PENDING_SELECTOR.read_text(encoding="utf-8")
predictive_collector_code = (
    (ROOT / "Planning" / "Execution" / "PredictivePendingZoneCollector.cs")
    .read_text(encoding="utf-8")
)
predictive_scorer_code = (
    (ROOT / "Planning" / "Execution" / "PredictivePendingCandidateScorer.cs")
    .read_text(encoding="utf-8")
)
reversal_limit_code = REVERSAL_LIMIT_PREP.read_text(encoding="utf-8")
setup_preview_code = SETUP_PREVIEW.read_text(encoding="utf-8")

for token in (
    "TrySelectPredictivePendingLevel(",
    "CollectPredictiveZoneCandidates(",
    "FindEqualLow(",
    "FindEqualHigh(",
):
    if token not in predictive_selector_code:
        raise SystemExit(
            f"Predictive pending selector contract missing: {token}"
        )

for token in (
    "BuildOrderBlockCandidate(",
    "BuildManagedFvgZone(",
    "HasZoneRetest(",
):
    if token not in predictive_collector_code:
        raise SystemExit(
            f"Predictive pending collector contract missing: {token}"
        )

for token in (
    "PredictivePendingContextQuality(",
    "PredictivePendingSourceKey(",
):
    if token not in predictive_scorer_code:
        raise SystemExit(
            f"Predictive pending scorer contract missing: {token}"
        )

if "TrySelectPredictivePendingLevel(" not in reversal_limit_code:
    raise SystemExit(
        "Reversal pending path must consume the predictive level selector"
    )

if (
    "reversalModel.IdealEntry" in reversal_limit_code or
    "Symbol.Bid - atr * 0.25" in reversal_limit_code or
    "Symbol.Ask + atr * 0.25" in reversal_limit_code
):
    raise SystemExit(
        "Reversal pending path must not fall back to a current-price-derived entry"
    )

if (
    "candidate.Source" not in reversal_limit_code or
    "candidate.DistanceAtr" not in reversal_limit_code
):
    raise SystemExit(
        "Reversal pending intent must retain predictive candidate diagnostics"
    )

preview_start = setup_preview_code.find("double entry")
preview_end = setup_preview_code.find(
    "if (!IsFinitePositive(entry)",
    preview_start,
)
if (
    preview_start < 0 or
    preview_end < 0 or
    "execution.IdealEntry" not in
    setup_preview_code[preview_start:preview_end]
):
    raise SystemExit(
        "Setup preview entry must prefer structural IdealEntry over live ActualEntry"
    )

CALC_LIVE = ROOT / "Runtime" / "Calculation" / "CalculationLiveCycle.cs"
calc_live_text = CALC_LIVE.read_text(encoding="utf-8")
execution_start = calc_live_text.find("private void UpdateExecutionModel(")
render_start = calc_live_text.find("private void RenderCalculationState(", execution_start)
if execution_start < 0 or render_start < 0:
    raise SystemExit("UpdateExecutionModel ownership block not found")
update_execution_code = calc_live_text[execution_start:render_start]

if "priceMoved" in update_execution_code or "intervalElapsed" in update_execution_code:
    raise SystemExit(
        "Execution-model geometry must not rebuild from raw quote movement"
    )

if (
    "if (!m5Changed &&" not in update_execution_code or
    "if (!m5Changed &&\n                !directionChanged)" not in update_execution_code
):
    raise SystemExit(
        "Execution-model rebuild must be closed-M5/direction gated"
    )

if "RenderPredictionObjects(" not in calc_live_text:
    raise SystemExit(
        "Prediction visuals must be part of the calculation presentation path"
    )

if "RenderLatestAlertSignalMarker(" not in calc_live_text:
    raise SystemExit(
        "Audible signal alerts must be mirrored by an on-chart marker"
    )

PENDING_RENDERER = ROOT / "UI" / "Chart" / "PendingOrderRenderer.cs"
pending_code = PENDING_RENDERER.read_text(encoding="utf-8")
if "RenderCompactPlanLabel(" not in pending_code:
    raise SystemExit(
        "Pending order levels must use the compact boxed label renderer"
    )
if "RemovePlanLabel(" not in pending_code:
    raise SystemExit(
        "Pending order label cleanup must remove boxed label objects"
    )

ALERT_ENGINE = ROOT / "Trading" / "Alerts" / "AlertEngine.cs"
alert_engine_code = ALERT_ENGINE.read_text(encoding="utf-8")
if "BuildCanonicalAlertEnvelope(" not in alert_engine_code:
    raise SystemExit(
        "AlertEngine must construct the canonical AlertEnvelope"
    )
if "Notifications.PlaySound" in alert_engine_code:
    raise SystemExit(
        "AlertEngine must not own audible transport"
    )
if "new AlertDelivery(" not in alert_engine_code or "envelope," not in alert_engine_code:
    raise SystemExit(
        "AlertEngine must enqueue the canonical alert envelope"
    )

STATE_CODE = INDICATOR_STATE.read_text(encoding="utf-8")
for obsolete in (
    "_lastVisualAlertM5",
    "_lastVisualAlertDirection",
    "_lastVisualAlertKind",
    "_lastVisualAlertUtc",
    "RememberVisualSignalAlert(",
):
    if obsolete in STATE_CODE or obsolete in alert_engine_code:
        raise SystemExit(
            f"Obsolete visual-alert side channel remains: {obsolete}"
        )

ALERT_SIGNAL_RENDERER = ROOT / "UI" / "Chart" / "AlertSignalRenderer.cs"
alert_signal_renderer_code = ALERT_SIGNAL_RENDERER.read_text(encoding="utf-8")
if "RenderLatestAlertSignalMarker(" not in alert_signal_renderer_code:
    raise SystemExit(
        "Alert signal renderer cleanup contract is missing"
    )
if "Chart.DrawIcon(" in alert_signal_renderer_code:
    raise SystemExit(
        "Legacy alert mirror must never create a second chart marker"
    )

cleanup_code = (
    ROOT / "UI" / "Chart" / "ChartObjectCleanup.cs"
).read_text(encoding="utf-8")
if "ALERT_SIGNAL" not in cleanup_code or "ALERT_SIGNAL_LABEL" not in cleanup_code:
    raise SystemExit("Alert signal marker cleanup is missing")

INTELLIGENT_PROTECTION_RULE = ROOT / "Core" / "Math" / "IntelligentProtectionRule.cs"
INTELLIGENT_PROTECTION_RULE_CODE = INTELLIGENT_PROTECTION_RULE.read_text(encoding="utf-8")
if (
    "pressureStop" in PROTECTION_MANAGER_CODE or
    "market - tightRoom" in PROTECTION_MANAGER_CODE or
    "market + tightRoom" in PROTECTION_MANAGER_CODE
):
    raise SystemExit(
        "Smart trailing must not construct stops as a raw market +/- distance"
    )

if (
    "pressureTighten" not in INTELLIGENT_PROTECTION_RULE_CODE or
    "IsStructuralFarEnough(" not in INTELLIGENT_PROTECTION_RULE_CODE or
    "IntelligentProtectionRule.Evaluate(" not in PROTECTION_MANAGER_CODE
):
    raise SystemExit(
        "Exit-pressure tightening must remain inside the canonical structural protection owner"
    )

# Planning/risk ownership checks.
PLANNING_ROOT = ROOT / "Planning"
RISK_ROOT = ROOT / "Trading" / "Risk"
REQUIRED_PLANNING_FILES = {
    "TradePlan/PlanBuilder.cs",
    "TradePlan/PlanIntegrityValidator.cs",
    "TradePlan/TargetProgressionValidator.cs",
    "TradePlan/StructuralStopPlanner.cs",
    "TradePlan/MinimumRequiredRiskRewardCalculator.cs",
    "TradePlan/TargetLevelBuilder.cs",
    "TradePlan/TargetLevelCandidateMerger.cs",
    "TradePlan/TargetLevelMerger.cs",
    "TradePlan/TargetSelector.cs",
    "TradePlan/TargetStageSelector.cs",
    "TradePlan/TargetMetadataEnricher.cs",
    "Entry/ClosedBarTriggerReadyEvaluator.cs",
    "Entry/BullTriggerScoreAnalyzer.cs",
    "Entry/BearTriggerScoreAnalyzer.cs",
    "TradePlan/TargetProgressionRule.cs",
}
for relative in REQUIRED_PLANNING_FILES:
    if not (PLANNING_ROOT / relative).exists():
        raise SystemExit(f"Planning owner missing: {relative}")

REQUIRED_RISK_FILES = {
    "DailyLossGuard.cs",
    "AutoRiskPolicy.cs",
    "AggressiveRiskPolicy.cs",
    "MarginSafetyCalculator.cs",
    "VolumeSizer.cs",
    "ManagedPositionCounter.cs",
    "AutoPlanRiskValidator.cs",
    "AggressiveVolumeSizer.cs",
    "SuitabilityCalculator.cs",
    "SessionWindowEvaluator.cs",
    "AverageAtrCalculator.cs",
    "MarketSuitabilityRefreshCoordinator.cs",
    "MarketSuitabilityGuard.cs",
    "SuitabilityRiskMultiplierCalculator.cs",
    "AutoTradeSafetyGuard.cs",
    "RiskPercentPolicy.cs",
    "RiskAmountCalculator.cs",
    "MarginUsagePolicy.cs",
}
for relative in REQUIRED_RISK_FILES:
    if not (RISK_ROOT / relative).exists():
        raise SystemExit(f"Risk owner missing: {relative}")

for p in sorted(PLANNING_ROOT.rglob("*.cs")):
    relative = str(p.relative_to(ROOT)).replace("\\", "/")
    text_module = strip_for_static_checks(p.read_text(encoding="utf-8"))
    if any(token in text_module for token in (
        "ExecuteMarketOrder", "PlaceLimitOrder", "PlaceStopOrder",
        "ModifyStopLossPrice", "ModifyTakeProfitPrice", "ClosePosition",
        "CancelPendingOrder",
    )):
        raise SystemExit(f"Broker mutation leaked into planning: {relative}")

plan_model = ROOT / "Core" / "Models" / "Plan.cs"
plan_text = plan_model.read_text(encoding="utf-8")
for required in ("Entry", "IdealEntry", "EntryTrigger", "EntryInvalidation", "Stop", "Tp1"):
    if required not in plan_text:
        raise SystemExit(f"Plan semantic field missing: {required}")

# Panel semantic renderer isolation.
PANEL_ROOT = ROOT / "UI" / "Panel"
ROW_EXPECTATIONS = {
    "PanelRowsRenderer.cs": "RenderPanelRows",
    "PanelRowWriter.cs": "SetPanelRow",
    "Rows/PanelOverviewRowsRenderer.cs": "RenderPanelOverviewRows",
    "Rows/PanelDecisionRowsRenderer.cs": "RenderPanelDecisionRows",
    "Rows/PanelExecutionRowsRenderer.cs": "RenderPanelExecutionRows",
    "Rows/PanelTradePlanRowsRenderer.cs": "RenderPanelTradePlanRows",
    "Rows/PanelContextRowsRenderer.cs": "RenderPanelContextRows",
}
for relative, method in ROW_EXPECTATIONS.items():
    path = PANEL_ROOT / relative
    if not path.exists():
        raise SystemExit(f"Panel renderer module missing: {relative}")
    text_module = path.read_text(encoding="utf-8")
    if text_module.count(f"private void {method}(") != 1:
        raise SystemExit(f"Panel renderer ownership check failed: {relative}")

oss_files = {
    "SkenderRsi.cs": "SkenderRsi",
    "SkenderMacd.cs": "SkenderMacdHistogram",
    "SkenderBollingerBands.cs": "SkenderBollingerPercentB",
    "SkenderMfi.cs": "SkenderMfi",
    "SkenderStoch.cs": "SkenderStochBias",
    "SkenderSuperTrend.cs": "SkenderSuperTrend",
    "OssIndicatorConfluenceAnalyzer.cs": "BuildOssIndicatorSnapshot",
    "SkenderAroon.cs": "SkenderAroonOscillator",
    "SkenderCci.cs": "SkenderCci",
    "SkenderObv.cs": "SkenderObvBias",
    "SkenderParabolicSar.cs": "SkenderParabolicSar",
}
OSS_ROOT = ROOT / "Analysis" / "Indicators" / "External"
for filename, method in oss_files.items():
    path = OSS_ROOT / filename
    if not path.exists() or method not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"OSS indicator module missing: {filename}")

PRODUCTION_CSPROJ = ROOT / "CFIP.Indicator.csproj"
BENCHMARK_CSPROJ = Path("tools/CFIP.StockIndicators.Benchmark/CFIP.StockIndicators.Benchmark.csproj")
PRODUCTION_PACKAGE = "PackageReference Include=\"Skender.Stock.Indicators\" Version=\"2.7.3\""
RESEARCH_PACKAGE = "PackageReference Include=\"FacioQuo.Stock.Indicators\" Version=\"3.0.1\""

if PRODUCTION_PACKAGE not in PRODUCTION_CSPROJ.read_text(encoding="utf-8"):
    raise SystemExit("Production OSS package pin is missing or changed")
if "FacioQuo.Stock.Indicators" in raw:
    raise SystemExit("Research-only FacioQuo.Stock.Indicators leaked into production source")
if not BENCHMARK_CSPROJ.exists():
    raise SystemExit("OSS benchmark project is missing")
benchmark_project = BENCHMARK_CSPROJ.read_text(encoding="utf-8")
if RESEARCH_PACKAGE not in benchmark_project:
    raise SystemExit("Research OSS benchmark package pin is missing")
TRACK_19_BENCHMARK_ROOT = Path("tools/CFIP.StockIndicators.Benchmark/Benchmark")
TRACK_19_BENCHMARK_FILES = {
    "BenchmarkModel.cs",
    "BenchmarkFixtures.cs",
    "IndicatorComparison.cs",
    "BenchmarkReport.cs",
    "QuoteCacheBenchmark.cs",
    "SkenderWarmupParityBenchmark.cs",
}
if not TRACK_19_BENCHMARK_ROOT.exists():
    raise SystemExit("Track 19 benchmark source boundary is missing")
benchmark_sources = {p.name for p in TRACK_19_BENCHMARK_ROOT.glob("*.cs")}
if benchmark_sources != TRACK_19_BENCHMARK_FILES:
    raise SystemExit(
        "Track 19 benchmark module set changed unexpectedly: "
        + ", ".join(sorted(benchmark_sources))
    )
comparison_code = (TRACK_19_BENCHMARK_ROOT / "IndicatorComparison.cs").read_text(encoding="utf-8")
for required_metric in (
    '"RSI"', '"MACD"', '"Bollinger Bands"', '"MFI"', '"Stochastic"',
    '"SuperTrend"', '"Aroon"', '"CCI"', '"OBV"', '"Parabolic SAR"',
):
    if required_metric not in comparison_code:
        raise SystemExit(f"Track 19 benchmark metric missing: {required_metric}")
fixture_code = (TRACK_19_BENCHMARK_ROOT / "BenchmarkFixtures.cs").read_text(encoding="utf-8")
for required_scenario in ("TREND_UP", "TREND_DOWN", "RANGE", "REGIME_SHIFT"):
    if f'"{required_scenario}"' not in fixture_code:
        raise SystemExit(f"Track 19 benchmark scenario missing: {required_scenario}")
if "docs/TRACK-19-OSS-NUMERICAL-BENCHMARK.md" not in Path("docs/ROADMAP.md").read_text(encoding="utf-8"):
    raise SystemExit("Track 19 continuity document is not linked from the roadmap")

if PRODUCTION_PACKAGE not in benchmark_project:
    raise SystemExit("Production OSS package must also be covered by the benchmark")


# Phase 9.2 parallel-opportunity / WaveTrend ownership.
required_phase_9_2 = (
    ROOT / "Core" / "Enums" / "OpportunityLane.cs",
    ROOT / "Core" / "Models" / "TradeOpportunityCandidate.cs",
    ROOT / "Core" / "Models" / "WaveTrendSnapshot.cs",
    ROOT / "Core" / "Math" / "WaveTrendEvidenceRule.cs",
    ROOT / "Core" / "Math" / "TacticalOpportunityRule.cs",
    ROOT / "Analysis" / "Market" / "WaveTrendEngine.cs",
    ROOT / "Analysis" / "Market" / "WaveTrendEvidenceAnalyzer.cs",
    ROOT / "Analysis" / "Market" / "ParallelOpportunityBuilder.cs",
    ROOT / "UI" / "Chart" / "ParallelOpportunityRenderer.cs",
)
for required_path in required_phase_9_2:
    if not required_path.exists():
        raise SystemExit(
            f"Phase 9.2 owner is missing: {required_path.as_posix()}"
        )

lane_code = (ROOT / "Core" / "Enums" / "OpportunityLane.cs").read_text(encoding="utf-8")
if "Strategic" not in lane_code or "Tactical" not in lane_code or "CounterHtfTactical" not in lane_code:
    raise SystemExit("Opportunity lane taxonomy is incomplete")

parallel_builder_code = (ROOT / "Analysis" / "Market" / "ParallelOpportunityBuilder.cs").read_text(encoding="utf-8")
parallel_renderer_code = (ROOT / "UI" / "Chart" / "ParallelOpportunityRenderer.cs").read_text(encoding="utf-8")
if "EvaluateTacticalOpportunityForDirection(" not in parallel_builder_code:
    raise SystemExit("Parallel opportunity builder must evaluate independent LTF directions")
if "_lastOpportunityCandidatesM5" not in parallel_builder_code:
    raise SystemExit("Parallel opportunity builder must cache structural rebuild cadence")
if (
    'string baseName =' not in parallel_renderer_code or
    '"OPP_"' not in parallel_renderer_code
):
    raise SystemExit("Parallel opportunities must use an isolated visual namespace")
if "ShowTacticalOpportunityLabels" not in parallel_renderer_code:
    raise SystemExit("Parallel opportunity renderer must honor label visibility control")

wt_rule_code = (ROOT / "Core" / "Math" / "WaveTrendEvidenceRule.cs").read_text(encoding="utf-8")
wt_snapshot_code = (ROOT / "Core" / "Models" / "WaveTrendSnapshot.cs").read_text(encoding="utf-8")
wt_engine_code = (ROOT / "Analysis" / "Market" / "WaveTrendEngine.cs").read_text(encoding="utf-8")
wt_analyzer_code = (ROOT / "Analysis" / "Market" / "WaveTrendEvidenceAnalyzer.cs").read_text(encoding="utf-8")
frame_code = (ROOT / "Analysis" / "Market" / "Models" / "Frame.cs").read_text(encoding="utf-8")
for expected in (
    "BullCross",
    "BearCross",
    "AboveZero",
    "BelowZero",
    "Oversold",
    "Overbought",
):
    if expected not in wt_snapshot_code or expected not in wt_rule_code:
        raise SystemExit(f"WaveTrend evidence state contract missing: {expected}")
if "new WaveTrendSnapshot(" not in wt_engine_code:
    raise SystemExit("WaveTrend engine must materialize the canonical snapshot")
if "ApplyWaveTrendEvidence(" not in wt_analyzer_code or "WaveTrendQuality" not in frame_code:
    raise SystemExit("WaveTrend evidence must flow into the canonical market frame")

label_renderer = ROOT / "UI" / "Chart" / "PlanLabelRenderer.cs"
label_code = label_renderer.read_text(encoding="utf-8")
if "GetReadableLabelTextColor(" not in label_code:
    raise SystemExit("Plan labels must use the canonical semantic text-color resolver")
compact_label_start = label_code.find("private void DrawCompactPlanLabel(")
compact_label_code = label_code[compact_label_start:] if compact_label_start >= 0 else ""
if compact_label_start < 0:
    raise SystemExit("Compact plan label renderer method is missing")
if "Chart.DrawRectangle(" in compact_label_code:
    raise SystemExit("Plan labels must remain background-free")
if "PlanLinePresentationRule.ResolveColor(" not in compact_label_code:
    raise SystemExit("Plan labels must reuse the canonical semantic line color")
if "Chart.DrawText(" not in compact_label_code:
    raise SystemExit("Plan labels must own their native ChartText object")
if "CompactPlanLabelFontSize = 10.0" not in label_code:
    raise SystemExit("Plan labels must use the canonical readable font size")
if "CompactPlanLabelGapBars = 1" not in label_code:
    raise SystemExit("Plan labels must keep exactly one chart-bar left clearance")
if "HorizontalAlignment.Right" not in compact_label_code:
    raise SystemExit("Plan labels must terminate at the left-of-line anchor")

live_calc = ROOT / "Runtime" / "Calculation" / "CalculationLiveCycle.cs"
live_calc_code = live_calc.read_text(encoding="utf-8")
if "RenderParallelOpportunityCandidates(" not in live_calc_code:
    raise SystemExit("Parallel opportunity renderer must be in the canonical render cycle")

# Phase 9.4 — actionability, divergence, multi-plan registry and visual signal authority.
divergence_model = ROOT / "Core" / "Models" / "DivergenceResult.cs"
divergence_analyzer = ROOT / "Analysis" / "Market" / "DivergenceAnalyzer.cs"
actionability_model = ROOT / "Core" / "Models" / "TradeActionabilityResult.cs"
actionability_evaluator = ROOT / "Trading" / "Validation" / "TradeActionabilityEvaluator.cs"
plan_registry = ROOT / "Trading" / "Intelligence" / "TradePlanRegistry.cs"
parallel_builder = ROOT / "Analysis" / "Market" / "ParallelOpportunityBuilder.cs"
parallel_candidate_builder = (
    ROOT / "Analysis" / "Market" / "ParallelOpportunityCandidateBuilder.cs"
)
decision_model = ROOT / "Core" / "Models" / "Decision.cs"
visual_builder = ROOT / "UI" / "Chart" / "SignalVisualSnapshotBuilder.cs"
alert_calc = ROOT / "Runtime" / "Calculation" / "CalculationDecisionAlerts.cs"
plan_eligibility = ROOT / "Trading" / "Validation" / "PlanCreationEligibility.cs"

for required_path in (
    divergence_model,
    divergence_analyzer,
    actionability_model,
    actionability_evaluator,
    plan_registry,
):
    if not required_path.exists():
        raise SystemExit(f"Phase 9.4 owner is missing: {required_path}")

div_code = divergence_analyzer.read_text(encoding="utf-8")
frame_code = (ROOT / "Analysis" / "Market" / "Models" / "Frame.cs").read_text(encoding="utf-8")
market_evidence_code = (ROOT / "Analysis" / "Market" / "MarketFrameEvidence.cs").read_text(encoding="utf-8")
for token in (
    "TryFindLastTwoSwingLows(",
    "TryFindLastTwoSwingHighs(",
    "Regular",
    "Hidden",
    "GetWaveTrendSnapshot(",
    "Rsi(",
):
    if token not in div_code:
        raise SystemExit(f"Divergence engine missing canonical component: {token}")
for token in (
    "DivergenceDirection",
    "DivergenceQuality",
    "DivergenceType",
):
    if token not in frame_code or token not in market_evidence_code:
        raise SystemExit(f"Divergence evidence is not flowing through Frame: {token}")

act_code = actionability_evaluator.read_text(encoding="utf-8")
actionability_gate_code = (
    ROOT / "Trading" / "Validation" / "TradeActionabilityDecisionGate.cs"
).read_text(encoding="utf-8")
decision_code = decision_model.read_text(encoding="utf-8")
for token in (
    "EntryGeometryRule.Evaluate(",
    "MaximumEntryExtensionAtr",
    "MaximumEntryDistanceAtr",
    "Tp1MinimumRR",
    "MinimumRequiredRRForRegime(",
    "microConflict",
    "OPPOSING REGULAR DIVERGENCE",
    "RR BELOW ACTIONABLE FLOOR",
    "LATE / PRICE EXTENDED",
):
    if token not in act_code and token not in actionability_gate_code:
        raise SystemExit(f"Actionability gate missing deterministic execution condition: {token}")
for token in (
    "ActionableNow",
    "EntryDistanceAtr",
    "ActionableTp1RR",
    "DivergenceQuality",
):
    if token not in decision_code:
        raise SystemExit(f"Decision model missing Phase 9.4 field: {token}")

if "_decision.ActionableNow" in plan_eligibility.read_text(encoding="utf-8"):
    raise SystemExit("Plan creation must not be blocked by live ActionableNow state")

registry_code = plan_registry.read_text(encoding="utf-8")
parallel_code = parallel_builder.read_text(encoding="utf-8")
parallel_candidate_builder_code = parallel_candidate_builder.read_text(encoding="utf-8")
for token in ("Dictionary<string, TradeOpportunityCandidate>", "Upsert(", "Snapshot()"):
    if token not in registry_code:
        raise SystemExit(f"Multi-plan registry contract missing: {token}")
if (
    "_tradePlanRegistry.Upsert(" not in parallel_code and
    "_tradePlanRegistry.UpsertScenario(" not in parallel_code and
    "_tradePlanRegistry.UpsertScenario(" not in parallel_candidate_builder_code
):
    raise SystemExit(
        "Parallel opportunity builder must register every materialized candidate"
    )

visual_code = visual_builder.read_text(encoding="utf-8")
alert_code = alert_calc.read_text(encoding="utf-8")
if "_decision.ActionableNow" not in visual_code:
    raise SystemExit("Visual signal authority must require ActionableNow")
for token in ("!_decision.ActionableNow", "GetManagedPendingOrder() != null"):
    if token not in alert_code:
        raise SystemExit(f"Entry alert gate missing Phase 9.4 hierarchy condition: {token}")

execution_state = (ROOT / "Indicator" / "State.cs").read_text(encoding="utf-8")
execution_factory = (ROOT / "UI" / "Controls" / "ExecutionControlsFactory.cs").read_text(encoding="utf-8")
execution_sync = (ROOT / "UI" / "Controls" / "ExecutionControlsSynchronizer.cs").read_text(encoding="utf-8")
execution_rule = (ROOT / "Core" / "Math" / "ExecutionControlPresentationRule.cs").read_text(encoding="utf-8")
for token in (
    "ToggleButton _autoTradingQuickToggle",
    "ToggleButton _automaticOrdersQuickToggle",
    "_executionToggleSyncing",
):
    if token not in execution_state:
        raise SystemExit(f"Functional execution control state missing: {token}")
if "_autoTradingQuickToggle.Click +=" in execution_factory or "_automaticOrdersQuickToggle.Click +=" in execution_factory:
    raise SystemExit("Execution status controls must not own click mutations")
if "IsEnabled = false" not in execution_factory:
    raise SystemExit("Execution status controls must be non-interactive")
if "_executionToggleSyncing = true" not in execution_sync:
    raise SystemExit("Execution toggle synchronizer must guard programmatic state changes")
if "RefreshCbotExecutionStateIfDue();" not in execution_sync:
    raise SystemExit("Execution-control synchronizer must consume the canonical cBot state snapshot")
if "EffectiveAutoTradingEnabled" not in execution_sync or "EffectiveAutomaticOrdersEnabled" not in execution_sync:
    raise SystemExit("Execution-control synchronizer must consume effective cBot execution state")
if "public static bool IsInteractive => false;" not in execution_rule:
    raise SystemExit("Execution-control interaction policy must remain read-only")

# Strict type isolation: production behavior files may not hide helper types
# inside the cTrader partial host. Every helper/model type must have a file owner.
for p in files:
    text_module = p.read_text(encoding="utf-8")
    if re.search(
        r"^\s{8,}(?:public|private|protected|internal)\s+"
        r"(?:static\s+|sealed\s+|abstract\s+|readonly\s+)*"
        r"(?:class|struct|record)\s+[A-Za-z_]\w*",
        text_module,
        re.MULTILINE,
    ):
        raise SystemExit(f"Nested helper type detected: {p}")

mtf_model = ROOT / "Runtime" / "Mtf" / "MtfClosedContext.cs"
if not mtf_model.exists():
    raise SystemExit("MTF context model must have its own source file")
if len(re.findall(r"\bclass\s+MtfClosedContext\b", mtf_model.read_text(encoding="utf-8"))) != 1:
    raise SystemExit("MTF context model isolation failed")


# Core must remain platform-neutral. cTrader API contracts belong to
# analysis/trading/infrastructure/host layers, not to Core.
for p in sorted((ROOT / "Core").rglob("*.cs")):
    text_module = p.read_text(encoding="utf-8")
    if re.search(r"\bcAlgo\.API(?:\.Indicators|\.Internals)?\b", text_module):
        raise SystemExit(f"Platform dependency leaked into Core: {p}")


# Presentation boundary: only UI-owned modules may touch cTrader chart objects.
chart_api = re.compile(r"\bChart\.(?:Draw|RemoveObject|FindObject)\b")
broker_mutation = re.compile(
    r"\b(?:ExecuteMarketOrder|PlaceLimitOrder|PlaceStopOrder|"
    r"ModifyStopLossPrice|ModifyTakeProfitPrice|ClosePosition|CancelPendingOrder)\b"
)
for p in files:
    relative = str(p.relative_to(ROOT)).replace("\\", "/")
    text_module = strip_for_static_checks(p.read_text(encoding="utf-8"))
    if chart_api.search(text_module) and "/UI/" not in ("/" + relative):
        raise SystemExit(f"Chart API leaked outside UI: {relative}")
    if broker_mutation.search(text_module):
        allowed = (
            relative.startswith("Trading/Execution/") or
            relative.startswith("Trading/Pending/") or
            relative.startswith("Trading/LiveManagement/") or
            relative.startswith("Trading/Lifecycle/")
        )
        if not allowed:
            raise SystemExit(f"Broker mutation leaked outside execution boundary: {relative}")
