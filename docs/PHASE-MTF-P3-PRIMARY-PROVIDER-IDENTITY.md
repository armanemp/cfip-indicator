# MTF-P3 — Primary M15/H1 Provider Scenario Identity Cohesion

Date: 2026-10-02

## Goal

Keep the source-timeframe and scenario identity of the canonical provider envelope
consistent with the exact scenario selected from the existing scenario registry.

The provider is a read-only bridge. This phase does not add broker authority and does
not create a second decision authority.

## Design

- The existing scenario registry remains the source of candidate identity.
- Existing canonical scenario resolution remains the authority for plan/execution matching.
- Provider source timeframe is derived from the exact scenario candidate rather than the
  chart's attached timeframe.
- Provider execution-intent identity receives the same resolved source timeframe.
- Pending Stop and Pending Limit submission paths reuse the canonical direction scenario
  identity rather than constructing independent hard-coded scenario identifiers.
- M15/H1 primary candidates remain observe-only for execution; this phase does not change
  that policy.

## Invariants

For a matched candidate:
- ScenarioId is the candidate ScenarioId.
- SourceTimeframe is the candidate SourceTimeframe.
- If no source-timeframe candidate is available, the canonical provider fallback is M5.
- The same resolved source timeframe is used by the envelope identity and canonical
  execution-intent identity.

This is an identity/traceability correction, not a strategy or threshold change.

## Safety

No public parameter changed. The public parameter count remains 568.

No confidence, quality, RR, Entry, SL, TP, risk or trading threshold changed.

No broker mutation API was added. No execution authority or decision authority was added or changed.

## Verification

Repository gates:
- accumulated Source/Architecture audit;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- accumulated MTF-P1 and MTF-P2 audits.

Runtime contract coverage includes:
- M15 candidate identity preservation;
- H1 candidate identity preservation;
- M5 fallback behavior;
- explicit fallback ScenarioId preservation.

Manual boundary:
- verify target-terminal provider identity against simultaneous M15/H1 chart presentation;
- verify cBot shadow telemetry shows the same ScenarioId/SourceTimeframe identity seen by
  the Indicator provider.

## Operator action

After merge to main, run: `git pull --ff-only`.
