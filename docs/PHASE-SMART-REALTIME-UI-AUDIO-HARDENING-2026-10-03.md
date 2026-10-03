# CFIP — Smart Realtime UI / Audio / Panel Hardening — 2026-10-03

Status: IMPLEMENTED — automated verification pending; target-terminal validation pending.

## Purpose

Close the presentation/runtime seams reported during realtime trading development without introducing another decision or execution authority.

## Implemented

### Higher-timeframe trend arrow intelligence

- Added one canonical HtfTrendArrowStrengthRule under Core/Math.
- The rule maps aligned H1/H4/D1/W1 trend evidence into exactly nine presentation levels.
- Levels 1–3 use the existing caution color, 4–6 the existing confirmed color and 7–9 the existing strong color.
- The renderer preserves the original single-arrow behavior when HTF evidence is unavailable and otherwise renders one, two or three vertically separated arrows from the same canonical marker owner.
- BUY/SELL direction still comes from the existing authoritative signal snapshot; HTF strength changes presentation intensity only.

### Panel header truth/freshness

- Added a single PanelHeaderLiveState owner for header text and status color.
- The header is refreshed from the lightweight heartbeat even when ShouldRenderFullPanel() correctly skips the expensive full layout.
- The header includes the current canonical signal status plus M15/H1 directional context and a live UTC clock.
- Initialization header content is now also routed through the same owner.
- Rebuilding/hiding the panel invalidates the header cache so a newly created header cannot inherit a stale cache key.
- PanelSurfaceAndHeaderLayout no longer owns header content; it owns geometry/style and delegates content to the live-header owner.

### cBot lifecycle/execution audio

- Added CbotRuntimeAudioCoordinator as the cBot-owned audio module.
- cBot start and stop now produce lifecycle cues.
- Explicit live-disarmed startup produces a negative safety cue.
- Broker execution confirmation/rejection and cBot blocked state produce corresponding cues.
- Indicator signal audio remains owned by AlertDeliveryProcessor; cBot lifecycle/execution audio does not duplicate that signal authority.

### Diagnostics

- Corrected misleading cBot execution log format placeholders for revision/session duration.
- Added a dedicated Source/Architecture regression audit for this phase.

## Preserved architecture

The production chain remains:

history/outcomes → pre-analysis → all MTF frame evidence → M15 canonical decision → M5 setup/trigger/tuning → optional M1 confirmation → current quote actionability → current Market/Aggressive OR future Stop/Limit → cBot preflight → broker mutation → broker confirmation → protection/management → outcome/history.

All existing live/realtime contracts remain in force:

- live execution is explicitly armed and OFF by default;
- current opportunities are the only inputs eligible for immediate market execution;
- future opportunities are routed to broker pending Stop/Limit actions;
- ScenarioId idempotency and bounded concurrency remain mandatory;
- stagnant RANGE/COMPRESSION reward floors remain mandatory;
- Indicator remains broker-mutation-free;
- existing quality, RR, risk, spread, margin, daily-loss, geometry and protection rules remain mandatory;
- no threshold was lowered merely to increase trade frequency;
- M15 remains the canonical decision reference, M5 remains entry precision/tuning and M1 remains optional closed-trigger confirmation;
- HTF frames remain context/reward evidence rather than a second decision engine.

## Verification

Repository checks required:

- accumulated Source/Architecture audit;
- dedicated smart realtime UI/audio audit;
- Runtime Acceptance Contracts;
- cTrader Compile/Build.

Target-terminal checks required:

- cTrader chart add/remove/reorder/restart binding;
- realtime header freshness while full-panel rendering is unchanged;
- 1/2/3 stacked-arrow transitions across HTF strength levels;
- eligible Indicator signal sound;
- cBot Start/Stop sound;
- live account with arm OFF must stay broker-mutation-free;
- live account with arm ON must pass all existing safety gates;
- same-tick market handoff/current-entry drift behavior;
- simultaneous distinct ScenarioIds;
- future Stop/Limit placement/fill/invalidation;
- restart/reconnect/idempotency/protection recovery.

cTrader Algo exposes Notifications.PlaySound to algos including Robots and Indicators; the official reference notes that sound playback does not work during backtesting/optimization, so actual audible behavior remains a target-terminal acceptance item.

Operator action after the phase is merged to the local main line:

    git pull --ff-only