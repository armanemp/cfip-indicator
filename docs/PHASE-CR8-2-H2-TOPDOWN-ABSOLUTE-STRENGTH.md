# CR8.2 / H2 — Top-Down Absolute Strength

Date: 2026-10-01

## Status

VERIFIED COMPLETE — PR #150, final implementation HEAD 675283b82e345a86b1a6094e7b7ad66ec3632b84.

## What changed

Top-down calibration now carries two distinct measures. Alignment describes
relative directional dominance. AbsoluteStrength describes the weighted average
quality of the dominant-direction frames. Both are bounded to the existing
0–100 domain.

The existing strong-threshold input remains unchanged. A higher-timeframe
anchor is strong only when both alignment and absolute strength satisfy that
threshold. Opposing mid/entry evidence must also have sufficient strength before
it can block that strong anchor.

## Warning cleanup

The CS0414 _executionToggleSyncing warning was resolved without deleting the
architecture-required guard. The existing flag is now read before programmatic
visual synchronization and remains reset in finally, making it a real
re-entrancy guard.

## Verification

- Source/Architecture PASS — workflow run 36892859244.
- Runtime Acceptance Contracts PASS — workflow run 36892859151.
- cTrader Compile PASS — workflow run 36892859090.

H2-specific audit_phase_8_2.py also passed and is accumulated in Source/Architecture CI.

## Safety boundary

- no public parameter/default changed;
- no RR/confidence/entry/SL/TP/risk/execution threshold retuned;
- no broker-mutation path or new decision authority added;
- no profitability or win-rate claim is made.

## Next

CR8.3a / H3-A — Prompt 8 next remediation phase.
