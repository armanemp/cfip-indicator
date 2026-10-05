# CBOT-0 — Boundary Inventory & Execution-Authority Freeze

Date: 2026-09-30

Status: **VERIFIED COMPLETE**

Baseline commit audited: `473d5f1c89afaf8880bb16092e5d9b16b98d0395` (current `main` before this phase).

This phase is intentionally a **no-production-behavior-change** phase. It freezes the current Indicator execution surface before any Contracts/cBot project is created.

## 1. Frozen architecture decision

The separation boundary is:

```
CFIP Indicator
Market → MTF → Evidence → Decision → Trigger → Scenario → Trade Plan
                         ↓
                  read-only intent/plan
                         ↓
CFIP cBot
Broker/account eligibility → submission → broker confirmation
→ protection → lifecycle → recovery → execution telemetry
```

The Indicator remains the analytical brain. The cBot becomes the sole future broker execution authority.

The current single-position/single-managed-plan capacity is preserved. Independent timeframe scenarios remain observe-only until a separately certified scenario execution policy exists.

No HTTP, sockets, database, Cloud service, second decision engine, multi-position capacity, or manual BUY/SELL executor is introduced.

## 2. Direct broker mutation inventory

The P4E migration removes the remaining Indicator broker-mutation owners. The Indicator now has zero direct mutation calls for cancellation, close/partial-close, stop, absolute TP, TP-by-pips and server TP ladder paths.

| Broker API | Current Indicator owner | Call-site count | Future cBot responsibility |
| --- | --- | ---: | --- |
| `ExecuteMarketOrder` | `Trading/Execution/BrokerMarketOrderMutation.cs` | 2 | Market + aggressive market submission |
| `ExecuteMarketRangeOrder` | `Trading/Execution/BrokerMarketOrderMutation.cs` | 2 | Market-range submission |
| `PlaceStopOrder` | `src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs` | 2 | Pending Stop submission |
| `PlaceLimitOrder` | `Trading/Execution/BrokerLimitOrderPlacement.cs` | 2 | Pending Limit submission |
| `CancelPendingOrder` | `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs` | 1 | Pending cancellation |
| `ClosePosition` | `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs` | 1 | Full/partial position close mutation |
| `ModifyStopLossPrice` | `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs` | 1 | Protective SL mutation |
| `ModifyTakeProfitPrice` | `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs` | 1 | TP mutation |
| `ModifyTakeProfit` | `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs` | 1 | Server TP ladder mutation |
| `ModifyTakeProfitPips` | `src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs` | 1 | TP-by-pips mutation |
| `ModifyPosition` / `ModifyPendingOrder` | no current direct call-site found in frozen owner scan | 0 | reserved for target cBot only if target API requires it |

Machine audit: `tools/audit_cbot_boundary.py`.

The audit fails if a new direct broker mutation call appears outside the frozen broker-owner set. This prevents a hidden second executor from growing while migration work is underway.

## 3. Frozen broker-state and lifecycle boundary

The following responsibilities are future cBot-owned because they depend on live broker/account state:

### Broker identity/state

- `Trading/Identity/BrokerIdentity.cs`
- `Trading/Identity/ManagedPositionGuards.cs`
- broker-facing parts of `Trading/Identity/TradeExecutionMetadata.cs`
- `Trading/Lifecycle/BrokerStateSnapshot.cs`
- `Trading/Lifecycle/ManagedPositionLookup.cs`
- `Trading/Lifecycle/BrokerProtectionStateEvaluator.cs`

### Submission authority

- `Core/Execution/SubmissionAttemptIdentity.cs`
- `Core/Execution/SubmissionGate.cs`
- `Core/Execution/SubmissionGateState.cs`
- `Trading/Execution/SubmissionGateCoordinator.cs`

The ScenarioId-scoped identity introduced by Phase 11.5 is preserved. Retry/backoff/circuit state must never become a global, cross-scenario executor.

### Broker lifecycle/event handlers

Future cBot ownership:

- `PositionOpenedHandler.cs`
- `PositionModifiedHandler.cs`
- `PositionClosedHandler.cs`
- `PendingCreatedHandler.cs`
- `PendingModifiedHandler.cs`
- `PendingCancelledHandler.cs`
- `PendingFilledHandler.cs`
- recovery/reconciliation owners under `Trading/Lifecycle/`

### Broker protection/mutation

- `BrokerProtectionCoordinator.cs` is a **split boundary**: deterministic geometry/validation stays with the Indicator-side analytical owner; broker mutation/reconciliation moves to the cBot.
- `BrokerStopLossMutation.cs`
- `BrokerTakeProfitMutation.cs`
- `BrokerPositionCloseMutation.cs`
- `Trading/LiveManagement/ProtectionManager.cs` is split: stop/target reasoning remains analytical; broker mutation moves to the cBot.
- `PartialTakeProfitExecutor.cs` is split: decision remains Indicator; close mutation moves to the cBot.
- target progression selection remains Indicator; broker TP mutation moves to cBot.

## 4. Execution-path split matrix

