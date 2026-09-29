# Phase 9.14 — Signal Evidence Integrity & Consensus Calibration

Date: 2026-09-29

## Objective

Improve the quality of the canonical directional consensus without blindly
raising global thresholds or weakening the existing top-down and execution
gates.

## Finding

The previous DecisionFrameContributionCalculator converted every frame into a
directional percentage before applying quality. A frame with a small absolute
evidence base could therefore contribute a very strong directional share merely
because one side was relatively larger.

This creates a false-equivalence between a weak frame such as 36 bull / 4 bear
and a genuinely strong frame such as 90 bull / 10 bear. Both can present a
similar directional percentage even though their evidence strength is very
different.

## Implementation

### 1. Evidence-strength-aware frame contribution

The canonical frame contribution now preserves the existing directional-share
model but shrinks directional influence toward 50/50 when absolute frame
strength or evidence coverage is weak.

The modulation has two bounded parts:

- absolute bull+bear score strength;
- the existing canonical frame evidence count.

The modulation is applied to the distance from neutral rather than directly
changing a side's raw score. This means weak evidence becomes less influential
without becoming a hard veto.

### 2. Canonical adapter propagation

FrameDecisionContributionAdapter now supplies the existing frame Evidence count
to the contribution owner. No duplicate evidence calculation was introduced.

### 3. Regression contracts

Decision Contracts cover:

- weak concentrated frames being weaker than strong frames;
- weak balanced frames remaining near neutral;
- strong evidence retaining high directional influence;
- BUY/SELL symmetry;
- deterministic compatibility behavior.

## Why this addresses the reported weak-signal problem

The correction attacks a concrete source of artificial directional confidence
rather than simply raising MinimumConfidence or SmartQualityThreshold.

The intended chain is now:

absolute evidence + evidence coverage -> directional influence -> MTF consensus

rather than:

relative bull/bear ratio -> strong frame vote -> MTF consensus

This should reduce consensus inflation from several individually weak frames.
At the same time, no existing hard trigger, reward/risk, trap, structural,
broker or lifecycle blocker was removed.

## Safety and boundaries

- no second decision authority;
- no broker mutation change;
- no removal of hard execution gates;
- no new public parameters;
- BUY/SELL symmetry preserved;
- public parameter count remains 552.

This phase is an engineering correction, not proof of improved profitability.
Actual false-signal frequency, missed-opportunity rate, realized R, timing and
execution quality still require target-terminal or controlled replay evidence.

## LocalStorage

CFIP uses LocalStorageScope.Type. Current cTrader documentation places Type-scope
storage under Documents/cAlgo/LocalStorage/Indicators/CFIPIndicator/.

cTrader also documents that LocalStorage persists between stops/deployments,
while backtesting and optimisation keep LocalStorage in memory only.

## Next phase

Phase 9.15 should add the target-terminal/replay measurement layer around this
calibrated decision path and use the measurements to refine production defaults
only when out-of-sample evidence supports a change.

## Verification

Phase 9.14 is verified and merged into main.

- Runtime Acceptance: PASS
- cTrader Compile/Build: PASS
- Source/Architecture + accumulated audits: PASS
- Decision Contracts: PASS within the build workflow
- PR #56: merged
- Merge commit: 16e788f6b196afcfe2580908cbdb4dabc46cb5b

The automated gates validate deterministic consensus behavior and compile/runtime
contracts. Target-terminal/replay remains required for empirical false-signal,
missed-opportunity, timing, realized-R and live execution measurements.
