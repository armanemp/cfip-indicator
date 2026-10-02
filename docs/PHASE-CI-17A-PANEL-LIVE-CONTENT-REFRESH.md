# CI-17A — Panel Live-Content Refresh Correction

Date: 2026-10-02

Status: **REPOSITORY IMPLEMENTATION VERIFIED — target-terminal visual acceptance pending.**

## Root cause

The panel had two different refresh paths:

- RenderPanel() rebuilt panel rows only when ShouldRenderFullPanel() reported a changed presentation key.
- the 500 ms runtime heartbeat deliberately avoided RenderPanel() and updated only the clock/live rows.

The optimization was incomplete because the presentation key did not cover every mutable value rendered by the panel, including live reaction/prediction/context values. The panel could therefore remain visually frozen while runtime/calculation state continued to change.

## Corrective design

The fix separates content refresh from layout refresh:

- PanelContentRefresh.cs performs a bounded 500 ms content refresh;
- it builds one SignalVisualSnapshot;
- it calls RenderPanelRows() directly;
- it does not call RenderPanel(), rebuild the panel container or reapply layout;
- SetPanelRow() continues to suppress duplicate UI property writes;
- the existing presentation-key optimization remains responsible for full layout/presentation work;
- restoring the panel resets the content-refresh timestamp so the next visible heartbeat refresh is immediate.

The live RR heartbeat row now reads the current execution-side Bid/Ask quote before calculating directional progress RR instead of relying only on the last calculation's stored market value.

## Runtime path

The ready-state path is now:

OnTimer → HandleRuntimeHeartbeat → RefreshPanelContentIfDue → RenderPanelRows → clock/live volatile-row overlays

The normal calculation path remains:

Calculate → RenderCalculationState → RenderPanel

This prevents timer-driven UI refresh from invoking full analysis and prevents panel freshness from depending on the completeness of the presentation-key shape.

## Performance boundary

The content refresh is bounded to the existing 500 ms UI cadence.

It reuses existing row objects and the row writer's change checks. Layout sizing, panel controls and container rebuilding are not performed by the content-only heartbeat.

No strategy threshold, decision rule, execution policy, broker mutation or public parameter is changed.

## Acceptance

Repository acceptance must prove:

- heartbeat invokes the content refresh;
- content refresh is row-only and bounded;
- full layout remains presentation-key optimized;
- restore/reset forces the next refresh;
- live RR uses the current quote;
- deterministic Runtime Contracts include the correction;
- the new static audit is accumulated after CI-17.

Target-terminal acceptance remains required for visible refresh latency, panel responsiveness, live reaction/context updates and absence of stale rows.

## Next gate

After repository verification and target-terminal panel acceptance, CI-17 continues to its final manual acceptance boundary, followed by CI-FINAL.


## Repository verification continuity

Historical CI-04/05/08/09/12/13 continuity checks were reconciled to the current CI-17A state so archived phase audits validate historical records without requiring an obsolete active-phase label.


## Repository verification closeout

PR #175 merged to `main` as `6ffff643ad5c24782ca7035355e31ee4a04465c2`.

Exact implementation HEAD `9641bfc02c7604d6432202459fa198866d4a5f53` passed:
- Source/Architecture #2742;
- Runtime Acceptance Contracts #2551;
- cTrader Compile #2735.

CI-17A is therefore repository-verified. The remaining CI-17 acceptance boundary is the actual cTrader terminal: visible panel refresh latency, live reaction/context updates, startup/reload behavior and end-to-end synchronization.
