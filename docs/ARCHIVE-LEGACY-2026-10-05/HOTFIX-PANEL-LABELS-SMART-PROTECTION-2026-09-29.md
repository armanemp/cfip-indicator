# CFIP Indicator — Runtime UI and Smart Protection Hotfix

Date: 2026-09-29

## Reported runtime issues addressed

### Panel Auto Trade / Auto Orders controls

The chart-panel quick controls already had runtime state fields, but their action
surface depended on `Checked/Unchecked` event handlers. The hotfix makes the
operator click itself the explicit action boundary.

Behavior:

- AUTO TRADE click reads the toggle state and updates the canonical auto-trading
  runtime state;
- AUTO ORDERS click reads the toggle state and updates the canonical automatic
  pending-order runtime state;
- programmatic UI synchronization is guarded by `_executionToggleSyncing` so
  refreshes cannot act as operator clicks;
- no broker mutation is performed by the UI.

### Compact line labels

The prior render path removed labels from setup previews, so levels visible before
plan activation could have no name/price tags.

Correction:

- setup previews now render the same compact labels as active plans;
- label values use the preview snapshot's Entry / Ideal / Trigger / SL / TP levels;
- the semantic-color box is created first, filled translucently and then the text is
  rendered above it;
- existing text/rectangle objects continue to be reused rather than recreated.

### Structural trailing / SL stability

The prior protection calculation contained a final market-distance clamp that could
derive a new stop from current price even without a new structural event. This made
the stop behave like a price-following trail.

Correction:

- removed the market-distance chase;
- structural trailing progression is evaluated on a newly closed M5 structural
  event;
- the structural candidate remains a swing-derived level;
- momentum alignment can tighten the structural breathing distance but does not
  create a raw market-price stop;
- pressure-based tightening is also closed-bar/structural-event gated;
- break-even remains a protective mechanism and retains broker validity checks;
- if a candidate is temporarily invalid, the previous protected level is retained.

The existing monotonic broker-protection rules remain authoritative.

## Auto Orders semantic boundary

The existing automatic pending-order engine already separates:

- continuation Stop orders at a future trigger;
- reversal Limit orders at a future structural zone.

Both are validated as genuine pending prices away from the current market before
submission and require broker confirmation.

The deeper enhancement requested by the user — forecasting reversal points from
multi-timeframe important levels, FVG/OB/liquidity/structure and indicator
evidence, then selecting and ranking the most meaningful future entry level — is
not folded into this UI/protection hotfix. It remains a dedicated smart-pending
planning improvement so the automatic-order path does not become a duplicate
decision authority.

## Invariants

- no new public parameters;
- 535-parameter production contract remains unchanged;
- no manual BUY/SELL control is introduced;
- broker confirmation remains authoritative;
- accepted submission remains distinct from fill confirmation;
- SL progression remains protective-only;
- automatic market/aggressive and automatic pending paths retain separate
  execution semantics.

## Required validation

- Source / Architecture
- Runtime Acceptance Contracts
- cTrader Compile
- hands-on cTrader validation for actual button response, label visibility and
  real trailing behavior.
