#!/usr/bin/env python3

import csv
import math
import sys
from collections import Counter
from pathlib import Path


def fnum(value):
    try:
        return float(value)
    except (TypeError, ValueError):
        return None


def inum(value):
    try:
        return int(value)
    except (TypeError, ValueError):
        return None


def finite(value):
    return value is not None and math.isfinite(value)


def main():
    if len(sys.argv) != 2:
        print("usage: analyze_runtime_log.py <CFIP_RuntimeLog_*.csv>")
        return 2

    path = Path(sys.argv[1])
    if not path.exists():
        print(f"file not found: {path}")
        return 2

    with path.open("r", encoding="utf-8-sig", newline="") as handle:
        schema = handle.readline().strip()
        if schema != "CFIP-RUNTIME-LOG,1":
            print("invalid schema: expected CFIP-RUNTIME-LOG,1")
            return 3

        events = Counter()
        states = Counter()
        paths = Counter()
        tfs = Counter()
        scenarios = Counter()
        anomalies = []

        rows = 0
        actionable = 0

        for row in csv.DictReader(handle):
            rows += 1
            events[row.get("EventType", "")] += 1
            states[row.get("State", "")] += 1
            paths[row.get("Path", "")] += 1

            tf = row.get("SourceTimeframe", "")
            if tf:
                tfs[tf] += 1

            sid = row.get("ScenarioId", "")
            if sid:
                scenarios[sid] += 1

            if row.get("ActionableNow") == "1":
                actionable += 1

            values = {}
            for field in ("Entry", "Stop", "Tp1", "Tp2", "Tp3", "Tp4"):
                raw = row.get(field, "")
                if raw == "":
                    continue
                values[field] = fnum(raw)
                if not finite(values[field]):
                    anomalies.append(f"non-finite {field} at row {rows}")

            direction = inum(row.get("Direction"))
            entry = values.get("Entry")
            stop = values.get("Stop")
            tp1 = values.get("Tp1")

            if direction in (1, -1) and finite(entry) and finite(stop):
                if direction == 1 and stop >= entry:
                    anomalies.append(f"BUY stop >= entry at row {rows}")
                if direction == -1 and stop <= entry:
                    anomalies.append(f"SELL stop <= entry at row {rows}")

            if direction in (1, -1) and finite(entry) and finite(tp1):
                if direction == 1 and tp1 <= entry:
                    anomalies.append(f"BUY TP1 <= entry at row {rows}")
                if direction == -1 and tp1 >= entry:
                    anomalies.append(f"SELL TP1 >= entry at row {rows}")

    print(f"rows: {rows}")
    print(f"actionable: {actionable}")

    for title, counter in (
        ("event types", events),
        ("states", states),
        ("paths", paths),
        ("source timeframes", tfs),
        ("top scenarios", scenarios),
    ):
        print(f"{title}:")
        for key, value in counter.most_common(20):
            print(f"  {key or '<empty>'}: {value}")

    print(f"anomalies: {len(anomalies)}")
    for item in anomalies[:100]:
        print(f"  - {item}")

    return 1 if anomalies else 0


if __name__ == "__main__":
    raise SystemExit(main())