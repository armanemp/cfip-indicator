# CR6.2 / F2 — Aggressive pre-trade RR/risk guard, direction consistency and actual-fill plan reconciliation

Date: 2026-10-01

Status: **VERIFIED COMPLETE — PR #128 merged to `main`; merge commit `217911ac5f484d156b8640f27f2a928b9622280b`.**

## Required phase confirmation

تأیید می‌کنم — `Trading/Execution/Aggressive/AggressiveFinalExecutionGuard.cs`,
`Trading/Execution/Aggressive/AggressivePreTradeEligibility.cs`,
`Trading/Execution/Aggressive/AggressiveExecutionPreparation.cs`,
`Trading/Risk/AutoPlanRiskValidator.cs`,
`Trading/Execution/Aggressive/AggressiveAcceptedFillHandler.cs` and the live
fill reconciliation path were checked before the correction.

## Root causes verified

1. The aggressive final RR/risk check was guarded by `_plan != null`, while
   `PassAggressivePreTradeEligibility()` explicitly rejects when `_plan != null`.
   Therefore the final reward-quality check was unreachable on the intended
   aggressive pre-trade path.
2. The final guard did not consume the already-prepared aggressive ATR/SL/TP
   geometry, so it could not validate the actual submission geometry at that
   boundary.
3. Aggressive accepted-fill processing reconstructed actual-fill SL/TP but then
   seeded the managed plan from the original pre-fill `stop`/`target` and ignored
   the boolean result of `ReconcileLivePlanToActualFill(...)`.
4. The lifecycle could enter `LivePosition` before the final fill-envelope and
   reconciliation checks completed.

## Implementation

- `AggressiveFinalExecutionGuard` now receives the prepared `atr`, `stop` and
  `target` and applies the existing canonical `PlanRewardRiskQualityRule`.
- The effective structural stop ceiling remains owned by
  `StructuralStopRiskRule`; no new RR/risk threshold or public parameter was
  introduced.
- Final BUY/SELL direction is checked against the actual `TradeType`.
- Existing `AggressiveRequireSmartAgreement` remains the explicit policy for
  reaction/decision-direction consistency; no new policy switch was added.
- `_plan != null` is no longer used as a precondition for the final aggressive
  reward/risk check.
- Accepted aggressive fills seed the managed plan from actual-fill-derived
  stop/target geometry.
- `ReconcileLivePlanToActualFill(...)` is now fail-closed and its result is
  mandatory before the aggressive position is adopted as live.
- Reconciled broker-safe `Entry/SL/TP` geometry is checked again before
  `LivePosition` is published.
- Post-close cleanup remains under the existing `PositionClosedHandler`; no
  second lifecycle owner was introduced.

## Repository verification

Implementation head: `3d7d819343c00135c4b0b9eb7ffa2dfa2b19a452`.
- Source/Architecture: PASS — run #2183.
- Runtime Acceptance Contracts: PASS — run #1992.
- cTrader Compile: PASS — run #2176.
- `audit_phase_6_2.py`: PASS within Source/Architecture.

## Deterministic verification added

- valid BUY/SELL reward-risk geometry is accepted;
- sub-minimum RR is rejected symmetrically;
- stop risk above the canonical effective ceiling is rejected symmetrically;
- wrong-side BUY/SELL managed-plan geometry is rejected;
- aggressive qualification remains two-sample and resets deterministically;
- source audit proves the unreachable `_plan` dependency is gone;
- source audit proves actual-fill reconciliation is required before live-state
  adoption and that existing close cleanup remains intact.

## Project-wide audit / optimization

The accumulated Source/Architecture gate continues to run the full prior audit
chain plus `audit_phase_6_2.py`. The F2 change adds no duplicate decision or
broker-mutation path, no second execution authority, and no repeated target
calculation beyond the existing actual-fill reconciliation flow.

The change is localized to the aggressive execution boundary and lifecycle
adoption boundary; unrelated modules were not rewritten.

## Safety / parameter boundary

- no public `[Parameter]` name, type or `DefaultValue` changed;
- no RR, confidence, SL, TP or execution threshold was retuned;
- existing canonical minimum-RR and stop-risk semantics were reused;
- no decision or execution authority was duplicated or moved.

## نیاز به تست دستی در cTrader

- verify actual aggressive market fill-price divergence at the target terminal;
- verify broker SL/TP acceptance/rejection after an aggressive fill;
- verify RecoveryRequired state when post-fill reconciliation cannot establish a
  valid broker-safe exit;
- verify restart/reconnect and broker-event ordering around aggressive fills;
- verify chart/panel state is not published as live until the fill reconciliation
  completes.

## خارج از scope / باگ‌های کشف‌شده

Prompt 6 F3–F9 and Prompt 7 G1–G6 remain separate phases. Any unrelated finding
discovered during F2 remains documented for its owning phase and is not fixed
here.