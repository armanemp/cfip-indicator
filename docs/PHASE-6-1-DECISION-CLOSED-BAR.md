# CFIP — Phase 6.1 Decision Closed-Bar Contract

## Purpose

Phase 6.1 establishes one deterministic temporal contract for confirmed
decision construction: every decision frame must correspond to the fully closed
bar selected from one UTC reference.

## Authoritative rule

ClosedBarReferenceRule.ResolveClosedIndex(...) selects the latest bar whose
next actual bar open is at or before the reference. This avoids duration
inference across weekends/holidays and avoids ambiguous time-series lookup
semantics.

At an exact bar-open boundary, the newly opened bar is not considered closed;
the preceding bar is the closed decision bar.

## Decision boundary

The preparation stage creates one MtfClosedContext from Server.TimeInUtc.
The decision builder receives that exact context and refuses to construct a
decision input when:

- the context/reference/closed-M5 identity is inconsistent;
- any required decision frame is absent or has a different index;
- an optional M1/D1/W1 frame is present at a different index;
- primary closed-bar history is insufficient.

Timeframe agreement consumes the same context indices rather than resolving a
second set of timestamps.

## Acceptance scenarios

The runtime contract covers:

- no closed bar before the first actual boundary;
- exact-boundary closure;
- between-boundary behavior;
- gaps between bar opens;
- a resolved bar proven fully closed at the reference;
- rejection of a future bar;
- bounding after the last loaded open.

## Scope

This phase does not change score thresholds, weights, RR policy, risk sizing,
trailing, execution policy or broker mutation semantics.

Phase 6.2 remains responsible for explicitly separating retained intrabar
reaction behavior from confirmed closed-bar decisions.