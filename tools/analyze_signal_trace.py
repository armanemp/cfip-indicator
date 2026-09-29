#!/usr/bin/env python3
"""Analyze CFIP closed-bar signal traces without changing live trading logic.

The analyzer treats trace rows as observations. It never feeds measurements back
into the indicator and never labels a missed setup as a profitable trade.
"""

from __future__ import annotations

import argparse
import base64
import csv
import json
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any


def decode(value: str) -> str:
    if not value:
        return ""
    try:
        return base64.b64decode(value).decode("utf-8")
    except Exception:
        return value


def as_int(row: dict[str, str], key: str, default: int = 0) -> int:
    try:
        return int(float(row.get(key, default)))
    except (TypeError, ValueError):
        return default


def as_float(row: dict[str, str], key: str, default: float = 0.0) -> float:
    try:
        return float(row.get(key, default))
    except (TypeError, ValueError):
        return default


def is_valid_price(value: float) -> bool:
    return value > 0.0


def direction_value(row: dict[str, str]) -> int:
    value = as_int(row, "Direction")
    return value if value in (-1, 1) else 0


def location_confluence(row: dict[str, str], direction: int) -> bool:
    return (
        as_int(
            row,
            "FvgObBullConfluence" if direction == 1 else "FvgObBearConfluence",
        )
        == 1
    )


def forward_metrics(
    rows: list[dict[str, str]],
    index: int,
    forward_bars: int,
) -> tuple[float, float, str]:
    row = rows[index]
    direction = direction_value(row)
    entry = as_float(row, "Entry")
    stop = as_float(row, "Stop")
    risk = abs(entry - stop)

    if direction == 0 or not is_valid_price(entry) or risk <= 0.0:
        return 0.0, 0.0, "INVALID"

    future_rows = rows[index + 1 : index + 1 + forward_bars]
    max_favorable = 0.0
    max_adverse = 0.0
    first_event = "NONE"

    tp1 = as_float(row, "Tp1")
    has_tp1 = is_valid_price(tp1)

    for future in future_rows:
        high = as_float(future, "High")
        low = as_float(future, "Low")

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

        max_favorable = max(max_favorable, favorable)
        max_adverse = max(max_adverse, adverse)

        if first_event != "NONE":
            continue

        if hit_tp and hit_stop:
            first_event = "AMBIGUOUS"
        elif hit_tp:
            first_event = "TP1"
        elif hit_stop:
            first_event = "STOP"

    return max_favorable, max_adverse, first_event


def load_rows(history_dir: Path) -> list[dict[str, str]]:
    rows: list[dict[str, str]] = []

    for path in sorted(history_dir.glob("CFIP_SignalTrace_*.csv")):
        try:
            with path.open("r", encoding="utf-8-sig", newline="") as handle:
                first = handle.readline().strip()
                if first != "CFIP-SIGNAL-TRACE,1":
                    continue
                reader = csv.DictReader(handle)
                for row in reader:
                    row["_file"] = str(path)
                    row["_topdown_stage"] = decode(row.get("TopDownStage", ""))
                    row["_trace_gate"] = decode(row.get("TraceGate", ""))
                    row["_block_reason"] = decode(row.get("BlockReason", ""))
                    row["_actionability_reason"] = decode(
                        row.get("ActionabilityReason", "")
                    )
                    row["_decision_reason"] = decode(
                        row.get("DecisionReason", "")
                    )
                    rows.append(row)
        except (OSError, csv.Error):
            continue

    rows.sort(key=lambda row: as_int(row, "BarOpenTimeUtcTicks"))
    return rows


