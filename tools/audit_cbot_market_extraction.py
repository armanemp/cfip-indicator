#!/usr/bin/env python3
"""CBOT-P4A market/mixed-range ownership audit."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
INDICATOR = ROOT / "src" / "CFIP.Indicator"
CBOT = ROOT / "src" / "CFIP.cBot"
CONTRACTS = ROOT / "src" / "CFIP.Contracts"

errors = []

def read(rel):
    path = ROOT / rel
    if not path.exists():
        errors.append(f"missing: {rel}")
        return ""
    return path.read_text(encoding="utf-8")

provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
provider_refresh = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
market_exec = read("src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketBrokerExecution.cs")
market_trade = read("src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketTradeExecution.cs")
range_calc = read("src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketRangeCalculator.cs")
profile = read("src/CFIP.Contracts/MarketExecutionProfile.cs")
intent_contract = read("src/CFIP.Contracts/ExecutionIntent.cs")
mutation = read("src/CFIP.cBot/Execution/MarketBrokerMutation.cs")
coordinator = read("src/CFIP.cBot/Execution/MarketExecutionCoordinator.cs")
rule = read("src/CFIP.cBot/Execution/MarketExecutionIntentRule.cs")
cbot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
shadow = read("src/CFIP.cBot/Shadow/ShadowHostValidator.cs")

if "class CFIPIndicator : Indicator" not in market_exec:
    errors.append("automatic market host seam missing")
if "CFIP cBot handoff" not in market_exec:
    errors.append("automatic market path missing explicit cBot handoff marker")
if re.search(r"(?:ExecuteMarketOrder|ExecuteMarketRangeOrder)s*(", market_exec):
    errors.append("automatic market orchestration still contains broker mutation")
if re.search(r"(?:ExecuteMarketOrder|ExecuteMarketRangeOrder)s*(", market_trade):
    errors.append("automatic market trigger still contains broker mutation")
if re.search(r"(?:ExecuteMarketOrder|ExecuteMarketRangeOrder)s*(", range_calc):
    errors.append("automatic market range calculator still contains broker mutation")
if (INDICATOR / "Trading/Execution/BrokerMarketOrderMutation.cs").exists():
    errors.append("legacy Indicator market mutation owner still exists")
if not re.search(r"ExecuteMarketOrders*(", mutation):
    errors.append("cBot market mutation owner missing ExecuteMarketOrder")
if not re.search(r"ExecuteMarketRangeOrders*(", mutation):
    errors.append("cBot market mutation owner missing ExecuteMarketRangeOrder")
if "new MarketExecutionProfile(" not in provider:
    errors.append("provider does not project canonical MarketExecutionProfile")
if "MarketProfile" not in intent_contract:
    errors.append("ExecutionIntent contract missing MarketProfile")
if "ExecutionLabel" not in intent_contract:
    errors.append("ExecutionIntent contract missing broker execution label")
if "MarketRangePips" not in provider_refresh:
    errors.append("provider fingerprint does not carry market-range geometry")
if "UseServerTakeProfitLadder" not in profile:
    errors.append("MarketExecutionProfile missing ladder transport")
if "MarketExecutionIntentRule.Validate(" not in coordinator:
    errors.append("cBot market coordinator missing canonical intent validation")
if "EnableMarketExecution" not in cbot:
    errors.append("cBot market execution arming control missing")
if "DefaultValue = false" not in cbot:
    errors.append("cBot market mutation is not fail-closed by default")
if "CBOT cBot handoff" not in market_exec:
    errors.append("Indicator handoff marker missing")
if "ExecuteMarketOrder" in shadow:
    errors.append("shadow validator must not own broker mutation")
if not Path("tools/CFIP.cBot.Shadow.Tests/Program.cs").exists():
    errors.append("shadow behavioral fixture missing")

if errors:
    print("CBOT-P4A MARKET EXTRACTION AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CBOT-P4A MARKET EXTRACTION AUDIT: PASS")
print("Indicator market mutation: 0")
print("cBot market mutation owner: active")
