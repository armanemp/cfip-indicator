# CR8.3b / H3-B — Skender Warm-up, Bounded Computation, Cache Design and Numerical Parity

Date: 2026-10-01

## Status

**VERIFIED COMPLETE — PR #152; final implementation head `b0ddaabed9723515d50ac183592f1eb7d56b5942`; merged to `main` as `db52531a5fe333d2645cdd5f63ac33844d01f8e0`.**

## Scope completed

- Added Core/Math/OssIndicatorWarmupPolicy.cs as the canonical owner of the
  fixed, non-public 768-bar stable quote window.
- Encoded conservative convergence guidance for the current production
  parameter envelope.
- Replaced the previously unbounded stable-prefix quote cache with an
  incremental bounded stable window.
- Added first/last cached-bar boundary fingerprints so append-only reuse
  remains invalidated by history replacement or boundary mutation.
- Removed stable-window GetRange copying.
- Replaced production Skender result-list materialization with LastOrDefault()
  or a streaming last-two OBV scan.
- Added deterministic Runtime Contract coverage for the warm-up policy.
- Added deterministic 2048-bar full-prefix versus bounded-window parity and
  performance coverage for RSI, MACD histogram, SuperTrend and Parabolic SAR.
- Added tools/audit_phase_8_3b.py and accumulated it after H3-A in Source /
  Architecture CI.
- Reconciled the accumulated CR4.4 audit wording with bounded stable-window
  semantics.

## Numerical-parity contract

The benchmark does not claim bit-for-bit identity for recursive indicators
after truncating historical state.

The deterministic safety gate requires:
- all compared outputs to be finite;
- zero directional-classification mismatches versus the full-prefix path;
- maximum, mean and RMS absolute error to be measured and published;
- full-prefix and bounded-window timing/allocation to be measured separately.

## Safety boundary

- No public parameter name, type or DefaultValue changed.
- No RR, confidence, entry, SL, TP, risk or execution threshold was tuned.
- No decision authority, broker-mutation owner or lifecycle authority changed.
- No new runtime I/O, network operation or unbounded cache was introduced.
- FacioQuo remains research-only.

## Manual cTrader boundary

CI validates repository behavior. Target-terminal startup latency, actual
cTrader CPU/memory behavior, history reload behavior and empirical
signal/outcome quality remain manual acceptance items.

## Repository verification

- Source / Architecture: **PASS** — final H3-B head.
- Runtime Acceptance Contracts: **PASS** — final H3-B head.
- cTrader Compile / Build: **PASS** — final H3-B head.
- OSS indicator benchmark: **PASS** — final H3-B head.

## Next phase

**CR8.4 / H4.**
