# CFIP Indicator — Roadmap

## Baseline

The complete v73 behavior remains the behavioral reference. The current repository preserves the full 513-parameter surface and 311 unique reference methods.

## Phase 1 — Artifact-level modularization

Completed in the current refactor:

- One native indicator per file.
- One analyzer per file.
- One domain model per file.
- One enum per file.
- One cTrader parameter Group per file.
- Execution-intent construction and validation moved out of model declarations.
- Legacy monolithic source paths blocked by architecture verification.
- Production source file-size limit set to 64 KiB without a configuration-file exception.

## Phase 2 — Object-level service isolation

Next:

- Convert high-coupling `partial class` method clusters into explicit internal services with narrow inputs/outputs.
- Introduce immutable cycle snapshots for market/analysis/decision/planning boundaries.
- Remove direct cross-layer field access where a service contract can replace it.
- Keep one broker mutation boundary and one execution authority.

## Phase 3 — Advanced indicators and numerical cross-validation

- Add an optional adapter for a netstandard-compatible OSS indicator library after fixture-level numerical validation.
- Compare selected indicators against CFIP calculations and keep CFIP decisions authoritative.
- Expand momentum, volatility, trend, channel and oscillator coverage without duplicating existing semantics.
- Add per-indicator health/availability metadata.

## Phase 4 — Trading correctness and scenario suite

Validate controlled scenarios for:

- market execution;
- stop/limit pending orders;
- rejection/slippage;
- structural SL/TP;
- partial close / break-even;
- dynamic target progression;
- reversal / invalidation / exhaustion;
- restart/reconnect reconciliation;
- duplicate events and idempotency;
- daily-loss and suitability guards;
- multi-position and multi-fill behavior.

## Phase 5 — cTrader acceptance and release hardening

- Compile against the target cTrader Automate API.
- Run the indicator on the target cTrader build.
- Verify chart/panel/popup behavior across timeframes.
- Verify resource usage and remove only proven inefficiencies.
- Freeze a release baseline only after static, compile and runtime gates pass.
