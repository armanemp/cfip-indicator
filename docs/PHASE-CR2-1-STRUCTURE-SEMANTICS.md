# CR2.1 — Structure / CHoCH / MSS / Sweep / Divergence / Rejection

Date: 2026-09-30

Status: IMPLEMENTED — CI acceptance pending.

## Scope

CR2.1 addresses the confirmed/partial Prompt 2 structural findings without changing public parameter names/types/defaults or introducing a second decision/execution authority.

## B1 — CHoCH semantics

Implemented:

- canonical fresh-break detection now requires a closed-bar threshold crossing;
- the previous close must be on the non-broken side of the same structural threshold;
- CHoCH requires prior opposite structural evidence plus a fresh opposite break;
- the previous rolling-high/rolling-low CHOCH proxy has been removed;
- structural event identity distinguishes direction, event type, source index and break index.

This preserves deterministic closed-bar semantics and prevents a persistent already-broken price state from being emitted as a new event.

## B2 — Structure / MSS event freshness

Implemented:

- Structure and MSS both use the same canonical fresh-break rule;
- StructuralSequence counts Structure/MSS/CHoCH as one canonical structural event on a bar;
- displacement remains a separate evidence dimension;
- the same structural break can no longer inflate the sequence score by stacking multiple labels.

## A4 — Active liquidity / sweep semantics

Implemented:

- latest canonical swing high/low helpers now return the structural plateau identity;
- a sweep is accepted only while the selected liquidity level remains active/unbroken;
- prior closed bars are checked for a directional close beyond the liquidity level, with a bounded two-pip anchor tolerance;
- volatility-relative sweep penetration remains owned by the existing ATR rule;
- rolling raw extremes are still not treated as structural liquidity identities.

The scan is bounded by the structural lookback and runs only within the existing closed-bar/frame analysis path.

## B11 — Divergence conflict

Implemented:

- divergence conflicts remain visible as `CONFLICT`;
- conflict Direction is neutral;
- conflict Quality is zero because it is not a directional-strength value;
- bullish/bearish diagnostic flags remain available;
- a conflict cannot satisfy the divergence signal contract.

The fixed RSI/WaveTrend thresholds are not retuned here; their centralization remains CR2.9.

## B12 — Rejection / doji

Implemented:

- directional rejection now has a meaningful-body requirement;
- very small-body/doji-like candles are treated as indecision rather than rejection evidence;
- bull/bear rejection geometry remains symmetric;
- no public rejection threshold parameter was introduced.

## B10 — structural timeframe fallback

Implemented:

- supported structural HTF names are centralized;
- unknown timeframe values fail validation;
- StructuralStopCandidateEvaluator no longer silently substitutes W1 for an unknown timeframe;
- unknown/non-resolvable structural-stop candidates are rejected before scoring.

No reward-path/tight-stop score retuning is performed here because that remains evidence-dependent.

## Verification

Deterministic runtime contracts cover:

- fresh bullish/bearish break crossings;
- repeated-break suppression;
- CHOCH prior-opposite-structure requirement;
- structural event identity;
- active/unbroken bullish and bearish liquidity;
- rejection/doji behavior;
- divergence conflict neutrality;
- supported/unknown structural timeframe behavior.

A dedicated Source/Architecture gate `tools/audit_phase_2_1.py` enforces the canonical owner boundaries and prevents reintroduction of the old CHOCH rolling-extreme/fallback patterns.

## Permanent routine audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed for structural evidence propagation.

Performance/code cleanliness:

- no network or persistence work was added;
- structure freshness uses O(1) local comparisons;
- active liquidity validation is bounded by the existing structural lookback;
- no duplicate decision or execution authority was introduced;
- public parameter surface is unchanged.

## Acceptance boundary

CI must pass Source/Architecture, Runtime Acceptance and cTrader Compile/Build for the final branch head.

Target-terminal replay remains required before any empirical claim about signal frequency, missed opportunities, false signals or realized trading outcomes.
