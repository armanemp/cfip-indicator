#!/usr/bin/env python3
"""WP-17 analytical-stack ownership audit.

Proves the platform-native core has one holder/registry/readiness path.
This gate guards ownership and registration only; it does not claim signal
quality has improved or approve adding redundant indicators.
"""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
native = ROOT / "src/CFIP.Indicator/Analysis/Indicators/Native/Native.cs"
registry = ROOT / "src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs"
readiness = ROOT / "src/CFIP.Indicator/Core/Math/NativeIndicatorReadinessRule.cs"
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
    check(re.search(r"\bpublic\s+\w+\s+" + re.escape(field) + r"\s*;", n) is not None,
          f"Native owns {field}")
check(re.search(r"\bpublic\s+bool\s+IsInitialized\s*;", n) is not None,
      "Native exposes explicit initialization state")
check("set.IsInitialized = true;" in r and "set.IsInitialized = false;" in r,
      "native registry records both successful and failed initialization")

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
wrapper_sources = [
    (ROOT / "src/CFIP.Indicator/Analysis/Indicators" / "AverageTrueRange.cs").read_text(encoding="utf-8"),
    (ROOT / "src/CFIP.Indicator/Analysis/Indicators" / "AverageDirectionalIndex.cs").read_text(encoding="utf-8"),
    (ROOT / "src/CFIP.Indicator/Analysis/Indicators" / "RelativeStrengthIndex.cs").read_text(encoding="utf-8"),
    (ROOT / "src/CFIP.Indicator/Analysis/Indicators" / "DirectionalMovementIndex.cs").read_text(encoding="utf-8"),
    (ROOT / "src/CFIP.Indicator/Analysis/Indicators" / "ExponentialMovingAverage.cs").read_text(encoding="utf-8"),
]
check(all("!set.IsInitialized" in source for source in wrapper_sources),
      "native wrappers fail closed after initialization failure")
macd_bias = (ROOT / "src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs").read_text(encoding="utf-8")
check("!set.IsInitialized" in macd_bias,
      "direct native MACD consumer fails closed after initialization failure")
check(re.search(r"\b(?:internal\s+static\s+)?bool\s+IsIndexedSeriesReady\s*\(", q) is not None,
      "indexed native series readiness is explicitly defined")

# Missing native values must fail closed; neutral 0/50 fallbacks can become
# directional evidence or hide provenance at downstream consumers.
for filename in (
    "AverageTrueRange.cs",
    "AverageDirectionalIndex.cs",
    "RelativeStrengthIndex.cs",
    "DirectionalMovementIndex.cs",
    "ExponentialMovingAverage.cs",
):
    source = (ROOT / "src/CFIP.Indicator/Analysis/Indicators" / filename).read_text(encoding="utf-8")
    check("return 0;" not in source and "return 50;" not in source,
          f"{filename} has no neutral numeric fallback")

# Match the exact Native type, not other names beginning with "Native".
sources = list((ROOT / "src/CFIP.Indicator").rglob("*.cs"))
holders = []
for path in sources:
    source = path.read_text(encoding="utf-8", errors="ignore")
    if re.search(r"\bclass\s+Native\b", source):
        holders.append(str(path.relative_to(ROOT)))
expected = "src/CFIP.Indicator/Analysis/Indicators/Native/Native.cs"
check(holders == [expected], f"exactly one Native holder exists: {holders}")

print("WP-17 native ownership audit PASS")
