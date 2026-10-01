# CI-08 — Divergence / WaveTrend / Reaction / Early Signal Integrity

Date: 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #163**

## Scope

CI-08 audits upstream analytical components that could still drift semantically after CI-00 through CI-07:

- regular and hidden divergence;
- divergence quality and strong-conflict thresholds;
- WaveTrend base/MFI semantics, smoothing readiness and evidence qualification;
- live reaction observation vs closed-bar confirmation;
- early prediction scoring/state separation;
- WATCH/REACTION alert ownership and delivery boundary.

## Corrections implemented

### Divergence

`DivergenceThresholdRule` now owns the explicit strong conflicting-divergence quality threshold (`70`) used by indicator fusion.
The production analyzer remains the only place that materializes regular/hidden divergence from confirmed swing pairs, RSI and WaveTrend corroboration.

### WaveTrend

`WaveTrendEvidenceRule` now owns minimum-quality normalization/qualification and the configured `MinimumWaveTrendQuality` is carried into indicator fusion and independent evidence provenance.
This removes the previous hidden hardcoded `58` floor in those consumers while preserving the public parameter contract and default value.

WaveTrend MFI money-flow contribution is isolated into the platform-neutral `WaveTrendMoneyFlowRule`.
Zero TickVolume contributes zero flow instead of a synthetic unit volume; invalid/non-finite volume fails closed.

### Reaction timing

`ReactionTimingRule` makes the live M5 observation / prior closed-M5 confirmation boundary explicit and is consumed by the reaction analyzer for closed confirmation.
This preserves the rule that a moving live-bar reaction cannot rewrite the closed-bar snapshot used for actionable confirmation.

### Early prediction and alerts

Early prediction remains downstream of the authoritative closed decision and upstream of plan creation, with no direct mutation of `_decision`, `_plan`, lifecycle state or broker positions.
WATCH/REACTION alert emission remains in the decision-owned alert stage and uses the existing unified delivery transport.

## Verification coverage

Added deterministic Runtime Contracts for divergence strong-conflict ownership, WaveTrend minimum-quality normalization, positive/negative/zero TickVolume MFI semantics, configured WaveTrend quality propagation, and reaction temporal separation.

Added `tools/audit_phase_ci_08.py` and wired it immediately after CI-07 in the Source/Architecture workflow.

## Safety / performance boundary

- No public `[Parameter]` name, type or `DefaultValue` changed.
- No RR, confidence, entry, SL, TP, risk or execution-policy threshold was tuned.
- No decision or broker-mutation authority was introduced or duplicated.
- WaveTrend MFI adds no full-history scan beyond the existing length window.
- Reaction timing adds only O(1) boundary validation.

## Repository verification

Final verification passed on the merged phase head before PR #163 merge:

- Source / Architecture checks: workflow run **#2545** — PASS.
- Runtime Acceptance Contracts: workflow run **#2354** — PASS.
- cTrader Compile: workflow run **#2538** — PASS.
- PR **#163** merged to `main` with merge commit `8b82074ad5bff8a2a9e3ccdd626f4ca5afb61874`.

## Manual boundary

CI cannot establish target-terminal WaveTrend numerical parity, intrabar event timing, popup/audio latency, panel/chart responsiveness or empirical signal quality. Those remain manual cTrader acceptance items.

**Next implementation phase: CI-09 — Decision engine mathematical audit.**
