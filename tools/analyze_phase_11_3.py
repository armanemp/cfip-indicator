#!/usr/bin/env python3
"""Phase 11.3 forensic analyzer for CFIP signal/execution history.

This tool is observational only. It never changes indicator parameters and never
feeds its measurements back into live trading. It correlates the append-only
signal traces and runtime logs already produced by CFIP.

Usage:
  python tools/analyze_phase_11_3.py --history-dir <cTrader-History>
"""

from __future__ import annotations

import argparse
import base64
import csv
import json
import math
import re
from collections import Counter
from pathlib import Path
from typing import Any


THRESHOLDS = {
    "MinimumConfidence": 72,
    "MinimumEdge": 15,
    "MinimumSmartQuality": 70,
    "MinimumStructuralConfirmations": 4,
    "MinimumIndependentEvidence": 4,
    "MinimumTimeframeAgreement": 72,
    "SmartMinimumTimeframeAgreement": 72,
    "SmartQualityThreshold": 70,
    "MinimumEntryLocationQuality": 64,
    "MinimumEntryQuality": 72,
    "MaximumEntryDistanceAtr": 0.45,
    "Tp1MinimumRR": 2.0,
    "MinimumStructuralStopQuality": 65,
    "MinimumFreshTriggerEvidence": 3,
}

THRESHOLD_TOLERANCES = {
    "MinimumConfidence": 5,
    "MinimumEdge": 3,
    "MinimumSmartQuality": 5,
    "MinimumStructuralConfirmations": 1,
    "MinimumIndependentEvidence": 1,
    "MinimumTimeframeAgreement": 5,
    "SmartMinimumTimeframeAgreement": 5,
    "SmartQualityThreshold": 5,
    "MinimumEntryLocationQuality": 5,
    "MinimumEntryQuality": 5,
    "MaximumEntryDistanceAtr": 0.08,
    "Tp1MinimumRR": 0.35,
    "MinimumStructuralStopQuality": 5,
    "MinimumFreshTriggerEvidence": 1,
}

REJECT_STATES = {
    "REJECTED",
    "FAILED",
    "NULL RESULT",
    "UNCONFIRMED",
}

CATEGORY_PATTERNS = (
    ("NEWS", ("NEWS", "ECONOMIC", "BLACKOUT", "HIGH IMPACT", "MEDIUM IMPACT")),
    ("PERMISSION", ("PERMISSION", "TRADING PERMISSION")),
    ("CAPACITY", ("CAPACITY", "MAX OPEN", "PENDING ORDER EXISTS", "ALREADY TRADED")),
    ("DUPLICATE", ("DUPLICATE", "ONE ORDER PER SIGNAL", "SUBMISSION GATE")),
    ("SUITABILITY", ("SUITABILITY", "SESSION", "FRIDAY CUTOFF", "MARKET CLOSED", "SYMBOL TRADING DISABLED")),
    ("SPREAD_RISK", ("SPREAD", "STOP RISK", "DAILY LOSS", "MARGIN", "RISK")),
    ("GEOMETRY", ("GEOMETRY", "STOP", "TP1", "TARGET", "ENTRY DISTANCE", "WRONG SIDE", "RR ")),
    ("VOLUME", ("VOLUME", "LOT")),
    ("RUNTIME", ("RUNTIME", "INITIALIZATION", "RECOVERY", "FAULT")),
    ("BROKER_REJECT", ("BROKER REJECT", "INVALID", "OFF QUOTES", "MARKET RANGE", "TRADE"))
)


def safe_b64decode(value: str) -> str:
    if not value:
        return ""
    text = value.strip()
    if len(text) % 4 != 0 or not re.fullmatch(r"[A-Za-z0-9+/=]+", text):
        return value
    try:
        decoded = base64.b64decode(text, validate=True).decode("utf-8")
    except Exception:
        return value
    if not decoded:
        return value
    if any(ord(ch) < 9 for ch in decoded):
        return value
    return decoded


def as_int(row: dict[str, str], key: str, default: int = 0) -> int:
    try:
        return int(float(row.get(key, default)))
    except (TypeError, ValueError):
        return default


