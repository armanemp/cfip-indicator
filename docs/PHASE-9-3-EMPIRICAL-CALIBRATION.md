# Phase 9.3 — Contextual Empirical Confidence Calibration

Date: 2026-09-29

Base: main @ 2ff94691cc465d8a2e776c737c1d61a9f5dcecd1 (Phase 9.2 close)

PR: #42 — merged to main at 445f2b77c5f60e3899937a923ac86f2db4f582b3

CI verification on the final pre-merge head: build PASS, verify PASS, runtime PASS.

## Objective

Replace the previous direction-only empirical confidence adjustment with a bounded, context-aware calibration layer driven only by broker-closed managed trade outcomes.

The calibration context is:

- direction (BUY/SELL),
- opportunity lane (Strategic/Tactical/Counter-HTF Tactical/Micro Reaction),
- detected M5 regime,
- deterministic confidence bucket:
  - <60,
  - 60–69,
  - 70–79,
  - 80–89,
  - 90–100.

## Decision semantics

The deterministic decision engine still owns the primary score, edge, quality and trigger semantics.

Calibration is applied only after top-down lane resolution:

1. Strategic when the HTF → midframe → entry calibration is complete.
2. The independently evaluated Tactical/Counter-HTF lane otherwise.
3. The existing decision remains the single execution authority.
4. Tactical opportunity discovery is not gated by the strategic calibration result.

The previous direction-only calibration path is explicitly disabled at the pre-context evidence stage, preventing historical direction aggregates from silently changing the raw decision before its lane/regime context exists.

The calibrated confidence remains bounded to 0–100 and is limited by the existing Calibration Max Confidence Adjustment parameter.

## Empirical estimator

The calibrator uses hierarchical fallback:

1. Exact direction + lane + regime + confidence bucket, when sample minimum is met.
2. Direction + lane + regime aggregate, when the exact bucket is too sparse.
3. Direction-only aggregate over calibration-eligible plans, when contextual history is still sparse.
4. No calibration when minimum sample requirements are not met.

A fixed prior strength of 8 observations toward a 50% baseline shrinks small/medium samples before converting the observed rate into the bounded confidence adjustment. The displayed outcome rate is explicitly labeled as observed historical win rate; it is not presented as a probability forecast.

## Outcome binding

Calibration context is captured on the executable Plan before entry.

Only managed positions that close through the broker lifecycle can add an empirical observation. Recovery-only plans created without a valid decision context are marked non-calibratable so they cannot contaminate the contextual dataset.

Pending fills preserve calibration metadata when a pre-existing pending plan carries it.

## Presentation

The canonical SignalVisualSnapshot now carries:

- base confidence,
- calibrated confidence,
- calibration adjustment,
- observed win rate,
- observation count,
- confidence bucket,
- calibration source.

The panel exposes these diagnostics as CAL / BASE / ADJ / OBS WIN / N plus the fallback source. No probability wording was introduced.

Existing Phase 9.2 multi-lane chart visuals, compact 40-bar level geometry, isolated opportunity IDs and solid line-color label backgrounds with contrast text are unchanged by this phase.

## Verification contracts

Deterministic decision contracts cover:

- exact contextual calibration,
- exact-bucket sparse fallback to lane/regime,
- contextual sparse fallback to direction history,
- minimum-sample suppression,
- bounded positive/negative symmetry,
- observed-rate reporting.

Repository verification completed on the merged implementation:

- cTrader compile,
- Decision contracts,
- Planning contracts,
- Execution contracts,
- Runtime acceptance contracts,
- Source / architecture,
- parameter/integrity audits.

No empirical win-rate improvement is claimed until target-terminal replay/historical evaluation provides measured results.

## Known limitation

The empirical history is currently in-memory for the indicator runtime. This phase deliberately does not invent a new persistence subsystem or external data store. Restarting the indicator therefore resets the calibration history; persistent walk-forward calibration remains a separate future phase.

## Boundary

This phase does not introduce simultaneous multi-position execution. Phase 9.2 already established multi-opportunity detection/presentation; broker execution remains the existing single-plan authority.

