# Phase 11.4 — Plan Reward/Risk Quality, Multi-Scenario Coverage & Execution Hardening

Date: 2026-09-30

## Scope

This phase addresses the concrete runtime-quality problem reported after Phase 11.3:

- setups with an oversized stop relative to the available reward path;
- low TP1 reward relative to risk;
- tactical/parallel lanes using weaker RR rules than the canonical plan;
- simultaneous scenarios being collapsed because their prices are close;
- final automatic market/aggressive/pending mutations not consuming one shared reward-risk contract.

The phase also keeps the existing authority boundaries intact.

## Implementation

### 1. Unified reward/risk contract

Added PlanRewardRiskQualityRule.

For every actionable plan candidate it validates:

- Entry/SL/TP1 directional geometry;
- stop risk in ATR;
- nominal TP1 RR;
- adaptive required RR when stop width exceeds the preferred structural risk;
- effective RR after including current spread;
- maximum stop-risk boundary.

The result is reusable by planning, live actionability, parallel candidates and the final broker submission validators.

This is a contract-unification change. It does not claim that the resulting thresholds are empirically optimal.

### 2. Stop selection becomes reward-path aware

Structural stop candidate selection now estimates whether each valid structural stop leaves a viable TP1 reward path before accepting the stop candidate.

Candidates that consume too much risk for the available reward path are rejected earlier. Among valid candidates, additional score is given to stop choices that leave more reward headroom.

This keeps the stop structural rather than replacing it with a pure distance optimizer.

### 3. Tactical/parallel RR bypass removed

MinimumPlanRiskReward now requires tactical, counter-HTF and micro lanes to respect the canonical Tp1MinimumRR / regime minimum before the lane-specific RR is considered.

Therefore a parallel scenario cannot create a lower-quality plan merely because it belongs to a tactical lane.

### 4. Multi-scenario coexistence

Scenario deduplication now collapses only the exact same scenario identity.

Different source timeframes, directions or lanes are kept distinct even when their price levels are nearby.

When the visible-candidate cap is reached, selection first preserves distinct (SourceTimeframe, Lane, Direction) coverage and then fills remaining slots by display priority.

### 5. Execution-path diagnostics

Signal trace schema v2 now records PlanRiskAtr, EffectiveTp1RR and RequiredTp1RR.
This makes weak-plan rejection measurable instead of only textual. Older schema v1
traces remain readable by the forensic analyzer.

### 6. Deterministic target-level cache

Repeated same-input target-level construction across multi-scenario evaluation is now
cached by closed M5, direction, entry and ATR. Callers receive a shallow copy so the
cache cannot become mutable shared decision state.

This optimization targets CPU/runtime cost without changing signal authority or live
quote semantics.

### 7. Final automatic execution gates

The same reward-risk contract is checked again immediately before:

- automatic market broker mutation;
- aggressive market broker mutation;
- pending-order broker mutation.

Existing permission, capacity, suitability/news, spread/risk, volume, submission identity, broker confirmation and lifecycle gates remain intact.

No safety gate was removed.

## Whole-chain ROUTINE execution

This phase explicitly re-reviewed:

Analysis -> Decision -> Signal -> Alert -> Execution -> Broker confirmation -> Protection/Lifecycle -> Outcome -> Learning

The review covered:

- MTF/top-down;
- structure/liquidity;
- OB/FVG and OB+FVG;
- WaveTrend and divergence;
- indicator fusion/conflict;
- regime;
- false/missed/stale/duplicate cohorts;
- Entry/Ideal Entry/Trigger;
- SL and TP1..TP4;
- reward path and RR;
- market/aggressive/pending execution;
- history/runtime telemetry;
- existing Swing canonical ownership.

No second swing authority was introduced.

## Multi-scenario execution boundary

The project now has stronger scenario separation and reward-risk gating, but independent timeframe scenarios still do not become a second broker execution authority merely by existing.

The canonical execution model remains M5-based unless a future phase introduces a separately tested scenario-plan materialization and broker policy.

This avoids pretending that a different source timeframe currently has independently validated broker geometry.

## Verification boundary

Required:

- Phase 11.4 source audit;
- accumulated source/architecture audits;
- Runtime Acceptance;
- cTrader Compile/Build;
- target-terminal replay for actual broker behavior;
- real runtime log/outcome study before any empirical performance claim.

No profitability or prediction-accuracy claim is made from source inspection alone.
