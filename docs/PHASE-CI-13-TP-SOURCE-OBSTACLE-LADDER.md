# CI-13 — TP Source, Target Obstacle and Coherent TP Ladder Audit

Date: 2026-10-02

Status: **IMPLEMENTED — repository verification pending merge**

## Scope

CI-13 audits the full take-profit construction boundary after the CI-12
structural-stop correction:

`target sources → source age/provenance → obstacle path → TP1..TP4 selection → plan metadata`

The phase is a correctness and ownership correction. It is not a threshold,
RR, confidence or profitability-tuning phase.

## Verified findings

### 1. TP selection was greedy

The previous `SelectTargets` implementation selected TP1, then TP2, then TP3,
then TP4 independently. A high-scoring early target could therefore eliminate
a globally better coherent sequence even when another TP1 candidate enabled a
stronger TP2/TP3 continuation.

The selection contract is now a bounded global path search over the already
validated stage candidates. Every next stage must be directionally progressive
relative to its actual predecessor.

### 2. Clustering could fabricate a target price

Target-level clustering previously replaced two nearby real source prices with a
weighted average. That value could be neither source price, weakening the
meaning of source provenance and making later metadata attribution ambiguous.

The cluster now preserves the first, score-ordered source as the authoritative
representative. Confluence changes only score and hit count; it does not invent
a new target price or overwrite the representative source identity.

### 3. Plan metadata could misidentify synthetic targets

Plan materialization previously searched the entire candidate list by proximity
to the final target and could label a synthetic RR fallback as a real FVG/OB,
liquidity or HTF source merely because another candidate was close.

Plan target metadata now binds directly to the selected ladder object for each
stage. A fallback without a selected source is explicitly labeled
`SYNTHETIC_RR`.

## Implementation

- Added platform-neutral `TargetLadderOption`.
- Added platform-neutral `TargetLadderSelectionRule`.
- Replaced stage-by-stage greedy selection with coherent path selection.
- Kept stage feasibility, source-age rules and canonical obstacle checks in their
  existing owners.
- Removed selected-list coupling from `TryScoreTargetCandidate`.
- Preserved target-source identity through target clustering.
- Bound plan TP source/quality metadata to the exact selected source.
- Kept live/enrichment callers of the legacy candidate-proximity helper intact
  where they are observing an already known level rather than materializing a
  new plan.
- Added deterministic BUY/SELL mirror, global-path regression, stage-termination
  and invalid-direction contracts.
- Added `tools/audit_phase_ci_13.py` and wired it into the accumulated
  Source/Architecture workflow.

## Source and obstacle boundary

CI-13 does not replace the existing canonical obstacle owners.

Target candidate evaluation continues to consume:

- M5 swing/equality obstacle validation;
- opposing FVG/OB reward-path validation;
- HTF zone-path validation.

Those path checks remain candidate-specific after their bounded structural scans,
so changing a target cannot reuse a stale target-specific boolean.

Target source families remain:

- M5 structure / swing;
- M5 FVG / Order Block;
- supply/demand;
- M5 liquidity and session levels;
- daily pivots;
- HTF swing/FVG/OB/liquidity;
- previous-period levels;
- extended liquidity forecasts;
- explicit synthetic RR fallback only when policy permits.

## Safety boundary

No public parameter name, type or `DefaultValue` changed.

No default RR, stop, confidence, target-age, obstacle or execution threshold was
tuned.

No second decision authority or execution authority was introduced. Broker
confirmation remains authoritative.

## Verification boundary

Required repository checks:

- Source / Architecture with accumulated CI-00..CI-13 audits;
- Runtime Acceptance Contracts;
- cTrader Compile / Build;
- Planning Contracts, including the new ladder invariants.

Manual acceptance still required for:

- target-terminal broker/runtime timing;
- live target rendering;
- broker-specific distance behavior;
- deterministic replay against real feed data;
- empirical distribution of TP1..TP4 selection and realized outcomes.

## Next phase

**CI-14 — Canonical risk/reward and protection mathematics.**
