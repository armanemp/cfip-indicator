# CBOT Broker-Confirmed Execution Facts — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

## Finding

Successful market execution was previously reported with PositionId and Entry only, while BrokerExecutionReport already exposes ConfirmedStop and ConfirmedTarget. This left the canonical broker-confirmed execution contract incomplete even though the actual Position object contained the live protection values.

In addition, the cBot waited for the next normal tick before refreshing reconciliation/state after a broker submission result. This could leave the panel temporarily behind the real broker state.

## Correction

Market BrokerExecutionReport now copies the actual Position EntryPrice, StopLoss and TakeProfit into ConfirmedEntry, ConfirmedStop and ConfirmedTarget.

After both pending and market submission results, the cBot now immediately:
- reconciles broker state;
- republishes the canonical cBot execution snapshot;
- reports a broker-confirmed state when the broker mutation returned success.

This keeps broker-confirmed facts and panel state aligned without adding a second execution authority.

## Pending orders

Pending-order placement remains represented as a pending broker object. ConfirmedEntry/Stop/Target are not fabricated from requested values because cTrader broker normalization/fill state is not the same thing as a submitted request.

When the pending order actually fills, the existing PendingOrders.Filled and Positions.Opened lifecycle events refresh reconciliation and state from the real broker Position.

## Preserved chain

Indicator analysis -> M15 decision -> M5 trigger/tuning -> M1 optional -> Entry/SL/TP/RR -> Scenario/Plan -> SignalEnvelope -> cBot preflight -> per-ScenarioId truth -> broker submission -> broker-confirmed facts -> reconciliation -> protection -> management -> history/panel.

## Safety

- live accounts remain blocked;
- cBot remains the sole broker mutation owner;
- M15 remains canonical trade decision/execution reference;
- M5 remains trigger/tuning/entry precision;
- M1 remains optional confirmation;
- no quality, RR, risk, margin, spread, market-hours, daily-loss or concurrency threshold is lowered;
- missing broker protection remains a recovery condition.

## Verification

Automated:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated broker-confirmed-facts audit.

Target terminal:
- successful market fill shows actual Entry/SL/TP in broker state and report;
- immediate panel state changes without waiting for a later tick;
- pending placement shows pending state;
- pending fill transitions to active state from broker events;
- missing/invalid broker protection remains fail-closed;
- a broker-created Position with incomplete protection is reported as RecoveryRequired rather than as a clean Confirmed execution.

Operator action after merge: git pull --ff-only on local main.