#!/usr/bin/env python3
"""Static acceptance gate for CR3.2 decision-gate and early-prediction semantics."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

smart_gates = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionSmartGates.cs")
reward_rule = read("src/CFIP.Indicator/Core/Math/RewardQualityFloorRule.cs")
early = read("src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs")
early_rule = read("src/CFIP.Indicator/Core/Math/EarlyPredictionScoreRule.cs")
prediction = read("src/CFIP.Indicator/Core/Models/Prediction.cs")
panel = read("src/CFIP.Indicator/UI/Panel/Rows/PanelDecisionRowsRenderer.cs")
readiness = read("src/CFIP.Indicator/UI/Panel/PanelPredictionState.cs")
alerts = read("src/CFIP.Indicator/Trading/Alerts/ContextAlertEmitter.cs")
params = read("src/CFIP.Indicator/Indicator/Parameters/16_accuracy.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")

checks = {
    "reward quality rule lives in platform-neutral Core math": (
        "RewardQualityFloorRule" in reward_rule and
        (ROOT / "src/CFIP.Indicator/Core/Math/RewardQualityFloorRule.cs").exists()
    ),
    "smart gate consumes the canonical reward-quality rule": (
        "RewardQualityFloorRule.Calculate(" in smart_gates and
        "EXPECTED VALUE" not in smart_gates and
        "ProxyExpectedValue(" not in smart_gates
    ),
    "obsolete proxy implementation is removed": (
        not (ROOT / "src/CFIP.Indicator/Trading/Intelligence/ProxyExpectedValueCalculator.cs").exists()
    ),
    "legacy public property names remain while labels describe reward quality": (
        "public bool UseProxyExpectedValueGate" in params and
        'Parameter("Use Reward Quality Gate"' in params and
        "public double MinimumProxyExpectedValue" in params and
        'Parameter("Minimum Reward Quality Floor"' in params
    ),
    "early prediction scoring has one canonical math owner": (
        "MtfEarlyPredictionFusionRule.Evaluate(" in early and
        "internal static EarlyPredictionScoreResult Finalize(" in early_rule and
        "M5Weight = 0.55" in early_rule and
        "M15Weight = 0.45" in early_rule and
        "LiquidityForecastBonus = 8.0" in early_rule and
        "EarlyPredictionScoreRule.Finalize(" in read("src/CFIP.Indicator/Core/Math/MtfEarlyPredictionFusionRule.cs") and
        "Math.Max(buy, sell)" not in read("src/CFIP.Indicator/Core/Math/MtfEarlyPredictionFusionRule.cs")
    ),
    "early prediction exposes directional share and absolute strength": (
        "DirectionalShare" in prediction and
        "AbsoluteStrength" in prediction and
        "p.DirectionalShare" in early and
        "p.AbsoluteStrength" in early
    ),
    "early prediction requires absolute strength in addition to share": (
        "minimumAbsoluteStrength" in early and
        "p.AbsoluteStrength <" in early
    ),
    "panel labels early prediction using share and strength": (
        '"  •  SHARE " +' in panel and
        '"  •  STRENGTH " +' in panel and
        '" PREDICTED • SHARE " +' in readiness and
        '" • STR " +' in readiness
    ),
    "early setup alert requires both share and absolute strength": (
        "_prediction.DirectionalShare >=" in alerts and
        "_prediction.AbsoluteStrength >=" in alerts
    ),
    "runtime contracts compile both new Core rules": (
        "RewardQualityFloorRule.cs" in runtime_project and
        "EarlyPredictionScoreRule.cs" in runtime_project
    ),
    "runtime contracts exercise CR3.2 low-total/high-ratio behavior": (
        "VerifyRewardQualityFloorSemantics();" in contracts and
        "VerifyEarlyPredictionScoreSemantics();" in contracts and
        "tiny absolute evidence cannot hide" in contracts
    ),
    "workflow runs the CR3.2 static gate": (
        "python tools/audit_phase_3_2.py" in workflow
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# Production code must not retain the old executable symbol.
old_symbol_hits = []
for path in (ROOT / "src/CFIP.Indicator").rglob("*.cs"):
    text = path.read_text(encoding="utf-8")
    if "ProxyExpectedValue(" in text:
        old_symbol_hits.append(str(path.relative_to(ROOT)))

if old_symbol_hits:
    errors.append("old ProxyExpectedValue method call remains in production")
    print(
        "FAIL | old ProxyExpectedValue method call remains: " +
        ", ".join(old_symbol_hits)
    )

print("CR3.2 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR3.2 STATIC GATE PASS")
