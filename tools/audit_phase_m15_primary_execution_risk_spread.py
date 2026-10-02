#!/usr/bin/env python3
"""M15 primary execution, defensive LTF and spread/margin risk audit."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"
BOT = ROOT / "src" / "CFIP.cBot"

errors = []

policy = (IND / "Core" / "Math" / "ExecutionTimeframePolicy.cs").read_text(encoding="utf-8")
action = (IND / "Trading" / "Validation" / "TradeActionabilityEvaluator.cs").read_text(encoding="utf-8")
rr = (IND / "Core" / "Math" / "RiskRewardMathRule.cs").read_text(encoding="utf-8")
target = (IND / "Planning" / "TradePlan" / "TargetStageSelector.cs").read_text(encoding="utf-8")
bot = (BOT / "CFIPExecutionBot.cs").read_text(encoding="utf-8")
coord = (BOT / "Execution" / "DemoMarketExecutionCoordinator.cs").read_text(encoding="utf-8")
margin = (BOT / "Risk" / "ExecutionMarginBudgetRule.cs").read_text(encoding="utf-8")

if "PrimaryExecution = \"M15\"" not in policy:
    errors.append("M15 primary execution policy missing")
if "LowerDefensiveM5 = \"M5\"" not in policy or "LowerDefensiveM1 = \"M1\"" not in policy:
    errors.append("M5/M1 defensive role missing")
if "IsHigherContext" not in policy or "H1" not in policy or "H4" not in policy:
    errors.append("H1+ higher-context role missing")
if "M15 EXECUTION FRAME UNAVAILABLE" not in action or "M15 EXECUTION DIRECTION CONFLICT" not in action:
    errors.append("canonical actionability must enforce M15 execution alignment")
if "double netReward" not in rr:
    errors.append("effective reward must subtract spread")
if "public double NetReward" not in rr:
    errors.append("RiskRewardMathResult must expose spread-adjusted net reward")
if "TargetFromRR(" not in target or "IncludeSpreadInRiskSizing" not in target:
    errors.append("synthetic target path must consume spread-aware target geometry")
if "DefaultTimeFrame = \"M15\"" not in bot:
    errors.append("cBot default timeframe must be M15")
if "Bars.TimeFrame != TimeFrame.Minute15" not in bot:
    errors.append("cBot must fail closed outside M15")
if "ExecutionMarginBudgetRule.AllowedMargin" not in coord:
    errors.append("cBot margin budget owner missing")
if "ScaleVolumeToBudget" not in coord or "NormalizeVolumeInUnits" not in coord:
    errors.append("broker-side final volume cap missing")
if "Execution Margin Usage" not in bot or "Execution Margin Buffer" not in bot:
    errors.append("cBot execution margin safety settings missing")
if "IncludeSpreadInRiskSizing" not in (IND / "Indicator" / "Parameters" / "13_auto_trading.cs").read_text(encoding="utf-8"):
    errors.append("existing spread-aware sizing switch missing")

if errors:
    print("M15 PRIMARY / RISK / SPREAD AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("M15 PRIMARY / RISK / SPREAD AUDIT: PASS")
print("Primary execution timeframe: M15")
print("M5/M1 role: defensive tuning")
print("H1+ role: higher-timeframe context / reward path")
print("Effective RR includes spread on risk and reward sides: PASS")
print("cBot final margin cap: PASS")