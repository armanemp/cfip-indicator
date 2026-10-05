# Phase 9.1 — Top-down opportunity calibration and runtime responsiveness

Date: 2026-09-29

## User intent

The system should first identify a meaningful directional opportunity from H1 and higher, then calibrate downward through M30/M15/M5 and finally confirm on closed M1. The goal is a cleaner actionable state with structurally defensible risk and reward, not a blanket increase in thresholds.

The user also reported panel slowness, ambiguity around the chart text SIGNAL BUY/SELL, and live management that appeared to react only after TP/new-M5 boundaries.

## Architecture adopted

H1/H4/D1/W1 -> directional anchor

M30/M15 -> calibration

M5 -> setup/location

M1 -> closed trigger confirmation

A strong HTF anchor has directional ownership. Lower frames can confirm it but cannot override a strong conflict.

Actionable decisions use the existing RequireHigherTfAgreement setting as the activation boundary and require the explicit top-down stage ENTRY CALIBRATED. No new public parameter or second decision engine is introduced.

## Panel runtime

The 500 ms heartbeat remains responsible for lightweight freshness and safety supervision.

Full panel layout work is keyed by presentation-state identity. Unchanged state does not rebuild the layout.

The heartbeat updates live clock/RR/position/exit rows directly and does not call the full RenderPanel path.

Existing conditional row-property writes are preserved; redundant hide-all-row churn was removed.

## TP / trailing responsiveness

Structural safety remains closed-bar based.

A bounded live structural pulse can re-evaluate already-closed M5 structure without consuming an unclosed M5 bar.

After a confirmed TP1 or TP2 partial-close mutation, the reward path is re-evaluated in the same calculation cycle when a managed position remains.

Broker SL/TP mutation remains monotonic and broker-confirmed.

## Chart label semantics

The previous chart label SIGNAL BUY/SELL plus KIND is an alert mirror from the unified alert path. It is not a second signal engine.

It is renamed to ALERT BUY/SELL plus KIND to make the presentation-only role explicit.

## Verification

Deterministic runtime contracts cover:
- strong HTF anchor with aligned M30/M15/M5;
- M5 conflict against strong HTF;
- strong middle-frame conflict;
- mixed HTF state;
- monotonic stop progression;
- monotonic target progression.

Required gates:
- Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture.

Empirical signal-quality, visual-timing and realized-RR claims still require target cTrader replay/outcome validation.

## Continuity

Branch: phase-9-1-topdown-evidence-runtime-responsiveness

PR: #40

Base main before phase: 60749410917a4888e2a4c2ad90ce98c8f0916cfa

Final verified main baseline after merge will be recorded here and in ROADMAP/DEVELOPMENT-LOG.


## Final verification — 2026-09-29

Status: MERGED.

PR #40 merged as `b0e17e93551b3760d50bb38bb4725b9c523afddd`.

Pre-merge head `4530f4043195378f58727db8a395121c43abcd52`: Runtime PASS, Build PASS, Source/Architecture PASS.

The branch was intentionally not advanced beyond Phase 9.1 after merge. The verified implementation baseline is now main. Local pull is required before next phase.
