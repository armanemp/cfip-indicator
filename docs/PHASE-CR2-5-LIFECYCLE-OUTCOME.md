# CFIP — CR2.5 Lifecycle Ordering and Outcome Aggregation

## Status

IMPLEMENTED — CI acceptance pending.

## Scope

CR2.5 covers B6 plus the shared outcome work required by C8.

## Lifecycle event ordering

`PositionOpened` is no longer allowed to assume that a plan was already bound when the broker event arrives.

When the opened position is managed but there is no exact live-plan binding, the handler enters the existing managed recovery path and then reuses the recovered plan if its `PositionId` matches the observed position.

This allows `PositionOpened` and `PendingFilled` to arrive in either order without silently losing the live-position/protection transition.

Unmanaged or foreign positions are still rejected at the handler boundary through the existing managed-identity checks.

## Idempotency

`LifecycleEventIdempotencyGuard` now keeps both a set and FIFO queue with a fixed 512-event memory bound.

Duplicate `(eventType, entityId)` pairs are rejected while recent lifecycle identity memory remains bounded. The bounded window is deliberate; it prevents unbounded process growth.

## Final outcome truth

At final position close, the indicator now queries cTrader `History.FindByPositionId` and aggregates all returned `HistoricalTrade` records for that position.

The aggregate sums:

- NetProfit;
- GrossProfit;
- Swap;
- Commissions;
- Pips.

Because the history API exposes historical trades associated with a position and each historical trade has its own NetProfit/Pips data, partial/final closing legs are aggregated before learning and win/loss classification.

When history is unavailable, the previous position-level values are used as a deterministic fallback rather than inventing a partial result.

## Realized R

Realized R now uses final aggregated monetary NetProfit divided by the monetary risk implied by the plan's original volume and initial stop distance through `Symbol.AmountRisked`.

This keeps a profitable partial-close sequence from being misclassified simply because the final closing leg has a lower individual result. The initial risk remains the reference denominator.

## Single outcome truth

`PositionClosedHandler` no longer independently classifies win/loss from `Position.NetProfit`. It consumes `OutcomeRegistrationResult` produced by the canonical outcome recorder.

This keeps telemetry, win/loss aggregates and calibration on the same final outcome.

## Verification

Deterministic runtime contracts cover:

- duplicate event rejection and bounded idempotency memory;
- multiple historical closing records aggregating into one outcome;
- non-finite historical data failing closed;
- deterministic position-level fallback.

A dedicated static audit checks event-order recovery, managed-position boundaries, bounded idempotency, History-based aggregation, monetary risk normalization and single-source win/loss classification.

Required CI gates:

- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile.

Target-terminal broker History/Deal behavior still requires manual verification on the actual cTrader terminal/account.

## Routine project audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning

The lifecycle change does not add another broker mutation authority. Recovery and outcome aggregation run only on lifecycle events/close events, not on the calculation hot path. Idempotency state is bounded.