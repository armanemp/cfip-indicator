# CBOT Effective Lifecycle State — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

## Finding

Broker reconciliation intentionally emits descriptive lifecycle states such as ACTIVE / RECONCILED and ACTIVE / MULTI-SCENARIO. The cBot state publisher previously accepted only the exact strings READY, ACTIVE and PENDING when computing the effective execution flags consumed by the Indicator panel.

Therefore a cBot could be armed and healthy while the published effective auto-trading state was false solely because the lifecycle state carried a descriptive suffix.

This mismatch directly affected the visible Auto Trader / Auto Orders state and also created a split semantic owner for lifecycle authorization.

## Correction

A canonical CbotExecutionLifecycleRule now owns the semantic:

- READY and READY / ... are execution-eligible state families;
- ACTIVE and ACTIVE / ... are execution-eligible state families;
- PENDING and PENDING / ... are execution-eligible state families;
- blank/UNKNOWN/RECOVERY REQUIRED remains fail-closed;
- recoveryRequired always overrides lifecycle eligibility.

CbotExecutionStatePublisher now consumes this single rule instead of duplicating lifecycle string comparisons.

Behavioral coverage was added for READY / RECONCILED, ACTIVE / RECONCILED, ACTIVE / MULTI-SCENARIO, PENDING / MULTI-SCENARIO, RECOVERY REQUIRED and UNKNOWN.

## Preserved execution chain

Indicator analysis -> M15 decision -> M5 trigger/tuning -> M1 optional -> Entry/SL/TP/RR -> SignalEnvelope/ScenarioBatch -> cBot preflight -> per-ScenarioId reconciliation -> effective lifecycle state -> execution -> broker confirmation -> protection -> management -> history/panel state.

## Safety

- live accounts remain blocked;
- cBot remains the sole broker mutation owner;
- M15 remains canonical trade decision/execution reference;
- M5 remains trigger/tuning/entry precision;
- M1 remains optional confirmation;
- no quality, RR, risk, margin, spread, market-hours, daily-loss or concurrent-scenario threshold is lowered;
- this phase changes state semantics only.

## Verification

Automated:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- dedicated lifecycle effective-state audit and behavioral test.

Target terminal:
- cBot armed + ACTIVE / RECONCILED;
- cBot armed + ACTIVE / MULTI-SCENARIO;
- recovery state must force effective execution OFF;
- panel must reflect the published effective state rather than local Indicator-side guesses.

Operator action after merge: git pull --ff-only on local main.
