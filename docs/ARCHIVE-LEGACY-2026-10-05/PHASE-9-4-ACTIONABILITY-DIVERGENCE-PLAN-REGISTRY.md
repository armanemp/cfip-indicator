# Phase 9.4 — Actionability, Divergence and Multi-Plan Registry

Date: 2026-09-29

## Objective

Close the remaining gap between directional analysis and executable trade action.

A directional BUY/SELL condition is not an entry by itself. The pipeline now requires the current quote to remain actionable against the frozen structural execution geometry, reward path and micro-price state.

The phase also adds a closed-bar divergence engine and a registry for parallel opportunity plans.

## End-to-end contract

The runtime path is:

```
MTF evidence
  -> authoritative Decision
  -> structural ExecutionModel / SetupPreview
  -> live Actionability evaluation
  -> Plan creation / alert / visual authority
  -> broker execution gates
```

The Decision remains the only directional authority. Actionability is a downstream execution-readiness gate and does not create a second BUY/SELL authority.

## Actionability gates

The actionability evaluator checks:

- current Ask/Bid versus the structural execution zone;
- current mode: waiting-for-trigger, retest, breakout, or outside execution window;
- execution-zone quality;
- entry distance in ATR;
- breakout extension / late-entry distance;
- TP1 reward-to-risk against the existing adaptive minimum-RR policy;
- M1 adverse pressure when price is not safely inside the execution zone;
- strong opposing regular divergence;
- entry location and timing quality.

A setup that is directionally valid but no longer actionable is not promoted to a trade plan and does not emit a HIGH/SMART entry alert.

## Late-entry protection

A breakout is evaluated against the canonical trigger and Maximum Entry Extension ATR.

A retest is evaluated against the canonical ideal-entry distance and Maximum Entry Distance ATR.

This prevents the chart from treating a direction that was valid earlier as a fresh entry after price has already travelled too far.

## Divergence engine

Divergence is calculated from confirmed swing pivots on closed bars.

Supported forms:

- Regular bullish: price lower low with oscillator higher low;
- Regular bearish: price higher high with oscillator lower high;
- Hidden bullish: price higher low with oscillator lower low;
- Hidden bearish: price lower high with oscillator higher high.

Oscillator confirmation uses RSI and the existing WaveTrend engine. Strong conflicting divergence is represented explicitly and can block or penalize actionability rather than silently changing the decision direction.

## Visual/alert authority

The chart no longer treats raw M5 directional strength or a separate trigger marker as an independent directional entry signal.

The canonical visual entry direction requires ActionableNow.

HIGH and SMART entry alerts also require ActionableNow and are suppressed behind an existing managed pending order or active plan.

Trigger state remains available as execution state/panel information.

## Parallel opportunity registry

`TradePlanRegistry` stores materialized `TradeOpportunityCandidate` objects by stable opportunity ID.

Registry state preserves:

- lane;
- direction;
- entry/stop/targets;
- RR values;
- actionability;
- entry distance;
- divergence quality/type;
- block/actionability reason.

Current broker execution capacity remains the existing single-plan capacity. This registry therefore enables multiple independent candidate plans to coexist and be compared/managed without falsely claiming that the broker runtime currently supports multiple concurrent executions.

## Execution controls

The Auto Trade and Auto Orders panel controls are restored as actual ToggleButton operator controls.

The click handler mutates only canonical runtime flags. Programmatic synchronization is guarded by `_executionToggleSyncing`, so rendering cannot accidentally toggle execution state.

Public cTrader parameters remain configuration inputs and are synchronized into runtime state by the existing initialization/configuration boundary.

## Verification added

The phase extends:

- architecture/static verification;
- runtime contract compilation;
- registry identity/removal tests;
- actionability/divergence state tests;
- runtime UI audit for functional controls.

Full cTrader terminal replay remains an empirical validation step for signal quality, especially around turning points, failed breakouts, late entries and opposing divergence.
