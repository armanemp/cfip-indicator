# CI-14 — Canonical Risk/Reward and Protection Mathematics

Date: 2026-10-02

Status: **IMPLEMENTED — awaiting repository CI verification before merge.**

## Scope

CI-14 removes the calculation seam where the same trade geometry could produce different RR values depending on whether it was evaluated during candidate selection, plan materialization, actionability, execution, live lifecycle or panel presentation.

The canonical owner is:

src/CFIP.Indicator/Core/Math/RiskRewardMathRule.cs

It owns:
- shared RiskFromLevels construction for stored plan risk;
- directional protection/target-side validation;
- normalized risk with an explicit physical distance floor;
- reward distance;
- nominal RR;
- spread-adjusted effective risk;
- spread-adjusted effective RR;
- minimum/maximum RR normalization;
- synthetic target construction from RR;
- directional live-progress RR.

## Semantic contract

For a valid directional plan:

Risk = max(distanceFloor, abs(entry - stop))

Reward = abs(target - entry)

NominalRR = Reward / Risk

EffectiveRisk = Risk + max(0, spread)

EffectiveRR = Reward / EffectiveRisk

A configured maximum RR is normalized so it can never be below the active minimum RR. The physical floor is passed explicitly by the caller; cTrader production callers use Symbol.PipSize so the stored plan risk and all RR consumers share the same boundary behavior.

The calculation owner is deliberately separated from policy. Stage-specific minimums, adaptive stop-risk policy, lane policy and execution eligibility remain in their respective policy modules and call the canonical owner.

## Caller migration

The canonical owner now feeds:

- target candidate filtering;
- target preparation;
- structural-stop reward-path evaluation;
- plan reward integrity for TP1..TP4;
- plan reward-quality validation;
- actionability;
- market pre-trade geometry;
- market/aggressive/pending execution validation;
- parallel opportunities and tactical opportunities;
- pending-plan snapshots;
- live-plan creation and RR recalculation;
- live further-target selection;
- live exit geometry;
- synthetic execution/recovery targets;
- chart LIVE-RR display and heartbeat refresh.

The previous risk/reward arithmetic in those boundaries is no longer an independent implementation.

## Important defect corrected

Before CI-14, some paths stored:

Risk = max(Symbol.PipSize, abs(entry - stop))

while calculating RR with:

abs(target - entry) / abs(entry - stop)

This produced different RR values for the same plan around sub-pip risk. Candidate selection had the same normalization in one place, while other final and live paths did not.

CI-14 makes the physical floor explicit and shared, eliminating this semantic drift.

## Spread semantics

Stored-plan reward validation evaluates nominal geometry without inventing a historical spread snapshot. Actionability and execution pass the current canonical quote spread into the same owner, producing the effective RR used for live authorization.

This keeps historical plan geometry deterministic while preventing a moved live spread from being ignored at execution time.

## Deterministic contracts

tools/CFIP.Planning.Contracts/Program.cs adds CI-14 fixtures for:

- canonical BUY/SELL mirror equality;
- nominal versus effective RR with spread;
- pip-floor boundary behavior;
- wrong-side target rejection;
- synthetic target BUY/SELL symmetry;
- live-progress RR BUY/SELL symmetry.

tools/CFIP.Decision.Contracts/Program.cs is updated to the new execution geometry contract.

## Static audit

tools/audit_phase_ci_14.py verifies:

- canonical owner presence and required semantics;
- canonical consumption at candidate/plan/action/execution/live/panel boundaries;
- absence of known duplicated RR formulas in audited production consumers;
- accumulated Source/Architecture workflow wiring;
- planning contract fixture wiring.

## Safety / non-goals

No public parameter names, types or defaults were changed.

No confidence, decision score, entry, SL, TP, risk or execution threshold was retuned as part of this phase. The phase changes calculation ownership and normalization consistency, not strategy tuning.

CI-15 remains the required next phase for exact value tracing through every execution/broker path.

## Verification gate

Required exact implementation-head checks before merge:

- Source/Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- Planning Contracts.

The target cTrader terminal, broker-specific distances, actual fills, panel render timing and empirical signal quality remain CI-15/CI-17 acceptance boundaries.