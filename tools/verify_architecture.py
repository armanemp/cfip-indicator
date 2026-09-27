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
if parameters != 513:
    raise SystemExit(f"Expected 513 parameters, found {parameters}")

parameter_files = sorted(PARAMETER_ROOT.glob("*.cs"))
if len(parameter_files) != 26:
    raise SystemExit(f"Expected 26 parameter-group files, found {len(parameter_files)}")
for p in parameter_files:
    groups = set(re.findall(r'\bGroup\s*=\s*"([^"]+)"', p.read_text(encoding="utf-8")))
    if len(groups) != 1:
        raise SystemExit(f"Parameter group isolation failed: {p}")

label = re.search(
    r'\[Parameter\("Auto Trade Label"[^\n]*DefaultValue\s*=\s*"([^"]+)"',
    "\n".join(p.read_text(encoding="utf-8") for p in parameter_files),
)
if not label or label.group(1) != "CFIP-SMART-CLEAN66":
    raise SystemExit("Managed broker identity label parity check failed")

model_files = sorted(MODEL_ROOT.glob("*.cs"))
expected_models = {
    "Frame", "Level", "Zone", "ExecutionIntent", "ExecutionModel",
    "Prediction", "Decision", "Plan", "Native",
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
}
reference_methods = [m for m in methods if m not in MODULAR_HELPERS]
unique_methods = set(reference_methods)
if len(reference_methods) != 312:
    raise SystemExit(f"Expected 312 reference method declarations, found {len(reference_methods)}")
if len(unique_methods) != 311:
    raise SystemExit(f"Expected 311 unique reference methods, found {len(unique_methods)}")
if methods.count("AddScore") != 2:
    raise SystemExit("Expected exactly one overloaded method pair: AddScore")

if re.search(r"CFIPClean\d+|Clean\d+|CFIP_MTF_LiveEntryEngine_Clean", code, re.I):
    raise SystemExit("Legacy/versioned strategy identifier detected")

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
    f"{len(methods)} method declarations / {len(unique_methods)} unique methods."
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
