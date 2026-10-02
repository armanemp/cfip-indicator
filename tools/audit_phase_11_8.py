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

for path, label in (
    (pending_stop, "pending stop"),
    (pending_limit, "pending limit"),
):
    if "PendingOrderLabel()" not in path:
        errors.append(label + " submission is not instance-scoped through PendingOrderLabel()")

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
