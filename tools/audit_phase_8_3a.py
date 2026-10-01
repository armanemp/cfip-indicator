#!/usr/bin/env python3
"""Static acceptance gate for CR8.3a / H3-A Skender settings ownership."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def check(name: str, condition: bool) -> None:
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        errors.append(name)


settings = read("src/CFIP.Indicator/Core/Math/OssIndicatorSettings.cs")
parameters = read(
    "src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorParameters.cs"
)
contracts = read("tools/CFIP.Planning.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Planning.Contracts/CFIP.Planning.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

property_types = [
    ("int", "MacdSignalPeriod"),
    ("int", "BollingerPeriod"),
    ("double", "BollingerStandardDeviations"),
    ("int", "MfiPeriod"),
    ("int", "StochLookbackPeriod"),
    ("int", "StochSignalPeriod"),
    ("int", "StochSmoothPeriod"),
    ("int", "SuperTrendPeriod"),
    ("double", "SuperTrendMultiplier"),
    ("int", "AroonPeriod"),
    ("int", "CciPeriod"),
    ("double", "ParabolicSarAccelerationFactor"),
    ("double", "ParabolicSarMaximumAccelerationFactor"),
]

check(
    "immutable central settings owner exists",
    "internal sealed class OssIndicatorSettings" in settings
    and "internal static OssIndicatorSettings Default" in settings
    and "private OssIndicatorSettings()" in settings
    and all(
        f"internal {type_name} {name} {{ get; }}" in settings
        for type_name, name in property_types
    ),
)

check(
    "all legacy settings values exist exactly once in the central owner",
    all(
        settings.count(f"{name} = {value};") == 1
        for name, value in [
            ("MacdSignalPeriod", "9"),
            ("BollingerPeriod", "20"),
            ("BollingerStandardDeviations", "2.0"),
            ("MfiPeriod", "14"),
            ("StochLookbackPeriod", "14"),
            ("StochSignalPeriod", "3"),
            ("StochSmoothPeriod", "3"),
            ("SuperTrendPeriod", "10"),
            ("SuperTrendMultiplier", "3.0"),
            ("AroonPeriod", "25"),
            ("CciPeriod", "20"),
            ("ParabolicSarAccelerationFactor", "0.02"),
            ("ParabolicSarMaximumAccelerationFactor", "0.20"),
        ]
    ),
)

check(
    "legacy parameter file no longer owns indicator defaults",
    all(name not in parameters for _, name in property_types),
)

for relative in [
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderAroon.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderBollingerBands.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderCci.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMfi.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderParabolicSar.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderStoch.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderSuperTrend.cs",
]:
    source = read(relative)
    check(
        f"{relative} consumes central settings",
        "OssIndicatorSettings.Default." in source,
    )
    check(
        f"{relative} has no moved default owner",
        not any(
            f"OssIndicatorParameters.{name}" in source
            for _, name in property_types
        ),
    )

check(
    "RSI and MACD fast/slow remain parameter-driven",
    "SafeRsiPeriod(RsiPeriod)" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderRsi.cs"
    )
    and "SafeMacdFastPeriod(MacdFastPeriod)" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs"
    )
    and "SafeMacdSlowPeriod" in read(
        "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs"
    ),
)

check(
    "deterministic H3-A defaults contract is wired",
    "VerifyOssIndicatorSettings();" in contracts
    and "H3-A MACD signal period default" in contracts
    and "H3-A Parabolic SAR defaults" in contracts
    and "all legacy values preserved" in contracts,
)

check(
    "planning contracts compile the central settings owner",
    "OssIndicatorSettings.cs" in contracts_project,
)

check(
    "H3-A static audit is accumulated in Source/Architecture CI",
    "audit_phase_8_3a.py" in workflow
    and workflow.index("audit_phase_8_3a.py") > workflow.index("audit_phase_8_2.py"),
)

print("CR8.3a / H3-A SUMMARY")
print("=" * 72)
print(f"Errors: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR8.3a / H3-A STATIC GATE PASS")