def as_float(row: dict[str, str], key: str, default: float = 0.0) -> float:
    try:
        value = float(row.get(key, default))
        return value if math.isfinite(value) else default
    except (TypeError, ValueError):
        return default


def direction_value(row: dict[str, str]) -> int:
    value = as_int(row, "Direction")
    return value if value in (-1, 1) else 0


def is_positive(value: float) -> bool:
    return value > 0.0 and math.isfinite(value)


def config_key(path: Path) -> str:
    name = path.name
    name = re.sub(r"^CFIP_RuntimeLog_v2_", "", name)
    name = re.sub(r"^CFIP_SignalTrace_", "", name)
    name = re.sub(r"_\d{8}_\d{8}\.csv$", "", name)
    return name


def load_csv_files(history_dir: Path, glob: str) -> list[dict[str, str]]:
    rows: list[dict[str, str]] = []
    for path in sorted(history_dir.glob(glob)):
        try:
            with path.open("r", encoding="utf-8-sig", newline="") as handle:
                schema = handle.readline().strip()
                if glob.startswith("CFIP_SignalTrace") and schema != "CFIP-SIGNAL-TRACE,1":
                    continue
                if glob.startswith("CFIP_RuntimeLog") and schema not in {"CFIP-RUNTIME-LOG,1", "CFIP-RUNTIME-LOG,2"}:
                    continue
                for row in csv.DictReader(handle):
                    row["_file"] = str(path)
                    row["_config"] = config_key(path)
                    for field in ("TraceGate", "BlockReason", "ActionabilityReason", "DecisionReason", "TopDownStage"):
                        if field in row:
                            row["_" + field] = safe_b64decode(row.get(field, ""))
                    rows.append(row)
        except (OSError, csv.Error, UnicodeError):
            continue
    return rows


def forward_metrics(rows: list[dict[str, str]], index: int, forward_bars: int) -> tuple[float, float, str]:
    row = rows[index]
    direction = direction_value(row)
    entry = as_float(row, "Entry")
    stop = as_float(row, "Stop")
    risk = abs(entry - stop)
    if direction == 0 or not is_positive(entry) or not is_positive(stop) or not is_positive(risk):
        return 0.0, 0.0, "INVALID"

    current_m5 = as_int(row, "ClosedM5")
    future: list[dict[str, str]] = []
    for candidate in rows[index + 1 :]:
        if as_int(candidate, "ClosedM5") <= current_m5:
            continue
        if as_int(candidate, "ClosedM5") > current_m5 + forward_bars:
            break
        future.append(candidate)

    tp1 = as_float(row, "Tp1")
    has_tp1 = is_positive(tp1)
    mfe = 0.0
    mae = 0.0
    first_event = "NONE"

    for candidate in future:
        high = as_float(candidate, "High")
        low = as_float(candidate, "Low")
        if direction == 1:
            favorable = (high - entry) / risk
            adverse = (entry - low) / risk
            hit_tp = has_tp1 and high >= tp1
            hit_stop = low <= stop
        else:
            favorable = (entry - low) / risk
            adverse = (high - entry) / risk
            hit_tp = has_tp1 and low <= tp1
            hit_stop = high >= stop

        mfe = max(mfe, favorable)
        mae = max(mae, adverse)

        if first_event != "NONE":
            continue
        if hit_tp and hit_stop:
            first_event = "AMBIGUOUS"
        elif hit_tp:
            first_event = "TP1"
        elif hit_stop:
            first_event = "STOP"

    return mfe, mae, first_event


def near_threshold_cohort(row: dict[str, str]) -> list[str]:
    checks: list[tuple[str, float]] = [
        ("MinimumConfidence", as_float(row, "Confidence")),
        ("MinimumEdge", as_float(row, "Edge")),
        ("MinimumSmartQuality", as_float(row, "SmartQuality")),
        ("MinimumStructuralConfirmations", as_float(row, "StructuralConfirmations")),
        ("MinimumIndependentEvidence", as_float(row, "IndependentEvidence")),
        ("MinimumTimeframeAgreement", as_float(row, "HtfAlignment")),
        ("SmartMinimumTimeframeAgreement", as_float(row, "HtfAlignment")),
        ("SmartQualityThreshold", as_float(row, "SmartQuality")),
        ("MinimumEntryLocationQuality", as_float(row, "EntryLocationQuality")),
        ("MinimumEntryQuality", as_float(row, "EntryPositionQuality")),
        ("MaximumEntryDistanceAtr", as_float(row, "EntryDistanceAtr")),
        ("Tp1MinimumRR", as_float(row, "ActionableTp1RR")),
    ]
    result: list[str] = []
    for name, value in checks:
        threshold = THRESHOLDS[name]
        tolerance = THRESHOLD_TOLERANCES[name]
        if abs(value - threshold) <= tolerance:
            result.append(name)
    return result


