#!/usr/bin/env python3
"""CBOT-P3 static gate for the deterministic shadow host under demo execution handoff."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
CBOT = ROOT / "src" / "CFIP.cBot"
SHADOW = CBOT / "Shadow"
HOST = CBOT / "CFIPExecutionBot.cs"
TEST = ROOT / "tools" / "CFIP.cBot.Shadow.Tests"

MUTATIONS = (
    "ExecuteMarketOrder(",
    "ExecuteMarketRangeOrder(",
    "PlaceStopOrder(",
    "PlaceLimitOrder(",
    "ModifyPosition(",
    "ModifyPendingOrder(",
    "ClosePosition(",
    "CancelPendingOrder(",
)

errors = []

def read(path):
    if not path.exists():
        errors.append("missing: " + path.relative_to(ROOT).as_posix())
        return ""
    return path.read_text(encoding="utf-8")

host = read(HOST)
shadow_files = sorted(SHADOW.glob("*.cs"))
shadow = "\n".join(read(p) for p in shadow_files)

required_shadow = {
    "ShadowHostContracts.cs",
    "ShadowHostValidator.cs",
    "ShadowHostCoordinator.cs",
}
actual_shadow = {p.name for p in shadow_files}

def check(name, ok):
    print(("PASS" if ok else "FAIL") + " | " + name)
    if not ok:
        errors.append(name)

check(
    "shadow host production owners are complete",
    required_shadow.issubset(actual_shadow),
)

check(
    "cBot owns the shadow coordinator",
    "private readonly ShadowHostCoordinator _shadow" in host and
    "_shadow.Observe(" in host,
)

check(
    "host uses broker state for safety preconditions",
    "ReadBrokerSnapshot()" in host and
    "Positions" in host and
    "PendingOrders" in host,
)

check(
    "single-plan capacity is explicit",
    'const string ManagedLabel = "CFIP-SMART"' in shadow and
    'const string PendingManagedLabel = "CFIP-SMART-PENDING"' in shadow and
    re.search(
        r"ManagedPositionCount\s*\+\s*broker\.ManagedPendingOrderCount\s*>=\s*1",
        shadow,
    ) is not None,
)

check(
    "revision and idempotency semantics exist",
    "STALE REVISION" in shadow and
    "REVISION CONFLICT" in shadow and
    "DUPLICATE IDEMPOTENCY KEY" in shadow and
    "_seenIdempotencyKeys" in shadow,
)

check(
    "seen idempotency cache is bounded",
    "MaxSeenIdempotencyKeys = 128" in shadow and
    "while (_seenOrder.Count > MaxSeenIdempotencyKeys)" in shadow,
)

check(
    "expired and blocked provider states are distinct",
    "SignalStage.Expired" in shadow and
    "ShadowHostState.Expired" in shadow and
    "SignalStage.Blocked" in shadow and
    "ShadowHostState.Blocked" in shadow,
)

check(
    "BUY/SELL contract geometry is validated symmetrically",
    "BUY EXECUTION GEOMETRY WRONG SIDE" in shadow and
    "SELL EXECUTION GEOMETRY WRONG SIDE" in shadow and
    "BUY PLAN STOP WRONG SIDE" in shadow and
    "SELL PLAN STOP WRONG SIDE" in shadow,
)

check(
    "transported envelope revision is the execution revision",
    "identity.Revision" in shadow and
    "envelope.Identity.Revision" in host and
    "ContractVersion.Current" in host,
)

check(
    "dynamic broker safety can be rechecked without consuming a ScenarioId revision",
    "RevalidateBrokerSafety(" in shadow and
    "SINGLE-PLAN CAPACITY BLOCKED" in shadow and
    "_lastResultByScenario" in shadow and
    "_lastBrokerRecheckUtcByScenario" in shadow and
    "GetLastBrokerRecheck(scenarioKey)" in shadow and
    "SetScenarioBrokerRecheck(" in shadow,
)

check(
    "all cBot host/shadow sources contain zero direct broker mutation",
    all(token not in host + shadow for token in MUTATIONS),
)

check(
    "host remains free of unsupported reflection/network/chart scraping",
    all(token not in host + shadow for token in (
        "System.Reflection",
        "GetType(",
        "Invoke(",
        "ChartObjects",
        "Chart.Draw",
        "HttpClient",
        "WebSocket",
    )),
)

check(
    "host launch is M5 and execution remains chart-timeframe independent",
    "Bars.TimeFrame != TimeFrame.Minute15" not in host and
    'DefaultTimeFrame = "M5"' in host,
)

check(
    "Pending Stop action is routed through the host",
    "ExecutionAction.PendingStop" in host and
    "EnableDemoPendingStopExecution" in host and
    "DemoPendingOrderExecutionCoordinator" in host,
)

check(
    "demo market execution is explicit and fail-closed",
    "EnableDemoMarketExecution" in host and
    "DefaultValue = false" in host and
    "Account.IsLive" in host and
    "DemoMarketExecutionCoordinator" in host and
    "_market.TryExecute(" in host,
)

check(
    "behavioral test project exists",
    (TEST / "Program.cs").exists() and
    (TEST / "CFIP.cBot.Shadow.Tests.csproj").exists(),
)

test_text = read(TEST / "Program.cs")
check(
    "behavioral fixtures cover revision/expiry/capacity/BUY-SELL seams",
    "RevisionRules();" in test_text and
    "Expiry();" in test_text and
    "Capacity();" in test_text and
    "WrongSide();" in test_text and
    "CoordinatorRecheck();" in test_text and
    "ProviderRevisionMismatch();" in test_text,
)

print("CBOT-P3 SHADOW HOST SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("CBOT-P3 STATIC SHADOW HOST GATE PASS")
