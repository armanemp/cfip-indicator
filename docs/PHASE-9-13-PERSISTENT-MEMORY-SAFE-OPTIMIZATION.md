# Phase 9.13 — Persistent Outcome Memory, Adaptive Risk & Optimization Routine

Date: 2026-09-29

## Status

VERIFIED COMPLETE on `phase/9-13-persistent-memory-safe-optimization`.

## Objective

Extend Phase 9.12 so the indicator can retain high-value outcome evidence across cTrader indicator restarts and use it safely for:
- recent contextual calibration;
- conservative automatic-risk reduction when recent realized performance deteriorates;
- repeatable optimization/audit coverage.

The objective is to improve signal quality and automatic execution discipline without adding a second decision authority, increasing default trading aggression, or introducing new public parameters.

## Persistent memory

The indicator now uses cTrader LocalStorage with `AccessRights.None`.

The memory is scoped by:
- symbol;
- chart timeframe;
- a deterministic fingerprint of key decision/risk configuration values.

The store:
- retains at most 128 outcomes;
- rejects entries older than 90 days on restore;
- persists broker-confirmed outcome observations immediately and on indicator destruction;
- rebuilds the existing direction/context calibration aggregates after restore;
- uses invariant numeric encoding and Base64 for free-text fields;
- ignores malformed, unknown-enum or invalid-position records.

This gives the indicator continuity after stop/start without enabling unrestricted file-system access. cTrader documents LocalStorage as persistent between deployments for cBots and indicators and notes that it uses the local file system in real-time while remaining in-memory during backtesting/optimisation. citeturn380699search2

## Adaptive risk

Added a downstream `AdaptiveOutcomeRiskPolicy`.

It does not change direction, signal eligibility, entry location or execution mode. It only reduces automatic risk when enough recent observations exist and both profitability dimensions deteriorate:
- fewer than 8 recent outcomes => neutral;
- recent window => latest 12 outcomes;
- negative average realized R plus weak win rate => bounded reduction;
- severe losing streak => bounded reduction;
- hard multiplier floor remains 0.25;
- when `EnableOutcomeTelemetry` is disabled, historical outcome data cannot influence automatic risk.

This is intentionally asymmetric: the system does not increase risk above the existing suitability-derived risk from a recent winning streak. That avoids turning a short lucky run into larger automatic exposure.

## Optimization routine

Added `tools/audit_optimization_readiness.py` and wired it into Source/Architecture CI.

The routine verifies:
- persistent memory uses the platform-safe LocalStorage boundary;
- memory is configuration-scoped and bounded;
- outcome persistence is connected to broker-confirmed close telemetry;
- adaptive risk is downstream of canonical suitability risk;
- adaptive risk has minimum-sample and hard-floor protection;
- public parameter additions remain zero.

The project workflow now treats optimization-readiness as a permanent phase-level audit rather than a one-time tuning exercise.

## Expected practical effect

Persistent memory itself does not create predictive power. Its value comes from preventing the model from forgetting its recent realized outcomes after restart.

The most direct expected effect is:
1. recent contextual calibration can resume immediately instead of starting from an empty runtime history;
2. conservative risk scaling can remember recent deterioration and avoid immediately returning to base exposure after restart;
3. optimization review becomes repeatable and auditable.

No win-rate or profitability improvement is claimed until target-terminal replay/historical evaluation demonstrates it.

## Safety boundary

- No new public parameter.
- Existing 552-parameter contract remains intact.
- No second decision authority.
- No second execution authority.
- No unrestricted file-system access.
- No automatic risk increase from winning streaks.
- Existing capacity, suitability, spread/risk, signal quality and broker protection gates remain upstream.

## External-file answer

A conventional local file is also technically possible in current cTrader/.NET 6: cTrader supports restricted file operations in a designated per-algorithm folder, and it also provides LocalStorage specifically for persistent algorithm data. citeturn380699search1turn380699search9

For CFIP, LocalStorage is the safer default because it avoids requiring unrestricted `FullAccess`. A future export/debug phase can add a human-readable CSV/JSONL artifact if inspection or offline research needs it.

## Verification

Pending:
- Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture + accumulated audits.

Target-terminal replay remains required for:
- persistence across actual indicator restarts;
- broker event ordering;
- realized R accuracy;
- signal timing and false-signal behavior;
- observed effect of recent calibration;
- observed effect of adaptive risk scaling.

## Next phase

Phase 9.14 — target-terminal replay, calibration/optimization measurement and evidence-driven parameter refinement.

 
## Verification closeout
 
- Runtime Acceptance #1107: PASS
- cTrader Compile/Build #1291: PASS
- Source/Architecture + accumulated audits #1298: PASS
- Decision Contracts including adaptive-risk checks: PASS
- Production parameter count: 552
- Final verified code head: `b742aae2dd1f94452f41743ad749374433dbc763`


## Merge closeout

PR #55 merged into `main` as `6246782125718af184568eb9339965e3f228c9a7`.
