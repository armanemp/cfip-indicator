#!/usr/bin/env python3
"""Reference benchmark for CR6.8 / F9 target-obstacle scan reuse."""

from __future__ import annotations

import math
import time
import tracemalloc


INDEX = 1_500
STRENGTH = 2
LOOKBACK = 220
TARGETS = tuple(100.0 + 0.02 * i for i in range(48))
REPEATS = 400
BARS = 1_900


def build_series() -> tuple[list[float], list[float]]:
    highs = []
    lows = []
    for i in range(BARS):
        base = 100.0 + math.sin(i / 9.0) * 0.8 + math.sin(i / 31.0) * 1.7
        spread = 0.35 + (i % 7) * 0.01
        highs.append(base + spread)
        lows.append(base - spread)
    return highs, lows


def is_swing(highs: list[float], lows: list[float], i: int, direction: int) -> bool:
    for j in range(1, STRENGTH + 1):
        if direction == 1:
            if highs[i] <= highs[i - j] or highs[i] <= highs[i + j]:
                return False
        else:
            if lows[i] >= lows[i - j] or lows[i] >= lows[i + j]:
                return False
    return True


def uncached(highs: list[float], lows: list[float]) -> tuple[int, int]:
    scan_comparisons = 0
    blocked = 0
    start = max(STRENGTH + 1, INDEX - LOOKBACK)
    last = max(start, INDEX - STRENGTH - 1)

    for target in TARGETS:
        for i in range(start, last + 1):
            for j in range(1, STRENGTH + 1):
                scan_comparisons += 1
            if not is_swing(highs, lows, i, 1):
                continue
            if highs[i] > 99.0 and highs[i] < target:
                blocked += 1
                break
    return scan_comparisons, blocked


def cached(highs: list[float], lows: list[float]) -> tuple[int, int]:
    scan_comparisons = 0
    blocked = 0
    start = max(STRENGTH + 1, INDEX - LOOKBACK)
    last = max(start, INDEX - STRENGTH - 1)

    snapshot = []
    for i in range(start, last + 1):
        for j in range(1, STRENGTH + 1):
            scan_comparisons += 1
        if is_swing(highs, lows, i, 1):
            snapshot.append(highs[i])

    for target in TARGETS:
        for level in snapshot:
            if level > 99.0 and level < target:
                blocked += 1
                break
    return scan_comparisons, blocked


def timed(fn, highs, lows):
    tracemalloc.start()
    start = time.perf_counter()
    comparisons = blocked = 0
    for _ in range(REPEATS):
        c, b = fn(highs, lows)
        comparisons += c
        blocked += b
    elapsed_ms = (time.perf_counter() - start) * 1000.0
    _, peak = tracemalloc.get_traced_memory()
    tracemalloc.stop()
    return comparisons, blocked, elapsed_ms, peak


def main() -> None:
    highs, lows = build_series()
    old = timed(uncached, highs, lows)
    new = timed(cached, highs, lows)

    print("CR6.8 / F9 reference benchmark")
    print("=" * 72)
    print(f"repeats={REPEATS} targets={len(TARGETS)} bars={BARS}")
    print(
        "uncached: "
        f"comparisons={old[0]} blocked={old[1]} "
        f"elapsed_ms={old[2]:.2f} peak_bytes={old[3]}"
    )
    print(
        "cached:   "
        f"comparisons={new[0]} blocked={new[1]} "
        f"elapsed_ms={new[2]:.2f} peak_bytes={new[3]}"
    )
    print(
        f"scan-work reduction={(1.0 - new[0] / old[0]) * 100.0:.2f}%"
    )
    print(f"elapsed speedup={old[2] / new[2]:.2f}x")
    print(
        "Note: this benchmark is a structural reference model, "
        "not a cTrader terminal latency measurement."
    )


if __name__ == "__main__":
    main()
