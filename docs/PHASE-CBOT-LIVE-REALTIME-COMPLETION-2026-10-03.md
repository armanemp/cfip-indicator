# CFIP — cBot Live / Realtime Completion — 2026-10-03

Status: IMPLEMENTED — repository verification pending; target-terminal validation remains required.

## Purpose

Close the final runtime seams found during the live/realtime hardening pass without introducing a second analysis or execution authority.

## Completed

- cBot now republishes symbol-scoped presence immediately after successful CFIP Indicator binding discovery, carrying the exact bound Indicator InstanceId.
- cBot publishes explicit attachment-loss presence when binding fails so stale presence cannot masquerade as a current attachment.
- Indicator queued audible alerts are serviced from the Calculate finally boundary while the delivery processor still enforces the realtime IsLastBar rule.
- Panel presentation remains timer-owned; sound remains single-owned by AlertDeliveryProcessor.
- Existing current-vs-future scenario separation, per-ScenarioId idempotency, bounded concurrency, adaptive stagnant-market reward floor, history/forecast evidence ranking and explicit live arm remain unchanged.

## Root causes closed

### Attachment

OnStart/OnTick published cBot presence before Indicator discovery completed. A successful later bind therefore left the symbol-scoped presence heartbeat with an empty BoundIndicatorInstanceId until a subsequent cycle.

The correction publishes presence immediately at the binding transition.

### Alert audio

Queued sound delivery was already restricted to the Indicator realtime last-bar path, but early returns or recoverable exceptions could skip the normal delivery call in a cycle. The delivery call is now placed on the Calculate finally boundary; it remains guarded by IsLastBar inside the processor.

## Full-chain audit

History/outcomes -> pre-analysis -> M15 canonical decision -> M5 trigger/tuning/entry precision -> optional M1 confirmation -> current quote actionability -> current Market/Aggressive OR future Stop/Limit -> ScenarioBatch -> cBot preflight -> broker submission/placement -> broker confirmation -> protection/management -> outcome/history.

## Safety

- Live execution remains explicitly armed and OFF by default.
- A live cBot without the arm performs no broker mutation.
- Current market actions and future pending orders remain distinct.
- Concurrent scenarios remain bounded.
- Existing quality, RR, risk, margin, spread, daily-loss, geometry and protection gates remain mandatory.
- Indicator remains broker-mutation-free.
- No prediction or profitability guarantee is introduced.

## Latest verification correction — 2026-10-03

- The M3 sound-ownership audit was using a superseded method name (`ProcessQueuedAlertDelivery`).
- The production owner is `ProcessQueuedAlertSoundDelivery` inside `AlertDeliveryProcessor`, reached from the Indicator Calculate finally boundary and guarded by `IsLastBar`.
- Audit correction commit: `111b124857fb98061d3c31e0f6d889c19ea4176f`.
- Targeted inspection of the exact branch head confirms the AlertEngine has no direct `Notifications.PlaySound` call and the realtime Calculate path services the queued sound processor.
- GitHub did not start a new aggregate Source/Architecture workflow for the API-created audit-only commit; therefore aggregate CI is not claimed green here.

## Verification

Repository:
- Source/Architecture accumulated audit;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- dedicated live/realtime completion audit.

Target terminal:
- add/remove/reorder/restart Indicator binding;
- immediate CBOT LINK state transition;
- eligible signal -> panel rail -> audible cue;
- current market same-tick handoff;
- future Stop/Limit placement/fill/invalidation;
- multiple independent ScenarioIds;
- restart/reconnect/idempotency/protection recovery;
- live account arm OFF/ON behavior.

Operator action after merge:
`git pull --ff-only` on local `main`.