def row_cohort_key(row: dict[str, str]) -> tuple[str, str, str, str, str]:
    gate = row.get("_TraceGate", "") or row.get("TraceGate", "") or "UNKNOWN"
    reason = (
        row.get("_ActionabilityReason", "")
        or row.get("_BlockReason", "")
        or row.get("_DecisionReason", "")
        or "UNKNOWN"
    )
    lane = row.get("Lane", "0") or "0"
    direction = "BUY" if direction_value(row) == 1 else "SELL"
    regime = row.get("Regime", "") or "UNKNOWN"
    return gate, reason, lane, direction, regime


def analyze_traces(rows: list[dict[str, str]], forward_bars: int, min_mfe_r: float) -> dict[str, Any]:
    grouped: dict[str, list[dict[str, str]]] = {}
    for row in rows:
        grouped.setdefault(row["_file"], []).append(row)
    for values in grouped.values():
        values.sort(key=lambda item: as_int(item, "ClosedM5"))

    gate_counts = Counter()
    reason_counts = Counter()
    missed_reasons = Counter()
    missed_gates = Counter()
    missed_features = Counter()
    near_threshold_counts = Counter()
    confluence = Counter()
    adverse_actionable_reasons = Counter()
    cohort_examples: dict[str, list[dict[str, Any]]] = {}

    trace_count = 0
    actionable_count = 0
    potential_missed_count = 0
    adverse_actionable_count = 0
    actionable_sub_one_r_count = 0
    near_threshold_rows = 0

    for file_rows in grouped.values():
        for index, row in enumerate(file_rows):
            trace_count += 1
            gate = row.get("_TraceGate", "") or row.get("TraceGate", "") or "UNKNOWN"
            direction = direction_value(row)
            gate_counts[gate] += 1

            reason = (
                row.get("_ActionabilityReason", "")
                or row.get("_BlockReason", "")
                or row.get("_DecisionReason", "")
                or "UNKNOWN"
            )
            reason_counts[reason] += 1

            combo = (
                as_int(row, "FvgObBullConfluence" if direction == 1 else "FvgObBearConfluence")
                == 1
            )
            confluence["OB+FVG ACTIONABLE" if combo and gate == "ACTIONABLE" else "OB+FVG NON-ACTIONABLE" if combo else "OTHER ACTIONABLE" if gate == "ACTIONABLE" else "OTHER NON-ACTIONABLE"] += 1

            if direction == 0:
                continue

            mfe, mae, first_event = forward_metrics(file_rows, index, forward_bars)
            near = near_threshold_cohort(row)
            if near:
                near_threshold_rows += 1
                for name in near:
                    near_threshold_counts[name] += 1

            if gate == "ACTIONABLE":
                actionable_count += 1
                if first_event in {"STOP", "AMBIGUOUS"}:
                    adverse_actionable_count += 1
                    adverse_actionable_reasons[reason] += 1
                if mfe < 1.0:
                    actionable_sub_one_r_count += 1
                continue

            if mfe >= min_mfe_r and first_event != "STOP":
                potential_missed_count += 1
                missed_reasons[reason] += 1
                missed_gates[gate] += 1

                features = near or []
                if combo:
                    features.append("OB+FVG")
                if as_int(row, "WaveTrendQuality") >= 60:
                    features.append("WAVETREND_Q>=60")
                if as_int(row, "IndicatorConfluenceQuality") >= 65:
                    features.append("INDICATOR_FUSION_Q>=65")
                if as_int(row, "HtfAlignment") >= 70:
                    features.append("HTF_ALIGNMENT>=70")
                for feature in features:
                    missed_features[feature] += 1

                key = reason[:120]
                bucket = cohort_examples.setdefault(key, [])
                if len(bucket) < 8:
                    bucket.append(
                        {
                            "file": row["_file"],
                            "closed_m5": as_int(row, "ClosedM5"),
                            "direction": "BUY" if direction == 1 else "SELL",
                            "gate": gate,
                            "reason": reason,
                            "mfe_r": round(mfe, 4),
                            "mae_r": round(mae, 4),
                            "first_event": first_event,
                            "lane": row.get("Lane", "0"),
                            "regime": row.get("Regime", "UNKNOWN"),
                            "near_thresholds": near,
                            "ob_fvg": combo,
                            "wave_trend_quality": as_int(row, "WaveTrendQuality"),
                            "indicator_confluence_quality": as_int(row, "IndicatorConfluenceQuality"),
                            "indicator_conflict": as_int(row, "IndicatorConflict"),
                            "mtf_alignment": as_int(row, "HtfAlignment"),
                        }
                    )

    return {
        "trace_files": len(grouped),
        "trace_rows": trace_count,
        "gate_counts": dict(gate_counts),
        "reason_counts": dict(reason_counts.most_common(50)),
        "actionable_count": actionable_count,
        "potential_missed_count": potential_missed_count,
        "potential_missed_by_gate": dict(missed_gates.most_common(50)),
        "potential_missed_by_reason": dict(missed_reasons.most_common(50)),
        "potential_missed_features": dict(missed_features.most_common(50)),
        "ob_fvg_cohorts": dict(confluence),
        "adverse_actionable_count": adverse_actionable_count,
        "adverse_actionable_by_reason": dict(adverse_actionable_reasons.most_common(50)),
        "actionable_sub_one_r_count": actionable_sub_one_r_count,
        "near_threshold_rows": near_threshold_rows,
        "near_threshold_counts": dict(near_threshold_counts.most_common(50)),
        "potential_missed_examples": [example for group in cohort_examples.values() for example in group][:100],
        "forward_window_bars": forward_bars,
        "potential_missed_mfe_threshold_r": min_mfe_r,
        "interpretation": {
            "potential_missed": "A non-actionable trace followed by directional forward movement in the original trace geometry. It is a price-opportunity diagnostic, not proof that a tradable order would have filled or won.",
            "adverse_actionable": "An actionable trace whose observed forward window first hit STOP/AMBIGUOUS or never reached 1R MFE. This is a screening cohort, not a realized P&L result.",
        },
    }


