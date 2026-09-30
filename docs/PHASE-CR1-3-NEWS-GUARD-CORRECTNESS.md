# CR1.3 — News Guard Correctness and Non-Blocking Refresh

Date: 2026-09-30

## Objective

Make the economic-news guard deterministic, cache-driven and non-blocking so
network latency cannot stall initialization, calculation, decision or execution.

## Root causes addressed

The previous calendar client used synchronous `Http.Get`. Although refresh was
called from the Timer, initialization also invoked the same method, so a slow or
unavailable feed could delay the indicator startup path.

The previous status model collapsed several materially different states into
`OK` / `FEED ERROR` and did not expose an explicit never-loaded/stale model.

Currency relevance was inferred mainly by finding known three-letter currencies
inside the symbol name. That was insufficient for broker-specific index,
commodity and crypto symbols that do not embed an ISO currency code.

## Implementation

### Asynchronous feed transport

The calendar client now uses cTrader `Http.GetAsync`.

Refresh is owned by the existing Timer heartbeat. The decision path never starts
an HTTP request.

Only one refresh may be in flight. Each request receives a monotonically
increasing generation id. A response from an abandoned/timed-out generation is
ignored so a late response cannot overwrite newer state.

A 30-second internal transport bound invalidates a stuck request and allows the
next scheduled refresh to proceed. This is a transport safety bound, not a
trading threshold.

The last valid event cache is retained across refresh failures. A failed refresh
does not erase previously validated calendar events; the feed becomes stale only
when the last successful refresh exceeds the configured maximum age.

### Explicit feed states

The feed state is now resolved by a pure deterministic rule:

- DISABLED
- NEVER_LOADED
- HEALTHY
- STALE
- BLOCKING_EVENT

The runtime additionally exposes:

- REFRESHING when an asynchronous request is active;
- LAST REFRESH FAILED with a bounded error message when the latest refresh failed.

For automatic execution (Auto Trading and Auto Orders), NEVER_LOADED or STALE is fail-closed when
`NewsFailClosedWhenStale` is enabled. This preserves safe behavior without
making the indicator's analytical engine wait for the network.

### Symbol / index / crypto currency relevance

A dedicated configurable mapping was added:

`AdditionalNewsCurrencies` (with `SYMBOL=CUR` mapping entries)

Format:

`SYMBOL_PATTERN=CUR1,CUR2;SYMBOL_PATTERN=CUR3`

Broker punctuation is normalized before matching. Standard FX currency detection,
additional currencies and configured symbol mappings are merged into one
deterministic sorted set.

The default mapping covers common commodities, indices and crypto symbols while
remaining overridable for broker-specific naming.

### Initialization

Initialization no longer performs a synchronous news refresh. The indicator
reaches its normal READY/panel state without waiting for the calendar.

The first asynchronous refresh is started by the runtime Timer.

### Decision / execution boundary

`NewsBlocked` remains a pure cache/status consumer. It never performs network I/O.

Existing market, aggressive and pending paths continue to use the same news gate.
The existing pending cancellation and optional active-position pre-news policy are
unchanged.

Manual `NewsBlackoutUtc` remains an explicit operator override.

## Tests

Added deterministic runtime contracts for:

- disabled / never-loaded / healthy / stale / blocking-event state transitions;
- FX currency inference;
- index mapping;
- crypto mapping;
- extra-currency merging and deterministic ordering;
- broker-symbol punctuation normalization.

The News Guard source audit now rejects synchronous `Http.Get` / `Http.Send`
usage in production source and requires the asynchronous request-generation and
state contracts.

## Safety

No live trading threshold was tuned.

No second decision authority or second execution authority was introduced.

No additional position capacity was introduced.

The news feed remains an input gate. It does not claim to prevent broker stops,
slippage or gaps during fast releases.

## Verification boundary

Automated source and deterministic runtime-contract checks are mandatory.

Target-terminal verification remains required for:

- actual network/feed behavior under the installed cTrader build;
- weekly XML availability and timestamp interpretation;
- instrument-specific currency mapping;
- refresh failure/stale behavior;
- pending cancellation before high-impact events;
- optional active-position close behavior;
- startup responsiveness in the target environment.

cTrader's current HTTP API exposes `Http.GetAsync`, and current documentation
states that `AccessRights.None` is sufficient for network functions.
