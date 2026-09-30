#!/usr/bin/env python3
"""Static acceptance gate for CR2.5 lifecycle ordering and outcome aggregation."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")

errors = []

guard = read("src/CFIP.Indicator/Trading/Lifecycle/LifecycleEventIdempotencyGuard.cs")
opened = read("src/CFIP.Indicator/Trading/Lifecycle/PositionOpenedHandler.cs")
recovery = read("src/CFIP.Indicator/Trading/Lifecycle/ManagedLivePlanRecovery.cs")
pending_filled = read("src/CFIP.Indicator/Trading/Lifecycle/PendingFilledHandler.cs")
closed = read("src/CFIP.Indicator/Trading/Lifecycle/PositionClosedHandler.cs")
outcome = read("src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs")
aggregate = read("src/CFIP.Indicator/Core/Math/HistoricalOutcomeAggregationRule.cs")
contracts = read("tools/CFIP.Runtime.Contracts/Program.cs")
project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")

checks = {
    "idempotency has bounded storage": (
        "MaxRememberedEvents = 512" in guard and
        "Queue<string>" in guard and
        "Trim()" in guard
    ),
    "duplicate lifecycle events are rejected": (
        "_processedKeys.Add(key)" in guard and
        "if (!_processedKeys.Add(key))" in guard
    ),
    "position-open event tolerates ordering": (
        "if (!boundToActivePlan)" in opened and
        "RecoverManagedLivePlan(" in opened
    ),
    "position-open recovery is managed-only": (
        "IsManagedPosition(args.Position)" in opened and
        "IsManagedPosition(position)" in recovery
    ),
    "pending-filled starts a fresh outcome lifecycle": (
        "_outcomeRegistered = false" in pending_filled
    ),
    "history aggregation owner exists": (
        "HistoricalOutcomeAggregationRule" in aggregate and
        "AggregateHistoricalOutcomeRecords(" in aggregate
    ),
    "close path uses historical aggregation": (
        "History.FindByPositionId(" in outcome and
        "HistoricalTrade[] historicalTrades" in read("src/CFIP.Indicator/Trading/Lifecycle/../Intelligence/HistoricalOutcomeReader.cs")
    ),
    "realized outcome uses aggregated net profit": (
        "aggregate.NetProfit" in outcome and
        "realizedNetProfit" in outcome
    ),
    "realized R uses monetary initial risk": (
        "Symbol.AmountRisked(" in outcome and
        "plan.OriginalVolume" in outcome
    ),
    "win-loss counters use canonical recorded outcome": (
        "OutcomeRegistrationResult outcome" in closed and
        "outcome.Profitable" in closed
    ),
    "duplicate outcome protection remains": (
        "HasRecordedOutcome(position.Id)" in outcome and
        "outcome.Recorded" in closed
    ),
    "runtime contracts cover aggregation/idempotency": (
        "VerifyLifecycleOutcomeSemantics();" in contracts and
        "LifecycleEventIdempotencyGuard" in contracts and
        "HistoricalOutcomeAggregationRule" in contracts
    ),
    "runtime project links aggregation": (
        "HistoricalOutcomeAggregationRule.cs" in project
    ),
}

for name, ok in checks.items():
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# The close handler must not independently read Position.NetProfit to decide
# win/loss once the canonical outcome recorder has run.
if "args.Position.NetProfit > 0" in closed:
    errors.append("position close handler still independently classifies outcome from Position.NetProfit")
    print(f"FAIL | {errors[-1]}")

print("CR2.5 SUMMARY")
print("=" * 72)
print(f"Failures: {len(errors)}")

if errors:
    for error in errors:
        print(f"- {error}")
    sys.exit(1)

print("CR2.5 STATIC GATE PASS")
