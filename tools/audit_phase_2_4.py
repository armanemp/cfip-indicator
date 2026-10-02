#!/usr/bin/env python3
"""Static acceptance gate for CR2.4 pending-order arbitration."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

arbiter = read("src/CFIP.Indicator/Core/Math/PendingDecisionArbiterRule.cs")
orchestrator = read("src/CFIP.Indicator/Trading/Pending/Placement/SmartPendingOrderOrchestrator.cs")
cleanup = read("src/CFIP.Indicator/Trading/Pending/Placement/PendingOrderCleanup.cs")
limit = read("src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPreparation.cs")
stop = read("src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPreparation.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
contracts_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

required = {
    "canonical pending arbiter exists": (
        "internal static class PendingDecisionArbiterRule" in arbiter and
        "SelectWinner(" in arbiter and
        "ScoreCandidate(" in arbiter
    ),
    "one explicit winner when both eligible": (
        "REVERSAL LIMIT WINS BY QUALITY" in arbiter and
        "CONTINUATION STOP WINS TIE" in arbiter
    ),
    "mode is part of arbitration": "AllowsContinuation(mode)" in arbiter and "AllowsReversal(mode)" in arbiter,
    "existing order maps to one choice": "IsSameChoice(" in arbiter,
    "cancellation hysteresis is canonical": "ShouldCancelAfterHysteresis(" in arbiter,
    "orchestrator uses arbiter": "ResolvePendingDecision(" in orchestrator and "PendingDecisionArbiterRule.SelectWinner(" in orchestrator,
    "orchestrator prevents silent fallback": (
        "if (!arbiter.HasChoice)" in orchestrator and
        "PENDING STOP • READY FOR CBOT" in orchestrator and
        "PENDING LIMIT • READY FOR CBOT" in orchestrator and
        "PENDING EXECUTION BLOCKED" in orchestrator and
        "return;" in orchestrator
    ),
    "cleanup uses arbiter": "ResolvePendingDecision(" in cleanup and "ObservePendingInvalidation(" in cleanup,
    "limit uses executable side": "Symbol.Ask" in limit and "Symbol.Bid" in limit,
    "stop fallback has explicit boundaries": "fallbackStart" in stop and "fallbackEnd" in stop and "INVALID LOOKBACK" in stop,
    "runtime arbiter contracts invoked": "VerifyPendingDecisionArbiterSemantics();" in contracts,
    "runtime project links arbiter": "PendingDecisionArbiterRule.cs" in contracts_project,
}

for name, ok in required.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# The old independent continuation-first branch must not return.
legacy = (
    "if (continuationStrong &&" in orchestrator and
    "if (reversalStrong &&" in orchestrator
)
if legacy:
    errors.append("legacy independent continuation/reversal placement branches remain in orchestrator")
    print(f"FAIL | {errors[-1]}")

# Cleanup must not cancel a live pending order after a single transient mismatch.
if "ObservePendingInvalidation(" not in cleanup:
    errors.append("pending cleanup does not route invalidation through its canonical hysteresis observer")
    print(f"FAIL | {errors[-1]}")

print("CR2.4 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.4 STATIC GATE PASS")
