# Alert Audio — Single Owner / Event-Specific Sound Policy — 2026-10-04

## Objective

Every user-facing Indicator alert event must have one canonical audio decision. The
same causal event must not produce duplicate sounds, and no renderer or delivery
component may invent a second sound classification.

## Canonical ownership

The only production decision owner is:

`src/CFIP.Indicator/Core/Runtime/AlertSoundPolicy.cs`

The delivery path is:

`SendUnifiedAlert → AlertSoundPolicy → AlertDelivery → AlertDeliveryQueue → AlertDeliveryProcessor → cTrader Notifications.PlaySound`

`AlertEngine` creates the canonical envelope and stores the policy-selected sound
type and sound-group key in the immutable delivery event. `AlertDeliveryProcessor`
does not classify alert keys; it only suppresses an already-played canonical group
and performs physical delivery.

## Canonical semantic cue map

| Event family | Cue |
|---|---|
| ACTION / HIGH / SMART | PositiveNotification |
| TP / PARTIAL / POSITION-OPEN / confirmed pending fill | PositiveNotification |
| WATCH / EARLY / DAYEND / AUTO-OFF / PLANUPDATE / OUTCOME-TIMEOUT | Announcement |
| REACTION / AUTO-REACTION / BOS / MSS/CHOCH / LIQUIDITY SWEEP | Doorbell |
| SL / invalidation / restriction / reversal / protection failure / fill mismatch / daily-loss lock / execution failure | NegativeNotification |
| Any critical event without a more specific mapping | Confirmation |
| Non-critical unmapped event | Announcement |

cTrader exposes these five built-in notification sound types and also supports
file-based playback through `PlaySound(fileName)`.

## Deduplication contract

WATCH → REACTION → ACTION stages belonging to the same symbol, closed M5 event and
direction share one canonical signal sound group. The first audible delivery wins;
later stages remain visible in the alert rail but do not replay the sound.

Blocked/restricted candidates remain silent on the normal signal-audio path.

## Custom sound contract

When semantic sounds are enabled, the event-specific semantic cue is authoritative;
the old single `SoundFilePath` cannot override it.

When semantic sounds are disabled, the configured sound type / custom file remains
available as the explicit user override.

## No-duality audit

The phase must fail review if any of the following reappear:

- a second `Resolve...Sound...` classifier outside `AlertSoundPolicy`;
- sound-family classification in `AlertDeliveryProcessor`;
- direct `Notifications.PlaySound` from `AlertEngine`;
- a second Indicator alert delivery owner;
- blocked candidates reaching audible delivery;
- a second cBot lifecycle/execution audio owner.

cBot lifecycle/execution audio remains separately owned by
`CbotLifecycleAudioService`, because it is a different execution-domain owner;
Indicator signal audio remains Indicator-owned.
