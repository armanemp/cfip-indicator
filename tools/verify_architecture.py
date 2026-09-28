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

parameters = len(re.findall(r"\[Parameter\s*\(", code))
if parameters != 535:
    raise SystemExit(f"Expected 535 total parameters, found {parameters}")
parameter_files = sorted(PARAMETER_ROOT.glob("*.cs"))
if len(parameter_files) != 27:
    raise SystemExit(f"Expected 27 parameter-group files, found {len(parameter_files)}")
baseline_parameter_files = [p for p in parameter_files if p.stem != "25_oss_analytics"]
baseline_parameters = sum(len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8"))) for p in baseline_parameter_files)
if baseline_parameters != 532:
    raise SystemExit(f"Expected 532 baseline parameters, found {baseline_parameters}")
extension_parameters = len(re.findall(r"\[Parameter\s*\(", (PARAMETER_ROOT / "25_oss_analytics.cs").read_text(encoding="utf-8")))
if extension_parameters != 3:
    raise SystemExit(f"Expected 3 OSS extension parameters, found {extension_parameters}")

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
    "Prediction", "Decision", "Plan", "OssIndicatorSnapshot", "MarketRegimeSnapshot", "MarketRegimeClassificationInput",
}
if {p.stem for p in model_files} != expected_models:
    raise SystemExit("Domain model file isolation failed")
for p in model_files:
    text = p.read_text(encoding="utf-8")
    if len(re.findall(r"\bclass\s+[A-Za-z_]\w*", text)) != 1:
        raise SystemExit(f"Expected one model type in {p}")
if re.search(r"\b(?:BuildExecutionIntent|ValidateExecutionIntent|ValidateActualMarketFill)\b", "\n".join(p.read_text(encoding="utf-8") for p in model_files)):
    raise SystemExit("Execution logic leaked into model files")

enum_files = sorted(ENUM_ROOT.glob("*.cs"))
if len(enum_files) != 8:
    raise SystemExit(f"Expected 8 enum files, found {len(enum_files)}")
