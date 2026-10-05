# CFIP Indicator — CI-03 Indicator Fusion / Correlation / Evidence Independence

Date: 2026-10-01

## Status

**VERIFIED COMPLETE — PR #157, final implementation head `feb87be7620326cc6af92077d6089ec63d94b28b`.**

## Objective

Audit indicator measurements through numerical fusion, frame scoring, decision
contribution and actionability while preventing correlated measurements from
being treated as independent evidence.

## Corrected finding

Parallel timeframe scenario enrichment maintained a second raw boolean evidence
counter and could overwrite the canonical independent-evidence score. It now
delegates to the canonical per-frame evidence owner.

## Ownership

`IndicatorEvidenceFusionRule` remains the sole numerical indicator-fusion owner.
`IndicatorEvidenceIndependenceRule` is diagnostic/provenance only and groups
correlated measurements into Trend, Momentum and Context.

Divergence remains a momentum modifier. Aggregate OSS consensus remains an
aggregate measurement and does not create another independent group.
`IndependentEvidenceFusionRule` remains the broader structural/location/context
independent-evidence family owner.

## Integration

Market-frame scoring builds one fusion input and uses it for both numerical
fusion and indicator-group attribution.

Indicator-group provenance is retained on Frame, Decision and
TradeOpportunityCandidate. It is not a new decision gate.

Parallel timeframe enrichment reuses canonical per-frame independent score/group
semantics and the canonical LocationEvidenceRule.

## Optional indicators

Existing MACD, VWAP, volume and healthy-volatility switches remain under their
existing parameter owners. Disabling one removes only its intended fusion
contribution and group presence; unrelated groups remain available.

## Decision chain

indicator measurement → fusion → frame score/quality/conflict → frame contribution
→ decision score → smart quality → confidence → actionability.

CI-03 does not change thresholds, weights, execution authority or broker mutation.

## Safety/performance

- no public parameter name/type/DefaultValue changed;
- no score/weight/confidence/RR/entry/SL/TP/risk/execution threshold tuned;
- no broker mutation path or second decision authority added;
- group attribution is constant-size;
- no additional history scan, I/O or cache added;
- duplicate parallel-scenario raw evidence counting removed.

## Manual boundary

Target-terminal timing, replay cohorts, empirical signal quality and realized
trading outcomes remain manual/replay acceptance items.

## Next phase

**CI-04 — Structure / swing / liquidity semantics audit.**

## Verification

- Source / Architecture: **PASS** — workflow run `36914068345`.
- Runtime Acceptance Contracts: **PASS** — workflow run `36914068399`.
- cTrader Compile / Build: **PASS** — workflow run `36914068401`.
- CI-03 static audit: **PASS** — accumulated as `audit_phase_ci_03.py` after CI-02.
- No public parameter/API/DefaultValue or trading-policy threshold was changed.

## Final repair during acceptance

The accumulated architecture verifier rejected overloaded/global helper names introduced by the
new frame-scoped evidence API. The helpers were renamed to unique owners and the runtime/static
contracts were aligned. No production decision threshold or numerical fusion weight changed.

## Next phase

**CI-04 — Structure / swing / liquidity semantics audit.**
