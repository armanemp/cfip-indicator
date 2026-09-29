# Phase 9.17 — Exit Geometry, TP Progression & Protection Integrity

Date: 2026-09-29

## Status

Source/contract implementation complete, corrective hardening merged, and automated CI verified; target-terminal replay remains required.

Branch:
`phase/9-17-exit-geometry-progression-mainline`

## Problem addressed

The reported TP regression had a concrete architectural cause rather than one bad numeric
parameter.

A live target was previously allowed to be "better than the previous target" without also
being required to remain ahead of the current executable market quote. In addition, the
server-ladder adoption path could reconstruct internal target state from the plan instead
of the broker-owned last target, and post-fill/recovery paths could reuse stale exit geometry.

A separate stop-side defect was also found: post-fill structural stop repricing compared the
new structural stop with itself in one branch, so a valid tighter stop could never advance.
These were fixed together so entry, SL, TP, broker protection and lifecycle state use one
consistent live geometry contract.

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

## Corrective numerical hardening — 2026-09-29

The deep review after the initial 9.17 implementation closed several additional calculation
and synchronization gaps:

- live TP spacing is centralized in MinimumLiveTargetDistancePrice(direction, atr) and
  combines broker minimum TP distance, tick/pip granularity and ATR spacing;
- percentage-based broker distances are quote-direction aware: BUY uses Bid and SELL uses Ask;
- server-side ladder adoption trusts AbsoluteTakeProfitProtections.LastTakeProfit.Price
  as the broker-authoritative final target instead of reconstructing it from _plan;
- server-ladder progression validates every active TP against the live Bid/Ask before mutation;
- actual-fill reconciliation now compares the prior SL against the new structural SL before
  applying ProtectionProgressionRule.ShouldAdvanceStop;
- recovery no longer accepts a fallback target that exceeds MaximumRewardRR;
- plan RR progression is recomputed from current entry/target geometry instead of stale cached RR;
- NaN/Infinity is fail-closed for risk, TP ladder stages and Smart Break-Even calculations;
- stale broker protection is no longer considered synchronized merely because the server ladder
  object exists.

No new public parameter or second decision/execution authority was introduced.

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

## Automated verification closeout — 2026-09-29

Phase 9.17 automated source/contract verification is complete:
- Runtime Acceptance #1196: PASS
- cTrader Compile/Build #1380: PASS
- Source/Architecture + accumulated audits #1387: PASS
- Decision Contracts: PASS within cTrader Compile/Build
- Phase 9.16 signal measurement audit: PASS
- Phase 9.17 exit geometry audit: PASS

Phase 9.17 merge commit:
`af4ef4edb5ce032c4a71feaf2cc0be1203f4992b`

PR #61 (`Phase 9.17.1: corrective exit and risk hardening`) is merged into `main`.
Target-terminal replay remains required before claiming empirical elimination of the reported
TP rollback or any improvement in realized R, exit efficiency, false exits or continuation capture.