if {p.stem for p in enum_files} != {
    "PanelCorner", "SizingMode", "TargetStage", "PendingOrderMode",
    "ExecutionMode", "DecisionPolicyMode", "ExecutionIntentKind", "LifecycleState"
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

# The historical behavioral baseline contributes 311 unique methods. Modularization
# may legitimately add new helpers, so the gate enforces a minimum baseline rather
# than a brittle exact total.
if len(unique_methods) < 311:
    raise SystemExit(
        f"Reference method parity regression: expected at least 311 unique methods, found {len(unique_methods)}"
    )

duplicate_counts = {
    name: count
    for name in set(reference_methods)
    if (count := reference_methods.count(name)) > 1
}
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

if 'stageName == "CLOSED-BAR ANALYSIS"' not in stage_isolation_code:
    raise SystemExit("Closed-bar fault stage boundary is missing")

if "ProcessLiveCalculationStages(" not in calculation_cycle_code:
    raise SystemExit("Calculate must retain management path after preparation fault")

if "if (newClosedBar &&" in calculation_cycle_code and    "RunClosedBarAnalysisStage(" in calculation_cycle_code:
    raise SystemExit("Calculate must not return early on closed-bar analysis result")

# Signal/execution synchronization gates.
PLAN_RENDER = ROOT / "UI" / "Chart" / "PlanRenderCoordinator.cs"
PLAN_LABEL_RENDER = ROOT / "UI" / "Chart" / "PlanLabelRenderCoordinator.cs"
for visual_path in (PLAN_RENDER, PLAN_LABEL_RENDER):
    visual_code = visual_path.read_text(encoding="utf-8")
    if "_plan.IsLivePosition" not in visual_code or "_plan.Stop" not in visual_code:
        raise SystemExit(f"Pre-trade plan stop must render from the plan; live stop must use broker-confirmed state: {visual_path.name}")

for broker_path in (
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketBrokerExecution.cs",
    ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveBrokerExecution.cs",
):
    broker_code = broker_path.read_text(encoding="utf-8")
    if "GetActiveBrokerStopPrice()" not in broker_code or "GetActiveBrokerTargetPrice()" not in broker_code:
        raise SystemExit(f"Live trade reporting must use broker-confirmed protection: {broker_path.name}")

for pending_path in (
    ROOT / "Trading" / "Pending" / "Placement" / "ContinuationStopPlacement.cs",
    ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPlacement.cs",
):
    pending_code = pending_path.read_text(encoding="utf-8")
    if "PendingOrder confirmedOrder = result.PendingOrder;" not in pending_code:
        raise SystemExit(f"Pending-order reporting must use broker-confirmed order levels: {pending_path.name}")
    if "confirmedOrder.TargetPrice" not in pending_code or        "confirmedOrder.StopLoss" not in pending_code or        "confirmedOrder.TakeProfit" not in pending_code:
        raise SystemExit(f"Pending-order reporting is not fully broker-confirmed: {pending_path.name}")

for entry_path in (
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketBrokerExecution.cs",
    ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveBrokerExecution.cs",
    ROOT / "Trading" / "Pending" / "Placement" / "ContinuationStopPlacement.cs",
    ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPlacement.cs",
):
    entry_code = entry_path.read_text(encoding="utf-8")
    if "CanRunAutomaticEntry()" not in entry_code:
        raise SystemExit(f"Runtime entry gate missing: {entry_path.name}")
    if "ApplyRuntimeEntryGate()" not in entry_code:
        raise SystemExit(f"Runtime entry block action missing: {entry_path.name}")

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
if "IsSupportedSinglePlanCapacity" not in CAPACITY_RULE_CODE:
    raise SystemExit("Pure execution capacity rule missing")

LIVE_RECOVERY_RULE = ROOT / "Core" / "Math" / "LivePlanRecoveryRule.cs"
LIVE_RECOVERY_RULE_CODE = LIVE_RECOVERY_RULE.read_text(encoding="utf-8")
if "ShouldClearStaleLivePlan" not in LIVE_RECOVERY_RULE_CODE:
    raise SystemExit("Pure stale live-plan recovery rule missing")

CAPACITY_GUARD = ROOT / "Trading" / "Risk" / "ExecutionCapacityGuard.cs"
CAPACITY_CODE = CAPACITY_GUARD.read_text(encoding="utf-8")
if "ExecutionCapacityRule.IsSupportedSinglePlanCapacity" not in CAPACITY_CODE:
    raise SystemExit("Single-plan execution capacity guard must delegate to the pure rule")
for execution_path in [
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPreTradeEligibility.cs",
    ROOT / "Trading" / "Execution" / "Aggressive" / "AggressivePreTradeEligibility.cs",
    ROOT / "Trading" / "Pending" / "Placement" / "SmartPendingOrderOrchestrator.cs",
]:
    execution_code = execution_path.read_text(encoding="utf-8")
    if "ValidateConfiguredPositionCapacity(" not in execution_code:
        raise SystemExit(f"Execution capacity guard missing in {execution_path.name}")

AUTO_MARKET_EXECUTION = ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketBrokerExecution.cs"
AUTO_MARKET_CODE = AUTO_MARKET_EXECUTION.read_text(encoding="utf-8")
if (
    "SetAutoTradingState" not in AUTO_MARKET_CODE or
    "protectionOk" not in AUTO_MARKET_CODE or
    '"RECOVERY"' not in AUTO_MARKET_CODE
):
    raise SystemExit("Market execution must expose broker protection recovery in auto state")

AGGRESSIVE_EXECUTION = ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveBrokerExecution.cs"
AGGRESSIVE_CODE = AGGRESSIVE_EXECUTION.read_text(encoding="utf-8")
if (
    "SetAutoTradingState" not in AGGRESSIVE_CODE or
    "protectionOk" not in AGGRESSIVE_CODE or
    '"RECOVERY"' not in AGGRESSIVE_CODE
):
    raise SystemExit("Aggressive execution must expose broker protection recovery in auto state")

AUTO_STATE = ROOT / "Trading" / "Execution" / "State" / "AutoTradingStateStore.cs"
AUTO_STATE_CODE = AUTO_STATE.read_text(encoding="utf-8")
if '"RECOVERY"' not in AUTO_STATE_CODE or "PanelWarningColor" not in AUTO_STATE_CODE:
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
    f"{len(methods)} method declarations / {len(unique_methods)} unique baseline methods (minimum 311)."
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
    "FindPreviousSelectedTargetPrice(",
    "RequiresHtfRewardForTargetStage(",
    "TryScoreTargetCandidate(",
):
    if token not in TARGET_SELECTOR_CODE:
        raise SystemExit(f"TargetSelector orchestration call missing: {token}")
