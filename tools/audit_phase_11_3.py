#!/usr/bin/env python3
"""Static/source contract audit for Phase 11.3."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
ANALYZER = ROOT / "tools/analyze_phase_11_3.py"
PHASE_DOC = ROOT / "docs/PHASE-11-3-EXECUTION-FORENSICS-THRESHOLD-EVIDENCE.md"
WORKFLOW = ROOT / ".github/workflows/source-check.yml"

errors: list[str] = []

def read(path: Path) -> str:
    if not path.exists():
        errors.append(f"missing file: {path}")
        return ""
    return path.read_text(encoding="utf-8")

analyzer = read(ANALYZER)
doc = read(PHASE_DOC)
workflow = read(WORKFLOW)

required_analyzer_tokens = [
    "CFIP_SignalTrace_*.csv",
    "CFIP_RuntimeLog_v2_*.csv",
    "potential_missed_count",
    "adverse_actionable_count",
    "near_threshold_rows",
    "trace_gate_to_rejection_category",
    "classify_rejection",
    "OB+FVG",
    "WaveTrend",
    "IndicatorConfluenceQuality",
    "--thresholds-json",
    "automatic_threshold_changes",
    "live_mutation",
]

for token in required_analyzer_tokens:
    if token not in analyzer:
        errors.append(f"analyzer missing: {token}")

for token in (
    "Phase 11.3",
    "No automatic threshold change",
    "OB+FVG",
    "WaveTrend",
    "missed-actionable",
    "execution rejection",
    "target-terminal",
    "Swing",
):
    if token.lower() not in doc.lower():
        errors.append(f"phase document missing: {token}")

if "python tools/audit_phase_11_3.py" not in workflow:
    errors.append("source-check workflow does not run Phase 11.3 audit")

if re.search(r"from cAlgo|using cAlgo|ExecuteMarketOrder|PlaceLimitOrder|PlaceStopOrder", analyzer):
    errors.append("offline analyzer must not contain live trading APIs")

if re.search(r"subprocess|os\.system|powershell|dotnet run", analyzer, re.IGNORECASE):
    errors.append("offline analyzer must not invoke production/runtime mutation commands")

if errors:
    print("Phase 11.3 audit FAILED")
    for error in errors:
        print(" - " + error)
    raise SystemExit(1)

print("Phase 11.3 audit OK")
