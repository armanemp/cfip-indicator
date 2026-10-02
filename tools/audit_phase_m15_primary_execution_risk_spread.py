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
safety = (BOT / "Execution" / "BrokerExecutionSafety.cs").read_text(encoding="utf-8")

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
if "Bars.TimeFrame != TimeFrame.Minute15" in bot:
    errors.append("cBot must not bind execution to host chart timeframe")
if "Bars.TimeFrame" in action:
    errors.append("Indicator actionability must not read host chart timeframe")
pending_indicator = (IND / "Trading" / "Pending" / "Placement" / "ContinuationStopPlacement.cs").read_text(encoding="utf-8")
if "PrepareContinuationStopForCbot(" not in pending_indicator:
    errors.append("Indicator Pending Stop path must prepare a cBot intent")
if "PlaceStopOrder(" in pending_indicator:
    errors.append("Indicator Pending Stop placement mutation remains")
if (IND / "Trading" / "Execution" / "BrokerPendingOrderPlacement.cs").exists():
    errors.append("migrated Indicator Pending Stop broker owner still exists")
pending = (BOT / "Execution" / "DemoPendingOrderExecutionCoordinator.cs").read_text(encoding="utf-8")
if "class DemoPendingOrderExecutionCoordinator" not in pending or "PlaceStopOrder(" not in pending:
    errors.append("cBot Pending Stop mutation owner missing")
if "ExecutionAction.PendingStop" not in bot:
    errors.append("cBot Pending Stop action routing missing")
if "EnableDemoPendingStopExecution" not in bot:
    errors.append("cBot Pending Stop arm missing")
if "DefaultTimeFrame = \"M15\"" not in bot:
    errors.append("cBot default timeframe must be M15")
if "class ExecutionMarginBudgetRule" not in margin or "AllowedMargin" not in margin:
    errors.append("cBot margin budget owner missing")
if ("BrokerExecutionSafety.TryConstrainVolumeForMargin(" not in coord or
        "BrokerExecutionSafety.CountManagedPositions(" not in coord or
        "BrokerExecutionSafety.CountManagedPending(" not in coord or
        "NormalizeVolumeInUnits" not in safety):
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
