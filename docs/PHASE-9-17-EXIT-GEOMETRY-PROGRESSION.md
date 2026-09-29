# Phase 9.17 — Exit Geometry, TP Progression & Protection Integrity

Date: 2026-09-29

## Status

Implementation complete pending CI and target-terminal replay.

Branch:
`phase/9-17-exit-geometry-progression`

## Problem addressed

The reported TP regression had a concrete architectural cause rather than one bad numeric
parameter.

A live target was previously allowed to be "better than the previous target" without also
being required to remain ahead of the current market quote. In addition, live target
progression stopped while the server-side TP ladder was active, and post-fill/recovery
paths could rebuild targets from stale plan geometry.

That combination could produce a visible TP that had already been crossed by price.

## Canonical exit geometry

Phase 9.17 introduces `LiveExitGeometryRule` as the single mathematical guard for live
exit geometry.

For BUY:
- protective SL is below the live Bid by the required broker distance;
- every live TP is above the live Bid by the required forward distance;
- a new TP may never be below the existing TP.

For SELL:
- protective SL is above the live Ask by the required broker distance;
- every live TP is below the live Ask by the required forward distance;
- a new TP may never be above the existing TP.

The rule is independent of the legacy `PreventBrokerTpBackwardMove` tuning option, so
live protection safety cannot be disabled by a tuning parameter.

## TP progression model

The live TP lifecycle is now:

Analysis/Plan
-> actual broker fill
-> live-market-aware TP/SL reconciliation
-> TP progression
-> broker synchronization
-> server-side ladder progression
-> final target progression.

Before TP1:
- the full server ladder can be re-evaluated;
- TP1, TP2 and final target must remain strictly progressive;
- all targets must remain ahead of the live market.

After TP1:
- the consumed first partial is removed from the server ladder;
- the remaining TP2 partial and final target can be rebuilt forward;
- the final target can continue to advance while the trend provides valid structural
  targets.

After TP2:
- the remaining position can be collapsed to one forward final target;
- normal live target progression can continue.

This means a target already crossed by price is not restored merely because the underlying
structural target list still contains that historical price.

## Post-fill integrity

The broker fill is now reconciled through
`LiveFillExitReconciler`.

The reconciler calculates stop and TP candidates locally and validates the complete
geometry before modifying the in-memory plan. A failed reconciliation does not fall back
to the old non-live-aware rebuild.

The previous flow could rebuild the complete ladder after a fill using structural levels
without checking whether those levels were already behind the live market. That path has
been removed from post-fill reconciliation.

## Stop-loss review

The existing SL model was already monotonic and structurally driven:
- original structural stop;
- spread-aware break-even;
- structural swing repricing;
- smart trailing;
- broker minimum-distance validation;
- monotonic-only broker mutation.

Phase 9.17 adds a final explicit live protective-stop geometry check. A proposed stop must
remain on the protective side of the current market and respect the broker's minimum
distance.

A less-protective stop cannot overwrite a more-protective broker stop.

## Risk/reward review

Plan RR remains:

`abs(target - entry) / abs(entry - stop)`

with risk derived from the actual plan entry/stop geometry.

Phase 9.17 additionally verifies:
- BUY/SELL symmetry;
- strict TP ladder progression;
- target-forward-of-market geometry;
- protective-stop geometry;
- actual-fill exit reconciliation;
- broker TP mutation monotonicity.

## Broker distance review

Live TP spacing now includes the broker's declared minimum take-profit distance in
addition to the configured ATR/pip spacing.

The live SL guard likewise uses the broker minimum stop-loss distance.

This avoids a mathematically valid level being sent to the broker while still violating
broker price-distance constraints.

## Server-side protection

The cTrader Advanced Protection API supports relative/absolute multi-target protection,
including partial take-profits followed by a final target. Phase 9.17 keeps that server-side
ownership where available and updates the ladder only through confirmed broker mutations.

The indicator never invents a broker fill or synthetic TP hit. It observes the broker-owned
position/volume state and then reconciles its internal lifecycle.

## Verification

Decision Contracts include deterministic BUY/SELL live exit tests for:
- forward TP;
- TP behind market;
- backward TP movement;
- progressive/non-progressive ladders;
- protective stop symmetry.

Source/Architecture CI includes a dedicated `audit_exit_geometry.py` check covering:
- canonical exit geometry ownership;
- live fill reconciliation;
- forward-only target progression;
- broker protection mutation boundaries;
- server ladder lifecycle;
- absence of destructive history cleanup.

Target-terminal replay remains required for:
- actual chart behavior;
- live tick-to-bar timing;
- broker slippage;
- broker rejection behavior;
- actual server-side advanced-protection behavior;
- realized exits and R;
- confirmation that the reported TP regression is eliminated on the user's symbol/timeframe.

No profitability or win-rate improvement is claimed from source-level verification.

## Required routine gates

This phase continues the permanent project discipline:
- source/architecture and accumulated audits;
- cTrader compile/build;
- Decision Contracts;
- runtime acceptance;
- execution/protection review;
- history/learning review;
- performance review;
- target-terminal replay boundary explicitly recorded.

## Next phase

Phase 9.18 should use the new exit/protection traces and target-terminal evidence to
measure:
- target-regression incidents;
- target advancement frequency;
- stop-repricing frequency;
- broker protection failures;
- false exits versus valid structural exits;
- missed continuation opportunities after TP1/TP2.

Only then should default exit thresholds be changed.


## Final implementation note — 2026-09-29

Phase 9.17 also normalizes live target spacing against the broker-reported minimum TP
 distance, in addition to ATR/pipe spacing, and applies the same BUY/SELL geometry rules
 during startup recovery. The existing risk denominator remains the original Entry-to-SL
 plan risk so peak-R and realized-R measurements remain comparable even while the protective
 stop ratchets forward.
