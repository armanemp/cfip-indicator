from pathlib import Path
import re

ROOT = Path("src/CFIP.Indicator")
PARAMETER_ROOT = ROOT / "Indicator" / "Parameters"
MODEL_ROOT = ROOT / "Core" / "Models"
ENUM_ROOT = ROOT / "Core" / "Enums"

LEGACY_FILES = {
    "Analysis/Indicators.cs", "Analysis/Market.cs", "Analysis/Reaction.cs", "Analysis/Structure.cs",
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
if parameters != 516:
    raise SystemExit(f"Expected 516 total parameters, found {parameters}")
parameter_files = sorted(PARAMETER_ROOT.glob("*.cs"))
if len(parameter_files) != 27:
    raise SystemExit(f"Expected 27 parameter-group files, found {len(parameter_files)}")
baseline_parameter_files = [p for p in parameter_files if p.stem != "25_oss_analytics"]
baseline_parameters = sum(len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8"))) for p in baseline_parameter_files)
if baseline_parameters != 513:
    raise SystemExit(f"Expected 513 baseline parameters, found {baseline_parameters}")
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
    "Prediction", "Decision", "Plan", "OssIndicatorSnapshot",
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
    "FacioQuoRsi",
    "FacioQuoMacdHistogram",
    "FacioQuoBollingerPercentB",
    "FacioQuoMfi",
    "FacioQuoStochBias",
    "FacioQuoSuperTrend",
    "BuildOssIndicatorSnapshot",
    "FacioQuoAroonOscillator",
    "FacioQuoCci",
    "FacioQuoObvBias",
    "FacioQuoParabolicSar",
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
if set(duplicate_counts) - {"AddScore"}:
    raise SystemExit(
        "Unexpected duplicate/overloaded method names: "
        + ", ".join(sorted(set(duplicate_counts) - {"AddScore"}))
    )
if reference_methods.count("AddScore") != 2:
    raise SystemExit("Expected exactly one overloaded method pair: AddScore")

VERSION_RESIDUE_PATTERNS = (
    re.compile(r"\bv\d+\b", re.I),
    re.compile(r"\b(?:rev|release)[-_ ]?\d+\b", re.I),
    re.compile(r"\bClean\d+\b", re.I),
    re.compile(r"\bCFIPClean\d+\b", re.I),
    re.compile(r"CFIP_MTF_LiveEntryEngine_Clean", re.I),
    re.compile(r"\bCFIP[\s_-]*(?:SMART|AUTO)[\s_-]*\d+\b", re.I),
)
for production_file in files:
    source_text = production_file.read_text(encoding="utf-8")
    for residue_pattern in VERSION_RESIDUE_PATTERNS:
        if residue_pattern.search(source_text):
            raise SystemExit(f"Version/historical residue detected in production source: {production_file}")

if re.search(r"\b(?:Buy|Sell)\b.{0,100}\b(?:Button|ToggleButton)\b", code, re.I):
    raise SystemExit("Manual trade-entry controls detected")

oversized = [
    str(p.relative_to(ROOT))
    for p in files
    if p.stat().st_size > 65536
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
    "FacioQuoRsi.cs": "FacioQuoRsi",
    "FacioQuoMacd.cs": "FacioQuoMacdHistogram",
    "FacioQuoBollingerBands.cs": "FacioQuoBollingerPercentB",
    "FacioQuoMfi.cs": "FacioQuoMfi",
    "FacioQuoStoch.cs": "FacioQuoStochBias",
    "FacioQuoSuperTrend.cs": "FacioQuoSuperTrend",
    "OssIndicatorConfluenceAnalyzer.cs": "BuildOssIndicatorSnapshot",
    "FacioQuoAroon.cs": "FacioQuoAroonOscillator",
    "FacioQuoCci.cs": "FacioQuoCci",
    "FacioQuoObv.cs": "FacioQuoObvBias",
    "FacioQuoParabolicSar.cs": "FacioQuoParabolicSar",
}
OSS_ROOT = ROOT / "Analysis" / "Indicators" / "External"
for filename, method in oss_files.items():
    path = OSS_ROOT / filename
    if not path.exists() or method not in path.read_text(encoding="utf-8"):
        raise SystemExit(f"OSS indicator module missing: {filename}")


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
