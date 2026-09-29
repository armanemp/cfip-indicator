#!/usr/bin/env python3
"""Audit Phase 9.16 location hierarchy and signal-measurement invariants."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]

LOCATION = ROOT / "src/CFIP.Indicator/Core/Math/LocationEvidenceRule.cs"
FRAME = ROOT / "src/CFIP.Indicator/Analysis/Market/Models/Frame.cs"
SCORING = ROOT / "src/CFIP.Indicator/Analysis/Market/MarketFrameScoringService.cs"
ORCHESTRATION = ROOT / "src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs"
TRACE_MODEL = ROOT / "src/CFIP.Indicator/Core/Models/SignalEvaluationTrace.cs"
TRACE_STORE = ROOT / "src/CFIP.Indicator/Trading/Intelligence/SignalEvaluationTraceArchiveStore.cs"
PANEL = ROOT / "src/CFIP.Indicator/UI/Panel/Rows/PanelCalibrationRowsRenderer.cs"
ANALYZER = ROOT / "tools/analyze_signal_trace.py"

errors = []


def require(path: Path, pattern: str, label: str) -> None:
    if not path.exists():
        errors.append(f"missing file for {label}")
        return

    text = path.read_text(encoding="utf-8")
    if not re.search(pattern, text, re.MULTILINE | re.DOTALL):
        errors.append(f"missing {label}")


require(
    LOCATION,
    r"LocationEvidenceScore[sS]*?public static LocationEvidenceScore Evaluate",
    "canonical location evidence owner",
)
require(
    LOCATION,
    r"if (confluence)[sS]*?minimumQuality[sS]*?18[sS]*?16[sS]*?12[sS]*?8",
    "bounded OB+FVG synergy tiers",
)
require(
    LOCATION,
    r"return new LocationEvidenceScore(s*score,s*1,s*confluence)",
    "one evidence unit for correlated location group",
)
require(
    SCORING,
    r"LocationEvidenceRule.Evaluate([sS]*?f.FvgObBullConfluence[sS]*?LocationEvidenceRule.Evaluate([sS]*?f.FvgObBearConfluence",
    "BUY/SELL canonical location scoring",
)
require(
    FRAME,
    r"LocationEvidenceBull[sS]*?LocationEvidenceBear[sS]*?LocationEvidenceBullCount[sS]*?LocationEvidenceBearCount",
    "location telemetry fields",
)
require(
    ORCHESTRATION,
    r"RecordSignalEvaluationTrace(s*decision[sS]*?decisionLane[sS]*?closedM5",
    "canonical decision trace capture",
)
require(
    TRACE_MODEL,
    r"Open[sS]*?High[sS]*?Low[sS]*?Close[sS]*?ActionabilityReason[sS]*?DecisionReason",
    "OHLC and gate-reason trace fields",
)
require(
    TRACE_STORE,
    r"MaxSignalEvaluationTraceHistorys*=s*256",
    "bounded in-memory signal trace",
)
require(
    TRACE_STORE,
    r"OutcomeArchivePeriodStart([sS]*?start.AddDays(90)",
    "same 90-day archive bucket contract",
)
require(
    TRACE_STORE,
    r'File.AppendAllText(',
    "append-only signal trace persistence",
)
require(
    TRACE_STORE,
    r"EnableOutcomeTelemetry",
    "trace I/O safety switch",
)
require(
    TRACE_STORE,
    r"BarOpenTimeUtcTicks",
    "idempotent closed-bar trace key",
)
require(
    TRACE_STORE,
    r"TraceGate[sS]*?DECISION-FILTER[sS]*?TRIGGER[sS]*?ACTIONABILITY[sS]*?ACTIONABLE",
    "canonical gate attribution order",
)
require(
    PANEL,
    r"SignalTracePanelText()",
    "panel trace diagnostics",
)
require(
    ANALYZER,
    r"argparse[sS]*?ActionableNow[sS]*?max_favorable_r[sS]*?max_adverse_r",
    "offline forward-measurement analyzer",
)
require(
    ANALYZER,
    r"future_rows[sS]*?future_rows",
    "strict forward-window measurement",
)

trace_text = TRACE_STORE.read_text(encoding="utf-8")
if "File.Delete" in trace_text or "Directory.Delete" in trace_text:
    errors.append("signal trace archive must not delete historical files")

if "EvaluateTradeActionability" in trace_text:
    errors.append("signal trace store must not become a second actionability owner")

if errors:
    print("Phase 9.16 signal measurement audit FAILED")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("Phase 9.16 signal measurement audit OK")
