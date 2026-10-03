# 2026-10-03 — Panel Footer / MTF Lamp / Alert Delivery Hardening

Status: IMPLEMENTED ON MAIN — automated CI verification runs from the new source/architecture audit.

## Findings

- The MTF trend-lamp rail was added outside the ScrollViewer, but the panel's scroll-height budget did not reserve the alert rail height. The ScrollViewer could therefore consume vertical space that belonged to the footer, pushing existing footer content out of the visible panel.
- The timeframe rail was a single-line `● M1` presentation, which made the rail dense and reduced visibility.
- Signal audio already had one production playback owner in `UI/Panel/AlertDeliveryProcessor.cs`, but the shared delivery queue had no transport-boundary idempotency check. Exact duplicate canonical events could therefore be queued twice if independent runtime boundaries attempted the same enqueue.

## Changes

- MTF timeframe lamps are now a two-line vertical presentation: lamp on the first line, timeframe name directly below.
- The lamp rail has explicit top/bottom spacing and its geometry is included in the panel's fixed vertical chrome calculation.
- The panel scroll budget now reserves the actual alert-footer height before estimating ScrollViewer content height.
- `AlertDeliveryQueue` now suppresses an identical canonical `AlertEnvelope.AlertId` while that event is pending. The id is released on dequeue/eviction/clear, so legitimate later alerts remain possible after delivery and normal cooldown.
- Runtime Acceptance Contracts now cover duplicate queue suppression and post-delivery re-arming.
- Source/Architecture CI receives a dedicated audit for this fix.

## Alert review result

The Indicator signal path remains canonical:

`SendUnifiedAlert → AlertDeliveryQueue → RecordPanelAlertDelivery → sound queue → DeliverAlertSound`.

Custom sound playback returns immediately on success, so it does not fall through into a second semantic cue. cBot lifecycle/execution audio remains a separate owner with its own debounce and is not mixed with Indicator signal audio.

## Verification boundary

Static/source audits and Runtime Acceptance Contracts are automated. Actual cTrader visual geometry and audible terminal behavior remain target-terminal acceptance checks.


## Follow-up — compiler warning cleanup

The Release build exposed CS0649 on `TradeOpportunityCandidate.RewardDistanceAtr` and `MinimumRequiredRewardDistanceAtr`. The canonical parallel-candidate builder now assigns both from the existing ATR/reward geometry and the existing `RegimeAdaptiveRewardFloorRule`; no new public threshold or behavior gate was introduced.


## Second-pass correction — 2026-10-03

The footer now has a shared minimum-height owner of 132px and the full footer reserve is applied before
ScrollViewer sizing. The maximum-height resolver also protects a minimum renderable viewport so the footer
cannot be clipped merely because PanelMaxHeight is configured low.

The M1/M5/M15/M30/H1/H4/D1/W1 rail now uses the exact real content width divided into eight equal cells.
Lamp-to-label spacing is tighter, lamp strength tiers are 18/17/16px, and the header heartbeat lamp uses
the same 18px font and 28px geometry.

The deep alert review found that the canonical primary ScenarioId could be announced twice: once by the
parallel scenario alert owner and again by the generic canonical ACTION/WATCH owner. The primary canonical
ScenarioId is now excluded from the parallel user-facing alert loop; independent simultaneous scenarios
remain eligible.

The AlertEngine acknowledgement boundary was also corrected. Cooldown and last-alert state now commit only
after AlertDeliveryQueue accepts the event, SendUnifiedAlert returns that acceptance, and canonical local
WATCH/REACTION/ACTION/RESTRICTION guards advance only after successful enqueue. Queue rejection is therefore
retryable instead of being incorrectly remembered as a delivered event.