def analyze(
    rows: list[dict[str, str]],
    forward_bars: int,
    min_mfe_r: float,
) -> dict[str, Any]:
    gates = Counter()
    actionability_reasons = Counter()
    decision_reasons = Counter()
    candidate_counts = Counter()
    lane_counts = Counter()
    confluence = Counter()

    missed: list[dict[str, Any]] = []
    actionable_metrics: list[float] = []
    blocked_metrics: list[float] = []

    for i, row in enumerate(rows):
        direction = direction_value(row)
        if direction == 0:
            gates[row["_trace_gate"] or "CONSENSUS"] += 1
            continue

        gate = row["_trace_gate"] or "UNKNOWN"
        gates[gate] += 1
        lane = row.get("Lane", "0")
        lane_counts[lane] += 1

        if gate == "ACTIONABLE":
            candidate_counts["actionable"] += 1
        else:
            candidate_counts["directional_non_actionable"] += 1

        if gate == "ACTIONABILITY":
            actionability_reasons[
                row["_actionability_reason"] or "UNKNOWN"
            ] += 1
        if gate == "DECISION-FILTER":
            decision_reasons[
                row["_block_reason"] or "UNKNOWN"
            ] += 1

        has_combo = location_confluence(row, direction)
        key = "confluence" if has_combo else "non_confluence"
        confluence[f"{key}_directional"] += 1
        if gate == "ACTIONABLE":
            confluence[f"{key}_actionable"] += 1

        mfe, mae, first_event = forward_metrics(
            rows,
            i,
            forward_bars,
        )

        if gate == "ACTIONABLE":
            actionable_metrics.append(mfe)
        else:
            blocked_metrics.append(mfe)

        # "Potential missed" is intentionally only a forward-price diagnostic:
        # the candidate was not actionable at its observation time, yet price
        # later moved at least min_mfe_r in its original direction.
        if (
            gate != "ACTIONABLE"
            and first_event != "STOP"
            and mfe >= min_mfe_r
            and as_float(row, "Entry") > 0
            and as_float(row, "Stop") > 0
            and as_float(row, "Tp1") > 0
        ):
            missed.append(
                {
                    "bar_open_time_utc_ticks": as_int(
                        row,
                        "BarOpenTimeUtcTicks",
                    ),
                    "direction": direction,
                    "gate": gate,
                    "reason": (
                        row["_actionability_reason"]
                        or row["_block_reason"]
                        or row["_decision_reason"]
                    ),
                    "lane": lane,
                    "mfe_r": mfe,
                    "mae_r": mae,
                    "first_event": first_event,
                    "ob_fvg_confluence": has_combo,
                }
            )

    return {
        "trace_rows": len(rows),
        "gate_counts": dict(gates),
        "actionability_reasons": dict(actionability_reasons),
        "decision_filter_reasons": dict(decision_reasons),
        "candidate_counts": dict(candidate_counts),
        "lane_counts": dict(lane_counts),
        "ob_fvg_confluence": dict(confluence),
        "potential_missed_count": len(missed),
        "potential_missed_by_reason": dict(
            Counter(item["reason"] or "UNKNOWN" for item in missed)
        ),
        "potential_missed_by_gate": dict(
            Counter(item["gate"] for item in missed)
        ),
        "potential_missed_examples": missed[:50],
        "forward_window_bars": forward_bars,
        "potential_missed_mfe_threshold_r": min_mfe_r,
        "actionable_mean_mfe_r": (
            sum(actionable_metrics) / len(actionable_metrics)
            if actionable_metrics
            else 0.0
        ),
        "non_actionable_mean_mfe_r": (
            sum(blocked_metrics) / len(blocked_metrics)
            if blocked_metrics
            else 0.0
        ),
    }


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Analyze CFIP signal trace archives.",
    )
    parser.add_argument(
        "--history-dir",
        required=True,
        type=Path,
        help="cTrader CFIP History directory",
    )
    parser.add_argument(
        "--forward-bars",
        type=int,
        default=12,
        help="closed-M5 bars to inspect after each trace",
    )
    parser.add_argument(
        "--min-mfe-r",
        type=float,
        default=1.0,
        help="forward MFE threshold used only for potential-missed diagnostics",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=None,
        help="optional JSON report output path",
    )
    args = parser.parse_args()

    if args.forward_bars < 1:
        parser.error("--forward-bars must be at least 1")
    if args.min_mfe_r <= 0:
        parser.error("--min-mfe-r must be positive")

    rows = load_rows(args.history_dir)
    result = analyze(
        rows,
        args.forward_bars,
        args.min_mfe_r,
    )

    if args.output is not None:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(
            json.dumps(result, indent=2, ensure_ascii=False),
            encoding="utf-8",
        )

    print(json.dumps(result, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
