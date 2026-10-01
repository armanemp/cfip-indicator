# CI-09 — Decision Engine Mathematical Integrity Audit

Date: 2026-10-02

## Objective

Audit the canonical decision chain:

`frame contribution → score → consensus → edge → quality → confidence → gates → EntryAllowed`

The phase is restricted to mathematical correctness, ownership, traceability, symmetry, finite-value handling and deterministic behavior. It does not retune public trading parameters or execution policy.

## Findings addressed

### 1. Consensus tie bias

The previous consensus selector used `buyShare >= sellShare` for BUY. With a valid 50/50 consensus this made an exact tie directional BUY when the minimum share threshold was 50.

Correction:
- exact ties are now neutral (`Direction = 0`);
- BUY requires strictly greater BUY share;
- SELL requires strictly greater SELL share;
- share/edge symmetry remains unchanged.

### 2. Non-finite consensus inputs

The consensus boundary previously used the generic clamp path for the exponent argument. NaN/Infinity inputs were not explicitly rejected there.

Correction:
- non-finite BUY/SELL/temperature inputs fail closed to a neutral 50/50 consensus;
- exponent inputs use the NaN/Infinity-safe `ClampDouble` boundary;
- invalid/non-positive temperature is neutral rather than producing undefined direction behavior.

### 3. Conflict-penalty symmetry

The adaptive-regime conflict path previously selected BUY when BUY and SELL scores were exactly equal because it used `buy >= sell`.

Correction:
- the stronger side alone receives the conflict reduction;
- an exact score tie receives the same bounded reduction on both sides;
- conflict cannot manufacture a directional bias.

### 4. Score traceability

`DecisionScoreSnapshot` now exposes the canonical score components:
- per-timeframe BUY/SELL contributions;
- Advanced Confluence;
- Premium/Discount;
- adaptive-regime adjustment;
- conflict penalties;
- choppiness multiplier;
- final BUY/SELL totals.

This is diagnostic provenance only. Direction still comes from the single canonical consensus calculator.

### 5. Finite frame-score handling

Decision frame contributions are now treated as fail-closed when their numeric score/weight inputs are non-finite. This prevents malformed numeric inputs from propagating into consensus.

## Mathematical contract

The canonical score decomposition is:

`PreConflictSide = Σ(frame contributions) + optional confluence + optional premium/discount + adaptive adjustment`

`PostConflictSide = PreConflictSide - ConflictPenaltySide`

`FinalSide = PostConflictSide × ChoppinessFactor`

Consensus uses the bounded exponential transform:

`pBUY = exp(clamp((BUY-SELL)/T)) / (exp(clamp((BUY-SELL)/T)) + exp(clamp((SELL-BUY)/T)))`

with the existing bounded exponent range and configured direction-share threshold.

Confidence remains a deterministic weighted score, not an empirical probability:
- strongest consensus share: 45%;
- timeframe agreement: 25%;
- Smart Quality: 30%;
- then bounded contextual calibration adjustment and higher-timeframe penalty.

Calibration remains downstream of opportunity-lane resolution and is bounded by the existing calibration cap.

## Verification contracts added

Runtime/Decision Contracts now verify:
- exact consensus ties remain neutral;
- score components are materialized and reconstruct final BUY/SELL totals;
- choppiness modifies both sides symmetrically;
- score outputs remain finite;
- existing BUY/SELL consensus symmetry remains intact;
- deterministic confidence behavior remains intact.

Static audit `tools/audit_phase_ci_09.py` verifies:
- one score owner;
- explicit score provenance;
- M1 remains confirmation-only;
- symmetric conflict handling;
- finite-safe consensus;
- canonical confidence/quality formulas;
- penalty-only higher-timeframe handling;
- post-context calibration;
- explicit filter ordering;
- CI accumulation and continuity records.

## Safety / non-change boundary

- no public parameter name, type or `DefaultValue` changed;
- no confidence, RR, SL/TP, risk or execution threshold was retuned;
- no second decision or broker-mutation authority was introduced;
- no new broker enumeration or unbounded cache was introduced;
- indicator remains analysis/signal authority; execution separation remains a later architectural track.

## Performance / cleanliness

The changes add only scalar arithmetic and immutable diagnostic fields to the existing decision calculation. No new loops over market history, persistence I/O, chart enumeration or broker enumeration were introduced.

## Empirical boundary

Static and deterministic contracts prove mathematical/architectural behavior only. They do not prove improved win rate, expectancy, false-signal rate or live execution quality. Target-terminal replay remains required for those measurements.

## Current verification state

Implementation branch: `phase/ci-09-decision-mathematical-audit`

Repository verification is intentionally reported only after the CI workflow executes on the exact implementation head.
