# Phase 10 — MTF Scenarios, Level Presentation, Routine and Runtime Logging

Date: 2026-09-29

## What was already complete before this phase
Phase 9.17 and 9.17.1 were merged into main. Their source-level scope included canonical live exit geometry, forward-only and monotonic TP progression, broker minimum-distance handling, transactional fill reconciliation, post-fill protection, recovery RR bounds and fail-closed numeric validation.

The latest main commit also passed the repository source/architecture and runtime-acceptance workflows.

## Gap found
The existing parallel-opportunity system had multiple lanes, but the candidates were still built from the M5 execution model and had no first-class source-timeframe identity. Therefore lane-parallel was not the same as true simultaneous timeframe scenarios.

## Phase 10 changes
- TradeOpportunityCandidate now carries ScenarioId and SourceTimeframe.
- M5/M15/M30/H1/H4/D1/W1 are evaluated from their own closed Frame objects for independent timeframe opportunity scenarios.
- Same-direction scenarios from different source timeframes are retained instead of being collapsed by proximity.
- Scenario labels identify the timeframe and use a common scenario prefix for all ENTRY/SL/TP lines.
- Main plan labels use (MTF) because the canonical decision is an aggregate multi-timeframe consensus.
- SL and TP labels show their absolute distance from Entry in pips.
- Level label anchoring has a stronger minimum horizontal gap.
- Maximum visible opportunities can now reach 8, with a default of 6.
- CFIP_RuntimeLog_*.csv records decision, scenario and execution events with scenario/timeframe, state, direction, plan geometry and broker identifiers.
- analyze_runtime_log.py performs a basic forensic pass and flags geometry and non-finite anomalies.
- ROUTINE.md defines the recurring engineering, verification, logging and reporting checklist.

## Auto-trading boundary
The new timeframe scenarios are intentionally not direct auto-trading authorities. Automatic market and pending execution keep their existing canonical decision, capacity, risk, plan-integrity and broker-geometry gates.

## Prediction boundary
Early prediction already exists and is enabled by default. This phase makes its evaluation an explicit project routine through trace/log review and outcome calibration. No accuracy claim is made without empirical replay or outcome data.

## Verification
Required after these changes:
- source/architecture checks
- runtime acceptance contracts
- parameter usage audit
- UI label/object cleanup checks
- multi-timeframe scenario coexistence checks
- runtime-log schema smoke test

Historical replay and terminal-level market/pending execution tests remain runtime evidence boundaries.