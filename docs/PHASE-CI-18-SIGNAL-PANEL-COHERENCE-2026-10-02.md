# CI-18 — Signal / Panel Coherence — 2026-10-02

## Status

VERIFIED COMPLETE — merged to `main` via PR #199 as `8ba701ea288641bc1435ac8f94ea6890713ed63c`.

## Problem addressed

The repository had multiple valid states but the presentation and actionability boundaries were not aligned:
- a directional Decision could be hidden as WAITING because the visual snapshot only exposed its direction after EntryAllowed/TriggerReady;
- the panel mixed market direction with trade readiness;
- primary M15/H1 rows used raw frame direction while other frame presentation used the shared display-bias rule;
- the panel exposed only coarse pipeline state and did not show live trigger lifecycle details;
- DecisionConfirmationGates could reject a quality directional setup before the canonical mode-specific actionability path, preventing Retest opportunities from being evaluated;
- EntryGeometryRule prioritized continuation waiting before an actual in-zone Retest.

## Canonical corrections

1. Preserve directional Decision state in SignalVisualSnapshotBuilder even when the setup is not yet actionable. This affects presentation only; it does not authorize a trade.
2. Add an explicit MARKET BIAS panel state derived from the primary M15/H1/M5 directional presentation path, separate from SIGNAL readiness.
3. Reuse ResolvePanelTimeframeState(...) for primary M15/H1 panel rows so the same DirectionLabel/readiness/color owner is shared with the MTF lamp rail.
4. Expose trigger lifecycle score, required score, M1 direction and runtime reason in the panel.
5. Move M5OnlyConfirmedTrigger enforcement out of decision-level EntryAllowed filtering and into the mode-specific actionability evaluator. Retest remains zone-driven; trigger-dependent modes retain the trigger requirement.
6. Make an in-zone Retest take precedence over generic continuation waiting in EntryGeometryRule.
7. Keep the final ActionableNow quality gate, RR validation, late-entry, trap-risk, divergence, indicator-fusion and market constraints intact.

## Safety / quality boundary

No public strategy threshold was lowered in this phase. The purpose is to stop the system from hiding a valid directional thesis or skipping the proper Retest/actionability evaluation path.

A directional WATCH presentation is not an execution authorization. Strong/actionable arrows remain governed by the existing quality and actionability gates.

## Verification

- Source / Architecture accumulated audits: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile/Build: PASS;
- CI-18 deterministic Retest/geometry contract and static coherence audit: PASS;

Manual target-terminal validation remains required for real chart/panel synchronization, current-price response, MTF frame updates, alert timing and empirical signal quality.

## Next

After acceptance, continue with the broader signal-quality / target-quality audit: investigate why fallback TP selection clusters around 2R, validate the full decision-to-plan-to-alert chain on replay evidence, and then address intelligent trailing/progressive target behavior without creating parallel calculation owners.