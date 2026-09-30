# CR2.7 — WaveTrend Mathematical Correctness

Status: VERIFIED COMPLETE — PR #92 merged 2026-09-30, merge commit b021f56c4b75331fb52127547e006f2f8bb287c4.

## Scope

Claude review item B8:
- MovingAverageType mathematical semantics;
- warm-up/readiness dependency correctness;
- cache/state invalidation after history extension;
- explicit TickVolume-based MFI semantics.

## Implementation completed

- Added deterministic moving-average ownership for all ten currently exposed cTrader MovingAverageType values.
- Corrected DEMA/TEMA state-chain initialization so dependent EMA series are populated before the first valid output.
- Corrected HMA final raw-window construction and WMA weighting.
- Made RSI/MFI/RMI non-ready before their dependencies stabilize.
- Made final WaveTrend snapshot readiness depend on the complete smoothing/signal chain and previous-signal stability.
- Added HistoryLoaded/Reloaded invalidation plus detection of history prefix/count changes.
- Preserved TickVolumes as the MFI input; this path does not claim exchange-level real volume.
- Preserved all public parameter names, types and DefaultValues.
- Did not change decision authority, auto-trade/auto-order policy, broker mutation or capacity.

## Verification

Verified head: `181b238a548280fc01a67fb1e3ba8a617f63e42a`

- Source/Architecture: PASS
- Runtime Acceptance: PASS
- cTrader Compile: PASS
- CR2.7 static audit: PASS
- Previous-phase CR2.6 static audit: PASS

The repository-wide verification chain also passed its existing parameter, project-integrity, lifecycle, optimization, news, cBot-boundary, and CR2.1–CR2.6 audit stages.

## Routine audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was rechecked.

CR2.7 changed only WaveTrend analytical calculation/readiness/state invalidation. Existing canonical Decision/Plan/Execution/Protection ownership remains intact. No execution safety gate was bypassed.

## Manual boundary

CI cannot prove exact target-terminal numerical parity for every platform-specific MA implementation or the empirical effect on live signal quality. Target-terminal replay remains required before making accuracy, win-rate, realized-RR or profitability claims.
