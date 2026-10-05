# CR4.7 / D7 — TP pipeline feasibility, rejection telemetry and HTF-age semantics

## Scope

This phase hardens the target-selection pipeline without changing public parameter
names, types, defaults, RR floors, stop limits or execution policy.

## Implementation

- Added a pure Core target-constraint rule for geometry, target direction,
  RR minimum/maximum, target extension, spacing, progression, HTF source and
  HTF quality.
- Added a pure Core target-reward envelope rule exposing the maximum RR reachable
  from the existing ATR extension and RR caps.
- Added a pure Core age-semantics rule:
  - M5 targets retain setup-bar age semantics.
  - M15/W1 targets use elapsed time derived from the source bar timestamp.
  - the effective default HTF age ceiling is preserved by the existing
    `min(MaximumSetupAgeBars, MaximumZoneAgeBars)` equivalent.
- Extended HTF, D1/W1 previous-period, D1 pivot and D1 liquidity target producers
  to carry real source elapsed age.
- Preserved source age when target levels are merged.
- Added bounded per-stage `PLAN_TARGET` rejection telemetry with deterministic
  reason ordering and per-M5 deduplication.
- Added explicit reasons for RR, extension, spacing/progression, age, HTF and
  obstacle rejection paths.
- Added early identification of stages that are mathematically unreachable from
  the existing risk/extension envelope.

## Deterministic feasibility evidence

The Planning Contracts include a fixed matrix for the current TP1–TP4 RR values
(2.0 / 3.2 / 4.8 / 6.5):

- 4/4 valid BUY stage fixtures accepted;
- 4/4 below-minimum-RR fixtures rejected with the canonical RR reason;
- 4/4 SELL mirrors accepted.

The reward-envelope fixtures use the existing 4 ATR maximum extension and 12 RR
maximum with risk/ATR values 0.55, 0.75, 1.00 and 1.80. TP4 is mathematically
reachable only in the 0.55-Risk-ATR case; the other three are unreachable from
the existing envelope.

These are deterministic feasibility fixtures, not historical-market acceptance
rates or profitability evidence.

## Safety

- No public parameter name/type/DefaultValue changed.
- No default RR, SL, target-age or execution threshold was tuned.
- No second decision or execution authority was introduced.
- Existing plan-level `PLAN_REWARD` telemetry remains in place.
- cTrader-dependent runtime behavior remains subject to target-terminal
  verification.

## Verification boundary

The actual empirical TP1/TP2/TP3/TP4 acceptance distribution, broker/runtime
timing and trading-quality impact still require deterministic replay and
hands-on target-terminal validation.


## Final verification

Implementation HEAD: `441611a8d1ca513ee332f9a8a006a3f37eb9a727`  
Merged by PR #108: `2a586ca353f79d6151b9b8375edf46cb94df7880`

- Source/Architecture: PASS — run #1999
- Runtime Acceptance Contracts: PASS — run #1808
- cTrader Compile: PASS — run #1992

The phase is complete. The next implementation phase is CR4.8 / D8.
