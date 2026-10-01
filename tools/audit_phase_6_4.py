#!/usr/bin/env python3
"""Static acceptance gate for CR6.4 / F5 smart-threshold regime identity."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative):
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name, condition):
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


def normalized(source):
    return re.sub(r"\s+", " ", source).strip()


identity = read("src/CFIP.Indicator/Core/Math/MarketRegimeIdentity.cs")
classifier = read("src/CFIP.Indicator/Core/Math/MarketRegimeClassifier.cs")
frame = read("src/CFIP.Indicator/Core/Math/FrameRegimeResolutionRule.cs")
smart = read("src/CFIP.Indicator/Trading/Validation/SmartThresholdPolicy.cs")
smart_rule = read("src/CFIP.Indicator/Core/Math/SmartThresholdPolicyRule.cs")
fusion = read("src/CFIP.Indicator/Core/Math/IndicatorEvidenceFusionRule.cs")
no_trade = read("src/CFIP.Indicator/Trading/Intelligence/NoTradeRegimeAnalyzer.cs")
regime_filter = read("src/CFIP.Indicator/Planning/Filters/RegimeFilter.cs")
analyzer = read("src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")


for name, token in (
    ("UNKNOWN identity", 'public const string Unknown = "UNKNOWN";'),
    ("TREND identity", 'public const string Trend = "TREND";'),
    ("EXPANSION identity", 'public const string Expansion = "EXPANSION";'),
    ("RANGE identity", 'public const string Range = "RANGE";'),
    ("TRANSITION identity", 'public const string Transition = "TRANSITION";'),
    ("HIGH_VOLATILITY identity", 'public const string HighVolatility = "HIGH_VOLATILITY";'),
    ("COMPRESSION identity", 'public const string Compression = "COMPRESSION";'),
):
    check(name, token in identity)

check(
    "identity owns normalization",
    "public static string NormalizeMarketRegime(" in identity and
    "default:" in identity and
    "return Unknown;" in identity
)

check(
    "classifier returns only canonical regime identities",
    all(token in classifier for token in (
        "return MarketRegimeIdentity.Unknown;",
        "return MarketRegimeIdentity.Compression;",
        "return MarketRegimeIdentity.HighVolatility;",
        "return MarketRegimeIdentity.Range;",
        "return MarketRegimeIdentity.Expansion;",
        "return MarketRegimeIdentity.Trend;",
        "return MarketRegimeIdentity.Transition;",
    ))
)

check(
    "classifier has no REVERSAL regime branch",
    "REVERSAL" not in classifier
)

check(
    "frame regime resolution delegates to the canonical identity owner",
    "MarketRegimeIdentity.NormalizeMarketRegime(regime)" in frame and
    "MarketRegimeIdentity.Unknown" in frame
)

check(
    "adaptive smart thresholds delegate to one platform-neutral policy owner",
    "SmartThresholdPolicyRule.ResolveSmartThresholds(" in smart and
    "GetAdaptiveSmartThresholds(" in smart
)

check(
    "REVERSAL dead branch is removed from smart threshold policy",
    "case MarketRegimeIdentity.Compression:" in smart_rule and
    "case MarketRegimeIdentity.Range:" in smart_rule and
    "case MarketRegimeIdentity.Trend:" in smart_rule and
    "case MarketRegimeIdentity.Expansion:" in smart_rule and
    "REVERSAL" not in smart_rule
)

check(
    "HIGH_VOLATILITY and TRANSITION remain explicit neutral branches",
    "public const string HighVolatility = \"HIGH_VOLATILITY\";" in identity and
    "public const string Transition = \"TRANSITION\";" in identity and
    "case MarketRegimeIdentity.HighVolatility" not in smart_rule and
    "case MarketRegimeIdentity.Transition" not in smart_rule
)

check(
    "known regime consumers use canonical identities",
    "MarketRegimeIdentity.Trend" in fusion and
    "MarketRegimeIdentity.Compression" in no_trade and
    "MarketRegimeIdentity.Range" in no_trade and
    "MarketRegimeIdentity.Transition" in no_trade and
    "MarketRegimeIdentity.Unknown" in regime_filter and
    "MarketRegimeIdentity.Unknown" in analyzer
)

check(
    "runtime contract covers all six canonical regimes and unknown/future safety",
    "VerifySmartThresholdRegimeSemantics();" in runtime and
    "MarketRegimeIdentity.Trend" in runtime and
    "MarketRegimeIdentity.Expansion" in runtime and
    "MarketRegimeIdentity.Range" in runtime and
    "MarketRegimeIdentity.Compression" in runtime and
    "MarketRegimeIdentity.HighVolatility" in runtime and
    "MarketRegimeIdentity.Transition" in runtime and
    '"REVERSAL"' in runtime and
    '"FUTURE_REGIME"' in runtime
)

check(
    "runtime contract includes classifier inputs, identity and smart policy owners",
    "Core/Models/MarketRegimeClassificationInput.cs" in runtime_project and
    "Core/Math/MarketRegimeIdentity.cs" in runtime_project and
    "Core/Math/MarketRegimeClassifier.cs" in runtime_project and
    "Core/Math/SmartThresholdPolicyRule.cs" in runtime_project
)

check(
    "F5 static audit is accumulated after F4",
    "audit_phase_6_3.py" in workflow and
    "audit_phase_6_4.py" in workflow and
    workflow.index("audit_phase_6_4.py") > workflow.index("audit_phase_6_3.py")
)

# The adaptive rule must preserve the current numerical behavior.
smart_rule_normalized = normalized(smart_rule)
for token in (
    "Math.Max(1, b / 3)",
    "Math.Max(1, b / 2)",
    "qualityThreshold -= b",
    "qualityThreshold += Math.Max(1, b / 2)",
    "qualityThreshold += b",
):
    check(
        "legacy adaptive behavior preserved: " + token,
        token in smart_rule_normalized,
    )

print("CR6.4 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR6.4 STATIC GATE PASS")
