# CR4.6 / D6 — Frame-scoring constant ownership

## Scope

This phase centralizes the existing market-frame scoring constants under one Core
owner without changing the established numerical behavior.

## Implementation

Created `FrameScoringConstants` in `src/CFIP.Indicator/Core/Math/` and migrated
`MarketFrameScoringService` to consume it for:

- structural event contributions (Structure/MSS/CHoCH);
- displacement/liquidity/equal-level contributions;
- direction minimum score and lead;
- indicator-conflict penalty threshold, baseline, cap and divisor;
- RSI exhaustion thresholds and penalty;
- regime contribution coefficients/caps;
- frame-quality composition weights and bounds;
- defensive denominator/percentage constants.

The established numerical values are preserved exactly. No public parameters,
RR floors, confidence thresholds, execution policy, or decision authority were
changed.

## Verification

Deterministic runtime coverage was added through
`VerifyFrameScoringConstants()`, including the preserved values for all migrated
constant groups.

The Source/Architecture workflow now runs `audit_phase_4_6.py` after the
accumulated CR4.1–CR4.5 audits.

A pre-existing CR4.5 static-audit false positive was also corrected separately:
the cache-refresh assertion now accepts the actual multiline C# call formatting
instead of requiring one exact source spelling.

Final verification on implementation HEAD
`a134b72e863067291fb04eae7ac530c3fbcae999`:

- Source/Architecture — PASS, run #1978;
- Runtime Acceptance Contracts — PASS, run #1787;
- cTrader Compile — PASS, run #1971.

## Acceptance boundary

Target-terminal timing, live cTrader rendering behavior, replay and empirical
signal-quality/profitability validation remain manual acceptance boundaries.
This phase is structural hardening only and does not claim trading-performance
improvement.
