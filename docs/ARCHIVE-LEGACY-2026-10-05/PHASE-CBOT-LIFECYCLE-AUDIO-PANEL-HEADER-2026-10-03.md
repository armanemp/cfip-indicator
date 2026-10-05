# cBot Lifecycle Audio + Realtime Panel Header — 2026-10-03

Status: IMPLEMENTED — verification pending.

## Scope

This phase closes two user-visible runtime seams without mixing responsibilities:

- cBot owns lifecycle/execution audio for Start, Stop, confirmed execution, rejection and recovery;
- Indicator owns the live panel header presentation and refreshes it from current signal/cBot state.

## Audio ownership

Indicator signal audio remains in `AlertDeliveryProcessor`.

cBot lifecycle/execution audio is isolated in:

`src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs`

The service is debounced and failure-tolerant. It does not mutate broker state.

## Panel header ownership

The panel header is isolated in:

`src/CFIP.Indicator/UI/Panel/PanelHeaderRenderer.cs`

The header refresh consumes current canonical signal state and fresh cBot heartbeat/presence state. It updates only when the displayed value changes to avoid unnecessary UI work.

## Verification

Required:

- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile;
- lifecycle/audio audit;
- panel-header audit;
- target cTrader verification for actual sound playback and realtime text freshness.

No strategy threshold or public trading parameter is changed.

Operator action after verified merge:

`git pull --ff-only` on local `main`.