def classify_rejection(reason: str, state: str) -> str:
    text = (reason + " " + state).upper()
    for category, needles in CATEGORY_PATTERNS:
        if any(needle in text for needle in needles):
            return category
    if state.upper() in {"REJECTED", "UNCONFIRMED"}:
        return "BROKER_REJECT"
    if state.upper() == "NULL RESULT":
        return "NULL_RESULT"
    if state.upper() == "FAILED":
        return "RUNTIME"
    return "OTHER"


def analyze_execution(rows: list[dict[str, str]], trace_rows: list[dict[str, str]]) -> dict[str, Any]:
    state_counts = Counter()
    category_counts = Counter()
    path_counts = Counter()
    reason_counts = Counter()
    correlated = Counter()
    examples: list[dict[str, Any]] = []

    trace_by_config_m5: dict[tuple[str, int], list[dict[str, str]]] = {}
    for row in trace_rows:
        trace_by_config_m5.setdefault((row["_config"], as_int(row, "ClosedM5")), []).append(row)

    reject_rows = 0
    for row in rows:
        state = safe_b64decode(row.get("State", "")) or row.get("State", "")
        path = safe_b64decode(row.get("Path", "")) or row.get("Path", "")
        reason = safe_b64decode(row.get("Reason", "")) or row.get("Reason", "")
        event = row.get("EventType", "")
        if event != "EXECUTION" or state.upper() not in REJECT_STATES:
            continue

        reject_rows += 1
        category = classify_rejection(reason, state)
        state_counts[state] += 1
        category_counts[category] += 1
        path_counts[path or "UNKNOWN"] += 1
        reason_counts[reason or "UNKNOWN"] += 1

        m5 = as_int(row, "M5", -1)
        matches = trace_by_config_m5.get((row["_config"], m5), [])
        if matches:
            latest = matches[-1]
            gate = latest.get("_TraceGate", "") or latest.get("TraceGate", "") or "UNKNOWN"
            correlated[gate + " -> " + category] += 1
        if len(examples) < 100:
            examples.append(
                {
                    "file": row["_file"],
                    "m5": m5,
                    "path": path,
                    "state": state,
                    "category": category,
                    "reason": reason,
                    "trace_gate": (
                        matches[-1].get("_TraceGate", "")
                        if matches
                        else ""
                    ) or "UNMATCHED",
                }
            )

    return {
        "runtime_files": len({row["_file"] for row in rows}),
        "execution_rejection_rows": reject_rows,
        "states": dict(state_counts.most_common(50)),
        "categories": dict(category_counts.most_common(50)),
        "paths": dict(path_counts.most_common(50)),
        "reasons": dict(reason_counts.most_common(50)),
        "trace_gate_to_rejection_category": dict(correlated.most_common(50)),
        "examples": examples,
        "interpretation": "Execution rejection counts describe broker/runtime observations. They do not prove a broker fault without the terminal/broker context.",
    }


