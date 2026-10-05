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

The footer content minimum is now 40px, while outer panel padding is charged exactly once. The M1/M5/M15/M30/H1/H4/D1/W1 rail is a compact two-line 38px area and uses the exact real content width divided into eight equal cells. Lamp-to-label spacing is compact, labels inherit lamp colors, and the shared status-lamp geometry is 30x30 with a 20px base font.

The deep alert review found that the canonical primary ScenarioId could be announced twice: once by the
parallel scenario alert owner and again by the generic canonical ACTION/WATCH owner. The primary canonical
ScenarioId is now excluded from the parallel user-facing alert loop; independent simultaneous scenarios
remain eligible.

The AlertEngine acknowledgement boundary was also corrected. Cooldown and last-alert state now commit only
after AlertDeliveryQueue accepts the event, SendUnifiedAlert returns that acceptance, and canonical local
WATCH/REACTION/ACTION/RESTRICTION guards advance only after successful enqueue. Queue rejection is therefore
retryable instead of being incorrectly remembered as a delivered event.


## 2026-10-03 — Second-pass panel/audio correction

Implemented on `main` after live visual/audio feedback:

- Footer content reserve reduced to 40px; the outer panel padding is no longer double-counted.
- The five-message in-memory alert history is retained, while only the two latest messages are rendered in the compact footer rail so the panel footer stays visible.
- The actual MTF rail geometry is now included in the final panel-height equation; previously the height budget reserved the rail for scroll calculation but omitted it from the final panel height.
- M1/M5/M15/M30/H1/H4/D1/W1 labels now inherit the exact semantic color of their lamp. Status lamps were enlarged slightly through the shared panel constants.
- Indicator signal sounds now use a second semantic-event gate: same signal/bar/direction can produce one sound, while a later higher-priority escalation may produce the next sound. Panel messages remain independent, so multiple scenarios are still visible without multiple identical/near-identical audio cues.


## Live footer relayout / sound idempotency hardening

A timer-delivered alert now immediately recalculates only the compact footer geometry. The panel keeps its existing scroll content height as far as the configured maximum allows, expands the Footer to the real two-row alert rail, and updates the button-stack height without invoking the expensive full panel row renderer.

Signal audio deduplication now stores a bounded set of recent `symbol | signalId | CreatedClosedM5 | direction` event fingerprints. Interleaving different alerts cannot reset the dedup state, and the same signal event therefore reaches audible playback at most once during the retained window.


## 2026-10-03 — Third-pass forensic correction

The previous 132px footer reserve was not actually eliminated because the visible lower stack also included the MTF rail and the footer resolver duplicated outer panel padding. The canonical fix is now:

- Footer content minimum: 40px.
- MTF two-line rail: 38px including compact top/bottom spacing.
- Footer resolver returns content height only; outer panel padding is charged exactly once.
- Alert rows remain the audit-required 20px height, but now use no-wrap + ellipsis so the visible text cannot escape the row.
- Signal-family audio is one event per symbol/closed-M5/direction across WATCH/REACTION/ACTION/SMART/EARLY variants.
- MTF trend arrows use a dedicated `MTF_ARROW_1..3` namespace and therefore cannot overwrite/remove canonical signal arrows.
- The repeated Local/Cloud prompt was traced to cTrader's synchronization/algorithm-source behavior, not to a repository Cloud transport; local-first operation must be selected at the terminal level.


## Third-pass terminal synchronization note — 2026-10-03

The repository does not introduce a Cloud execution path for the Indicator. cTrader's current documentation states that cloud synchronisation makes created/installed algorithms and their updates available across cTrader apps, while custom indicators execute locally on Windows/Mac and cloud execution applies to cBots. For this local-first CFIP workflow, repeated Local/Cloud reconciliation is therefore treated as a cTrader terminal synchronization/instance-state concern, not a CFIP source-code transport feature. Target terminal cleanup must keep one intended CFIP local instance and remove stale duplicate instances before evaluating realtime sound/panel behavior.
