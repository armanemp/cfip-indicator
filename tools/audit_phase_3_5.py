#!/usr/bin/env python3
"""Static acceptance gate for CR3.5 calibration, outcome and rejection transparency."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing required CR3.5 file: {relative}")
    return path.read_text(encoding="utf-8")

key = read("src/CFIP.Indicator/Analysis/Market/Decision/EmpiricalConfidenceCalibrator.cs")
collector = read("src/CFIP.Indicator/Analysis/Market/Decision/ConfidenceCalibrationCollector.cs")
observation = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeObservation.cs")
outcome = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs")
validator = read("src/CFIP.Indicator/Planning/TradePlan/PlanRewardIntegrityValidator.cs")
decision = read("src/CFIP.Indicator/Core/Models/Decision.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelCalibrationRowsRenderer.cs")
contracts = read("tools/CFIP.Decision.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")

errors = []

checks = {
    "calibration key implements structural equality": (
        "IEquatable<ConfidenceCalibrationKey>" in key and
        "bool Equals(ConfidenceCalibrationKey other)" in key
    ),
    "calibration key hash is consistent with equality": (
        "StringComparer.Ordinal.GetHashCode(Regime)" in key and
        "string.Equals(" in key
    ),
    "outcome observation stores realized R": (
        "public double RealizedR { get; set; }" in observation
    ),
    "realized R comes from aggregated broker outcome": (
        "HistoricalOutcomeAggregate aggregate" in outcome and
        "aggregate.NetProfit" in outcome and
        "realizedNetProfit /" in outcome
    ),
    "calibration snapshot exposes sample count and realized R": (
        "public int Samples" in key and
        "public double AverageRealizedR" in key
    ),
    "recent calibration aggregates realized R by selected context": (
        "CalculateAverageRealizedR(" in key and
        "observation.RealizedR" in key
    ),
    "decision carries realized-R calibration evidence": (
        "EmpiricalCalibrationAverageRealizedR" in decision and
        "snapshot.AverageRealizedR" in collector
    ),
    "panel exposes observed win rate, realized R and sample count": (
        "OBS WIN" in panel and
        "AVG R" in panel and
        "EmpiricalCalibrationSamples" in panel
    ),
    "plan reward rejection reasons are explicitly recorded": (
        "RejectPlanRewardStructure(" in validator and
        '"PLAN_REWARD"' in validator and
        '"REJECTED"' in validator
    ),
    "rejection telemetry is bounded against same-bar duplicates": (
        "_lastPlanRewardRejectionM5" in validator and
        "_lastPlanRewardRejectionReason" in validator
    ),
    "decision contracts cover structural calibration key equality": (
        "VerifyConfidenceCalibrationKeyEquality();" in contracts
    ),
    "decision contracts cover realized-R calibration evidence": (
        "AverageRealizedR == 1.5" in contracts
    ),
    "CR3.5 static gate is wired into CI": (
        "python tools/audit_phase_3_5.py" in workflow
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# Calibration must remain observational: no threshold/default mutation is allowed here.
if "DefaultValue" in key or "Tp1MinimumRR =" in validator:
    print("FAIL | CR3.5 contains public threshold/default mutation")
    errors.append("CR3.5 contains public threshold/default mutation")

# The reward validator may report why a plan failed, but must not become a second decision engine.
if "Calculate(" in validator or "DecisionEvaluator" in validator:
    print("FAIL | reward validator appears to contain a second decision authority")
    errors.append("reward validator appears to contain a second decision authority")

print("CR3.5 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR3.5 STATIC GATE PASS")