| Current area | Indicator remains responsible for | cBot receives |
| --- | --- | --- |
| Automatic Market | plan eligibility, Entry/SL/TP intent, scenario identity | broker submission, confirmation, adoption, execution telemetry |
| Aggressive | aggressive analytical eligibility and intent | market mutation + broker/account safety + confirmation |
| Pending Stop | continuation setup, trigger/stop/target plan | Stop-order mutation + broker confirmation/cancellation |
| Pending Limit | reversal setup, limit/stop/target plan | Limit-order mutation + broker confirmation/cancellation |
| Protection | structural candidate/target reasoning | actual SL/TP mutation and broker-confirmed protection state |
| Live management | analytical exit/reprice/progression decision | actual close/partial-close/SL/TP mutation |
| Lifecycle | analytical state interpretation and display | broker-confirmed events, reconciliation, recovery |
| SubmissionGate | no new analytical authority | execution idempotency/retry/backoff/circuit |
| Identity | display/diagnostic identity fields | managed broker label/position/pending ownership |

Important: **do not copy mixed modules wholesale**. The method and field dependency closure must travel with each moved broker owner in the later extraction phases.

## 5. What is explicitly NOT moving

Entire areas that remain Indicator-owned:

- `Analysis/**`
- `Planning/**`
- `UI/**`
- alert/popup/chart presentation;
- WaveTrend, FVG, OB/OB+FVG, structure, liquidity, divergence and MTF intelligence;
- regime/no-trade intelligence;
- analytical Entry/SL/TP proposal;
- analytical RR/reward-path quality;
- scenario generation/materialization;
- learning/outcome analysis and calibration;
- `ScenarioExecutionPolicy.cs`;
- `Trading/Execution/ExecutionPlanPreparation.cs`.

`Core/Models/Plan.cs` and `Core/Models/ExecutionIntent.cs` are not shared as mutable models. Their externally consumed fields will later be projected into platform-neutral immutable Contracts.

## 6. Parameter ownership/migration matrix rule

CBOT-0 does not blindly move whole parameter files.

`tools/audit_cbot_boundary.py` parses the complete public parameter surface (current audited baseline: 562) and, for every execution-related parameter:

1. finds all production references;
2. classifies the reference domains as Analysis, Planning, Trading/Execution, Lifecycle, Risk, Identity, UI and Runtime;
3. classifies ownership as:
   - **INDICATOR** — analytical/presentation authority;
   - **CBOT** — broker/account/live-execution authority;
   - **SPLIT** — current parameter is consumed by both analytical and execution domains and must be decomposed before final cutover.

The key groups are explicitly covered: `09_risk_targets`, `10_live_management`, `11_filters`, `13_auto_trading`, `15_control_advanced`, `23_structural_execution`, `24_smart_execution`, and `28_news_guard`.

Current ownership principle:

- signal evidence, confidence, MTF interpretation, Entry/SL/TP proposal and analytical scenario selection stay in the Indicator;
- account permission, position sizing from live account state, capacity, daily-loss enforcement, broker spread/market execution safety, broker label/identity and live protection controls move to the cBot;
- presentation-only controls stay in the Indicator;
- a parameter that currently mixes analytical and live-execution effects is **not duplicated**: it becomes a split refactor item with one final owner per resulting behavior.

No public parameter is changed or removed in CBOT-0.

## 7. Dependency closure freeze

Before CBOT-1, the following are mandatory:

- every moved method has its required fields/helpers/dependencies identified;
- no moved method may continue to call a hidden private Indicator executor after migration;
- no Contracts type may depend on cTrader types;
- no cBot type may reach into Indicator private fields;
- no static mutable cross-project singleton is permitted;
- no chart-object scraping, reflection or chart-label parsing is a transport;
- no fourth project is introduced unless a compiler/dependency hard requirement is proven.

The authoritative target remains:

```
CFIP.Indicator → CFIP.Contracts
CFIP.cBot      → CFIP.Contracts
CFIP.cBot      → CFIP.Indicator (only through the supported cTrader custom-indicator mechanism)
```

The exact supported host capability is a **separate blocking CBOT-Preflight test** on the target cTrader terminal. It is deliberately not guessed in CBOT-0.

## 8. Verification

CBOT-0 source gate:

`python tools/audit_cbot_boundary.py`

The source-check workflow must execute this gate on every push/PR during the separation track.

CBOT-0 is accepted only when:

- direct broker mutations are completely inventoried;
- no mutation exists outside the frozen owners;
- lifecycle/account ownership markers are present;
- all execution-related parameters receive a deterministic ownership classification;
- public parameter count remains 562;
- no duplicate public parameter declaration exists;
- no production behavior was changed.

## 9. Operator boundary

This phase creates the inventory and guard only. It does **not** create `CFIP.Contracts` or `CFIP.cBot`, and it does not disable the current executor.

Verification: Source/Architecture `109870832998` PASS; Runtime `109870832972` PASS; Build `109870832958` PASS.

Next gate: **CBOT-Preflight**.

Local checkout action after this phase is merged: `git pull --ff-only`.


## CR1.8 remediation delta

CBOT-0 remains a historical boundary-freeze record; its original 562-parameter count is preserved as historical evidence. CR1.8 added exactly one required safety parameter, `ReversalCloseMinimumNetProfit`, so the current machine-audited public parameter baseline is now **563**. This safety correction does not create a new execution authority, reopen the CBOT-0 inventory design, or authorize cBot implementation. `tools/audit_cbot_boundary.py` now expects the post-CR1.8 count of 563 before the separation track can continue.


## CBOT-P4E closeout — 2026-10-02

Canonical remaining mutation owner:
`src/CFIP.cBot/Execution/ManagementExecutionCoordinator.cs`.

Transport:
Indicator `ManagementCommand` -> Device queue -> cBot broker mutation -> `BrokerExecutionReport` -> Device queue -> Indicator reconciliation.

The requested plan is not adopted as broker truth; confirmation is state-based.
