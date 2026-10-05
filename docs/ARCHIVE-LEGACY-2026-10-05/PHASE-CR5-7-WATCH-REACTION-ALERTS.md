# CR5.7 / E7 — Decision-owned WATCH/REACTION alerts

Date: 2026-10-01

Status: IMPLEMENTATION COMPLETE — repository verification pending CI

## Objective

Separate WATCH and REACTION alert qualification/emission from chart rendering so alert behavior does not disappear when visual rendering is disabled, skipped, optimized or otherwise unavailable.

## Source finding

The pre-existing WATCH alert path lived inside `SignalRenderer.RenderWatchAndReaction`. That renderer returned immediately when `snapshot.ActionableNow` was false, while the WATCH condition itself required `!decisionReady` where `decisionReady` was equivalent to actionable state. This made the WATCH alert branch unreachable for its intended non-actionable early-WATCH state.

The REACTION alert also lived in the renderer and was coupled to presentation lifecycle. Its qualification depended on `ShowReactionArrow`, and the renderer was not invoked for some active-plan presentation states.

## Implementation

### Core authority

Added `Core/Math/WatchReactionAlertRule.cs` as the platform-neutral owner for:

- early-WATCH confidence floor and four-point allowance;
- strong-WATCH qualification;
- decision-owned WATCH alert eligibility;
- REACTION alert eligibility;
- deterministic WATCH/REACTION alert identities.

The established numeric values remain unchanged: confidence floor 60 and gap 4.

### Alert ownership

Moved WATCH/REACTION emission into `Runtime/Calculation/CalculationDecisionAlerts.cs` through a dedicated decision-owned stage.

The stage:

- refreshes live actionability before alert qualification;
- requires broker-state reconciliation for the current cycle;
- suppresses alerts when the decision is blocked;
- suppresses duplicate alerts for the same M5 identity;
- suppresses alerts when an existing plan, pending order or live position already owns the state;
- preserves live REACTION cadence after `UpdateLiveReaction` without depending on the presentation renderer.

`SendUnifiedAlert` remains the transport/delivery owner for sound, email and popup behavior.

### Presentation

`UI/Chart/SignalRenderer.cs` is now presentation-only for WATCH/REACTION. It no longer emits alerts or owns alert deduplication.

`SignalPresentationRenderer.IsStrongWatchSnapshot` now consumes the canonical Core WATCH rule instead of duplicating the threshold formula.

### Verification

Added deterministic Runtime Contracts coverage for:

- confidence floor/gap;
- WATCH BUY/SELL symmetry;
- strong-WATCH threshold rejection;
- blocked/non-actionable/plan/pending/live guards;
- REACTION BUY/SELL symmetry;
- alert enablement and live-reaction guards;
- deterministic direction-distinct alert identities.

Added `tools/audit_phase_5_7.py` and wired it immediately after E6 in the accumulated Source/Architecture workflow.

## Safety boundary

- No public `[Parameter]` name, type or `DefaultValue` changed.
- No RR, confidence, stop, target, actionability or execution threshold was tuned.
- No second decision authority or broker execution authority was introduced.
- Blocked signals remain silent through the canonical alert eligibility boundary.
- Target-terminal timing, popup/audio delivery timing, broker lifecycle and empirical signal-quality behavior remain manual acceptance boundaries.

## Next phase

**CR5.8 / E8 — Small constant ownership and TargetSelection consistency.**
