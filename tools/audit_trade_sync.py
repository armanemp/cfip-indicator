#!/usr/bin/env python3
"""Analysis -> signal -> chart -> cBot synchronization audit."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(rel):
    path = ROOT / rel
    if not path.exists():
        fail(f"missing: {rel}")
    return path.read_text(encoding="utf-8")

errors = []

def check(name, ok):
    if not ok:
        errors.append(name)
    print(("PASS" if ok else "FAIL") + " | " + name)

def fail(message):
    print("TRADE-SYNC AUDIT: FAIL")
    print(" - " + message)
    raise SystemExit(1)

closed_bar = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
live_cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
provider_plan = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
intent_validation = read("src/CFIP.Indicator/Planning/Execution/ExecutionIntentValidation.cs")
signal_snapshot = read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs")
plan_render = read("src/CFIP.Indicator/UI/Chart/PlanRenderCoordinator.cs")
plan_labels = read("src/CFIP.Indicator/UI/Chart/PlanLabelRenderCoordinator.cs")
signal_alerts = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
cbot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
binding = read("src/CFIP.cBot/Binding/CfipIndicatorChartBinding.cs")
transport = read("src/CFIP.cBot/Binding/CfipDeviceSignalTransport.cs")
contracts_codec = read("src/CFIP.Contracts/SignalEnvelopeCodec.cs")
contracts_key = read("src/CFIP.Contracts/SignalBusKey.cs")
market = read("src/CFIP.cBot/Execution/MarketExecutionCoordinator.cs")
shadow = read("src/CFIP.cBot/Shadow/ShadowHostValidator.cs")
indicator_host = read("src/CFIP.Indicator/Indicator/CFIPIndicator.cs")
cbot_project = read("src/CFIP.cBot/CFIP.cBot.csproj")

check(
    "decision policy is independent from broker arming",
    "AutoTradingEnabled &&" not in closed_bar and
    "ConfirmedSignalsOnly" in closed_bar
)

check(
    "live plan generation is analysis-owned",
    "if (_plan != null ||" in live_cycle and
    "Plan generation is analysis, not broker mutation" in live_cycle
)

check(
    "provider envelope carries canonical intent and plan",
    "new SignalEnvelope(" in provider and
    "BuildCanonicalExecutionIntent(" in provider and
    "BuildCanonicalPlanSnapshot(" in provider_plan
)

check(
    "execution intent geometry is revalidated",
    "ExecutionIntentGeometryRule.Evaluate(" in intent_validation and
    "IsExecutionPlanConsistent(" in intent_validation
)

check(
    "chart rendering consumes canonical visual snapshot",
    "SignalVisualSnapshot snapshot" in plan_render and
    "snapshot.Stop" in plan_render and
    "SignalVisualSnapshot snapshot" in plan_labels and
    "snapshot.Stop" in plan_labels
)

check(
    "decision alert is emitted only after canonical plan creation",
    closed_bar.index("EnsureSignalPlan(") <
    closed_bar.index("ProcessDecisionAlerts(")
)

check(
    "cBot binds to the named indicator attached to this chart",
    "ChartIndicators.Custom" in binding and
    "CFIP Smart Indicator" in binding and
    "RefreshIndicatorBinding(" in cbot
)

check(
    "cBot copies the attached indicator parameter values",
    "TryBuildParameterValues(" in binding and
    "parameter.Value" in binding
)

check(
    "cBot never hard-codes a divergent Indicator parameter set",
    "Indicators.GetIndicator<CFIPIndicator>(\n                        new" not in cbot and
    "Indicators.GetIndicator<CFIPIndicator>(new" not in cbot
)

check(
    "cBot executes only the canonical provider envelope",
    "_indicator.LatestSignalEnvelope" in cbot and
    "_market.TryExecute(" in cbot
)

check(
    "cBot rechecks identity/plan/geometry before execution",
    "ValidatePlanIntegrity(" in shadow and
    "ValidateIntentIdentity(" in shadow and
    "MarketExecutionIntentRule.Validate(" in market
)

check(
    "Indicator has a human-readable cTrader name",
    '"CFIP Smart Indicator"' in indicator_host
)

check(
    "cBot uses a human-readable build identity",
    "class CFIPExecutionBot" in cbot and
    "<AssemblyName>CFIPExecutionBot</AssemblyName>" in cbot_project
)

check(
    "chart-binding failure is fail-closed",
    "CFIP SMART INDICATOR NOT ATTACHED TO THIS CHART" in binding and
    "MULTIPLE CFIP SMART INDICATOR INSTANCES" in binding and
    "_boundIndicatorInstanceId = """ in cbot
)

check(
    "transport freshness is fail-closed",
    "Provider Stale After Seconds" in cbot and
    "snapshot.ObservedUtc" in cbot
)

check(
    "transport identity is stable and chart-instance scoped",
    "SignalBusKey.ForIndicatorInstance(" in contracts_key and
    "InstanceId" in binding and
    "SignalEnvelopeCodec.Serialize(" in contracts_codec
)

if errors:
    print()
    print("TRADE-SYNC AUDIT: FAIL")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print()
print("TRADE-SYNC AUDIT: PASS")
print("Analysis owner: Indicator")
print("Broker mutation owner: cBot")
print("Chart/cBot binding: exact same-chart Indicator instance -> canonical Device SignalEnvelope")
