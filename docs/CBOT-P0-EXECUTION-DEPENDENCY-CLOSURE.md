## CBOT-P4C closeout note — 2026-10-02

Pending Stop extraction is now implemented in src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs. The old Indicator mutation owner Trading/Execution/BrokerPendingOrderPlacement.cs has been deleted. Indicator retains only analytical preparation and immutable execution intent publication.

The internal execution clock is M15. Chart timeframe is not an execution input. M5/M1 remain defensive tuning and H1+ remains higher-timeframe context/reward support.

# CBOT-P0 — Execution Dependency-Closure Inventory

Date: 2026-10-02

Status: **HISTORICAL EXTRACTION INVENTORY — superseded by completed execution-owner cutover.**

This document preserves the original extraction map as audit evidence. It is not a current ownership registry and must not be used as an implementation source of truth.

## 1. Historical broker mutation owners

The following Indicator owners were the pre-cutover mutation inventory. They are retained here only to prove migration scope and deletion; they are **not current production owners**.

| Historical owner | Broker APIs | Current owner after cutover |
| --- | --- | --- |
| `Trading/Execution/BrokerMarketOrderMutation.cs` | ExecuteMarketOrder, ExecuteMarketRangeOrder | `src/CFIP.cBot/Execution/DemoMarketExecutionCoordinator.cs` |
| `Trading/Execution/BrokerPendingOrderPlacement.cs` | PlaceStopOrder | `src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs` |
| `Trading/Execution/BrokerLimitOrderPlacement.cs` | PlaceLimitOrder | `src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs` |
| legacy management mutation owners | CancelPendingOrder, ClosePosition, ModifyStopLossPrice, ModifyTakeProfitPrice, ModifyTakeProfit, ModifyTakeProfitPips | `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs` |

The authoritative current boundary is maintained by `docs/ARCHITECTURE.md`, `docs/CBOT-0-BOUNDARY-INVENTORY.md` and the executable audits.

## 2. Current production callers

### Market
- `AutomaticMarket/AutomaticMarketRangeCalculator.cs` — market fallback.
- `AutomaticMarket/AutomaticMarketBrokerExecution.cs` — market-range and market submission.
- `Aggressive/AggressiveBrokerExecution.cs` — aggressive market submission.

### Pending
- `Pending/Placement/ContinuationStopPlacement.cs` — Stop submission.
- `Pending/Placement/ReversalLimitPlacement.cs` — Limit submission.

### Cancellation
- `Lifecycle/PendingOrderCircuitBreaker.cs`
- `Pending/Placement/PendingOrderCleanup.cs`
- `Alerts/EndOfDayAlert.cs`
- `Intelligence/EconomicNewsProtection.cs`

### Full/partial close
- `Lifecycle/PositionCircuitBreaker.cs`
- `Lifecycle/LivePlanExitCoordinator.cs`
- `Alerts/EndOfDayAlert.cs`
- `LiveManagement/ActivePlanIntegrityHandler.cs`
- `LiveManagement/ReversalCloseGuard.cs`
- `Intelligence/EconomicNewsProtection.cs`
- `Execution/AutomaticMarket/AutomaticMarketFillReconciliation.cs`
- `LiveManagement/PartialTakeProfitExecutor.cs`
- `Intelligence/Prediction/LiveReversalAnalyzer.cs`
- `Execution/Aggressive/AggressiveAcceptedFillHandler.cs`
- `LiveManagement/StructuralSetupInvalidationExit.cs`

### SL
- `Execution/BrokerProtectionCoordinator.cs`
- `LiveManagement/PartialTakeProfitExecutor.cs`
- `Execution/Aggressive/BoundPlanProtection.cs`

### TP
- `Execution/BrokerProtectionCoordinator.cs`
- `Execution/Aggressive/BoundPlanProtection.cs`

### TP ladder
- `Lifecycle/PendingFillProtectionCoordinator.cs`
- `Execution/ServerSideTakeProfitLadderProgression.cs`

### TP-by-pips
- `Execution/ServerSideTakeProfitLadderProgression.cs`

## 3. Mixed boundaries that must be split, not copied

- `Trading/Execution/BrokerProtectionCoordinator.cs`: analytical validation/geometry remains Indicator-side; broker mutation/reconciliation moves to cBot.
- `Trading/Execution/Aggressive/BoundPlanProtection.cs`: decision-side protection intent remains Indicator-side; actual broker mutation moves to cBot.
- `Trading/LiveManagement/ProtectionManager.cs`: analytical protection state remains Indicator-side; broker mutation moves to cBot.
- `Trading/LiveManagement/PartialTakeProfitExecutor.cs`: analytical partial-TP decision remains Indicator-side; close mutation moves to cBot.
- `Trading/Lifecycle/*`: analytical lifecycle interpretation remains Indicator-side where needed; broker-confirmed state/event/reconciliation authority moves to cBot.

## 4. Identity/state dependency closure

The following must migrate with the broker authority and must never be reimplemented independently in the cBot:

- `Trading/Identity/BrokerIdentity.cs`
- `Trading/Identity/ManagedPositionGuards.cs`
- broker-facing `TradeExecutionMetadata.cs`
- `Trading/Lifecycle/BrokerStateSnapshot.cs`
- `Trading/Lifecycle/ManagedPositionLookup.cs`
- `Trading/Lifecycle/BrokerProtectionStateEvaluator.cs`
- `Core/Execution/SubmissionAttemptIdentity.cs`
- `Core/Execution/SubmissionGate.cs`
- `Core/Execution/SubmissionGateState.cs`
- `Trading/Execution/SubmissionGateCoordinator.cs`

The same managed identity and ScenarioId-scoped idempotency semantics must survive extraction unchanged.

## 5. Extraction order

1. Contracts
2. Read-only Indicator provider
3. cBot shadow host
4. Market / Market Range
5. Aggressive
6. Pending Stop
7. Pending Limit
8. Cancel
9. Close / Partial Close
10. SL
11. TP / server ladder
12. Lifecycle/recovery
13. Account/execution risk
14. UI state cutover
15. Physical removal of Indicator execution authority

No step may remove an Indicator caller before its cBot replacement is available and verified.

## 6. P4C migration record

Pending Stop broker mutation now lives only in `src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs`; Indicator keeps only analytical preparation and immutable intent publication. The old Indicator mutation owner was deleted. The cBot host itself remains Chart-TF independent.

## 7. Explicit non-transfer set

Do not move wholesale:

- Analysis
- Planning
- FVG / OB / OB+FVG
- WaveTrend
- divergence
- MTF/regime
- scenario generation
- analytical Entry/SL/TP selection
- analytical RR/reward quality
- chart/panel renderers
- alert delivery/presentation
- outcome learning/calibration

## 7. Definition of extraction completeness

The migration is complete only when:

- Indicator has zero direct broker mutation APIs;
- cBot is the single broker mutation authority;
- Indicator exposes only read-only analytical/management intent;
- all execution/account/lifecycle parameters have exactly one cBot owner;
- cBot and Indicator use the same canonical contract identity;
- restart/reconnect begins from broker-confirmed state;
- no hidden executor/fallback remains;
- deterministic parity fixtures pass;
- target-terminal proof passes.

## 8. Anti-regression rule

Every subsequent M2–M28 phase must run the cBot-boundary audit and may not introduce new execution authority into Indicator.