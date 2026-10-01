# CR5.3 / E3 — Independent-evidence group counting for parallel opportunities

Status: **IMPLEMENTATION COMPLETE — repository verification pending.**

## Scope

Make independent-evidence semantics explicit for parallel opportunities without
reinterpreting or retuning the existing numerical decision score.

The existing detailed evidence score remains authoritative for current decision
thresholds and quality calculations. A separate group count records how many
independent evidence families are actually represented.

## Canonical ownership

IndependentEvidenceFusionRule is the single platform-neutral owner for:

- the existing bounded independent-evidence score;
- independent evidence-family counting.

The four independent families are:

1. Structural: Structure / transition (MSS/CHOCH) / displacement;
2. Location: liquidity / FVG / Order Block;
3. Trend-Momentum: trend / momentum / MACD / VWAP;
4. Context: volume / volatility / rejection / equal-level.

Multiple correlated observations inside one family count as one independent
group. The existing score still preserves its established bounded intra-family
weights and 0–8 range.

## Integration

- IndependentEvidenceAnalyzer builds one canonical directional evidence input
  and delegates both score and group-count semantics to Core;
- Decision exposes the selected-direction independent group count;
- TradeOpportunityCandidate carries both the legacy independent score and the
  independent group count;
- ParallelOpportunityBuilder populates both fields without changing candidate
  quality thresholds or execution policy;
- the previous duplicate IndependentEvidenceFusionCalculator owner was removed.

## Deterministic evidence

Covers:

- three correlated structural observations remain one independent group;
- liquidity/FVG/OB observations remain one location group;
- all enabled families resolve to exactly four groups;
- BUY/SELL directional projections preserve the same family semantics;
- empty evidence resolves to zero groups;
- the legacy structural score remains unchanged at its established value.

## Safety boundary

- no public parameter name, type or DefaultValue changed;
- no RR, confidence, stop, target, actionability or execution threshold changed;
- no decision authority or broker execution authority changed;
- the existing independent-evidence score remains the behavior-driving contract;
- group count is additive diagnostic/provenance state only.

## Routine audit

The phase reviewed the complete project flow:

Analysis → Decision → Signal → Alert → Execution → Broker confirmation →
Protection/Lifecycle → Outcome → Learning.

No alternate evidence authority or execution path was introduced. The performance
surface remains bounded: group counting is a constant-size Core operation over a
small fixed evidence vector and does not add market-history scans.

## Verification boundary

Repository verification must pass:

- Source/Architecture, including audit_phase_5_3.py;
- Runtime Acceptance Contracts;
- cTrader Compile.

Target-terminal startup/readiness, panel behavior, broker lifecycle, restart/
reconnect and empirical signal-quality/outcome validation remain manual and are
not inferred from repository CI.

## Transition

After repository verification passes, the next phase is CR5.4 / E4 —
Pending-order post-fill absolute SL/TP reconciliation.