for required_path in (
    ROOT / "Planning" / "TradePlan" / "TargetSelectionPolicy.cs",
    ROOT / "Planning" / "TradePlan" / "TargetCandidateEvaluator.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Target selection owner missing: {required_path}")
for declaration in (
    "private double[] BuildTargetSelectionRequiredRR(",
    "private double FindPreviousSelectedTargetPrice(",
    "private bool RequiresHtfRewardForTargetStage(",
    "private bool TryScoreTargetCandidate(",
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

# Structural-stop selection ownership.
STRUCTURAL_STOP_SELECTOR = ROOT / "Planning" / "TradePlan" / "StructuralStopCandidateSelector.cs"
STRUCTURAL_STOP_SELECTOR_CODE = STRUCTURAL_STOP_SELECTOR.read_text(encoding="utf-8")
if STRUCTURAL_STOP_SELECTOR.stat().st_size > 4096:
    raise SystemExit("StructuralStopCandidateSelector.cs must remain a selection orchestration boundary")
for token in (
    "TrySelectBestStructuralStopCandidate(",
    "MaterializeStructuralStop(",
):
    if token not in STRUCTURAL_STOP_SELECTOR_CODE:
        raise SystemExit(f"Structural-stop orchestration call missing: {token}")
for declaration in (
    "private bool TrySelectBestStructuralStopCandidate(",
    "private double MaterializeStructuralStop(",
):
    if declaration in STRUCTURAL_STOP_SELECTOR_CODE:
        raise SystemExit(f"StructuralStopCandidateSelector retains extracted responsibility: {declaration}")
for path, token in (
    (
        ROOT / "Planning" / "TradePlan" / "StructuralStopCandidateEvaluator.cs",
        "TrySelectBestStructuralStopCandidate("
    ),
    (
        ROOT / "Planning" / "TradePlan" / "StructuralStopFinalizer.cs",
        "MaterializeStructuralStop("
    ),
):
    if not path.exists() or token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Structural-stop owner missing: {path}")

# FVG lifecycle ownership.
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
for token in (
    "TrySelectBestStructuralStopCandidate(",
    "MaterializeStructuralStop(",
):
    if token not in STRUCTURAL_STOP_SELECTOR_CODE:
        raise SystemExit(f"Structural-stop selector orchestration call missing: {token}")
if "private bool TrySelectBestStructuralStopCandidate(" in STRUCTURAL_STOP_SELECTOR_CODE:
    raise SystemExit("StructuralStopCandidateSelector retains candidate evaluation")
if "private double MaterializeStructuralStop(" in STRUCTURAL_STOP_SELECTOR_CODE:
    raise SystemExit("StructuralStopCandidateSelector retains stop finalization")
for path, token in {
    ROOT / "Planning" / "TradePlan" / "StructuralStopCandidateEvaluator.cs": "TrySelectBestStructuralStopCandidate(",
    ROOT / "Planning" / "TradePlan" / "StructuralStopFinalizer.cs": "MaterializeStructuralStop(",
}.items():
    if not path.exists() or token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Structural-stop owner missing: {path}")

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

# Cross-path automatic execution consistency.
CROSS_PATH_CONTRACTS = {
    "AutomaticMarketBrokerExecution.cs": (
        "TryValidateAutomaticMarketSubmission(",
        "TryExecuteMarketOrder(",
        "BrokerConfirmationPolicy.CanAdoptPosition(",
        "TryAcceptAutomaticMarketFill(",
        "EnsureBrokerProtectionForPosition(",
    ),
    "AggressiveBrokerExecution.cs": (
        "BuildExecutionIntent(",
        "ValidateExecutionIntent(",
        "TryExecuteMarketOrder(",
        "BrokerConfirmationPolicy.CanAdoptPosition(",
        "TryProcessAcceptedAggressiveFill(",
        "EnsureBrokerProtectionForPosition(",
    ),
    "ContinuationStopPlacement.cs": (
        "TryPrepareContinuationStop(",
        "ValidatePendingSubmission(",
        "TryPlaceStopOrder(",
        "BrokerConfirmationPolicy.CanAdoptPendingOrder(",
    ),
    "ReversalLimitPlacement.cs": (
        "TryPrepareReversalLimit(",
        "ValidatePendingSubmission(",
        "TryPlaceLimitOrder(",
        "BrokerConfirmationPolicy.CanAdoptPendingOrder(",
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
    "TryPrepareContinuationStop(",
    "ValidatePendingSubmission(",
    "TryPlaceStopOrder(",
    "BrokerConfirmationPolicy.CanAdoptPendingOrder(",
):
    if token not in PENDING_STOP_CODE:
        raise SystemExit(f"Continuation stop ownership call missing: {token}")

PENDING_STOP_PREP = ROOT / "Trading" / "Pending" / "Placement" / "ContinuationStopPreparation.cs"
if not PENDING_STOP_PREP.exists() or "BuildStructuralStop(" not in PENDING_STOP_PREP.read_text(encoding="utf-8"):
    raise SystemExit("Continuation stop preparation owner missing")

PENDING_LIMIT = ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPlacement.cs"
PENDING_LIMIT_CODE = PENDING_LIMIT.read_text(encoding="utf-8")
if PENDING_LIMIT.stat().st_size > 4096:
    raise SystemExit("ReversalLimitPlacement.cs must remain a placement orchestration boundary")
for token in (
    "TryPrepareReversalLimit(",
    "ValidatePendingSubmission(",
    "TryPlaceLimitOrder(",
    "BrokerConfirmationPolicy.CanAdoptPendingOrder(",
):
    if token not in PENDING_LIMIT_CODE:
        raise SystemExit(f"Reversal limit ownership call missing: {token}")

PENDING_LIMIT_PREP = ROOT / "Trading" / "Pending" / "Placement" / "ReversalLimitPreparation.cs"
if not PENDING_LIMIT_PREP.exists() or "BuildExecutionModel(" not in PENDING_LIMIT_PREP.read_text(encoding="utf-8"):
    raise SystemExit("Reversal limit preparation owner missing")

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
        "ValidateConfiguredPositionCapacity("
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

AGGRESSIVE_RUNTIME = ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveBrokerExecution.cs"
AGGRESSIVE_RUNTIME_CODE = AGGRESSIVE_RUNTIME.read_text(encoding="utf-8")
if AGGRESSIVE_RUNTIME.stat().st_size > 8192:
    raise SystemExit("AggressiveBrokerExecution.cs must remain an execution orchestration boundary")
for token in (
    "EnsureTradingPermission(",
    "PassesAutoTradeSafetyGuards(",
    "BuildExecutionIntent(",
    "TryExecuteMarketOrder(",
    "BrokerConfirmationPolicy.CanAdoptPosition(",
    "TryProcessAcceptedAggressiveFill(",
    "EnsureBrokerProtectionForPosition(",
    '"RECOVERY"',
):
    if token not in AGGRESSIVE_RUNTIME_CODE:
        raise SystemExit(f"Aggressive execution boundary missing: {token}")
# Automatic-market execution boundary.
AUTO_MARKET_PRETRADE = ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPreTrade.cs"
AUTO_MARKET_PRETRADE_CODE = AUTO_MARKET_PRETRADE.read_text(encoding="utf-8")
for token in (
    "PassAutomaticMarketPreTradeEligibility(",
    "TryPrepareAutomaticMarketExecution(",
):
    if token not in AUTO_MARKET_PRETRADE_CODE:
        raise SystemExit(f"Automatic-market orchestration call missing: {token}")
if AUTO_MARKET_PRETRADE.stat().st_size > 4096:
    raise SystemExit("AutomaticMarketPreTrade.cs must remain an orchestration boundary")

AUTO_MARKET_ELIGIBILITY = ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPreTradeEligibility.cs"
AUTO_MARKET_PREPARATION = ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketExecutionPreparation.cs"
for path, token in (
    (AUTO_MARKET_ELIGIBILITY, "ValidateConfiguredPositionCapacity("),
    (AUTO_MARKET_PREPARATION, "TryPrepareExecutablePlan("),
):
    if not path.exists() or token not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"Automatic-market owner missing or incomplete: {path}")

AUTO_MARKET_RUNTIME = ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketBrokerExecution.cs"
AUTO_MARKET_RUNTIME_CODE = AUTO_MARKET_RUNTIME.read_text(encoding="utf-8")
if AUTO_MARKET_RUNTIME.stat().st_size > 8192:
    raise SystemExit("AutomaticMarketBrokerExecution.cs must remain an execution orchestration boundary")
for token in (
    "TryValidateAutomaticMarketSubmission(",
    "TryExecuteMarketOrder(",
    "BrokerConfirmationPolicy.CanAdoptPosition(",
    "TryAcceptAutomaticMarketFill(",
    "TryResolveAutomaticPostFillTarget(",
    "EnsureBrokerProtectionForPosition(",
):
    if token not in AUTO_MARKET_RUNTIME_CODE:
        raise SystemExit(f"Automatic-market execution boundary missing: {token}")
for required_path in (
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketSubmissionValidator.cs",
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketFillReconciliation.cs",
    ROOT / "Trading" / "Execution" / "AutomaticMarket" / "AutomaticMarketPostFillTargetResolver.cs",
):
    if not required_path.exists():
        raise SystemExit(f"Automatic-market execution owner missing: {required_path}")

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
    "private double FindImprovedLiveTarget(",
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
    ROOT / "Trading" / "Validation" / "RewardPathGeometryRule.cs",
    ROOT / "Trading" / "Validation" / "TargetObstacleValidator.cs",
    ROOT / "Trading" / "Validation" / "HigherTfRewardPathValidator.cs",
    ROOT / "Trading" / "Validation" / "HtfTargetPresenceValidator.cs",
)
for required_path in REWARD_PATH_FILES:
    if not required_path.exists():
        raise SystemExit(f"Reward-path owner missing: {required_path}")
