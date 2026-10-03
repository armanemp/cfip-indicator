## 2026-10-03 — cBot Broker-Confirmed Execution Facts

Status: implementation complete, verification pending.

Deep cBot lifecycle audit found two gaps:
- market BrokerExecutionReport exposed ConfirmedStop/ConfirmedTarget but the coordinator left them null even when the real Position had protection values;
- cBot state/reconciliation was not refreshed immediately after a broker submission result, so panel state could lag until the next tick.

Correction:
- market report now carries actual Position EntryPrice/StopLoss/TakeProfit;
- market and pending submission results immediately reconcile and republish cBot state;
- pending request values are not mislabeled as broker-confirmed fill facts;
- a market Position with missing/invalid broker protection is explicitly surfaced as RecoveryRequired while idempotency remains confirmed to prevent duplicate execution;
- pending-fill/position-open events remain the source of live broker Position truth.

No execution/risk/quality threshold was lowered.

Verification required:
Source/Architecture, Runtime Acceptance, cTrader Compile/Build, dedicated broker-confirmed-facts audit and target-terminal fill/pending/panel validation.

Phase record: docs/PHASE-CBOT-BROKER-CONFIRMED-FACTS-2026-10-03.md.

Operator action after merge: git pull --ff-only.
