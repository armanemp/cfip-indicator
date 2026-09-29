# Phase 9.10 — Smart Auto-Trade / Auto-Order Protection & Accumulated Audit

Date: 2026-09-29

## Status

Implementation in progress on `phase/9-10-smart-auto-trade-protection-audit`.

## Objectives

- Make every automatic market/aggressive/pending execution path consume the same submission and protection authority.
- Improve SL handling by allowing broker/server-owned smart break-even protection when the advanced TP ladder is valid.
- Preserve intelligent structural SL and target selection; do not replace them with a new independent strategy.
- Prevent local break-even mutation from competing with a confirmed server-owned break-even rule.
- Keep all signal/plan level lines Solid.
- Keep all level text white and background-free.
- Add a standing accumulated audit that checks auto-trade, auto-order, SL/TP, duplicate mutation, stale-state and UI invariants on every phase.

## Implementation

### Smart server break-even

Added `SmartBreakEvenRule`.

It derives a deterministic server break-even contract from existing inputs:

- initial structural risk in pips;
- TP1 distance;
- current spread;
- existing break-even RR trigger;
- existing break-even buffer;
- existing risk-free lock;
- existing spread-aware break-even switch.

The rule avoids arming break-even so late that it collides with TP1 and keeps the offset inside a bounded portion of the initial risk.

### Automatic execution integration

The smart server break-even rule is transported through:

- automatic market;
- aggressive market;
- continuation stop;
- reversal limit.

The same broker-side protection call remains the mutation authority.

### Local protection de-duplication

When the broker confirms server-owned break-even, the local polling-based break-even logic yields to that state. Structural SL repricing remains monotonic and protective-only.

### Signal-line presentation

All plan/signal level lines now use `LineStyle.Solid`.

Level annotations remain white text with no generated rectangle background.

### Accumulated audit

Added `tools/audit_phase_accumulation.py` and wired it into Source / Architecture CI.

The audit checks:

- automatic market/aggressive/pending paths consume the shared submission gate;
- automatic execution consumes the canonical capacity gate;
- server TP + smart break-even ownership exists;
- local TP/BE mutation yields to server-owned protection;
- signal lines are Solid;
- level labels are white/background-free;
- public parameter count remains 552.

## Safety boundary

No new public parameters are introduced.

Single-plan/single-managed-identity execution remains unchanged.

The phase does not claim profitability, win-rate, false-signal reduction or realized RR improvement from static analysis.

## Verification required

- Decision Contracts;
- Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture and accumulated audit;
- target-terminal cTrader replay for broker/server protection, partial fills, live rendering, duplicate suppression and realized outcomes.