def threshold_report(trace_rows: list[dict[str, str]]) -> dict[str, Any]:
    near_rows = 0
    per_gate = Counter()
    current_snapshot = dict(THRESHOLDS)
    for row in trace_rows:
        if direction_value(row) == 0:
            continue
        near = near_threshold_cohort(row)
        if not near:
            continue
        near_rows += 1
        gate = row.get("_TraceGate", "") or row.get("TraceGate", "") or "UNKNOWN"
        per_gate[gate] += 1

    return {
        "current_threshold_snapshot": current_snapshot,
        "near_threshold_trace_rows": near_rows,
        "near_threshold_by_gate": dict(per_gate),
        "policy": "Measurement only. This analyzer never proposes an automatic parameter mutation and never writes to production configuration.",
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="CFIP Phase 11.3 forensic analyzer")
    parser.add_argument("--history-dir", required=True, type=Path)
    parser.add_argument("--forward-bars", type=int, default=12)
    parser.add_argument("--min-mfe-r", type=float, default=1.0)
    parser.add_argument("--thresholds-json", type=Path, default=None)
    parser.add_argument("--output", type=Path, default=None)
    args = parser.parse_args()

    if not args.history_dir.exists():
        parser.error(f"history directory not found: {args.history_dir}")
    if args.forward_bars < 1:
        parser.error("--forward-bars must be at least 1")
    if args.min_mfe_r <= 0:
        parser.error("--min-mfe-r must be positive")

    if args.thresholds_json is not None:
        data = json.loads(args.thresholds_json.read_text(encoding="utf-8"))
        for key, value in data.items():
            if key not in THRESHOLDS:
                raise SystemExit(f"unsupported threshold key: {key}")
            if not isinstance(value, (int, float)) or isinstance(value, bool):
                raise SystemExit(f"threshold {key} must be numeric")
            THRESHOLDS[key] = value

    trace_rows = load_csv_files(args.history_dir, "CFIP_SignalTrace_*.csv")
    runtime_rows = load_csv_files(args.history_dir, "CFIP_RuntimeLog_v2_*.csv")

    trace_report = analyze_traces(trace_rows, args.forward_bars, args.min_mfe_r)
    execution_report = analyze_execution(runtime_rows, trace_rows)
    threshold = threshold_report(trace_rows)

    result: dict[str, Any] = {
        "phase": "11.3",
        "status": (
            "READY_WITH_EVIDENCE"
            if trace_rows or runtime_rows
            else "NO_EVIDENCE_FILES_FOUND"
        ),
        "signal_trace": trace_report,
        "execution_forensics": execution_report,
        "threshold_evidence": threshold,
        "source_boundary": {
            "signal_traces": "CFIP_SignalTrace_*.csv",
            "runtime_logs": "CFIP_RuntimeLog_v2_*.csv",
            "live_mutation": False,
            "automatic_threshold_changes": False,
        },
    }

    encoded = json.dumps(result, indent=2, ensure_ascii=False)
    print(encoded)
    if args.output is not None:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(encoded + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
