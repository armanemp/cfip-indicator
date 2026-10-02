# CI-16 — Deterministic Replay, Latency and Counterexample Suite

Date: 2026-10-02

Status: **VERIFIED COMPLETE — CI-16 accepted on implementation HEAD `1c292d3162f6085869238029fe599630ab3ca3a9`.**

## Purpose

CI-16 adds a deterministic, platform-neutral replay layer around the already-canonical calculation owners. It is deliberately implemented under `tools/CFIP.Runtime.Contracts` rather than inside the live cTrader calculation path.

The suite is diagnostic/verification-only. It does not add a second decision engine, execution engine, broker path, threshold policy, or public parameter.

## Recorded replay trace

Each scenario records:

- reference UTC time;
- quote-observation UTC time;
- causal-event UTC time;
- indicator snapshot;
- structure snapshot;
- decision snapshot;
- trigger snapshot;
- Entry/SL/TP1..TP4 geometry;
- risk/reward and effective RR;
- final ExecutionIntent geometry/pip projection;
- first-actionable timestamp;
- alert timestamp;
- execution-attempt timestamp;
- fill timestamp;
- authoritative-geometry fingerprint;
- submission-geometry fingerprint.

The suite runs the same 16 fixtures twice and compares the complete serialized trace, making nondeterministic output fail the contract.

## Counterexample matrix

The required fixtures are:

1. fast breakout;
2. slow breakout;
3. fast reversal;
4. retest;
5. range market;
6. expansion;
7. compression;
8. strong OB+FVG confluence;
9. weak single-zone setup;
10. high spread;
11. large displacement;
12. M1 confirmation late in M5;
13. M1 confirmation early in M5;
14. target obstruction;
15. opposite divergence;
16. mirrored BUY/SELL.

The mirrored case additionally verifies risk distance, target distance, nominal RR and effective RR symmetry.

## Geometry integrity

The replay does not recalculate an alternate submission geometry after the authoritative plan snapshot is created. The same canonical `ExecutionIntentGeometryRule` projection is captured and its serialized fingerprint is compared with the submission-side fingerprint.

This specifically protects the CI-15 seam against a future regression where a broker-path caller reconstructs Entry/SL/TP after validation.

## Latency

The trace exposes:

- causal → first actionable;
- actionable → alert;
- alert → execution attempt;
- execution attempt → fill.

Blocked scenarios intentionally retain no fabricated downstream timestamps. This keeps latency measurements causal and distinguishes “no action” from “zero latency”.

The current values are deterministic fixture timings, not target-terminal performance measurements. CI-17 remains the mandatory target-terminal timing/broker validation phase.

## Verification

CI-16 is verified through:

- Runtime Acceptance Contracts;
- the dedicated `tools/audit_phase_ci_16.py`;
- accumulated Source/Architecture workflow;
- existing cTrader compile and parameter/architecture gates.

No public parameter name/type/default, decision threshold, Entry/SL/TP/RR/risk/execution policy, or broker-mutation authority was changed.

## Manual boundary

CI-16 does not claim:

- real cTrader terminal latency;
- broker/server event ordering;
- actual slippage distribution;
- live pending-fill timing;
- chart/panel timing;
- empirical false-signal rate, missed-opportunity rate, realized R or profitability.

Those remain CI-17 / target-terminal or later outcome-validation boundaries.

## Next phase

**CI-17 — Target-terminal cTrader validation.**

After CI-16 is merged to `main`, the operator should run:

`git pull --ff-only`

on the local `main` checkout before continuing.

## Verified workflow results

- Source/Architecture: **PASS** — workflow run `36950516204` / run #2715;
- Runtime Acceptance Contracts: **PASS** — workflow run `36950516284` / run #2524;
- cTrader Compile/Build: **PASS** — workflow run `36950516224` / run #2708;
- accumulated CI-16 audit: **PASS**.