if (ROOT / "Trading" / "Validation" / "RewardPathValidation.cs").exists():
    raise SystemExit("Obsolete RewardPathValidation.cs must not return")
for path, declarations in {
    REWARD_PATH_FILES[0]: ("private bool HasOpposingZonePathObstacle(",),
    REWARD_PATH_FILES[1]: ("private bool ZoneBlocksRewardPath(",),
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

AGGRESSIVE_EXECUTION = ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveBrokerExecution.cs"
AGGRESSIVE_CODE = AGGRESSIVE_EXECUTION.read_text(encoding="utf-8")
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

AGGRESSIVE_FILL_HANDLER = ROOT / "Trading" / "Execution" / "Aggressive" / "AggressiveAcceptedFillHandler.cs"
AGGRESSIVE_FILL_CODE = AGGRESSIVE_FILL_HANDLER.read_text(encoding="utf-8")
if "FILL MISMATCH" not in AGGRESSIVE_FILL_CODE:
    raise SystemExit("Aggressive accepted-fill handler must explicitly handle fill-envelope mismatch")

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
if "IsValidManagedStop(" not in BROKER_PROTECTION_STATE_CODE:
    raise SystemExit("Shared broker protection state must validate SL against current market")
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
MUTATION_ALLOWED_ROOT = ROOT / "Trading" / "Execution"
MUTATION_ALLOWED_FILES = {
    "BrokerMarketOrderMutation.cs",
    "BrokerPendingOrderPlacement.cs",
    "BrokerLimitOrderPlacement.cs",
    "BrokerPendingOrderCancellation.cs",
    "BrokerStopLossMutation.cs",
    "BrokerTakeProfitMutation.cs",
    "BrokerPositionCloseMutation.cs",
}
REQUIRED_BROKER_MUTATION_FILES = {
    "BrokerMarketOrderMutation.cs",
    "BrokerPendingOrderPlacement.cs",
    "BrokerLimitOrderPlacement.cs",
    "BrokerPendingOrderCancellation.cs",
    "BrokerStopLossMutation.cs",
    "BrokerTakeProfitMutation.cs",
    "BrokerPositionCloseMutation.cs",
    "BrokerProtectionCoordinator.cs",
    "BrokerConfirmationPolicy.cs",
}
for filename in REQUIRED_BROKER_MUTATION_FILES:
    path = MUTATION_ALLOWED_ROOT / filename
    if not path.exists():
        raise SystemExit(f"Broker mutation owner missing: {filename}")

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
    if not direct:
        continue
    if p.parent != MUTATION_ALLOWED_ROOT or p.name not in MUTATION_ALLOWED_FILES:
        raise SystemExit(
            f"Broker mutation escaped boundary: {relative}"
        )

EXECUTION_PLAN = ROOT / "Trading" / "Execution" / "ExecutionPlanPreparation.cs"
if any(re.search(pattern, strip_for_static_checks(EXECUTION_PLAN.read_text(encoding="utf-8")))
       for pattern in BROKER_MUTATION_PATTERNS):
    raise SystemExit("Broker mutation leaked into execution plan preparation")

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
    "Rows/PanelAutoTradingRowsRenderer.cs": "RenderPanelAutoTradingRows",
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
