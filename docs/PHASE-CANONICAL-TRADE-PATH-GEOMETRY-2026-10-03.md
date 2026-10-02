## Candidate handoff hardening

A second live-path seam was found after the PlanBuilder correction: actionable `TradeOpportunityCandidate` objects could still copy Entry/SL/TP from the presentation preview even though Actionability had already approved the canonical actual-entry geometry.

Closed:
- actionable candidates now rebind their executable preview from `CanonicalTradePathGeometry`;
- candidate Entry/SL/TP/Risk/RR are therefore the same geometry that passed Actionability;
- non-actionable WATCH/presentation candidates remain preview-oriented so early source visibility is not coupled to executable planning;
- the canonical static audit now enforces this provider/cBot handoff contract.

This does not lower any quality, RR or risk gate.

# Canonical Trade-Path Geometry — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

## Goal

Make one exact actual-entry trade path authoritative from live actionability through Plan creation and cBot handoff, so Entry/SL/TP/RR cannot drift between analytical preview and executable geometry.

## Root cause

The live actionability path could evaluate the current ActualEntry against Stop/TP1 values originating from a presentation-oriented preview. The initial canonicalization fixed actionability, but a deeper audit showed that PlanBuilder still independently rebuilt the structural stop and TP1..TP4 ladder. Two calculation paths meant future drift could reappear even when current values matched.

## Completed

- added `CanonicalTradePathGeometry` as the canonical executable geometry snapshot;
- one canonical builder derives structural SL and TP1..TP4 from `ExecutionModel.ActualEntry`;
- the builder validates direction, execution mode, structural stop side, stop-risk envelope, target path and TP1 RR;
- selected target provenance (source, quality and HTF count) is carried in the canonical snapshot;
- live actionability consumes the canonical path and rejects Entry/Mode drift;
- live reward/RR checks use canonical Entry/SL/TP1;
- PlanBuilder no longer independently calls structural-stop or target-ladder construction;
- Plan preparation now consumes the canonical path and passes its Entry/SL/risk into plan materialization;
- bounded entry/spread cache reuse keeps the correction off the per-tick rebuild hot path;
- trace/panel lifecycle state prioritizes `ActionableNow` before generic trigger-wait presentation;
- dedicated canonical geometry audit remains accumulated in Source/Architecture.

## Architecture boundary

`Pre-analysis -> M15 decision -> M5 tuning/zone -> M1 optional -> Entry geometry -> Structural SL -> TP1..TP4 reward path -> Actionability -> Plan -> SignalEnvelope/Scenario -> cBot preflight -> Broker -> Protection -> Outcome/History`

Canonical timeframe contract:
- M15 = trade-decision / execution reference;
- M5 = trigger, tuning and entry precision;
- M1 = optional confirmation;
- H1+ = context/reward support.

Indicator remains broker-mutation-free. cBot remains the sole broker execution/lifecycle authority.

## Safety and quality

No public confidence, smart-quality, MTF, evidence, structure, RR or risk threshold was lowered.

RetestMarket remains zone-driven; BreakoutMarket and predictive pending modes remain trigger-dependent through the canonical mode-aware trigger policy.

The phase makes the calculation path more consistent; it does not claim improved profitability. Empirical replay/OOS/forward-demo evidence remains required.

## Verification

Required:
- canonical trade-path static audit;
- accumulated Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- target-terminal validation of exact Entry/SL/TP values at proposal and cBot broker handoff;
- restart/reconnect and stale/rebind validation.

## Operator action

After verified merge: `git pull --ff-only` on local `main`.

