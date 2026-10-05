# Phase 8.1 — M1 Trigger Correctness

Date: 2026-09-29

## Objective

Make UseM1Trigger a real M1 confirmation path rather than a small directional vote added to the higher-timeframe score.

The authoritative chain is:

closed M5 context → MTF directional decision → closed M5 trigger → optional closed M1 confirmation → plan creation → execution

M1 does not become a second decision engine and does not override the higher-timeframe directional consensus.

## Findings

Before this phase:

- DecisionScoreCalculator added a fixed +3 to BUY or SELL from _m1Frame.Direction when UseM1Trigger was enabled.
- DecisionEvaluator then calculated TriggerReady only from ClosedBarTriggerReady, which evaluated M5.
- Therefore an M1 direction could change the selected decision without being required to satisfy an independent M1 trigger condition.

This coupling allowed temporal/causal disagreement between the direction source and the trigger source.

## Implementation

### 1. Remove M1 from directional scoring

DecisionScoreCalculator no longer uses M1Frame.Direction as a fixed directional vote.

M1 is now a confirmation input, not another weighted timeframe vote.

### 2. Add deterministic M1 trigger rule

Core/Math/M1TriggerRule.cs owns platform-neutral M1 trigger semantics.

A qualifying M1 confirmation requires:

- valid selected direction;
- M1 frame direction equal to the selected decision direction;
- the M1 bar is fully closed and belongs to the same closed M5 window;
- a non-zero ATR/range;
- candle body meets the configured ATR minimum;
- candle range does not exceed the configured ATR maximum;
- candle body direction matches the selected direction;
- close location meets the configured minimum;
- M1 trigger score meets the same selected trigger threshold policy;
- at least one causal catalyst exists: a buffered micro-structure break, or configured displacement of at least DisplacementAtr.

The BUY and SELL rules are mirrored.

### 3. Add real M1 bar evaluator

Planning/Entry/M1TriggerReadyEvaluator.cs binds the pure rule to the actual M1/M5 cTrader data:

- uses the canonical MtfClosedContext.M1 index;
- verifies actual M1/M5 bar-time containment;
- uses the actual M1 OHLC and ATR values;
- computes BullTriggerScore / BearTriggerScore from M1 bars;
- rejects stale/misaligned M1 evidence.

### 4. Causal trigger evaluation

The micro-structure window is derived from the existing SwingStrength setting and bounded to 3–8 prior M1 bars. The canonical StructureBreakAtr buffer is applied symmetrically. Displacement uses the existing DisplacementAtr policy; no new parameter or alternate trigger authority is introduced.

### 5. Keep execution causally gated

When UseM1Trigger is enabled:

TriggerReady = ClosedM5TriggerReady AND M1TriggerReady

When it is disabled:

TriggerReady = ClosedM5TriggerReady

Direction remains the MTF decision result. Plan creation remains downstream of TriggerReady.

## Why this is safer

The M1 path can no longer vote a direction and then disappear from the actual trigger test.

A directional decision must first pass the established M5 closed-bar trigger. When M1 confirmation is enabled, the same selected direction must also be confirmed by a real, fully closed M1 bar inside that exact M5 candle, with causal micro-structure or displacement evidence rather than indicator-only agreement.

This reduces temporal mismatch without introducing a second decision authority or indiscriminately changing global confidence thresholds.

## Deterministic contracts

Runtime acceptance now covers:

- exact M1/M5 window containment;
- rejection of an M1 bar that is outside the selected M5 window;
- rejection of an M1 bar that is not fully closed at the reference;
- BUY/SELL symmetry;
- opposite-direction rejection;
- weak-body rejection;
- poor-close-location rejection;
- insufficient M1 trigger-score rejection.

Source/architecture verification additionally enforces:

- one M1 trigger rule owner;
- real M1 OHLC/ATR/trigger-score consumption;
- recent M1 micro-structure and canonical displacement policy consumption;
- causal catalyst gate is enforced before TriggerReady can become true;
- no fixed M1 +3/-3 score vote;
- M1 trigger evidence is consumed by DecisionEvaluator;
- decision orchestration captures M1 trigger evidence.

## Scope boundary

This phase does not yet solve the remaining analytical issues around:

- structural/liquidity evidence duplication;
- cross-timeframe structural overlap;
- swing plateau equality;
- FVG mathematical semantics;
- Order Block causal/mathematical quality;
- confidence calibration against realized outcomes.

Those remain in the subsequent Track 8 phases.

## Verification boundary

Automated CI has now passed all three required repository gates on verified branch head `929700e154d4b84a5a0b9efeae345b92017834b6`:
- Runtime Acceptance Contracts — PASS, run 801;
- cTrader Compile — PASS, run 985;
- Source / Architecture — PASS, run 992.

Therefore the automated Phase 8.1 boundary is verified and the phase is ready for merge. Hands-on cTrader replay/live validation remains required to measure actual false-signal reduction and terminal behavior.

## Continuity

Current public parameter surface remains 532 = 529 baseline + 3 OSS extension parameters.

Next phase: Phase 8.2 — Swing plateau correctness.

Operator pull: required only after the verified Phase 8.1 merge.
