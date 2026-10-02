# CI-21 — Primary M15 Signal Visibility / M5 Entry Tuning

Date: 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending on branch `phase/ci-21-primary-m15-signal-visibility`.**

## Objective

A visually obvious M15 setup must not disappear merely because the downstream entry plan is not yet materialized. The canonical architecture remains:

- **M15 = primary execution / trade-decision timeframe**
- **M5 = trigger, entry tuning and microstructure defence**
- **M1 = optional confirmation only**
- H1+ remains higher-timeframe context/reward support.

## Root causes confirmed

1. The M15/H1 timeframe candidate path called the full `BuildLaneCandidate` path before validating the primary-timeframe contract. Geometry, execution-model, preview and reward-path failures could therefore remove the whole primary setup before presentation.
2. Primary candidates were also subjected to the generic parallel-candidate quality margin, which could hide a valid source-quality M15/H1 setup even when it was above the primary-source floor.
3. A presentation-only primary setup had no explicit execution boundary, so the implementation needed a deterministic fail-closed marker to prevent any accidental reuse for broker execution.

## Canonical corrections

- Primary M15/H1 source validation now occurs before plan construction.
- Qualified primary source candidates can survive missing/temporarily invalid downstream plan geometry as **presentation-only** candidates.
- Presentation-only candidates show the primary setup marker but do not render invalid zero-valued entry/SL/TP levels.
- The execution policy explicitly rejects presentation-only candidates.
- Primary display uses the existing source-quality floor (`max(60, TacticalOpportunityMinimumQuality - 5)`) and does not receive the extra generic parallel quality margin.
- No public confidence, RR, risk, spread or strategy threshold is lowered.
- The existing M5 trigger/tuning and actionability gates remain the authority for executable entry readiness.

## Verification

Deterministic/runtime and source-level coverage added for:

- M15/H1 source validation ordering;
- presentation fallback ownership;
- primary display-quality floor;
- explicit execution rejection of presentation-only state.

Manual cTrader validation remains required for target-terminal marker timing, live M15/M5 behaviour, alert timing, panel coherence and empirical signal frequency/quality.

## Operator action

When this branch is merged to `main`, run:

```bash
git pull --ff-only
```
