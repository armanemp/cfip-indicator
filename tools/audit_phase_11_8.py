#!/usr/bin/env python3
"""Static/source contract audit for Claude Review remediation CR1.8 / A11."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
IDENTITY_RULE = ROOT / "src/CFIP.Indicator/Core/Math/ManagedIdentityRule.cs"
IDENTITY = ROOT / "src/CFIP.Indicator/Trading/Identity/BrokerIdentity.cs"
LABELS = ROOT / "src/CFIP.Indicator/Trading/Execution/State/TradeLabelFormatter.cs"
AUTO_MARKET = ROOT / "src/CFIP.Indicator/Trading/Execution/AutomaticMarket/AutomaticMarketBrokerExecution.cs"
AGGRESSIVE = ROOT / "src/CFIP.Indicator/Trading/Execution/Aggressive/AggressiveBrokerExecution.cs"
PENDING_STOP = ROOT / "src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs"
PENDING_STOP_CBOT = ROOT / "src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs"
PROVIDER_PLAN = ROOT / "src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs"
CBOT_HOST = ROOT / "src/CFIP.cBot/CFIPExecutionBot.cs"
PENDING_LIMIT = ROOT / "src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs"
REVERSAL = ROOT / "src/CFIP.Indicator/Trading/LiveManagement/ReversalCloseGuard.cs"
PARAMS = ROOT / "src/CFIP.Indicator/Indicator/Parameters/13_auto_trading.cs"
RUNTIME_PROJECT = ROOT / "tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj"
RUNTIME = ROOT / "tools/CFIP.Runtime.Contracts/Program.cs"

errors = []

def read(path: Path) -> str:
    if not path.exists():
        errors.append(f"missing file: {path}")
        return ""
    return path.read_text(encoding="utf-8")

def read_optional(path: Path) -> str:
    return path.read_text(encoding="utf-8") if path.exists() else ""

identity_rule = read(IDENTITY_RULE)
identity = read(IDENTITY)
labels = read(LABELS)
auto_market = read_optional(AUTO_MARKET)
aggressive = read_optional(AGGRESSIVE)
pending_stop = read(PENDING_STOP)
pending_stop_cbot = read(PENDING_STOP_CBOT)
provider_plan = read(PROVIDER_PLAN)
cbot_host = read(CBOT_HOST)
pending_limit = read(PENDING_LIMIT)
reversal = read(REVERSAL)
params = read(PARAMS)
runtime_project = read(RUNTIME_PROJECT)
runtime = read(RUNTIME)

for token in (
    'InstanceMarker = "|CFIP-I:"',
    "TryBuildLabel(",
    "string baseLabel",
    "string instanceId",
):
    if token not in identity_rule:
        errors.append("managed identity rule missing: " + token)

for token in (
    "InstanceId",
    "ManagedExecutionLabel()",
    "IsManagedPosition(Position position)",
    "string.Equals(",
):
    if token not in identity:
        errors.append("broker identity boundary missing: " + token)

if "!ManagedActionsOnly" not in identity:
    errors.append("ManagedActionsOnly compatibility boundary is no longer explicit")

if "string.IsNullOrWhiteSpace(managedLabel)" not in identity:
    errors.append("managed position lookup must fail closed on unavailable instance identity")

TELEMETRY = ROOT / "src/CFIP.Indicator/Trading/Execution/SubmissionGateCoordinator.cs"
telemetry = read(TELEMETRY)
if "InstanceId" not in telemetry or "INSTANCE=" not in telemetry:
    errors.append("execution telemetry must persist the managed instance identity")

if 'NormalizeLabel() + "-PENDING"' in identity:
    errors.append("pending orders must not use base label without instance identity")

for path, label in (
    (auto_market, "automatic market"),
    (aggressive, "aggressive market"),
):
    if not path:
        continue
    if "ManagedExecutionLabel()" not in path:
        errors.append(label + " broker submission is not instance-scoped")
    if "NormalizeLabel()" in path:
        errors.append(label + " broker submission still contains an unscoped NormalizeLabel()")

cbot = read_optional(
    ROOT / "src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs"
)
if "ExecuteMarketOrder(" not in cbot or "ExecuteMarketRangeOrder(" not in cbot:
    errors.append("cBot Market owner must expose both Market and Market-Range submission")

if (
    "CapturePendingOrderPlanSnapshot(" not in pending_stop or
    "PlaceStopOrder(" in pending_stop
):
    errors.append(
        "pending stop Indicator path must remain intent/snapshot-only after P4C"
    )

if (
    "ManagedExecutionLabel()" not in provider_plan or
    "string executionLabel" not in provider_plan or
    "executionLabel," not in provider_plan
):
    errors.append(
        "provider must transport the canonical instance-scoped execution label"
    )

if (
    "envelope.Intent.ExecutionLabel" not in pending_stop_cbot or
    'string label = executionLabel + "-PENDING";' not in pending_stop_cbot or
    "MISSING EXECUTION LABEL" not in pending_stop_cbot
):
    errors.append(
        "pending stop cBot must consume the transported instance-scoped execution label"
    )

if "envelope.Intent.ExecutionLabel" not in cbot_host:
    errors.append(
        "cBot host must use the transported execution label for broker-state ownership"
    )

if (
    "PlaceLimitOrder(" not in pending_stop_cbot or
    "ExecutionAction.PendingLimit" not in pending_stop_cbot or
    "BrokerAction.SubmitPendingLimit" not in pending_stop_cbot
):
    errors.append(
        "pending limit cBot broker owner is missing"
    )

if "PlaceLimitOrder(" in pending_limit:
    errors.append(
        "pending limit Indicator broker mutation must remain removed"
    )

if "PrepareReversalLimitForCbot(" not in pending_limit:
    errors.append(
        "pending limit Indicator must expose the canonical intent-only handoff"
    )

if "ManagedExecutionLabel()" not in labels or "InstanceId" not in labels:
    errors.append("canonical managed execution label formatter is missing InstanceId binding")

if "ReversalProfitThresholdRule.MeetsMinimumNetProfit(" not in reversal:
    errors.append("reversal close guard does not consume the canonical profit threshold rule")

if "ReversalCloseMinimumNetProfit" not in params:
    errors.append("reversal close minimum net profit parameter is missing")

if not re.search(
    r'Parameter\("Reversal Close Minimum Net Profit"[^\]]*DefaultValue\s*=\s*0\.0[^\]]*MinValue\s*=\s*0',
    params,
):
    errors.append("reversal close profit parameter must default to 0 and reject negative values")

if "ManagedIdentityRule.cs" not in runtime_project:
    errors.append("runtime contracts project does not include managed identity rule")

if "ReversalProfitThresholdRule.cs" not in runtime_project:
    errors.append("runtime contracts project does not include reversal profit threshold rule")

for token in (
    "VerifyManagedIdentitySemantics()",
    "VerifyReversalProfitThresholdSemantics()",
):
    if token not in runtime:
        errors.append("runtime contracts missing CR1.8 regression test: " + token)

if errors:
    print("CR1.8 / A11 audit FAILED")
    for error in errors:
        print(" - " + error)
    sys.exit(1)

print("CR1.8 / A11 audit PASS")
