# F3 — Price / Time / Closed-Bar / MTF Integrity — 2026-10-04

Status: **IMPLEMENTATION COMPLETE — verification pending.**

## Root cause
Several downstream MTF/M1 lookups were resolving a closed-bar index using the opening timestamp of the closed M5 bar. At that timestamp the just-closed M5 bar is not yet closed, so higher-timeframe lookups can select the previous HTF bar.

## Canonical correction
IndexMath.ClosedBarBoundaryReference(...) is now the single reusable owner for the reference boundary of a known closed M5 bar. It returns the actual next M5 open, which is the first timestamp at which the closed M5 bar is fully closed, with a safe current-UTC fallback for invalid input.

Consumers corrected: TimeframeScenarioBuilder; DirectionAcceptanceGate; PredictivePendingLevelSelector; ExecutionZoneCandidateSelectionCore; StructuralStopCandidateEvaluator; StructuralStopCandidateCollector; M1MicroTargetSource; SupplyDemandLiquidityTargetSource.

No new MTF calculation path was created.

## Existing canonical boundaries preserved
ClosedBarReferenceRule remains the owner of closed-index semantics. MtfContextBuilder remains the owner of the eight canonical MTF indices. MtfClosedContextCache remains the owner of reference-aware cache reuse. M15 remains canonical decision/reference timeframe; M5 remains trigger/entry precision; M1 remains optional confirmation/precision. No M2 path was introduced.

## Regression
Added tools/audit_phase_f3_price_time_closedbar_mtf_integrity.py and wired it into Source / Architecture CI. The gate rejects the old opening-time lookup pattern and verifies the canonical boundary/cache architecture.

## Interaction audit
The correction affects only the temporal reference used to select already-closed MTF/M1 evidence. It does not change signal thresholds, strategy coefficients, RR policy, broker execution, alerts, rendering, or cBot authority. It prevents stale one-bar HTF evidence while preserving no-look-ahead behavior.

## Performance
The new helper is O(1), performs no allocations, and reuses existing bar timestamps. No extra MTF series, analysis pass, timer, chart object, or network/I/O operation was introduced.

## Verification boundary
Required before merge: Source / Architecture; Runtime Acceptance; cTrader Compile; accumulated audits; exact branch CI. Target-terminal validation remains required for actual MTF transition timing and visual/live behavior.
