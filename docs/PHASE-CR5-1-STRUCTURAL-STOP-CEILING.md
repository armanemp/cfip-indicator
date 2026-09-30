# CR5.1 / E1 — Effective maximum structural-stop risk ceiling

Status: **VERIFIED COMPLETE — repository acceptance closed 2026-10-01.**

## Scope

Centralize the existing effective structural-stop ceiling so every dual-cap
consumer uses one Core owner, without changing public parameter identity or
default numerical behavior.

## Parameter semantics

- `MaximumSlAtr`: the broad configured maximum stop-risk ATR ceiling.
- `MaximumStructuralStopAtr`: the additional structural-stop ceiling.
- `EffectiveMaximumStopRiskAtr`: the tighter normalized cap after preserving
  the existing minimum-SL floor behavior.

The canonical formula is intentionally equivalent to the previous inline
formula. No parameter value or default is tuned by this phase.

## Implementation

- added Core `StructuralStopRiskRule.EffectiveMaximumStopRiskAtr(...)`;
- migrated the structural-stop candidate evaluator;
- migrated plan preparation and plan-integrity validation;
- migrated parallel opportunity, actionability and automatic submission paths;
- migrated aggressive and pending submission validators;
- migrated signal-trace reward-risk calculation;
- kept early candidate rejection on `riskAtr > maxRiskAtr`;
- added deterministic BUY/SELL-independent truth-table coverage for cap ordering,
  minimum-floor handling and symmetry;
- added `audit_phase_5_1.py` to inventory all dual-cap consumers and reject
  duplicate inline ceiling formulas;
- wired the E1 gate into Source/Architecture after CR4.10.

## Safety boundary

- no public `[Parameter]` name, type or `DefaultValue` changed;
- no RR, confidence, SL, target or execution threshold was tuned;
- no decision or execution authority changed;
- no new broker mutation path was introduced;
- candidate rejection behavior remains fail-closed when structural-stop risk
  exceeds the effective ceiling.

## Verification boundary

Repository verification completed:
- Source/Architecture — PR #114 head run `36791329486`; merge run `36791455294`;
- Runtime Acceptance Contracts — PR #114 head run `36791329385`; merge run `36791455156`;
- cTrader Compile — PR #114 head run `36791329369`; merge run `36791455288`.

The merge commit is `62a119bef79e5a978f1f2ed66a913fa11a1bb2d4`.

Target-terminal broker timing, restart/reconnect, panel behavior and empirical
signal-quality/outcome validation remain manual and are not inferred here.

## Transition

After repository verification, the next phase is **CR5.2 / E2 —
Liquidity/session target-source semantics and multi-level target candidates**.
