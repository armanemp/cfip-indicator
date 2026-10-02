# CBOT-6M + Trade Quality Hardening — 2026-10-02

## Status

Implementation is complete on `phase/cbot-6m-quality-engine-hardening-2026-10-02`; verification is required before merge.

## Execution architecture

`Indicator analysis/signals → SignalEnvelope + SignalScenarioBatch → Device LocalStorage → exact Indicator InstanceId binding → cBot SignalEnvelope preflight → scenario policy/capacity → broker execution → broker-confirmed reconciliation/protection`.

M15 remains the canonical trade-decision/execution reference. M5 remains trigger/tuning/entry precision. M1 remains optional confirmation. H1 remains context/reward and is not independently auto-executed.

## CBOT-6M implementation

The cBot now:
- accepts a bounded `Max Concurrent Scenarios` capacity;
- consumes an instance-scoped scenario batch while retaining single-envelope fallback;
- processes each scenario through the same canonical SignalEnvelope preflight;
- preserves ScenarioId/PlanId/idempotency identity;
- uses scenario-specific broker labels derived from the stable instance ownership root;
- blocks a duplicate active object for the same scenario;
- counts broker positions and pending orders across the instance's scenario namespace;
- materializes Market, Pending Stop and Pending Limit actions independently per scenario;
- keeps the existing global demo-session execution cap;
- retains live-account blocking;
- tracks per-scenario envelopes and reconciliation results;
- sweeps per-scenario protection recovery after batch processing;
- clears scenario state when Indicator rebinding changes the active InstanceId.

## Trade quality hardening

The existing candidate evidence model is now actively used in scenario ranking. The composite trade-quality rank considers:
- independent evidence-group count;
- indicator-independent evidence groups;
- OB/FVG confluence;
- primary location confluence/quality;
- WaveTrend quality;
- TP1 RR;
- entry distance from the execution window;
- basic numeric/risk completeness.

This ranking is additive and bounded. It does not introduce a second signal engine or lower existing quality/actionability gates. M15 primary visibility remains governed by its existing source-quality contract.

## Position / SL / TP continuity

The execution-zone selector remains reward-aware across FVG/OB and structural families. Structural stop and target discovery continue to use their canonical planning owners. 6M does not calculate a second Entry/SL/TP model inside the cBot; it executes the already validated Indicator plan.

## Verification

Required:
- Source/Architecture full accumulated gate including CBOT-6M and trade-quality checks;
- cTrader compile;
- Runtime acceptance contracts;
- target terminal with multiple distinct ScenarioIds on the same symbol;
- duplicate ScenarioId replay must not create a second broker object;
- scenario A/B lifecycle and protection recovery must remain independent;
- cBot/Indicator restart order must not lose scenario identity;
- panel must distinguish cBot presence from exact-instance execution capability.

No claim of profitability or empirical win-rate improvement is made by static verification.

Operator action after merge: `git pull --ff-only`.
