#!/usr/bin/env python3
"""Static acceptance gate for CI-01 primitive indicator mathematical integrity."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append("missing file: " + relative)
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    print(("PASS" if condition else "FAIL") + " | " + name)
    if not condition:
        errors.append(name)


atr = read("src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs")
adx = read("src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs")
dmi = read("src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs")
ema = read("src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs")
rsi = read("src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs")
macd_init = read("src/CFIP.Indicator/Analysis/Indicators/MacdIndicator.cs")
macd_bias = read("src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs")
volume = read("src/CFIP.Indicator/Analysis/Market/VolumeExpansionAnalyzer.cs")
vwap = read("src/CFIP.Indicator/Analysis/Market/VwapBiasAnalyzer.cs")
range_eff = read("src/CFIP.Indicator/Analysis/Market/RangeEfficiencyAnalyzer.cs")
chop = read("src/CFIP.Indicator/Analysis/Market/ChoppinessIndexAnalyzer.cs")
regime = read("src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs")

dmi_rule = read("src/CFIP.Indicator/Core/Math/DmiBiasRule.cs")
macd_rule = read("src/CFIP.Indicator/Core/Math/MacdBiasRule.cs")
range_rule = read("src/CFIP.Indicator/Core/Math/RangeEfficiencyRule.cs")
chop_rule = read("src/CFIP.Indicator/Core/Math/ChoppinessIndexRule.cs")
vwap_rule = read("src/CFIP.Indicator/Core/Math/VwapBiasRule.cs")
volume_rule = read("src/CFIP.Indicator/Core/Math/VolumeExpansionRule.cs")

runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

check(
    "ATR uses one native cTrader ATR owner with Wilder smoothing",
    "Indicators.AverageTrueRange(" in atr and
    "MovingAverageType.WilderSmoothing" in atr and
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in atr,
)

check(
    "ADX uses one native directional movement owner with Wilder smoothing",
    "Indicators.DirectionalMovementSystem(" in adx and
    "MovingAverageType.WilderSmoothing" in adx and
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in adx,
)

check(
    "DMI bias requires the same warm-up boundary as ADX",
    "DmiBiasRule.Calculate(" in dmi and
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in dmi and
    "set.Dms.DIPlus.Count" in dmi and
    "set.Dms.DIMinus.Count" in dmi,
)

check(
    "EMA fast/slow use the configured native EMA periods",
    "Indicators.ExponentialMovingAverage(" in ema and
    "NativeIndicatorReadinessRule.IsIndexedSeriesReady(" in ema and
    "FastEma" in ema and
    "SlowEma" in ema,
)

check(
    "RSI is a bounded native Wilder oscillator with explicit neutral fallback",
    "Indicators.RelativeStrengthIndex(" in rsi and
    "Math.Max(2, RsiPeriod)" in rsi and
    "Clamp(value, 0, 100)" in rsi and
    "return 50;" in rsi,
)

check(
    "MACD bias is explicitly the two-EMA MACD line rather than a fabricated histogram",
    "MacdFast" in macd_init and
    "MacdSlow" in macd_init and
    "double macdLine" in macd_bias and
    "previousMacdLine" in macd_bias and
    "MacdBiasRule.IsMacdDirectional(" in macd_bias and
    "histogram" not in macd_bias,
)

check(
    "RangeEfficiency uses the configured number of price-change intervals",
    "RangeEfficiencyRule.ResolveFirstCloseIndex(" in range_eff and
    "first + 1" in range_eff and
    "bars.ClosePrices[first]" in range_eff and
    "Math.Min" not in range_eff,
)

check(
    "Choppiness uses the complete configured bar window",
    "ChoppinessIndexRule.ResolveFirstBarIndex(" in chop and
    "ChoppinessIndexRule.HasChoppinessEnoughHistory(" in chop and
    "Math.Min" not in chop,
)

check(
    "VWAP uses exactly N bars and never invents volume for zero-volume samples",
    "int length" in vwap and
    "index - length + 1" in vwap and
    "Math.Max(0" in vwap and
    "VwapBiasRule.AccumulateVolume(" in vwap,
)

check(
    "Volume expansion uses actual bar range rather than pip-scale distortion",
    "double currentRange" in volume and
    "bars.HighPrices[index] -" in volume and
    "Math.Max(1.0" in volume_rule and
    "VolumeExpansionRule.IsExpanded(" in volume,
)

check(
    "primitive mathematical rules have one owner each",
    "class DmiBiasRule" in dmi_rule and
    "class MacdBiasRule" in macd_rule and
    "class RangeEfficiencyRule" in range_rule and
    "class ChoppinessIndexRule" in chop_rule and
    "class VwapBiasRule" in vwap_rule and
    "class VolumeExpansionRule" in volume_rule,
)

check(
    "configured periods are not silently shortened by index",
    "Math.Min" not in range_eff and
    "Math.Min" not in chop and
    "Math.Min" not in vwap,
)

check(
    "runtime contracts cover DMI, MACD, RangeEfficiency, Choppiness, VWAP and volume",
    "VerifyPrimitiveIndicatorMathematics();" in runtime and
    "CI-01 primitive indicator mathematics contracts PASS" in runtime and
    "DmiBiasRule.cs" in runtime_project and
    "MacdBiasRule.cs" in runtime_project and
    "RangeEfficiencyRule.cs" in runtime_project and
    "ChoppinessIndexRule.cs" in runtime_project and
    "VwapBiasRule.cs" in runtime_project and
    "VolumeExpansionRule.cs" in runtime_project,
)

check(
    "CI-01 audit is accumulated in Source/Architecture CI",
    "audit_phase_ci_01.py" in workflow and
    workflow.index("audit_phase_ci_01.py") >
    workflow.index("audit_phase_ci_00.py"),
)

check(
    "MarketRegimeAnalyzer does not request a shorter primitive window implicitly",
    "Math.Max(" in regime and
    "ChoppinessPeriod" in regime and
    "RegimeLookbackBars" in regime,
)

print("CI-01 PRIMITIVE INDICATOR INTEGRITY SUMMARY")
print("=" * 72)
print("Errors: " + str(len(errors)))

if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("CI-01 STATIC GATE PASS")
