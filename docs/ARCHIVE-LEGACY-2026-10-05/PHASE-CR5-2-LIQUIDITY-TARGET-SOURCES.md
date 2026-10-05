# CR5.2 / E2 — Liquidity/session target-source semantics and multi-level target candidates

Status: **VERIFIED COMPLETE — repository acceptance closed 2026-10-01.**

## Scope

Correct liquidity target-source semantics so raw candle extremes are not treated
as liquidity forecasts. Preserve the existing session-window semantics while
allowing multiple valid, farther structural liquidity levels to reach the normal
target-selection pipeline.

## Implementation

- `LiquidityAboveTargetSource` now enumerates canonical swing highs above entry;
- `LiquidityBelowTargetSource` now enumerates canonical swing lows below entry;
- candidates must remain active/unbroken under `LiquiditySweepRule`;
- directional validity and minimum separation are owned by the platform-neutral
  `LiquidityTargetCandidateRule`;
- candidates are ordered by distance from entry, preserving nearest-first
  planning behavior while exposing farther valid levels;
- `SmartExtraTargetSource` now emits all valid `LIQUIDITY_FORECAST` levels;
- legacy nearest-liquidity helper methods delegate to the canonical multi-level
  source and contain no separate raw-extreme algorithm;
- the existing `MinimumTpSpacingAtr` is reused for ATR-bounded source-level
  discrimination; no new public parameter was introduced;
- session forecasts continue to resolve through `SessionWindowRule` using the
  existing `SessionStartUtc` / `SessionEndUtc` values. No Asia/London/New York
  session reinterpretation was introduced.

## Deterministic evidence

Covers:
- BUY/SELL directional symmetry;
- near/far candidate separation;
- deterministic distance ordering;
- high/low active-unbroken behavior;
- broken high/low rejection;
- valid target survival through the existing reward/extension contract;
- distant target rejection by the existing maximum-extension contract.

## Safety boundary

- no public `[Parameter]` name, type or `DefaultValue` changed;
- no RR, confidence, stop, target-extension or execution threshold was tuned;
- no new decision or execution authority was introduced;
- existing target-selection and reward-risk validation remain authoritative.

## Verification boundary

Repository verification completed on PR #116 head `9c7c2acdfe8575915ad1dc4129281bd429144a94`:
- Source/Architecture — PASS — run `36792555340` / workflow #2072, including `audit_phase_5_2.py` and the accumulated routine/optimization audits;
- Runtime Acceptance Contracts — PASS — run `36792555225` / workflow #1881;
- cTrader Compile — PASS — run `36792555189` / workflow #2065.

PR #116 was merged to `main` as merge commit `10e01bd2610ce0c42b6d365f55fae24c75a3edfb`.

Target-terminal broker timing, restart/reconnect, panel behavior and empirical
signal-quality/outcome validation remain manual and are not inferred here.

## Transition

After repository verification, the next phase is **CR5.3 / E3 —
Independent-evidence group counting for parallel opportunities**.
