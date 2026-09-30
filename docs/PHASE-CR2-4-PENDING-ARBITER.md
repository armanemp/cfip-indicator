# CFIP — CR2.4 Pending-Order Decision Arbiter

## Status

IMPLEMENTED — CI acceptance pending.

## Scope

CR2.4 covers B5 from the Claude review-remediation program.

The phase makes the two pending strategies—Continuation Stop and Reversal Limit—compete through one deterministic arbiter rather than separate placement branches.

## Canonical arbitration

`src/CFIP.Indicator/Core/Math/PendingDecisionArbiterRule.cs` owns:

- candidate scoring;
- PendingOrderMode constraints;
- one-winner selection when both candidates are eligible;
- exact-tie behavior;
- existing-order type alignment;
- two-consecutive-closed-bar cancellation hysteresis.

Both-candidate behavior:

- higher candidate quality wins;
- exact ties select Continuation Stop, preserving the previous continuation-first behavior as an explicit policy;
- no eligible candidate produces `None`.

The arbiter returns exactly one actionable choice per cycle.

## Per-candidate validation

Continuation and reversal candidates are validated independently for the actual candidate direction before arbitration:

- Range/Compression suitability;
- market suitability;
- existing strategy-specific quality/confirmation rules.

An unsuitable candidate is removed from the competition rather than allowed to win and fail later.

Once a winner is selected, a failed broker-preparation or placement attempt never falls through silently to the other strategy in the same cycle.

## Cancellation hysteresis

Pending cleanup now treats strategy mismatch, direction change, and loss of a stable eligible setup as transient until the same invalidation persists for two consecutive closed M5 bars.

Expiry remains immediate because it is time-based rather than a one-bar signal fluctuation.

This prevents place/cancel oscillation caused by a single reaction/decision change.

## Pending price-side correction

Predictive Reversal Limit candidate discovery now uses the executable side of the quote:

- Buy Limit reference: current Ask;
- Sell Limit reference: current Bid.

The existing broker validation remains the final authority before mutation.

## Lookback boundary hardening

Continuation Stop fallback trigger construction now explicitly validates its closed-bar lookback range before calling Highest/Lowest. Invalid short history fails closed with a diagnostic rather than relying on an implicit index assumption.

## Verification

Deterministic runtime contracts cover:

- single-candidate selection;
- simultaneous-candidate quality arbitration;
- exact tie policy;
- PendingOrderMode constraints;
- order-type alignment;
- two-bar cancellation hysteresis;
- bounded candidate scores.

A dedicated static audit verifies that the old independent placement branches are removed and that the arbiter owns selection, cleanup and side-of-market semantics.

Required CI gates:

- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile.

Target-terminal broker behavior remains a separate manual acceptance boundary.

## Routine project audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning

The change introduces no new broker mutation owner. Broker placement remains in the existing placement modules; CR2.4 only centralizes candidate selection and stabilizes pending lifecycle decisions. Hysteresis state is bounded to the single managed pending lifecycle.