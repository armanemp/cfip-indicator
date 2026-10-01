#!/usr/bin/env python3
"""Static acceptance gate for CR2.6 Order Block quality and cache discipline."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

quality = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockQualityCalculator.cs")
quality_rule = read("src/CFIP.Indicator/Core/Math/OrderBlockQualityRule.cs")
rule = read("src/CFIP.Indicator/Core/Math/OrderBlockRule.cs")
lifecycle = read("src/CFIP.Indicator/Core/Math/OrderBlockLifecycleRule.cs")
candidate = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs")
analyzer = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockAnalyzer.cs")
mitigation = read("src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockMitigationGuard.cs")
cache = read("src/CFIP.Indicator/Analysis/Structure/Zones/ZoneLookupHotCache.cs")
zone = read("src/CFIP.Indicator/Core/Models/Zone.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

checks = {
    "quality delegates to canonical owner": (
        "OrderBlockQualityRule.Calculate(" in quality and
        "quality = 54" not in quality
    ),
    "quality base/clamp is explicit": (
        "BaseQuality = 54" in quality_rule and
        "return ClampQuality(" in quality_rule and
        "100" in quality_rule
    ),
    "independent evidence components remain separate": all(
        token in quality_rule
        for token in [
            "bool displacement",
            "bool structureBreak",
            "bool liquiditySweep",
            "bool fvgConfluence"
        ]
    ),
    "impulse normalization uses price-unit ATR": (
        "strongestBody /" in quality and
        "Math.Max(" in quality and
        "Symbol.PipSize" in quality and
        "atr);" in quality
    ),
    "side of market is canonical": (
        "IsOnCorrectMarketSide(" in rule and
        "zoneHigh <= market + tolerance" in rule and
        "zoneLow >= market - tolerance" in rule
    ),
    "lifecycle has fresh mitigated broken states": (
        "OrderBlockLifecycleState.Fresh" in lifecycle and
        "OrderBlockLifecycleState.Mitigated" in lifecycle and
        "OrderBlockLifecycleState.Broken" in lifecycle
    ),
    "active candidate records lifecycle": (
        "out OrderBlockLifecycleState lifecycleState" in candidate and
        "OrderBlockLifecycle =" in candidate
    ),
    "broken candidates cannot survive mitigation": (
        "OrderBlockLifecycleRule.ClassifyOrderBlockLifecycle(" in mitigation and
        "OrderBlockLifecycleRule.MinimumRetainedRatio" in mitigation and
        "Broken" in mitigation
    ),
    "selection enforces side and active lifecycle": (
        "candidate.OrderBlockLifecycle" in analyzer and
        "IsOnCorrectMarketSide(" in analyzer
    ),
    "cache is closed-context bound": (
        "bars.Count - 1" in analyzer and
        "ResetZoneLookupCacheIfNeeded(" in analyzer and
        "TryGetCachedObCandidates(" in analyzer and
        "StoreCachedObCandidates(" in analyzer
    ),
    "cache context excludes quote selection": (
        "selectionPrice" in analyzer and
        "cacheKey =" in analyzer
    ),
    "zone model carries OB lifecycle only": "OrderBlockLifecycleState? OrderBlockLifecycle" in zone,
    "runtime contracts cover CR2.6": (
        "VerifyOrderBlockQualitySemantics();" in contracts and
        "OrderBlockLifecycleState.Broken" in contracts
    ),
    "runtime project links CR2.6 math": (
        "OrderBlockQualityRule.cs" in project and
        "OrderBlockRule.cs" in project and
        "OrderBlockLifecycleRule.cs" in project
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

key_fragment_start = analyzer.find("string cacheKey")
if key_fragment_start >= 0:
    key_fragment_end = analyzer.find("Zone[] candidates", key_fragment_start)
    key_fragment = analyzer[key_fragment_start:key_fragment_end]
    if "selectionPrice" in key_fragment or "market" in key_fragment:
        errors.append("Order Block cache key depends on live market selection price")
        print(f"FAIL | {errors[-1]}")

print("CR2.6 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.6 STATIC GATE PASS")
