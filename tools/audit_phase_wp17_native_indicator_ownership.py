#!/usr/bin/env python3
"""WP-17 analytical-stack ownership audit.

This audit proves the native indicator registry remains the single numerical
owner for the platform-native core set. It deliberately does not approve
additional indicators merely because they exist; promotion belongs to WP-18+
independence/calibration gates.
"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
native = ROOT / "src/CFIP.Indicator/Analysis/Indicators/Native.cs"
registry = ROOT / "src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs"
readiness = ROOT / "src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorReadinessRule.cs"
evidence = ROOT / "src/CFIP.Indicator/Analysis/Market/MarketFrameEvidence.cs"

def check(ok, message):
    if not ok:
        raise SystemExit("FAIL: " + message)
    print("PASS: " + message)

n = native.read_text(encoding="utf-8")
r = registry.read_text(encoding="utf-8")
q = readiness.read_text(encoding="utf-8")
e = evidence.read_text(encoding="utf-8")

for field in ("Bars", "Fast", "Slow", "Atr", "Rsi", "Dms", "MacdFast", "MacdSlow"):
    check(f"public" in n and f" {field};" in n, f"Native owns {field}")

for tf in ("_m1Bars", "_m5Bars", "_m15Bars", "_m30Bars", "_h1Bars", "_h4Bars", "_d1Bars", "_w1Bars"):
    check(f"RegisterNative({tf});" in r, f"native registry registers {tf}")

for initializer in (
    "InitializeExponentialMovingAverages",
    "InitializeAverageTrueRange",
    "InitializeRelativeStrengthIndex",
    "InitializeDirectionalMovementSystem",
):
    check(initializer in r, f"native registry initializes {initializer}")

check("InitializeMacd" in r, "native registry owns MACD initialization path")
check("NativeIndicatorReadinessRule.IsFrameReady" in e, "frame evidence uses canonical native readiness")
check("NativeIndicatorReadinessRule.IsIndexedSeriesReady" in q, "indexed native series readiness is explicit")

# Guard against creating a second native-holder implementation.
sources = list((ROOT / "src/CFIP.Indicator").rglob("*.cs"))
holders = []
for p in sources:
    text = p.read_text(encoding="utf-8", errors="ignore")
    if "class Native" in text:
        holders.append(str(p.relative_to(ROOT)))
check(len(holders) == 1 and holders[0].endswith("Analysis/Indicators/Native.cs"),
      f"exactly one Native holder exists: {holders}")

print("WP-17 native ownership audit PASS")
