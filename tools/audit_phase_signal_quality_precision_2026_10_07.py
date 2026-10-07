#!/usr/bin/env python3
"""CFIP 2026-10-07 signal-quality precision audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(rel):
    path = ROOT / rel
    return path.read_text(encoding="utf-8") if path.exists() else ""

tf_rule = read("src/CFIP.Indicator/Core/Math/TimeframeAgreementRule.cs")
tf_analyzer = read("src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs")
structure_rule = read("src/CFIP.Indicator/Core/Math/StructuralConfirmationRule.cs")
structure_analyzer = read("src/CFIP.Indicator/Analysis/Market/Decision/StructuralConfirmationAnalyzer.cs")
contracts = read("tools/CFIP.Decision.Contracts/SignalQualityPrecisionContracts.cs")
program = read("tools/CFIP.Decision.Contracts/Program.cs")
csproj = read("tools/CFIP.Decision.Contracts/CFIP.Decision.Contracts.csproj")

errors = []

def require(ok, message):
    if not ok:
        errors.append(message)

require(
    "internal static class TimeframeAgreementRule" in tf_rule and
    "qualityFactor" in tf_rule and
    "alignedWeight +=" in tf_rule,
    "MTF agreement must have one Core owner with quality-weighted aligned credit",
)
require(
    "TimeframeAgreementRule.Calculate(" in tf_analyzer and
    "frames[i].Quality" not in tf_analyzer,
    "TimeframeAgreementAnalyzer must delegate quality semantics instead of keeping a second formula",
)
require(
    "internal static class StructuralConfirmationRule" in structure_rule and
    "frameDirection == -requestedDirection" in structure_rule,
    "structural confirmation must reject contradictory opposite-frame structure",
)
require(
    "StructuralConfirmationRule.CountDirectionalStructureContribution(" in structure_analyzer and
    "_m15Frame.Direction" in structure_analyzer and
    "_h1Frame.Direction" in structure_analyzer and
    "_h4Frame.Direction" in structure_analyzer,
    "higher-timeframe structure confirmations must consume frame direction through the canonical rule",
)
require(
    "VerifyTimeframeAgreementPrecision" in contracts and
    "VerifyStructuralConfirmationDirectionality" in contracts,
    "precision regression coverage must include MTF and structural directionality",
)
require(
    "SignalQualityPrecisionContracts.VerifyTimeframeAgreementPrecision();" in program and
    "SignalQualityPrecisionContracts.VerifyStructuralConfirmationDirectionality();" in program,
    "precision contracts must be executed",
)
require(
    "TimeframeAgreementRule.cs" in csproj and
    "StructuralConfirmationRule.cs" in csproj and
    "SignalQualityPrecisionContracts.cs" in csproj,
    "precision rules and tests must be part of the contract project",
)

if errors:
    print("CFIP SIGNAL QUALITY PRECISION AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CFIP SIGNAL QUALITY PRECISION AUDIT: PASS")
