# CFIP — CR2.9 Structural Stop, Divergence and Rejection Guardrails — 2026-09-30

## Status

**VERIFIED COMPLETE — PR #94 merged to main.**

- Pull request: #94
- Final verified implementation head before merge: 8e51396c89498b84ac569b29517d4ba2ba6c8f45
- Merge commit: 6786af20d7b63d371c57890fb1adabb666f830bc

## Scope

This phase implements only Claude findings B10, B11 and B12.

## B10 — Structural stop guardrail refinement

The existing structural-stop path was audited first. Unknown structural timeframes already failed closed and the reward-path gate already existed, so no unverified live score tuning was introduced.

Implemented:
- extracted the existing reward-path score bonus into StructuralStopScoringRule;
- extracted the existing preferred-risk balance contribution into the same pure rule;
- preserved the current coefficients and caps;
- added deterministic fixtures proving the reward-path bonus is bounded and the risk-balance contribution peaks at preferred stop risk and is symmetric around it;
- retained the explicit fail-closed unknown-timeframe owner.

Boundary:
- no public stop/RR parameter or live scoring constant was tuned from source review alone;
- further empirical tight-stop bias decisions require target-terminal replay/outcome cohorts.

## B11 — Divergence threshold/conflict refinement

Implemented:
- added DivergenceThresholdRule as the single semantic owner for divergence quality, conflict margin, ATR price-excursion thresholds, RSI/WaveTrend deltas, recency boosts and quality-score components;
- migrated regular and hidden bullish/bearish divergence paths to the canonical thresholds;
- preserved the existing threshold values;
- retained non-directional CONFLICT semantics so conflicting divergence cannot become directional strength.

Deterministic contracts cover:
- threshold values;
- ATR scaling and absolute floors;
- recency boundaries;
- quality scoring monotonicity and fail-closed non-finite inputs.

## B12 — Doji/rejection hardening

Implemented:
- centralized minimum meaningful body and wick thresholds in RejectionRule;
- made Doji classification and directional rejection share the same meaningful-body contract;
- preserved the existing 0.10 pip / 5% range meaningful-body rule, 1.25x wick-to-body ratio and 20% wick-range floor;
- added deterministic boundary and BUY/SELL symmetry contracts.

## Verification

Final branch verification on head 8e51396c...:
- Source / Architecture: PASS
- Runtime Acceptance: PASS — run 1597
- cTrader Compile: PASS — run 1781
- CR2.9 static audit: PASS
- accumulated CR2.1–CR2.8 audits: PASS

The Runtime Contracts build still reports the repository's existing 109 CS0649 warnings, with zero errors. These are pre-existing model-field warnings and were not introduced by CR2.9.

## Routine whole-chain audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed.

No change was made to:
- public parameter names/types/defaults;
- live signal thresholds outside centralization of already-existing divergence/rejection constants;
- Entry / SL / TP / RR policy;
- Auto Trading;
- Auto Orders;
- execution capacity;
- broker mutation authority.

OB/FVG, WaveTrend, MTF, structure and scenario execution boundaries remain intact.

## Performance/code-cleanliness audit

- all new scoring/threshold logic is pure and testable;
- no broker/UI/network/file I/O was added to Core rules;
- duplicated hard-coded divergence thresholds were removed from the analyzer;
- rejection/doji body semantics now use one owner;
- structural-stop scoring components no longer embed duplicated arithmetic inside the selector;
- no new nested helper type was introduced;
- no new execution path was created.

## Manual boundary

CI does not prove the empirical market effects of stop-selection scoring, divergence quality or rejection classification. Target-terminal replay/outcome data remains required before any threshold tuning or accuracy/profitability claim.

## Next phase

CR3.1 — Live invalidation and false-signal semantics (C1/C2).

Track 12A local cBot separation remains blocked until CR-FINAL.
