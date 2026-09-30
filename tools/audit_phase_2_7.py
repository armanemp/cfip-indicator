#!/usr/bin/env python3
"""Static acceptance gate for CR2.7 WaveTrend mathematical correctness."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

engine = read("src/CFIP.Indicator/Analysis/Market/WaveTrendEngine.cs")
calculator = read("src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.cs")
base_kernels = read("src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.BaseKernels.cs")
advanced = read("src/CFIP.Indicator/Core/Math/WaveTrendMovingAverageCalculator.Advanced.cs")
readiness = read("src/CFIP.Indicator/Core/Math/WaveTrendReadinessRule.cs")
all_ma = calculator + base_kernels + advanced
parameters = read("src/CFIP.Indicator/Indicator/Parameters/26_wave_trend.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

checks = {
    "all declared cTrader MA types are mapped": all(
        token in engine
        for token in [
            "MovingAverageType.Simple",
            "MovingAverageType.Exponential",
            "MovingAverageType.TimeSeries",
            "MovingAverageType.Triangular",
            "MovingAverageType.VIDYA",
            "MovingAverageType.Weighted",
            "MovingAverageType.WilderSmoothing",
            "MovingAverageType.Hull",
            "MovingAverageType.DoubleExponential",
            "MovingAverageType.TripleExponential",
            "MovingAverageType.KaufmanAdaptive",
        ]
    ),
    "MA calculator owns all semantics": all(
        token in all_ma
        for token in [
            "case Simple:",
            "case Exponential:",
            "case TimeSeries:",
            "case Triangular:",
            "case Vidya:",
            "case Weighted:",
            "case WilderSmoothing:",
            "case Hull:",
            "case DoubleExponential:",
            "case TripleExponential:",
            "case KaufmanAdaptive:",
        ]
    ),
    "EMA and Wilder alpha semantics are explicit": (
        "2.0 /" in all_ma and
        "(_length + 1.0)" in all_ma and
        "1.0 /" in all_ma
    ),
    "DEMA and TEMA dependency depth is explicit": (
        "2 *" in calculator and
        "3 *" in calculator and
        "CalculateWaveTrendEmaSeries(" in advanced
    ),
    "HMA dependency uses sqrt period": (
        "Math.Sqrt(" in advanced and
        "halfLength" in advanced
    ),
    "KAMA efficiency ratio semantics exist": (
        "efficiency" in advanced and
        "fast" in advanced and
        "slow" in advanced
    ),
    "VIDYA adaptive efficiency semantics exist": (
        "CalculateWaveTrendVidya(" in advanced and
        "alpha" in advanced and
        "efficiency" in advanced
    ),
    "component readiness includes RMI depth": (
        "momentumLength + safeLength - 1" in readiness and
        "ResolveWaveTrendComponentReadyIndex(" in engine
    ),
    "snapshot readiness requires previous signal": (
        "index >" in readiness and
        "_signalReadyIndex" in engine
    ),
    "early component values fail closed": (
        "return double.NaN;" in engine and
        "CalculateWaveTrendRsi" in engine and
        "CalculateWaveTrendMfi" in engine
    ),
    "history extension invalidates WaveTrend cache": (
        "HistoryLoaded" in engine and
        "Reloaded" in engine and
        "ResetWaveTrendCalculationState" in engine
    ),
    "history identity change is detected": (
        "_knownFirstBarOpenTime" in engine and
        "firstOpenTime !=" in engine
    ),
    "public WaveTrend parameters remain unchanged": (
        '[Parameter("WaveTrend Smooth MA"' in parameters and
        '[Parameter("WaveTrend Signal MA"' in parameters and
        "DefaultValue = MovingAverageType.Exponential" in parameters and
        "DefaultValue = MovingAverageType.Simple" in parameters
    ),
    "runtime contracts cover CR2.7": (
        "VerifyWaveTrendMathematics();" in contracts and
        "WaveTrendMovingAverageCalculator.KaufmanAdaptive" in contracts
    ),
    "runtime project links CR2.7 calculator": (
        "WaveTrendMovingAverageCalculator.cs" in project
    ),
    "old partial MA fallback is removed": (
        "if (type == MovingAverageType.Exponential)" not in engine and
        "return simple / length;" not in engine
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# Keep tick-volume semantics explicit; cTrader exposes TickVolume rather than
# promising exchange-level real volume in this WaveTrend path.
if "TickVolumes[j]" not in engine:
    errors.append("WaveTrend MFI path must remain tick-volume based")
    print("FAIL | WaveTrend MFI path must remain tick-volume based")
else:
    print("PASS | WaveTrend MFI path remains explicitly tick-volume based")

print("CR2.7 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.7 STATIC GATE PASS")
