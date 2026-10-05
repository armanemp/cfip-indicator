# CR5.5 / E5 — Parallel-scenario computation, candidate ownership and MicroReaction safety

Status: **VERIFIED COMPLETE — PR #121; implementation head `5b34afbdc7ed252a3fabc68cdb9859818cb911e6`.**

## Scope

E5 closes three related issues in the parallel-opportunity path without changing
public parameters, default thresholds or execution authority:

1. repeated closed-M5 execution/stop geometry work across tactical evaluation and candidate materialization;
2. split candidate ownership between the presentation list and the registry;
3. MicroReaction parallel presentation consuming a live intrabar direction/quality without an exact closed-bar identity check.

## Root causes

### Shared computation

EvaluateTacticalOpportunityForDirection() and BuildLaneCandidate() both rebuilt the
same M5 execution zone and structural stop for the same closed M5/direction.
Target-level construction already had a bounded cache, but the preceding geometry
was still duplicated.

### Candidate ownership

Parallel candidate insertion performed identity/replacement decisions inside the
presentation list, then copied selected candidates into TradePlanRegistry.
This left replacement and display selection split across two state holders.

### MicroReaction

BuildReaction() intentionally evaluates the current open M5 bar for intrabar
reaction intelligence. The parallel scenario builder was using that live
Direction and Confidence directly. That crossed the documented intrabar/closed
bar boundary for a scenario that is archived and displayed against closedM5.

## Implementation

- added Core ParallelScenarioGeometry as a platform-neutral geometry model;
- added a bounded per-closed-M5/direction cache for shared parallel geometry;
- refactored regular preview construction through a shared geometry tail so the
  target-selection pipeline remains single-owner;
- tactical parallel assessment now consumes the same execution/stop/ATR geometry;
- TradePlanRegistry.UpsertScenario() is now the authoritative parallel candidate replacement owner;
- Core ParallelScenarioSelectionRule owns scenario identity, coverage,
  deterministic replacement and visible-scenario selection;
- presentation list becomes a snapshot selected from the registry;
- Decision now records ReactionConfirmedM5 and ReactionConfirmedDirection;
- closed reaction confirmation resolves BUY/SELL independently and only adopts
  confirmation metrics when the confirmed direction matches the live reaction;
- Core MicroReactionSafetyRule requires exact closed-M5 identity, direction match,
  closed confirmation and the existing strong-quality threshold before a
  MicroReaction parallel candidate can be presented;
- no public parameter name/type/default changed;
- no RR, confidence, stop, target or execution threshold was tuned.

## Deterministic evidence

Runtime contracts cover:

- explicit scenario identity and timeframe-scoped coverage;
- close-geometry higher-priority replacement;
- rejection of stale materially different geometry;
- acceptance of newer materially different geometry;
- visible-scenario coverage preservation;
- MicroReaction exact closed-bar acceptance;
- stale bar, direction mismatch, weak confirmed quality and unconfirmed-state rejection.

## Routine project audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation →
Protection/Lifecycle → Outcome → Learning was reviewed.

The parallel path remains observe-only for independent timeframe scenarios. The
phase does not create a second broker execution authority.

## Performance / cleanliness

The shared geometry cache is bounded to the active closed M5 and the two
directions. It prevents repeated execution-zone and structural-stop construction
during the same parallel refresh while preserving the existing target-level cache.

No full-history pass or timer-driven duplicate calculation was introduced.

## Verification

Final repository verification on implementation head `5b34afbdc7ed252a3fabc68cdb9859818cb911e6`:

- Source/Architecture: PASS — workflow run `36836762005`, including `audit_phase_5_5.py` and the accumulated routine/optimization audits;
- Runtime Acceptance Contracts: PASS — workflow run `36836762039`;
- cTrader Compile: PASS — workflow run `36836762006`.

During closeout, the accumulated Phase 11.4 audit was reconciled to the new Core
scenario-selection ownership; `audit_phase_11_4.py` now checks the same identity
and coverage semantics without requiring the removed presentation-layer helpers.

## Verification boundary

Target-terminal behavior remains a manual boundary for:

- intrabar/closed-bar timing under live ticks;
- panel/visual presentation;
- broker lifecycle/restart/reconnect;
- empirical scenario quality and profitability.

## Transition

After repository verification, the next phase is CR5.6 / E6 according to the
Prompt 5 sequence in docs/CFIP-ROADMAP.md.
