# Phase 8.2 — Swing Plateau Correctness and Structural Evidence Identity

Date: 2026-09-29
Base: main at `3dbbeed5b703f0e5be13f5a1aabc6c87fd13f3b7`
Status: MERGED and verified on main.

## Problem statement

The current swing high/low functions require strict inequality against every neighbor within SwingStrength. Flat/equal extrema therefore fail to resolve as a canonical pivot. Equal High/Low detection separately buckets raw highs/lows, while liquidity sweep logic consumes rolling extremes. These independent representations can treat one plateau or one price level as multiple structural/liquidity facts.

Structure, MSS and CHOCH are also computed as separate booleans. This phase must establish whether these booleans share the same causal break event before treating them as independent evidence.

## Scope

1. Define deterministic, symmetric plateau handling for swing highs and lows without look-ahead beyond the selected closed-bar index. **Implemented.**
2. Give a contiguous/equivalent plateau one canonical level identity and a deterministic representative/confirmation time.
3. Make equal-level clustering avoid chaining: every member must be within tolerance of a fixed cluster anchor/representative, not merely the previous member. **Implemented.**
4. Require a sweep to reference a previously established level and a closed-bar penetration/reclaim sequence, not an undifferentiated rolling extreme alone. **Implemented.**
5. Audit BOS/MSS/CHOCH consumers so a single causal break event cannot be counted as several independent confirmations. **Implemented at scoring/evidence consumers.**
6. Preserve parameter contract, closed-bar ownership, BUY/SELL symmetry, and all execution/risk ownership.
7. Add platform-neutral deterministic contracts and source gates before changing production behavior.

## Non-goals

- No claim of improved win rate or reduced false-signal rate without historical/replay measurement.
- No new public parameters unless an inspected requirement cannot be represented by existing settings and the parameter contract is explicitly reviewed.
- No changes to order sizing, broker execution, SL/TP, risk limits, or lifecycle.

## Existing parameter semantics to preserve

- Swing Strength: default 2, range 1–5.
- Structure Lookback: default 40, range 15–150.
- Structure Break ATR: default 0.05, range 0–0.5.
- Liquidity Lookback: default 40, range 10–150.
- Equal Level Tolerance ATR: default 0.12, range 0.02–0.5.
- Liquidity Sweep Minimum Depth ATR: default 0.05, range 0–1.

## Acceptance criteria

- Plateau high/low resolution is deterministic and symmetric.
- No future or still-open bar contributes to pivot confirmation.
- Flat tops/bottoms yield at most one canonical pivot per plateau identity.
- Equal-level grouping is bounded to anchor tolerance and cannot chain across a broad price range.
- A level cannot be swept before it is causally established.
- A single underlying structural break is not double-counted as independent BOS/MSS/CHOCH evidence.
- Existing runtime, source/architecture and cTrader compile gates pass.
- Live cTrader replay remains required for empirical signal-quality assessment.

## Verification and decision log

Automated verification before merge on head `5df5931828719fb635ec67fa59d57b519d4e70e7`: Runtime Acceptance PASS; cTrader Compile PASS; Source / Architecture PASS. Post-merge main `a9c63bb3e563126753206c49b070763108919b74` also passed Runtime, Build and Source/Architecture.

Implementation closeout: production Swing High/Low consumers use the canonical plateau rule; Equal High/Low scans canonical swings with fixed-anchor tolerance; liquidity sweeps require an established prior structural swing; Structure/MSS/CHOCH are collapsed to one causal structural event for scoring and confirmation evidence. Public parameters, risk, execution and lifecycle semantics are unchanged.

Phase 8.1 post-merge on main `3dbbeed5b703f0e5be13f5a1aabc6c87fd13f3b7`: Source and Build PASS; Runtime PASS. Phase 8.2 starts from this verified main.


## Merge record

PR #36 was merged into `main` as `a9c63bb3e563126753206c49b070763108919b74`. Post-merge gates: Runtime PASS, Build PASS, Source/Architecture PASS. The automated Phase 8.2 boundary is therefore closed.

Next phase: Phase 8.3 — FVG mathematical audit.
