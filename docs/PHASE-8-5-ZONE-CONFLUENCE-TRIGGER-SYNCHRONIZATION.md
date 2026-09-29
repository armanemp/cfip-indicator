# Phase 8.5 — Zone Confluence Symmetry and Timely M1 Trigger Runtime

Date: 2026-09-29

## Status

Implementation branch: `phase-8-5-zone-confluence-trigger-synchronization`

PR: #39

Base main before phase: `c1aacea2f57cd1c79aa06690bf42395e1310964f`

Current verification status: automated CI is still running on the latest branch head. The previous failed runs are documented below and were corrected on subsequent commits.

## User-reported problem

The user reported three related behavioral defects:

1. Trigger readiness appeared late.
2. The trigger/signal marker could be visually out of sync with the actual M1 confirmation.
3. The analysis, calculation, signal, execution and presentation paths did not behave as one timing contract.

The user also requested exact reuse of the supplied FVG and WaveTrend indicator logic where technically verifiable.

## Mathematical and causal findings

### M1 trigger timing

The pre-phase architecture evaluated the canonical TriggerReady state during the closed-M5 decision cycle. Even when a new M1 bar inside the current M5 became causally sufficient, the normal decision/plan path did not re-evaluate until the next M5 decision boundary.

The existing M1 rule itself was intentionally causal: it required a closed M1 bar, selected-direction alignment, valid ATR/body/range/close-location characteristics, and either a micro-structure break or configured displacement plus the trigger-score threshold.

The defect was therefore timing/ownership, not a missing threshold.

### Frozen M1 direction veto

A separate decision gate could reject the M5 decision when the current M1 frame direction did not match. That made a transient M1 state a frozen decision veto.

The M1 direction is now treated as a live closed-bar trigger input. Higher-timeframe consensus remains the decision-direction owner; M1 can confirm that direction but cannot replace it.

### Automatic plan retry

Automatic plan creation remembered the last attempt only by M5 index. A failed attempt before M1 confirmation could therefore suppress a second attempt during the same M5.

The runtime now records the M1 confirmation revision and retries when that revision changes, still respecting plan/capacity/risk ownership.

### Visual synchronization

The canonical visual state is still `SignalVisualSnapshot`.

Phase 8.5 adds trigger-runtime state to that snapshot and renders the exact confirming closed M1 bar as `CFIP_M1_TRIGGER`. The structural Trigger price remains the execution/setup level; the M1 marker identifies the actual causal confirmation event.

The Trigger marker is independently controlled by `ShowTrigger`, so it is not coupled to general signal-arrow visibility.

## FVG and Order Block integration

Phase 8.3 remains the mathematical owner of FVG geometry:

- three-bar and two-bar gap geometry;
- creation-bar ATR thresholding;
- directional overlap;
- partial/full mitigation;
- stable source identity.

Phase 8.5 preserves `FvgRule` as the owner for OB/FVG confluence and adds a symmetric `ZoneConfluenceRule` for generic execution-zone overlap semantics.

Order Block liquidity-sweep evidence no longer uses a second rolling-extreme definition. It now reuses the canonical confirmed-swing penetration/reclaim liquidity engine.

## Trigger score correction

Bullish trigger scoring no longer awards a directional point for neutral RSI=50 or DMI=0.

Bearish trigger scoring no longer awards a directional point for neutral RSI=50 or DMI=0.

This prevents a neutral oscillator state from contributing to both directional branches.

## WaveTrend reference boundary

The exact user-supplied custom WaveTrend source is not currently available in the repository or searchable conversation/Library content in this continuation.

No WaveTrend formula was guessed.

The intended integration boundary remains:

- closed-bar exact adapter;
- confluence/evidence input only;
- no independent directional authority;
- no repainting/live-bar shortcut;
- deterministic parity tests against the supplied source before enabling it in production decision scoring.

The audited FVG source/logic is already represented by the canonical `FvgRule`.

## Runtime contract

For `UseM1Trigger=true`:

`TriggerReady = ClosedM5TriggerReady AND LatchedClosedM1Confirmation`

The M1 confirmation may be established by a fully closed M1 bar inside the currently-forming M5 window. The current M5 OHLC is not consumed as evidence for that M1 confirmation.

Once confirmed, the M1 trigger is latched for that closed-M5 decision. A later weak M1 bar cannot revoke the already-confirmed decision.

Plan creation and broker execution remain downstream of TriggerReady, capacity, risk and execution ownership.

## Verification added

Runtime acceptance now covers:

- closed M1 inside the currently-forming M5 window;
- rejection of an M1 bar that has not fully closed;
- symmetric zone overlap;
- rejection of touch-only zone boundaries;
- explicit confluence tolerance;
- mirrored direction matching.

The existing M1 semantic contracts, FVG contracts, Order Block contracts, runtime stage isolation and broker execution contracts remain active.

## CI findings during implementation

Run 867 failed at compile because the new `ZoneConfluenceRule` was not linked into `CFIP.Runtime.Contracts`.

Run 1058 failed the architecture check because OB/FVG confluence had temporarily replaced the canonical Phase 8.3 `FvgRule.IsOverlapInclusive` owner.

Run 868 reached runtime contract execution successfully but caught an incorrect test assumption: touch-only boundaries were expected to be rejected while the implementation was inclusive. The rule was corrected to require positive-width overlap while retaining explicit tolerance support.

These were implementation/test-boundary defects and were corrected before the final verification boundary.

## Acceptance boundary

No public trading parameter was intentionally added.

No new decision authority was introduced.

No new broker mutation path was introduced.

No empirical win-rate, false-signal reduction or profitability claim is made from static/runtime CI. Target cTrader replay remains required for empirical validation of timing, signal quality and visual placement.

## Operator action

A local pull is required only after PR #39 is merged into `main`. Intermediate branch commits do not require a local pull.


## Final verification — 2026-09-29

Status: **MERGED AND VERIFIED**.

PR #39 merged to `main` as `cbda7910ad3b30fbd74cc526676e6372cb098cd7`. Pre-merge head `3e77b974cc00c82e9f134bfd0057493ea47c29e7`: Runtime PASS, Build PASS, Source/Architecture PASS. Post-merge merge commit `cbda7910ad3b30fbd74cc526676e6372cb098cd7`: Runtime PASS, Build PASS, Source/Architecture PASS.

The verified main branch is now the continuation baseline. Local pull is required before beginning the next phase.
