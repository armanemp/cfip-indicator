# Sub-phase 1 — Footer + Alert/Popup Lifecycle Hardening (2026-10-03)

Status: IMPLEMENTATION COMPLETE — verification pending on the exact branch head.

## Scope

This sub-phase closes the unfinished Footer and Alert/Popup parts of the larger user-requirement phase. It does not touch signal-quality thresholds or chart drawing authority.

## Root causes found

1. The Footer minimum was 40px, but the toggle button also reused the full outer PanelPadding (default 9px) as its own top/bottom margin. That duplicated spacing semantics and made the lower chrome taller than its actual content.
2. The compact alert rail still used 20px rows. Two visible rows therefore required 41px including the 1px gap, even though the footer was intended to be minimal.
3. Alert sound dedup already had a bounded semantic family set, but transport-boundary idempotency still keyed the pending set by AlertEnvelope.AlertId. AlertId can differ when envelope revision/transport text changes for the same causal event.
4. SendUnifiedAlert sent email after queue rejection. A duplicate or overflowed event could therefore be suppressed for the canonical panel/sound transport while still producing another email side effect.
5. The alert rail was reused structurally, but its row text/color update was still executed on ordinary panel renders even when the alert revision had not changed.

## Corrections

- Footer minimum is now 34px; the two-row alert rail is now 37px (18px + 18px + 1px gap).
- Footer button internal margin is now a dedicated 2px constant instead of reusing outer PanelPadding.
- Outer PanelPadding remains charged exactly once to the panel geometry.
- Alert transport dedup now derives its pending identity from canonical ContractIdentity fields plus AlertKey, independent of envelope revision and generated AlertId.
- A deterministic runtime contract proves that the same causal event is rejected even when the alert ID and revision change, then becomes re-armable after actual delivery.
- Email delivery now occurs only after canonical AlertDeliveryQueue acceptance.
- Alert rail rendering is revision-driven. Existing row controls are reused and skipped when `_panelAlertRevision` has not changed, reducing needless chart-control writes on realtime panel refreshes.
- Character budget now accounts for the visible bullet prefix so the manual truncation boundary matches the actual rendered text more closely.
- Alert-rail visibility remains lifecycle-driven even when the alert revision is unchanged, preventing hide/show transitions from leaving a stale invisible rail.

## Alert/Popup lifecycle

Canonical path remains:
`SendUnifiedAlert -> AlertDeliveryQueue -> ProcessQueuedAlertDelivery -> RecordPanelAlertDelivery -> sound queue -> last-bar sound delivery`.

Blocked candidate events remain diagnostic-only and do not produce the normal signal sound or marker. Queue rejection remains retryable at the engine boundary.

## Full-chain routine audit

Analysis -> Decision -> Signal -> Alert -> cBot execution -> Broker confirmation -> Protection/Lifecycle -> Outcome/History was reviewed at the ownership boundary. This sub-phase changed alert transport/presentation and footer geometry only; no signal, RR, risk, execution or broker-ownership rule was retuned.

Performance: no new market-data loop or calculation loop was added. The alert rail now avoids per-render row rewrites when the canonical alert revision is unchanged.

## Verification boundary

Required on the exact final head:
- Source/Architecture accumulated audits;
- Runtime Acceptance Contracts, including the new revision-independent alert identity contract;
- cTrader Compile/Build;
- target-terminal cTrader visual acceptance for the compact footer, readable alert rail, no repeated Popup refresh, and audible single-event behavior.

Operator action after merge: `git pull --ff-only`.
