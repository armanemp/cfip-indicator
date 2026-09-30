# CR5.1 — Effective Maximum Structural Stop Risk and Duplicate Ceiling Removal (E1)

## Scope

CR5.1 reconciles the duplicate effective structural-stop risk ceiling logic across planning, parallel-opportunity, actionability, automatic market, aggressive, pending and signal-trace consumers.

The canonical effective maximum is:
min(max(minimum SL ATR, maximum SL ATR), max(minimum SL ATR, maximum structural-stop ATR))

This preserves the current numerical behavior for valid cTrader parameter values while giving the ceiling one mathematical owner.

## Parameter semantics

Maximum SL ATR remains the general upper ceiling for stop risk.

Maximum Structural Stop ATR remains the structural-stop-specific upper ceiling.

The effective ceiling is the lower of those two ceilings after the existing minimum-SL floor is respected. Neither public parameter is renamed, removed, retyped, or retuned.

Preferred Stop Risk ATR remains a scoring preference, not a permission to exceed the effective maximum.

## Implementation

Added StructuralStopRiskRule in Core with:
- EffectiveMaximumStopRiskAtr;
- IsWithinEffectiveMaximumStopRiskAtr.

All nine scoped consumers now use the canonical owner:
- StructuralStopCandidateEvaluator;
- PlanInputPreparation;
- PlanIntegrityValidator;
- ParallelOpportunityBuilder;
- TradeActionabilityEvaluator;
- AutomaticMarketSubmissionValidator;
- AggressiveFinalExecutionGuard;
- PendingSubmissionValidator;
- SignalEvaluationTraceRecorder.

StructuralStopCandidateEvaluator rejects an over-ceiling candidate before TP1 estimation and scoring work.

PlanRewardRiskQualityRule now treats the supplied maximum stop-risk ceiling as a hard ceiling. Preferred Stop Risk ATR can no longer promote that maximum when the preferred value is configured above it.

## Deterministic verification

Runtime contracts cover:
- Min/Maximum truth-table combinations;
- exact-ceiling acceptance;
- over-ceiling rejection;
- non-finite fail-closed behavior;
- BUY/SELL symmetry;
- preferred-risk-above-ceiling interaction.

The E1 static gate also verifies:
- all scoped callers use the canonical owner;
- the duplicate nested formula is absent from callers;
- candidate early rejection precedes TP1 estimation;
- public parameter names/types/defaults remain unchanged.

## Safety and no-tuning boundary

No public parameter name/type/default was changed.

No default RR, confidence, SL/TP, signal or execution threshold was tuned.

No execution authority was added or moved.

No target-terminal behavior is inferred from static/runtime contracts.

## Routine audit

This phase also preserves the project-wide audit expectations:
- architecture remains single-owner for the ceiling rule;
- readiness/validation stays O(1) at the new rule boundary;
- no duplicate formula remains in the hot validation paths;
- target-terminal, replay and empirical signal-quality acceptance remain separate manual boundaries.

## Transition

CR5.1 / E1 is complete after repository verification passes.

Next phase: CR5.2 / E2 — Liquidity/session target-source semantics and multi-level target candidates.
