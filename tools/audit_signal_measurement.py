#!/usr/bin/env python3
"""Audit Phase 9.16 location hierarchy, signal measurement and final execution safety."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]


def read(path: Path) -> str:
    if not path.exists():
        errors.append(f"missing file: {path}")
        return ""
    return path.read_text(encoding="utf-8")


def require_text(path: Path, *tokens: str) -> None:
    source = read(path)
    for token in tokens:
        if token not in source:
            errors.append(f"{path.name} missing: {token}")


errors = []

location_path = ROOT / "src/CFIP.Indicator/Core/Math/LocationEvidenceRule.cs"
scoring_path = ROOT / "src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs"
frame_path = ROOT / "src/CFIP.Indicator/Analysis/Market/Models/Frame.cs"
orchestration_path = ROOT / "src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs"
trace_model_path = ROOT / "src/CFIP.Indicator/Core/Models/SignalEvaluationTrace.cs"
trace_store_path = ROOT / "src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchiveStore.cs"
panel_path = ROOT / "src/CFIP.Indicator/UI/Panel/Rows/PanelCalibrationRowsRenderer.cs"
analyzer_path = ROOT / "tools/analyze_signal_trace.py"
market_exec_path = ROOT / "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketPreTradeEligibility.cs"
geometry_path = ROOT / "src/CFIP.Indicator/Core/Math/ExecutionPlanGeometryRule.cs"
contracts_path = ROOT / "tools/CFIP.Decision.Contracts/Program.cs"
contracts_project = ROOT / "tools/CFIP.Decision.Contracts/CFIP.Decision.Contracts.csproj"

require_text(
    location_path,
    "public static LocationEvidenceScore Evaluate",
    "18",
    "16",
    "12",
    "8",
    "return new LocationEvidenceScore(",
)
require_text(
    scoring_path,
    "LocationEvidenceRule.Evaluate(",
    "f.FvgObBullConfluence",
    "f.FvgObBearConfluence",
    "f.LocationEvidenceBull",
    "f.LocationEvidenceBear",
)
require_text(
    frame_path,
    "LocationEvidenceBull",
    "LocationEvidenceBear",
    "LocationEvidenceBullCount",
    "LocationEvidenceBearCount",
)
require_text(
    orchestration_path,
    "ResolveSignalTraceLane(",
    "RecordSignalEvaluationTrace(",
)
require_text(
    trace_model_path,
    "Open",
    "High",
    "Low",
    "Close",
    "ActionabilityReason",
    "DecisionReason",
)
require_text(
    trace_store_path,
    "MaxSignalEvaluationTraceHistory = 256",
)
require_text(
    trace_persistence_path,
    "SignalTraceSchema =",
    "OutcomeArchivePeriodStart(",
    "start.AddDays(90)",
    "File.AppendAllText(",
    "ArchiveSignalTrace(trace);",
    "BarOpenTimeUtcTicks",
    "DECISION-FILTER",
    "TRIGGER",
    "ACTIONABILITY",
    "ACTIONABLE",
)
require_text(
    panel_path,
    "SignalTracePanelText()",
)
portable_path = ROOT / "src/CFIP.Indicator/Trading/Intelligence/PortableMemorySnapshotStore.cs"

require_text(
    portable_path,
    "CFIP-PORTABLE-MEMORY,1",
    "TryRestorePortableMemorySnapshot",
    "PersistPortableMemorySnapshot",
    "LocalStorageScope.Type",
    "OutcomePayloadBase64",
)

require_text(
    analyzer_path,
    "argparse",
    "gate == \"ACTIONABLE\"",
    "max_favorable",
    "max_adverse",
    "future_rows",
)
require_text(
    geometry_path,
    "ExecutionPlanGeometryResult",
    "STOP WRONG SIDE",
    "TP1 WRONG SIDE",
    "RR BELOW EXECUTION FLOOR",
)
require_text(
    market_exec_path,
    "ExecutionPlanGeometryRule.Evaluate(",
    "MinimumRequiredRRForRegime(",
    "PLAN GEOMETRY",
)
require_text(
    contracts_path,
    "VerifyLocationEvidenceHierarchy();",
    "VerifyExecutionPlanGeometry();",
    "ExecutionPlanGeometryRule.Evaluate(",
)
require_text(
    contracts_project,
    "LocationEvidenceRule.cs",
    "ExecutionPlanGeometryRule.cs",
)

trace_text = read(trace_store_path) + read(trace_persistence_path)
if "File.Delete" in trace_text or "Directory.Delete" in trace_text:
    errors.append("signal trace archive must not delete historical files")

if "EvaluateTradeActionability" in trace_text:
    errors.append("signal trace store must not become a second actionability owner")

analyzer_text = read(analyzer_path)
if "TODO" in analyzer_text or "pass" in analyzer_text.lower():
    errors.append("offline analyzer contains incomplete implementation markers")

if errors:
    print("Phase 9.16 signal measurement audit FAILED")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("Phase 9.16 signal measurement audit OK")
