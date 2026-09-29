# Phase 9.6 — Signal Quality & Visual Coherence

Date: 2026-09-29

## Status

VERIFIED COMPLETE on `phase/9-6-signal-quality-visual-coherence`. Runtime Acceptance, cTrader Compile/Build, and Source/Architecture all passed on the final implementation head before merge.

Pre-merge workflow runs: Runtime #988, cTrader compile #1172, Source/Architecture #1179.

Target cTrader replay remains required for empirical visual/signal-quality validation.

## Problem statement

The target-terminal feedback identified four remaining coherence problems:

1. low-quality setups could still reach the final actionable state after live quote re-evaluation;
2. weak WATCH / reaction / prediction states could clutter the chart;
3. the directional signal marker could appear as a diamond;
4. compact level-label backgrounds could drift away from their text and have inconsistent proportions.

## Implemented scope

### 1. Final actionable signal-quality gate

A pure deterministic `ActionableSignalQualityRule` now evaluates an already-actionable decision against stricter final thresholds. The gate is applied both:

- when the closed-M5 decision first becomes actionable;
- when live quote re-evaluation refreshes `ActionableNow`.

The second application is intentional: it prevents Phase 9.5.1 live actionability refresh from re-opening a signal that the closed-bar decision path had already rejected on final quality.

The gate checks confidence, Smart Quality, MTF agreement, independent evidence, structural confirmations, entry location, entry timing, price position and TP1 RR.

It does not create a new direction authority.

### 2. Weak signal presentation suppression

Directional WATCH arrows now require strong decision-quality evidence instead of appearing for any non-zero visual direction.

Reaction presentation now requires the existing strong reaction threshold and minimum evidence.

Prediction presentation now requires a higher presentation confidence floor tied to the existing early/minimum-confidence settings.

Parallel opportunity presentation receives a small additional quality floor while preserving the existing Strategic/Tactical/Counter-HTF/Micro lane model.

No broker execution semantics were changed.

### 3. Signal marker semantics

The main directional signal path uses `UpArrow` / `DownArrow`.

The M1 trigger marker remains explicitly non-directional and is rendered as `Circle`, rather than `Diamond`, so it cannot be mistaken for a second directional entry signal.

### 4. Compact level-label geometry

Compact labels now:

- cap the label anchor offset so it stays attached to the level line;
- position the rectangle from just before the text through a bounded compact width;
- use a smaller 8.5pt label font;
- clamp rectangle height to a narrow readable range;
- normalize the rectangle price bounds.

Parallel opportunity labels reuse the canonical anchor instead of maintaining a separate drifting geometry.

## Acceptance boundary

Required automated checks:

- Decision Contracts / Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture.

Target cTrader replay remains required for empirical confirmation of:

- actual chart-label placement across zoom levels;
- visual distinction between WATCH, ACTION, and M1 trigger markers;
- observed false-signal frequency;
- signal timing;
- realized RR / execution quality.

No profitability, win-rate or false-signal reduction claim is inferred from static or contract checks.

## Continuity

No new public parameter was introduced.

No second decision engine, broker mutation authority, or multi-plan execution authority was introduced.

The phase continues directly from verified Phase 9.5.1.
