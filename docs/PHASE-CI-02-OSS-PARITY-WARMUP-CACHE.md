# CFIP Indicator — CI-02 OSS Numerical Parity / Warm-up / Cache Integrity

Date: 2026-10-01

## Status

**VERIFIED COMPLETE — implementation head `c3720853edbcf5c04bb1f5cbf1e9533f39e87a4e`.**

## Objective

CI-02 verifies that the production Skender 2.7.3 boundary preserves numerical
meaning while using bounded computation and cache reuse.

The phase does not promote FacioQuo or create a second production indicator
engine. FacioQuo remains research-only.

## Canonical ownership

- `Core/Math/OssIndicatorSettings.cs`
  - single owner for fixed production Skender settings.
- `Core/Math/OssIndicatorWarmupPolicy.cs`
  - single owner for the bounded stable 768-bar convergence window.
- `Core/Math/OssQuoteWindowRule.cs`
  - single owner for first-index/window-count/rebuild decisions.
- `Core/Math/OssQuoteProjectionRule.cs`
  - single owner for finite, non-negative quote-volume normalization.
- `Analysis/Indicators/External/OssQuoteSeriesCache.cs`
  - owns adapter-local bounded quote materialization and broker-history
    invalidation events.

## Stable vs rolling adapters

Stable/path-dependent production adapters use the 768-bar stable window:

- RSI;
- MACD;
- SuperTrend;
- Parabolic SAR.

Window-local adapters use the 161-bar rolling window:

- Bollinger Bands;
- MFI;
- Stochastic;
- Aroon;
- CCI;
- OBV.

The benchmark also checks OBV direction rather than cumulative absolute value,
because the production consumer uses directional change between the final two
results.

## Correctness hardening

CI-02 completes these corrections:

1. Stable and rolling window first-index/rebuild arithmetic is centralized.
2. Zero-volume quotes remain zero-volume instead of being converted to an
   artificial unit volume.
3. Non-finite or non-positive volume is normalized to zero through one pure
   adapter rule.
4. Stable cache reuse continues to use first/last boundary fingerprints.
5. HistoryLoaded/Reloaded invalidation remains the explicit lifecycle boundary.
6. Existing Skender fixed settings and parameter-driven RSI/MACD periods remain
   owned by their existing canonical owners.

## Deterministic runtime contracts

Runtime contracts cover:

- initial, contiguous append, skipped and backward cache-window movement;
- bounded 161/768 collection sizes;
- zero/negative/NaN/infinite/positive quote-volume normalization.

## Numerical parity benchmark

The CI-02 benchmark uses deterministic 2048-bar fixtures at checkpoints
768/1024/1536/2047.

It compares:

- full-prefix vs bounded stable RSI;
- full-prefix vs bounded stable MACD histogram;
- full-prefix vs bounded stable SuperTrend;
- full-prefix vs bounded stable Parabolic SAR;
- full-prefix vs bounded rolling Bollinger PercentB/Width;
- full-prefix vs bounded rolling MFI;
- full-prefix vs bounded rolling Stochastic K/D;
- full-prefix vs bounded rolling Aroon oscillator;
- full-prefix vs bounded rolling CCI;
- full-prefix vs bounded OBV direction.

Each base fixture is also evaluated with deterministic zero-volume injections.

Acceptance for stable recursive indicators is:

- all comparable outputs finite;
- no directional-classification mismatch;
- max/mean/RMS error measured and published.

Acceptance for window-local indicators is:

- same final timestamp;
- finite outputs;
- zero numerical mismatch beyond machine floating-point noise (1e-12 gate).

The benchmark also records full-prefix vs bounded execution time and allocation.

## Performance boundary

The phase measures bounded computation but does not claim target-terminal CPU or
memory performance. cTrader target-terminal timing remains manual acceptance.

## Safety boundary

- No public parameter name, type or DefaultValue changed.
- No score, confidence, RR, entry, SL/TP, risk or execution threshold was tuned.
- No decision or broker-mutation authority changed.
- No second production indicator engine was introduced.
- FacioQuo remains research-only.

## Verification boundary

Repository Source/Architecture, Runtime Acceptance and cTrader Compile results
are required before this phase is marked VERIFIED COMPLETE.

Target-terminal cTrader behavior, broker-specific history replacement and live
signal-quality remain manual acceptance items.

## Verification

- Source / Architecture: **PASS** — workflow run 36909965454.
- Runtime Acceptance Contracts: **PASS** — workflow run 36909965513.
- cTrader Compile / Build: **PASS** — workflow run 36909965368.
- OSS benchmark: **PASS** — workflow run 36909965470.
- Final deterministic benchmark: 384 compared points; 0 directional mismatches;
  0 non-finite pairs; 0 rolling exact mismatches.
- Stable max/mean/RMS error: 0 / 0 / 0 on the deterministic fixtures.
- Full-prefix vs bounded mean timing: 21.2374 ms vs 7.1466 ms per iteration.
- Full-prefix vs bounded allocation: 11,303,818 vs 3,875,106 bytes per iteration.

## Final repair during acceptance

The CI-02 static audit and benchmark report had stale references to the previous
benchmark owner/result variable. They were aligned to the single consolidated
`SkenderWarmupParityBenchmark` owner; no production trading behavior changed.

## Next phase

**CI-03 — Indicator fusion / correlation / evidence-independence audit.**
