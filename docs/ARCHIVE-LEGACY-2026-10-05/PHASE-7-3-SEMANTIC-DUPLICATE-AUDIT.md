# Phase 7.3 — Semantic Duplicate Audit

Date: 2026-09-29

## Objective

Audit the full public parameter surface and production source tree for semantic duplication, redundant aliases, overlapping ownership and synchronization regressions while preserving meaningful stage-specific controls.

This phase also incorporates the user-reported chart/Signal synchronization defect discovered during the audit.

## Confirmed duplicate: structural stop enablement

The audit found a real redundant public parameter:

- `EnableDynamicSlTrail`
- `EnableStructuralSlRepricing`

Both controlled the same structural stop path in `ProtectionManager`. The runtime condition was effectively an OR between the two names with no distinct behavior.

Resolution:

- removed `EnableDynamicSlTrail`;
- `EnableStructuralSlRepricing` is now the single enablement owner;
- current public parameter surface is 533 = 530 baseline + 3 OSS extension.

This was an actual semantic duplicate, not a naming cleanup.

## Confirmed duplicate: decision facade

The whole-project duplicate-method audit found the same public method signature:

`Evaluate(DecisionInputSnapshot)`

in both `DecisionEngine` and `DecisionEvaluator`.

`DecisionEngine` was only a thin pass-through facade and added no independent decision semantics.

Resolution:

- `DecisionOrchestration` now calls `DecisionEvaluator` directly;
- redundant `DecisionEngine.cs` was removed;
- there is one concrete decision evaluation implementation.

The decision authority itself was not changed.

## Reviewed but intentionally retained parameter families

The semantic audit surfaced several groups with overlapping names or common consumers. These were reviewed and kept because their runtime roles are distinct.

### Target RR

`Tp1MinimumRR..Tp4MinimumRR` are primary target-stage minimum RR semantics.

`FallbackTp1RR..FallbackTp4RR` are fallback floors used when candidate target selection cannot provide the preferred structural target.

`SmartTargetMinimumRR` constrains smart target selection.

`MinimumTradeRR` is the overall plan acceptance floor.

`MinimumHtfTargetRR` is the HTF target requirement.

These are related but not interchangeable.

### Entry and quality

`MinimumEntryQuality`, `MinimumEntryLocationQuality`, `MinimumAutoLevelQuality`, `SmartTargetQuality`, `SmartStopQuality` and `MinimumStructuralStopQuality` apply at different decision, location, execution and protection stages.

No parameter was raised merely to make signals rarer.

### Trail geometry

`TrailDistanceAtr`, `SlRepriceBreathingAtr`, `TrailStepAtr`, `SlRepriceStepAtr`, `TargetUpdateStepAtr` and `SmartTrailMomentumBonusAtr` have different responsibilities: breathing room, reprice breathing limit, minimum stop progression step, live-plan update threshold, target update threshold and momentum tightening.

They are not semantic duplicates.

### Cooldowns

`CooldownBars`, `CooldownM5Bars`, `OppositeSignalCooldownM5` and `ExitReentryCooldownM5` are not silently merged because they protect different event classes even when they share M5 arithmetic.

The project audit records their coexistence for future empirical calibration.

## Signal / level synchronization regression found during this phase

The execution model and setup preview are constructed before `Decision.TriggerReady`. Actual plan creation remains correctly gated by `TriggerReady`.

The visual snapshot, however, previously exposed setup levels only when the pre-trigger preview direction matched the later canonical visual direction. During a directional but not-yet-triggered state this could hide Entry / Ideal / Trigger / SL / TP until price had already crossed the trigger.

Resolution:

- setup preview is now visible for a valid directional preview aligned with the current directional decision;
- this exposes the structural forecast before trigger confirmation;
- actual execution-plan creation remains `TriggerReady` gated.

Thus presentation is early, execution remains guarded.

## Permanent audit system

Two permanent source checks are now layered into CI:

- `tools/audit_parameter_semantics.py` — semantic parameter family review plus exact declaration uniqueness;
- `tools/audit_project_integrity.py` — whole production-tree integrity including exact duplicate method signatures, parameter uniqueness, canonical visual ownership, execution UI authority, and continuity documentation.

The existing parameter, runtime UI and architecture audits remain active.

The repository workflow now explicitly requires the whole-project audit for every phase and treats the repository documentation as the continuity source.

## Verification targets

A phase is complete only when all are green:

- Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile;
- parameter/dead-symbol audit;
- runtime UI audit;
- semantic parameter audit;
- full project integrity audit.

Hands-on cTrader validation is separate and must not be claimed without terminal observation.

## Continuity

Next planned phase after completion: Phase 7.4 — MaximumOpenPositions semantics.

Track 8 remains the dedicated analytical-correctness track, including the planned deep Order Block mathematical audit in Phase 8.4.


## Verification result

Final branch-head automated verification passed:

- Source / Architecture: PASS;
- dead/unused parameter audit: PASS — 533 declared, 533 read-by-code, 0 unused;
- runtime UI audit: PASS;
- semantic parameter audit: PASS;
- full project integrity audit: PASS — 419 production C# files scanned, 0 exact duplicate method signatures, canonical visual/safety/authority checks PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

The full audit also reports that the roadmap contains historical phase identifiers whose detailed log headings predate the current continuity discipline. Those historical gaps are informational; current phases are required to record implementation, findings, verification and operator pull state before completion.

## Merge

Merge is performed only after the above branch head remains green. The merge SHA and final main documentation commit are recorded immediately after merge.


## Merge record

Phase 7.3 was merged into main as PR #31 with merge commit a968550cae91e0dc2c54662f6dca2c5411654289.

The final pre-merge branch head passed all required automated gates:
- Source / Architecture: PASS;
- dead/unused parameter audit: PASS;
- runtime UI audit: PASS;
- semantic parameter audit: PASS;
- full project integrity audit: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

Local pull is required after the merge boundary before the next implementation phase.
