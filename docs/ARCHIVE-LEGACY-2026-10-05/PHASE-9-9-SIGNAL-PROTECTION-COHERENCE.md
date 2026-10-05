# Phase 9.9 — Signal / Execution / Protection Coherence

Date: 2026-09-29

## Status

VERIFIED COMPLETE on branch `phase/9-9-signal-protection-coherence` at head `98ac6f0844abb1171f70d7d5eaa50e8fdf623134`.

Final automated verification on head `98ac6f0844abb1171f70d7d5eaa50e8fdf623134`: Runtime Acceptance #1048 PASS; cTrader Compile #1232 PASS; Source/Architecture #1239 PASS.
Target-terminal cTrader replay remains required for empirical signal timing, visual rendering, broker event behavior, partial-fill observation, duplicate-alert observation, protection recovery and realized trading outcomes.

## User-facing objectives

- Keep every actionable BUY/SELL presentation tied to one current decision/actionability contract.
- Prevent live quote refresh from reopening a setup whose M5 indicator fusion is stale, weak or conflicted.
- Keep partial take-profit execution broker/server-owned when the configured plan can express a valid TP1/TP2/final ladder.
- Prevent the polling loop, target progression and protection synchronizer from issuing duplicate TP mutations against a server-owned ladder.
- Preserve the chart-label contract: no text background/box is created; compact level text is white.

## Implementation

### 1. Canonical live indicator gate

Added `IndicatorActionabilityRule`.

Rules:

- `COMPRESSION` is blocked.
- `TREND` / `EXPANSION` / `HIGH_VOLATILITY` require fusion quality >= 60 and conflict <= 52.
- `RANGE` / `TRANSITION` use a slightly wider tolerance: quality >= 58 and conflict <= 55.
- Quality/conflict are clamped before evaluation.
- `TradeActionabilityEvaluator` rejects a live action when the current M5 frame is stale or fails this gate.

This is a live gate only; it does not create a second decision authority.

### 2. Server-side partial TP ladder

Added `TryBuildServerSideTakeProfitLadder`.

The ladder is used only when:

- partial TP is enabled;
- TP1 and TP2 are valid and progressive;
- the selected automatic target stage provides a final target beyond TP2;
- partial volumes normalize to valid broker volume;
- remaining volume stays valid;
- ATR-aware spacing remains structurally meaningful.

The broker receives:

- relative SL;
- relative TP1 partial;
- relative TP2 partial;
- final TP for the remaining volume.

If those conditions are not met, the existing local/manual partial-close path remains available. No new public parameter was introduced.

### 3. Single broker authority after ladder activation

When a server ladder is confirmed:

- local TP1/TP2 mutation is suppressed;
- local target progression is suppressed;
- protection synchronization does not overwrite the advanced TP ladder with a simple single-TP mutation;
- pending fills adopt the broker ladder;
- TP1/TP2 progress is observed from broker-reported remaining volume rather than synthesized close requests.

The broker remains authoritative for actual position mutation.

### 4. Label rendering

The existing common compact label renderer remains the single owner.

Verified contract:

- `ChartText` is used;
- `Color.White` is the text color;
- no rectangle is created by the label renderer/coordinator;
- legacy `*_BOX` objects are removed when encountered.

No label background was added.

## Safety boundary

No new public parameters, execution authority or broker identity were introduced.

The existing single-plan/single-managed-position architecture remains intact. Advanced protection is additive: it activates only when a valid server-side ladder can be built; otherwise the established protection path remains the fallback.

### 5. Corrective coherence hardening

The final pass added three fail-safe corrections:

- server TP partial protections use the documented relative constructor with a `double` volume; the obsolete `OrderVolume(...)` form is not used;
- a present but unavailable M5 indicator-fusion snapshot (quality 0 / conflict 0) fails closed instead of bypassing the actionability gate;
- once a managed pending order or live position exists, lower-priority decision/reaction presentation layers and their watch/reaction alerts are suppressed so they cannot compete with execution state.

These corrections do not change the decision authority or add public parameters; they tighten the existing authority boundaries.

## Verification

Required:

- Decision contract tests;
- Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture;
- target-terminal cTrader replay for empirical behavior, visual rendering, duplicate-alert behavior, realized partial fills, and protection synchronization.

CI does not establish win rate, false-signal rate, or realized RR improvement. Those remain empirical replay/live-validation questions.

## Operational note

PR #51 is ready for merge after the verified head above. Because `main` will advance on merge, local `main` must be pulled after PR #51 is merged.
