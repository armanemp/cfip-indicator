# Phase F4 — Numerical / Formula / Boundary Integrity — 2026-10-04

Status: VERIFIED COMPLETE on implementation head; PR #279 pending merge

## Goal
Close the concrete build/harness defects surfaced by current verification evidence without changing trading strategy behavior or introducing a parallel owner.

## Problems
- CFIP.Runtime.Contracts emitted 152 CS0649 warnings because canonical model fields relied on implicit CLR defaults.
- Existing source audits encoded old declaration formatting and therefore failed after defaults became explicit.
- Affected audits included G2, MTF-P1, CI-21, CBOT-6M and realtime multi-scenario candidate-state checks.

## Root cause
The runtime harness compiles canonical production model types directly. Their implicit CLR defaults were valid runtime semantics but produced compiler warnings. Several legacy static audits coupled semantic requirements to old declaration text instead of checking the actual member contract.

## Changes
- Explicitly initialized existing fields in the canonical owners: Decision, Plan, TradeOpportunityCandidate and ParallelScenarioGeometry.
- Preserved exact prior defaults: bool=false, numeric=0, string=null, enum=default.
- Updated declaration-sensitive audits to semantic member-presence checks.
- No pragma/suppression, analyzer downgrade, duplicate model, duplicate contract, strategy threshold change, execution-path change, MTF-role change or UI behavior change.

## Verification
Final implementation head: 62f69dc703887486392fbfd59f0f726deea4bba4
- Source / Architecture: PASS
- Runtime Acceptance: PASS
- cTrader Compile: PASS

## Duplicate/dead/conflicting paths
None introduced. The existing model owners remain the sole state owners; no fallback or parallel implementation was added.

## Performance
No algorithmic path, allocation pattern, MTF pass, chart object, timer, I/O or broker mutation path was added. Field initializers preserve the same effective default state.

## Terminal-only boundary
Target cTrader manual acceptance remains required for runtime visual/audio/panel behavior; this phase did not change those behaviors.

## Git
- Branch: fix/runtime-contracts-unassigned-field-warnings-2026-10-04
- PR: #279
- Merge: pending