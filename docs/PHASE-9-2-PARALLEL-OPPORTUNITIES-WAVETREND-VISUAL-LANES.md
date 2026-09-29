# Phase 9.2 — Parallel opportunities, WaveTrend evidence and visual lanes

Date: 2026-09-29

## Purpose

Phase 9.1 established a Strategic top-down lane. Phase 9.2 adds independent Tactical LTF opportunity discovery so worthwhile lower-timeframe setups are not discarded merely because the higher-timeframe path is not calibrated.

## Opportunity lanes

Strategic:
H1/H4/D1/W1 anchor -> M30/M15 calibration -> M5 setup -> M1 closed trigger.

Tactical:
M5 structural opportunity evaluated independently from the Strategic permission path, with its own quality and RR constraints.

Counter-HTF Tactical:
A strong M5 opportunity against a strong HTF anchor. The candidate is retained only under stricter quality and RR requirements.

Micro Reaction:
Reserved for strong live reaction state and kept visually separate.

## Execution boundary

The existing runtime has a singleton plan state and an execution-capacity contract of one live plan. Phase 9.2 therefore supports parallel opportunity detection and presentation, but does not pretend to support safe simultaneous multi-position execution.

Phase 9.3 is expected to introduce a plan registry, position-isolated protection state and independent submission identities before simultaneous auto execution is enabled.

## WaveTrend source integration

The user's latest ZIP contains CUSTOMWAVETREND. Its source defines:
- RSI/MFI/RMI length = 10;
- RMI momentum = 5;
- RMI up/down exponential smoothing using the same length;
- average of RSI, MFI and RMI;
- EMA smoothing length = 4;
- SMA signal length = 5;
- OS1/OS2 = -30/-40;
- OB1/OB2 = 30/40;
- plotted WAVE and SIGNAL are shifted by -50.

The production engine reimplements that cascade as stateful evidence. Numerical parity against cTrader's built-in MFI implementation remains a target-terminal replay item.

## Visual lanes

Parallel candidates use isolated chart object prefixes and labels. Existing line length helpers are reused, so line geometry is not changed.

Compact labels use opaque backgrounds matching their line color and choose black or white text based on luminance.

## Verification

Required gates:
- Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture.

Empirical validation remains required for:
- actual terminal label readability;
- visual separation of multiple candidates;
- WaveTrend numerical parity;
- reduced missed-LTF-opportunity rate;
- realized RR and signal quality.

No performance claim is inferred from static code or CI alone.

## Continuity

Branch: phase-9-2-parallel-opportunities-wavetrend-visual-lanes

Main base before phase: 02e0bb81b12af079935804a61d9d5ee3cb076e4e


## Final verification

Merged: PR #41 -> `aedceba3f6d9e3791328f41c9e1fb01e2a474067`.

Verified final code head before merge: `c29608de510ebeb675c10c0438ede0cfcfbf59e5`.

Runtime Acceptance: PASS.
Build: PASS.
Source/Architecture: PASS.

Phase 9.2 is complete at code/CI level. Parallel visual detection is implemented; simultaneous execution remains intentionally deferred to Phase 9.3.
