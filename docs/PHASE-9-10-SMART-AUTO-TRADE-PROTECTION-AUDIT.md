# Phase 9.10 — Smart Auto-Trade / Auto-Order Protection & Accumulated Audit

Date: 2026-09-29

## Status

VERIFIED COMPLETE. PR #52 merged into `main` as `3f82fd9ad35ff33aafb9216c325524878e368a3a` from verified head `934d5aac56cfcd2769a743441d5260a2ad273997`.

Automated verification of the code-equivalent head `04f47949a9429d5921c83d851ceed23f923257f9` passed Runtime Acceptance #1066, cTrader Compile/Build #1250 and Source/Architecture #1257. The subsequent audit-only refinement is limited to stricter Solid-only chart-style scanning and requires a fresh final CI run before merge.

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

Level annotations use exact corresponding signal-line color, regular-weight native ChartText, no generated rectangle/background, and the visible text end remains one chart bar before the canonical line start.

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

## Final scope closeout

This phase establishes a permanent accumulated hardening loop for the project:

- every phase must change the automatic market/aggressive/pending pipeline in a concrete way;
- smart SL/TP protection must remain under one mutation authority;
- stale, duplicate and direction-conflict states must be auditable;
- every signal/plan chart line must remain Solid;
- level text remains white and background-free.

Next phase: 9.11 — Automatic Execution Telemetry & Deeper SL/TP Coherence.


## PR #52 merge closeout

Merged: `3f82fd9ad35ff33aafb9216c325524878e368a3a`.

The next continuation must begin from the new `main` baseline. Operator pull is required now.
