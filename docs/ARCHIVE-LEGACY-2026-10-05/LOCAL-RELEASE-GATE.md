# CFIP Indicator — Local Release Gate

## Purpose

This document is the operational gate for completing the local cTrader product before any Cloud migration work begins.

The local product is the reference implementation. Cloud work must not be used to hide an unresolved local execution, broker, lifecycle, or runtime defect.

## Release order

```
Repository gates
    ↓
Target cTrader compile
    ↓
Hands-on terminal baseline
    ↓
Automatic market execution
    ↓
Automatic pending orders
    ↓
Protection / lifecycle / recovery
    ↓
Cross-path execution stress
    ↓
Analytical accuracy / anti-lookahead
    ↓
Backtest / replay / outcome
    ↓
Calibration / regime / no-trade intelligence
    ↓
Local production hardening
    ↓
LOCAL RELEASE
    ↓
Cloud portability
    ↓
Cloud cBot host
```

Adaptive Learning is intentionally outside the local release gate. No online learning, automatic parameter mutation, or model training is required for local release.

## Safety rule

Real-money validation is never the first test of a new execution path.

Use this progression:

1. Repository/static/contract checks.
2. cTrader compile.
3. Demo account.
4. Minimum practical volume.
5. One managed position/order at a time.
6. Controlled live validation only after demo acceptance.

Any material broker rejection, protection failure, unexpected duplicate order, stale state, synthetic position/pending state, lifecycle desynchronization, or unexplained order mutation blocks progression.

## Local preflight

Record the exact:

- cTrader application version;
- Automate API assembly used for compilation;
- broker/server/account environment;
- symbol and symbol contract;
- account currency;
- leverage;
- minimum/step volume;
- spread/commission model where known;
- terminal architecture and OS;
- CFIP configuration hash or exported parameter set.

The tested terminal must use the same production assembly produced from the repository target.

## Execution acceptance matrix

| Scenario | Required evidence | Pass condition |
|---|---|---|
| BUY market | request, broker result, confirmed position | one correct managed position; no synthetic state |
| SELL market | request, broker result, confirmed position | one correct managed position; no synthetic state |
| Market rejection | broker error/result and post-state | no position adopted |
| Market slippage | requested vs actual fill | envelope policy is applied |
| Accepted fill + missing protection | broker position + protection state | explicit RECOVERY until protection is valid |
| BUY STOP | request, broker pending confirmation | one confirmed pending order |
| SELL STOP | request, broker pending confirmation | one confirmed pending order |
| BUY LIMIT | request, broker pending confirmation | one confirmed pending order |
| SELL LIMIT | request, broker pending confirmation | one confirmed pending order |
| Pending rejection | broker result and post-state | no synthetic pending state |
| Pending fill | pending event + position confirmation | transition only after broker confirmation |
| Pending cancellation | cancellation event + broker state | no stale pending state |
| Expiration | expiration event + broker state | no stale pending state |
| Initial SL/TP | requested and broker-confirmed values | directionally valid protection |
| Break-even | mutation + broker state | safe protected level |
| Profit lock / trailing | mutation + broker state | never crosses unsafe market side |
| Partial TP | close request + confirmation | remaining position state is broker-confirmed |
| Close | close request + confirmation | closed state only after confirmation |
| Restart | startup reconciliation | managed state rebuilt from broker state |
| Reconnect | recovery sequence | no duplicate entry or lifecycle transition |
| Duplicate lifecycle event | repeated event | idempotent result |
| End-of-day | policy evaluation + broker state | intended close/cancel behavior only |
| Invalidated plan | invalidation event + broker state | no unauthorized new entry |

## MTF and analytical acceptance

At minimum verify M1, M5, M15, M30, H1, H4, D1 and W1.

For every test, confirm:

- only closed-bar references are used for decision inputs;
- MTF timestamps are aligned;
- no future bar/value is visible to the decision;
- changing the chart timeframe does not corrupt higher/lower timeframe state;
- BUY and SELL paths use symmetric evidence rules.

## Runtime stability acceptance

Run a representative local session and record:

- startup time;
- memory after initialization;
- memory after normal operation;
- memory after extended operation;
- CPU during normal ticks;
- CPU during a new-bar analysis cycle;
- object/chart count growth;
- log/error rate;
- recovery frequency.

Any unbounded resource growth or recurring exception blocks local release.

## Evidence package

Keep a dated local acceptance record containing:

- screenshots where visual behavior matters;
- terminal logs;
- broker order/position history;
- requested vs accepted order values;
- protection state before/after mutations;
- restart/reconnect evidence;
- CPU/memory observations;
- pass/fail result for every required scenario.

The repository CI result is necessary but cannot replace terminal/broker evidence.

## Release blockers

The following are hard blockers:

- rejected broker mutation becoming internal position/pending state;
- missing or invalid broker SL represented as protected state;
- duplicate automatic entry;
- pending order treated as a position before fill confirmation;
- close/partial-close represented as complete before broker confirmation;
- lifecycle state diverging from broker-confirmed state;
- MTF future leakage;
- directionally invalid SL/TP;
- execution path bypassing the managed identity;
- unexplained resource growth;
- any manual entry control that can bypass the decision/risk chain.

## Local release boundary

Local Release is complete only when all repository gates pass and every required hands-on cTrader scenario has recorded PASS evidence on the target terminal/broker environment.

Cloud migration is a separate milestone. Because cTrader Cloud execution is available for cBots rather than custom indicators, Cloud migration must introduce a cBot host around the CFIP engine rather than pretending the current Indicator itself is cloud-runnable.
