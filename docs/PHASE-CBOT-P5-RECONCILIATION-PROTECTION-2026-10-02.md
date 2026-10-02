# CBOT-P5 — Protection / Lifecycle / Recovery — 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending on branch `phase/cbot-p5-reconciliation-protection`.**

## Objective

Complete the cBot lifecycle/recovery boundary without moving analytical decision logic back into the cBot or creating another execution engine.

Canonical chain remains:

`Pre-analysis → M15 decision → M5 trigger/tuning → entry plan → contract → cBot safety → broker → broker-confirmed lifecycle`

## Completed

- added a dedicated cBot broker reconciliation owner;
- startup/reconnect now performs an explicit broker-state reconciliation before accepting new execution;
- multiple managed positions/pending orders and simultaneous position+pending states are classified as recovery-required rather than silently accepted;
- broker positions missing SL/TP, or carrying wrong-side protection/target geometry, are classified as recovery-required;
- unresolved recovery blocks all new execution;
- when management execution is armed and the latest Indicator signal contains the canonical execution intent, the existing `ManagementExecutionCoordinator` can recover missing protection without introducing a second mutation owner;
- recovery uses the same cBot management mutation/confirmation owner and scenario/identity lineage;
- cBot execution-state contract now publishes lifecycle/protection/recovery fields;
- Indicator panel remains read-only and now surfaces recovery truth.

## Important safety boundaries

- cBot remains demo-only; live accounts remain blocked.
- No analytical M15/M5 decision rule is duplicated in cBot.
- No broker mutation is moved back into Indicator.
- Presentation and execution remain separate.
- Recovery never overwrites a healthy broker protection state.
- Missing or ambiguous broker state fails closed.

## Manual boundary

Repository checks cannot prove broker behavior on a live target terminal. Manual validation remains required for restart/reconnect, existing managed position adoption, missing-protection recovery, pending lifecycle and panel state latency.

## Operator action after merge

Run:

```bash
git pull --ff-only
```
