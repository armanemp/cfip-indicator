# CBOT-P4D — Pending Limit Authority + Signal/Popup Continuity — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending.

## Scope

This phase continues the staged execution migration from the Indicator into the cBot while fixing visible signal/popup continuity without creating parallel engines.

### Execution boundary

- Indicator remains the analysis/decision/scenario/plan authority.
- CFIP.Contracts.ExecutionIntent remains the single cross-boundary execution contract.
- Pending Limit broker mutation moves completely to the existing cBot pending execution owner.
- Pending Stop and Pending Limit share one broker mutation coordinator; no second pending executor is introduced.
- The obsolete Indicator BrokerLimitOrderPlacement.cs owner is removed after its callers are migrated.
- cBot Pending Limit execution is demo-only and fail-closed, with the same canonical margin, capacity, scenario and idempotency protections used by the other execution paths.
- The cBot consumes the instance-scoped execution label carried in the canonical ExecutionIntent; it does not recreate label/identity formatting.
- The host Chart timeframe remains presentation-only. M15 remains the internal execution clock.

### Pending Limit lifecycle

M15 decision → M5/M1 defensive tuning → predictive structural level → executable-side pending validation → SL/TP geometry → risk sizing → immutable ExecutionIntent + absolute lifecycle snapshot → cBot validation → margin/capacity guard → broker PlaceLimitOrder → confirmed pending state

The Indicator does not call PlaceLimitOrder, does not own the broker submission gate for Pending Limit, and does not own the broker mutation.

Initial Pending Limit submission preserves the canonical initial SL/TP distances. Advanced server-side TP ladder progression remains governed by later TP/protection migration phases rather than being recreated in a second Pending Limit owner.

### Signal continuity

The chart signal layer now separates directional analysis from execution actionability:

- canonical Decision direction may remain visible when execution is currently blocked/non-actionable, subject to the existing early-confidence floor;
- directional markers are arrow-only;
- the M1 trigger marker is an up/down arrow rather than a circle;
- Strong / Confirmed / Watch states use three distinct directional intensity colors for BUY and SELL;
- no new signal score or strategy threshold is introduced.

This is a presentation correction around existing decision evidence, not a strategy rewrite.

### Popup continuity

- default position: BottomRight;
- important alert classes only are routed to the popup by the canonical alert engine;
- critical alerts remain eligible;
- persistent mode stays visible until the next alert or manual CLOSE;
- the existing CLOSE button remains the manual dismissal control;
- popup delivery remains queue-based and idempotent.

### Cleanup / anti-duplication rule

No compatibility wrapper, duplicate executor, alternate identity formatter or detached patch subsystem is retained when the original owner can be corrected directly. Production behavior is changed at the canonical owner and every affected audit is updated to that same ownership model.

## Verification required

- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- CBOT-P4D acceptance audit;
- accumulated execution-boundary / UI audits.

## Manual target-terminal acceptance

Still required after repository verification:

- Indicator and cBot on M1, M5, M15, H1 and higher Chart TFs;
- identical internal execution identity across host charts;
- visible BUY/SELL arrows without circles/diamonds;
- distinct Strong/Confirmed/Watch arrow intensity colors;
- signal direction visible while waiting/blocked when existing evidence supports it;
- popup at lower-right, persistent and manually closable;
- Pending Limit placement/confirmation on demo;
- Pending Stop parity after the unified coordinator;
- spread-aware pending entry behavior;
- margin/capacity fail-closed behavior;
- panel responsiveness and live refresh.

No profitability claim is made from this structural/UI phase alone.

Next staged execution migration after verification: CBOT-P4E — Pending cancellation authority extraction.
