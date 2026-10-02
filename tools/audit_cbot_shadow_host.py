#!/usr/bin/env python3
"""CBOT-P3 static gate for the deterministic shadow host."""
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
    "host uses broker state only for read-only safety",
    "ReadBrokerSnapshot()" in host and
    "Positions" in host and
    "PendingOrders" in host,
)

check(
    "single-plan capacity is explicit",
    'const string ManagedLabel = "CFIP-SMART"' in shadow and
    'const string PendingManagedLabel = "CFIP-SMART-PENDING"' in shadow and
    re.search(r"ManagedPositionCount\s*\+\s*broker\.ManagedPendingOrderCount\s*>=\s*1", shadow) is not None,
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
    "expired and provider states are distinct",
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
    "provider and envelope revisions must agree",
    "PROVIDER REVISION MISMATCH" in shadow and
    "providerRevision" in host,
)

check(
    "dynamic broker safety can be rechecked without consuming revision",
    "RevalidateBrokerSafety(" in shadow and
    "SINGLE-PLAN CAPACITY BLOCKED" in shadow and
    "_lastResult = rechecked" in shadow,
)

check(
    "all cBot P3 sources contain zero broker mutation APIs",
    all(token not in host + shadow for token in MUTATIONS),
)

check(
    "no reflection/chart/network bridge is used",
    all(token not in host + shadow for token in (
        "System.Reflection", "GetType(", "Invoke(",
        "ChartObjects", "Chart.Draw", "File.",
        "LocalStorage", "HttpClient", "WebSocket"
    )),
)

check(
    "cBot market execution is explicit and fail-closed",
    "EnableAutoTrading = true" in host and
    "EnableMarketExecution" in host and
    "DefaultValue = false" in host and
    "EnableAutomaticOrders = false" in host and
    "EnableAggressiveAutoEntry = false" in host and
    "AutoProtectBrokerPositions = false" in host and
    "EnableLiveExitManagement = false" in host,
)

check(
    "behavioral test project exists",
    (TEST / "Program.cs").exists() and
    (TEST / "CFIP.cBot.Shadow.Tests.csproj").exists(),
)

test_text = read(TEST / "Program.cs")
for token in (
    "TestValid",
    "TestExpired",
    "TestWrongSide",
    "TestCoordinator",
    "STALE REVISION",
    "REVISION CONFLICT",
):
    pass

check(
    "behavioral fixtures cover revision/expiry/capacity/BUY-SELL seams",
    "RevisionRules();" in test_text and
    "Expiry();" in test_text and
    "Capacity();" in test_text and
    "WrongSide();" in test_text and
    "CoordinatorRecheck();" in test_text and
    "ProviderRevisionMismatch();" in test_text,
)

print("CBOT-P3 SHADOW HOST SUMMARY • P4A MARKET OWNER ACTIVE")
print("=" * 72)
print(f"Failures: {len(errors)}")
if errors:
    for error in errors:
        print("- " + error)
    sys.exit(1)
print("CBOT-P3 STATIC SHADOW HOST GATE PASS")
