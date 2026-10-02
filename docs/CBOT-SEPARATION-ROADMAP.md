# CFIP — Local cBot Separation Roadmap

## 0. Purpose

This document is the mandatory pre-development roadmap for separating broker execution from the CFIP Indicator into a dedicated local cBot.

This is a **local cTrader architecture change**. It does not introduce Cloud execution, a web API, IPC service, a remote server, or a second analysis engine.

The immediate product remains:

`CFIP Indicator` → analysis / decision / scenario / trade-plan intelligence

`CFIP cBot` → broker execution / account risk enforcement / live trade lifecycle

The indicator and cBot both run locally inside cTrader.

The only future-proofing required now is that the boundary between them uses small platform-neutral contracts. No Cloud implementation is part of this roadmap.

---

## 1. Repository baseline audited for this roadmap

Repository: `armanemp/cfip-indicator`

Baseline reviewed from `main` at `473d5f1c89afaf8880bb16092e5d9b16b98d0395`:

- Current host project: `src/CFIP.Indicator/CFIP.Indicator.csproj`.
- Target framework: `.NET 6`.
- cTrader package: `cTrader.Automate 1.0.21`.
- Current solution contains the Indicator project only.
- The cTrader host is the thin partial `CFIPIndicator` type under `Indicator/`.
- Analysis, Planning, Runtime, Trading and UI are already separated by source responsibility.
- Direct broker mutations are concentrated in explicit broker-mutation owners, but those owners still execute inside the Indicator host.
- The current execution architecture is single-position / single managed plan / single automatic execution authority.
- Phase 11.5 is already verified and merged; independent timeframe scenarios remain observe-only and scenario identity is already carried into submission identity and telemetry.
- Current architecture rules explicitly state that Planning produces executable intent as data, while broker-confirmed state is authoritative.

Important source examples confirmed during this audit:

- `Trading/Execution/BrokerMarketOrderMutation.cs` directly calls market-order broker APIs.
- `Trading/Execution/BrokerPendingOrderPlacement.cs` and pending placement owners handle broker pending mutation.
- `Trading/Execution/BrokerPendingOrderCancellation.cs` handles pending cancellation.
- `Trading/Execution/BrokerPositionCloseMutation.cs` handles broker position close mutation.
- `Trading/Execution/BrokerStopLossMutation.cs` and `BrokerTakeProfitMutation.cs` handle protection mutation.
- `Trading/Execution/BrokerProtectionCoordinator.cs` coordinates broker protection.
- `Trading/Identity/BrokerIdentity.cs` reads trading permission and managed Positions/PendingOrders state.
- `Core/Execution/SubmissionAttemptIdentity.cs` and `SubmissionGate.cs` implement submission identity and retry/circuit semantics.
- `Trading/Execution/ExecutionPlanPreparation.cs` explicitly states that executable plan preparation is **not** a broker-mutation owner.
- `Planning/TradePlan/PlanMaterialization.cs` creates the analytical/executable Plan data.
- `Core/Models/Plan.cs` and `ExecutionIntent.cs` are currently internal models, so they are not yet an external Indicator→cBot contract.

This distinction is the central finding of the roadmap: **we should move the broker authority, not the analytical brain.**

---

## 2. Non-goals

The separation must not accidentally become a second rewrite of CFIP.

Do not move:

- OB / FVG / OB+FVG engines;
- structure, liquidity, BOS/MSS/CHoCH analysis;
- WaveTrend / divergence / indicator-fusion analysis;
- MTF closed-bar analysis;
- regime and no-trade intelligence;
- decision consensus, confidence or quality calculation;
- trigger intelligence;
- structural stop selection;
- target candidate discovery and reward-path analysis;
- scenario construction;
- analytical RR / reward-path quality;
- panel/chart rendering;
- signal alert presentation;
- persistent outcome-learning stores;
- the Cloud roadmap itself.

Do not add:

- HTTP;
- sockets;
- local web servers;
- message brokers;
- databases;
- microservices;
- cloud credentials;
- a second decision engine;
- multi-position capacity;
- a second managed trade identity.

---

## 3. Target ownership

### Indicator owns

The Indicator remains authoritative for:

`Market data → MTF context → Evidence → Decision → Trigger → Scenario → Trade Plan`

Its output is a read-only executable opportunity/plan contract.

It may calculate:

- direction;
- source timeframe / lane / ScenarioId;
- Entry / IdealEntry / zone / Trigger / Invalidation;
- structural SL;
- TP1..TP4;
- analytical risk and RR;
- execution mode;
- analytical quality/actionability;
- scenario lifecycle and expiration;
- reason/evidence metadata needed for diagnostics.

It does **not** mutate broker state.

### cBot owns

The cBot becomes authoritative for:

`Signal contract → broker eligibility → order submission → broker confirmation → live protection → lifecycle → recovery`

It owns:

- trading permission;
- broker identity and managed label;
- account balance/equity/margin state;
- position sizing against the actual account;
- single-plan capacity;
- spread/session/daily-loss/event execution guards where they depend on live broker/account state;
- Market submission;
- Aggressive submission;
- Pending Stop submission;
- Pending Limit submission;
- pending cancellation;
- position close / partial close;
- broker SL/TP mutation;
- break-even and profit-lock broker mutation;
- trailing/protection broker mutation;
- broker confirmation;
- fill reconciliation;
- restart/reconnect reconciliation;
- duplicate-order prevention;
- submission retry/backoff/circuit state;
- execution telemetry and broker outcome events.

The cBot may reject a signal because of current broker/account conditions, but it does not reinterpret the Indicator's analytical direction or invent a competing plan.

---

## 4. What actually moves out of the Indicator

The rule is **split, do not blindly copy folders**.

### 4.1 Definitely cBot-owned broker-mutation owners

These broker-mutation owners must leave the Indicator execution host:

`Trading/Execution/BrokerMarketOrderMutation.cs`

`Trading/Execution/BrokerPendingOrderPlacement.cs`

`Trading/Execution/BrokerLimitOrderPlacement.cs`

`Trading/Execution/BrokerPendingOrderCancellation.cs`

`Trading/Execution/BrokerPositionCloseMutation.cs`

`Trading/Execution/BrokerStopLossMutation.cs`

`Trading/Execution/BrokerTakeProfitMutation.cs`

For `Trading/Execution/BrokerProtectionCoordinator.cs`, only the broker-facing mutation/reconciliation methods move to the cBot. Any deterministic geometry/validation calculation that is consumed by Indicator analysis must first be extracted into the correct analytical owner or a platform-neutral contract helper. The whole mixed class must never be copied to both projects.

The relevant broker-mutation portions of:

`Trading/Execution/AutomaticMarket/*`

`Trading/Execution/Aggressive/*`

`Trading/Pending/Placement/*`

must also move to the cBot where they perform submission/fill/protection mutation.

Their analytical preparation/eligibility pieces do not automatically move.

### 4.2 Definitely cBot-owned identity/state

The broker-facing parts of:

`Trading/Identity/BrokerIdentity.cs`

`Trading/Identity/ManagedPositionGuards.cs`

`Trading/Identity/TradeExecutionMetadata.cs`

move into the cBot execution boundary.

The single managed identity must be generated and enforced by the cBot. The Indicator may display the identity but must not own broker identity state.

### 4.3 Definitely cBot-owned submission protection

The execution-runtime portions of:

`Core/Execution/SubmissionAttemptIdentity.cs`

`Core/Execution/SubmissionGate.cs`

`Core/Execution/SubmissionGateState.cs`

move to the cBot execution layer.

The existing Phase 11.5 ScenarioId behavior must be preserved so retry/backoff/circuit state remains scenario-scoped.

### 4.4 cBot-owned live broker lifecycle

The broker-confirmed/event-driven portions of:

`Trading/Lifecycle/*`

move to the cBot, including:

- managed position lookup;
- broker state snapshots;
- position-opened/modified/closed handling;
- pending-created/modified/cancelled/filled handling;
- broker-confirmed lifecycle transitions;
- fill reconciliation;
- recovery;
- broker protection-state evaluation;
- restart/reconnect adoption.

However, any code in these files that is purely analytical or constructs an Indicator-side plan must be extracted or left in the Indicator.

### 4.5 cBot-owned live mutation portion

`Trading/LiveManagement/*` is a mixed boundary and must be split.

The cBot receives management decisions/commands and owns the actual broker mutation.

The Indicator remains the owner of analytical pressure, structural invalidation, target candidate reasoning and decision-side exit intent.

Examples:

- `ProtectionManager.cs`: split decision from broker mutation.
- `PartialTakeProfitExecutor.cs`: execution and broker close move to cBot.
- `ActivePlanLevelExitHandler.cs`: analytical exit condition stays Indicator; actual close moves to cBot.
- `ActivePlanReactionExitHandler.cs`: analytical reaction decision stays Indicator; actual close moves to cBot.
- `TargetProgression.cs`: analytical next-target selection stays Indicator; broker TP modification moves to cBot.
- `ReversalProtection.cs`: decision/guard logic stays Indicator; broker mutation moves to cBot.
- `SmartExitModeResolver.cs` and pressure calculations remain analytical unless a specific method directly mutates broker state.

### 4.6 cBot-owned account-dependent risk

`Trading/Risk/*` must be split by dependency.

Move/extract into cBot:

- `ExecutionCapacityGuard.cs`;
- `ManagedPositionCounter.cs`;
- account/margin/volume calculations whose inputs come from the live account or broker;
- `DailyLossGuard.cs` when enforcing account-level trading permission;
- broker/session/spread execution guards when their decision depends on the live account/broker state.

Keep in Indicator:

- analytical market suitability;
- signal quality;
- regime suitability;
- plan-level RR quality;
- evidence-driven risk interpretation that does not mutate or inspect broker account state.

No numerical threshold is tuned as part of this structural separation unless separately justified by runtime evidence.

---

## 5. What stays in the Indicator

The following entire areas remain Indicator-owned unless a later audit proves that a specific method directly performs a broker mutation:

`Analysis/**`

`Planning/**`

`UI/**`

Most of:

`Runtime/Calculation/**`

`Runtime/Mtf/**`

`Runtime/Initialization/**`

`Trading/Alerts/**`

Most of:

`Trading/Intelligence/**`

In particular, `ScenarioExecutionPolicy.cs` remains in the Indicator because Phase 11.5 made it the scenario eligibility/materialization authority. It must not become a second execution engine in the cBot.

`Trading/Execution/ExecutionPlanPreparation.cs` remains Indicator-owned because it explicitly prepares executable geometry without broker mutation.

`Core/Models/Plan.cs` and `Core/Models/ExecutionIntent.cs` do not move as-is. Their externally consumed fields should be represented by a small public contract project.

---

## 6. Minimal target repository structure

We deliberately avoid the much larger architecture proposed earlier.

Target:

```
cfip-indicator/
├── src/
│   ├── CFIP.Indicator/
│   │   └── existing analysis/planning/runtime/UI
│   │
│   ├── CFIP.Contracts/
│   │   ├── Signal/
│   │   ├── Execution/
│   │   └── Events/
│   │
│   └── CFIP.cBot/
│       ├── Broker/
│       ├── Execution/
│       ├── Identity/
│       ├── Risk/
│       ├── Lifecycle/
│       ├── Protection/
│       ├── Recovery/
│       ├── Telemetry/
│       └── CFIPBot.cs
│
├── tests/
│   ├── existing tests/contracts
│   └── cBot/...
│
└── docs/
    └── CBOT-SEPARATION-ROADMAP.md
```

Only the three source projects are essential:

1. `CFIP.Indicator`
2. `CFIP.Contracts`
3. `CFIP.cBot`

No Cloud project is created.

---

## 7. Minimal Indicator → cBot contract

The contract must be small enough that it can later be transported elsewhere, but it must not include Cloud-specific infrastructure.

### 7.1 Signal identity

Required:

- ContractVersion;
- SignalId;
- ScenarioId;
- SourceTimeframe;
- Lane;
- Symbol;
- Direction;
- CreatedUtc;
- CreatedClosedM5;
- ExpiryUtc / expiry policy;
- Revision.

### 7.2 Executable plan

Required:

- ExecutionMode;
- Entry;
- IdealEntry;
- EntryZoneLow;
- EntryZoneHigh;
- EntryTrigger;
- EntryInvalidation;
- Stop;
- TP1;
- TP2;
- TP3;
- TP4;
- PlanRisk;
- TP1RR / later RR values where needed for diagnostics;
- EntryQuality;
- PlanQuality/actionability;
- analytical source/reason code.

### 7.3 Execution intent

The Indicator sends an intent, not a cTrader broker object.

Minimum execution fields:

- action type: Market / Aggressive / Pending Stop / Pending Limit;
- requested entry;
- stop;
- initial target;
- analytical sizing request/mode where needed; final broker/account-normalized volume is cBot-owned;
- scenario identity;
- plan identity;
- timestamp;
- expiration;
- revision.

### 7.4 Management command

Minimum local management contract:

- PlanId / PositionId;
- command type: Keep / ModifyProtection / AdvanceTarget / BreakEven / PartialClose / FullClose / CancelPending;
- desired broker target/stop where applicable;
- reason;
- command revision/idempotency key.

The Indicator is allowed to request a management action. The cBot decides whether the requested broker mutation is currently valid and then owns the mutation.

### 7.5 Broker report

The cBot reports back:

- attempt identity;
- scenario identity;
- broker action;
- submitted / accepted / rejected / confirmed / recovery;
- broker position/pending identifier;
- broker-confirmed entry;
- broker-confirmed SL;
- broker-confirmed TP;
- error code/reason;
- timestamps.

This becomes the source for the Indicator's execution/visual telemetry; it does not make the Indicator a broker authority again.

---

## 8. Project dependency and packaging rule

The dependency direction is one-way:

```
CFIP.Indicator  →  CFIP.Contracts
CFIP.cBot       →  CFIP.Contracts
CFIP.cBot       →  CFIP.Indicator (supported custom-indicator reference only)
```

Rules:

- `CFIP.Contracts` references no cTrader package.
- `CFIP.Indicator` owns all analysis, planning, UI and read-only signal production.
- `CFIP.cBot` owns every live broker mutation and account-dependent execution enforcement.
- No project may reference the other execution project to call internal/private broker helpers.
- No shared mutable singleton is introduced across projects.
- Packaging/output must be validated in the target cTrader environment before live cutover.
- The solution file and all project/build metadata must explicitly include the three required source projects and their supported reference/build order.
- Parameter migration follows one rule: if a parameter changes the Indicator's analytical proposal/plan, it remains Indicator-owned; if it only controls broker/account execution or live broker management, it moves to cBot; presentation-only parameters remain Indicator-owned.
- No execution parameter is duplicated across Indicator and cBot with two independent values.
- CBOT-0 must explicitly classify every current parameter, including the existing `13_auto_trading`, `10_live_management`, `15_control_advanced`, `09_risk_targets` and `24_smart_execution` groups, rather than moving an entire mixed parameter file blindly.
- Parameters such as Auto Trading/Auto Orders enable state, managed broker label/identity, account-based risk sizing, margin/capacity/daily-loss enforcement and broker-modification throttles are expected cBot-owned when their behavior is confirmed to be execution/account-only.
- Parameters that change signal evidence, confidence, quality, MTF interpretation, Entry/SL/TP proposal or analytical scenario selection remain Indicator-owned.
- Adding a fourth "shared core" project is out of scope unless CBOT-0 proves a hard compiler/dependency requirement; any such exception must be documented and must not duplicate business authority.

## 8.1 Local connection design

### Preferred first implementation

The cBot references the custom Indicator through cTrader's supported custom-indicator mechanism and obtains its public output/contract from the Indicator instance.

cTrader documents custom indicators being referenced from cBots through `Indicators.GetIndicator<>()`. The cBot creates/uses an Indicator instance; we will not depend on static globals or chart-object scraping.

The important implementation task is therefore to expose a **read-only public CFIP signal provider surface** without exposing broker mutation methods.

### Instance rule

The visible Indicator attached to a chart and the Indicator instance used by the cBot must not be assumed to be the same object until the target cTrader API behavior is demonstrated.

Therefore:

- no static mutable trading state;
- no global singleton;
- no reading chart labels as the signal protocol;
- no scraping drawn lines/objects;
- no reflection-based access to private state.

The cBot must consume structured signal data.

The signal surface must include:

- producer instance scope (symbol/timeframe/strategy identity);
- CreatedUtc and CreatedClosedM5;
- ExpiryUtc;
- Revision plus a monotonic sequence/generation for ordering revisions;
- actionable/status state so "no signal", "watch", "confirmed", "expired" and "unavailable" are distinct;
- ScenarioId/Lane/SourceTimeframe;
- correlation/idempotency keys.

### UI control boundary

The Indicator may continue to display Auto Trading / Auto Orders status and execution diagnostics, but after separation it must not directly mutate the broker.

The authoritative live execution enable/disable state belongs to the cBot. During migration, the roadmap must either:

1. provide a supported local control/status contract, or
2. make the Indicator control explicitly read-only and expose the authoritative control on the cBot.

The final implementation must never make the Indicator appear to disable trading while the cBot remains armed, or vice versa. Any unavailable control/status channel is fail-closed for automatic execution.

If a target-terminal test proves that direct attached-instance access is supported and reliable for the deployment target, that can be documented as an optimization. It is not a prerequisite for the architecture.

---

## 9. Execution lifecycle in the cBot

The cBot state machine should be:

```
WAITING
  ↓
RECEIVE SIGNAL
  ↓
VALIDATE CONTRACT
  ↓
CHECK EXPIRY / REVISION / DUPLICATE
  ↓
CHECK BROKER & ACCOUNT SAFETY
  ↓
PREPARE BROKER REQUEST
  ↓
SUBMIT
  ↓
BROKER RESULT
  ├── REJECTED → retry/circuit policy
  ├── ACCEPTED BUT UNCONFIRMED → reconciliation
  └── CONFIRMED → ADOPT LIVE STATE
                       ↓
                  PROTECTION
                       ↓
                  LIVE MANAGEMENT
                       ↓
                  CLOSE / PARTIAL / TP / SL
                       ↓
                  BROKER CONFIRMATION
                       ↓
                  OUTCOME EVENT
```

Restart/reconnect:

```
START
 ↓
READ BROKER STATE
 ↓
RECONCILE MANAGED IDENTITY
 ↓
ADOPT ONLY BROKER-CONFIRMED STATE
 ↓
RECOVER PROTECTION
 ↓
ONLY THEN ACCEPT NEW ACTIONS
```

This preserves the project's existing safety matrix.

---

## 10. Mandatory separation phases

The implementation order is **CBOT-0 → CBOT-Preflight → CBOT-1 → CBOT-2 → CBOT-3 → CBOT-4 → CBOT-5 → CBOT-6 → CBOT-7**. CBOT-Preflight is a no-trade blocking gate, not a separate product rewrite phase.

### CBOT-0 — Boundary inventory and freeze (historical baseline; superseded as the active start gate by CBOT-P0)

Machine gate: `tools/audit_cbot_boundary.py` inventories direct broker mutations, broker/account lifecycle access, and execution-related parameter usage. It is wired into Source/Architecture CI during the separation track.

Output:

- exact file/method ownership matrix;
- exact direct broker-call inventory;
- exact mixed-class split list;
- dependency graph of Indicator → Contracts → cBot;
- method/field/helper dependency closure for every candidate moved owner;
- direct broker API inventory including both method calls and fluent/object mutation calls;
- permission/account-state access inventory;
- event-handler/timer/thread ownership inventory for broker lifecycle work;
- complete parameter ownership/migration matrix for every current execution-related parameter;
- explicit list of parameters that remain Indicator-owned because they change analytical proposal/plan behavior;
- explicit list of parameters that move to cBot because they control broker/account execution or live broker management;
- explicit list of presentation-only parameters that remain Indicator-owned;
- proof that no two public parameters control the same execution behavior after migration;
- no source behavior change.

Acceptance:

- every broker mutation has one future cBot owner;
- every analytical owner remains with Indicator;
- no ambiguous “shared executor” remains.

### CBOT-Preflight — cTrader host capability proof (required before live mutation cutover)

After CBOT-0 is accepted and before creating the Contracts project, prove the supported local cTrader integration surface on the actual target environment.

This is a **no-trade capability test**, not a production implementation.

The proof must establish:

- the cBot can instantiate the compiled custom Indicator through the supported custom-indicator mechanism;
- the cBot can read a public structured read-only signal surface from that Indicator instance;
- the required public types are visible across the two compiled components without reflection;
- the Indicator can run with broker mutation disabled and still calculate/render normally;
- Indicator and cBot startup order can be reversed without stale or fabricated signal consumption;
- symbol/timeframe/instance scope can be identified deterministically;
- the cBot can detect an unavailable, uninitialized or stale Indicator signal and fail closed;
- the deployment/package layout for Indicator, Contracts and cBot is accepted by the target cTrader environment.

Blocking rule:

**CBOT-1 cannot start until CBOT-0 is accepted and CBOT-Preflight is verified.**

If the target environment cannot support the intended structured read-only handoff, stop the migration and redesign the local transport boundary before moving any broker authority. Do not fall back to chart-object scraping, static globals, reflection, or a duplicated analysis engine.

### CBOT-1 — Contracts

Create `CFIP.Contracts`.

The contract boundary must be **data-only and platform-neutral**. It is not a place to duplicate Indicator business rules.

Implement:

- Signal identity;
- Scenario identity;
- executable plan snapshot;
- execution intent;
- management command;
- broker execution report;
- lifecycle/protection event types;
- contract schema version;
- explicit sequence/order information for same-signal revisions;
- correlation identifiers linking intent → attempt → broker report;
- execution-control/status messages needed to keep the Indicator UI read-only with respect to broker mutation.

Acceptance:

- no cTrader dependency in Contracts;
- no Account/Position/PendingOrder types in Contracts;
- deterministic equality/idempotency keys;
- monotonic sequence semantics for revisions;
- correlation from signal/scenario → execution command → broker result;
- multi-scenario identity preserved;
- no cTrader types, broker objects, account objects or UI objects.

### CBOT-2 — Indicator read-only signal surface

Status: **VERIFIED COMPLETE — 2026-10-02.**

Verification: Source / Architecture #2790 PASS; Runtime Acceptance #2599 PASS; cTrader Compile #2783 PASS.

The Indicator now exposes a public read-only provider interface.

The provider must expose an immutable snapshot/envelope rather than references to mutable internal `Plan` or broker objects. The provider surface must also expose freshness state and a deterministic "no actionable signal" state.

It exposes structured signal/plan data but no broker mutation API.

Refactor:

`Plan` / `ExecutionIntent` → public contract snapshots.

Acceptance:

- cBot can consume a current actionable plan;
- expired/revised scenarios are distinguishable;
- no chart parsing is required;
- Indicator remains able to run as an analysis/display component without broker mutation;
- stale/uninitialized output cannot be mistaken for a valid actionable signal;
- revised signals are ordered deterministically;
- loss of cBot presence does not activate a hidden Indicator execution fallback.

### CBOT-3 — cBot host and shadow execution

Create `CFIP.cBot` and wire it to the Indicator contract.

Shadow mode is a temporary validation mode only; it must never coexist with a live Indicator executor. The legacy execution path may remain only on a validation branch/snapshot used as a parity oracle and must not be shipped as a second live engine.

Initially:

- receive;
- validate;
- identify;
- log;
- simulate submission.

No live broker mutation yet.

Acceptance:

- Market/Aggressive/Stop/Limit intents are distinguishable;
- duplicate intents are suppressed;
- scenario-scoped retry identity works;
- one-position capacity is enforced;
- restart does not duplicate a pending action;
- offline/missing Indicator is fail-closed;
- Indicator UI/control status cannot directly invoke broker mutation from the Indicator process;
- solution/project/package wiring is reproducible from a clean checkout.

### CBOT-4 — Broker execution extraction

Move/extract the actual broker mutation owners into the cBot.

First:

- Market;
- Aggressive;
- Pending Stop;
- Pending Limit.

Then:

- cancellation;
- close;
- partial close;
- SL/TP mutation.

Acceptance:

- no direct broker mutation remains in the Indicator project;
- all four submission paths use the same identity and confirmation rules;
- rejection cannot become fill state.

### CBOT-5 — Protection, lifecycle and recovery extraction

Move/extract:

- broker-confirmed state;
- protection reconciliation;
- break-even;
- target progression mutation;
- trailing/profit-lock mutation;
- partial/full close;
- pending lifecycle;
- restart/reconnect recovery.

Acceptance:

- broker state is authoritative;
- missing/invalid protection is explicit recovery state;
- SL moves are protective-only;
- target progression is monotonic;
- pending is not treated as a position before confirmation.

### CBOT-6 — Account/execution risk ownership

Move live account-dependent enforcement into cBot:

- position size normalization;
- margin safety;
- capacity;
- daily loss;
- spread;
- session/broker execution restrictions;
- trading permission;
- managed identity.

Keep analytical signal quality and plan RR in Indicator.

Acceptance:

- no account mutation/permission dependency in analytical code;
- no cBot re-implementation of the decision engine;
- execution rejects unsafe broker conditions immediately before mutation.

## Hard migration rules

- A moved method carries its required fields/helpers/dependencies with it; no hidden dependency remains in the Indicator.
- Semantic business rules have one authoritative owner. Pure DTO/serialization code may be shared only when it contains no business rule.
- The legacy Indicator executor may be used only as a repository parity oracle during migration; it must never ship or run as a second live executor.
- At every live test stage exactly one executor is armed.
- Missing, stale or incompatible Indicator/cBot state fails closed.
- Analytical parameters remain Indicator-owned; broker/account/live-management parameters move to cBot; no duplicate execution controls.
- No direct broker mutation remains in Indicator after CBOT-7.
- The Indicator remains usable as an analysis/display component when the cBot is absent.

### CBOT-7 — Cutover and removal of Indicator execution authority

Remove the old automatic broker path from the Indicator.

This phase includes the final physical cleanup, not merely disabling the old code path. Any execution file/method moved to cBot must be deleted from the Indicator project or reduced to a strictly analytical/read-only component. No disabled duplicate executor, dead broker helper, or hidden fallback may remain in production source.

The Indicator becomes analysis/signal/display only.

The cBot becomes the single broker execution authority.

Acceptance is a hard architectural gate, not a gradual permanent dual-engine mode.

---

## 11. Required verification matrix

### Static/source

The audit must prove:

- Indicator contains no direct calls to broker mutation APIs;
- Contracts contains no cTrader broker types;
- cBot contains all broker mutation calls;
- no duplicate execution owner exists;
- no duplicate managed identity exists;
- no second decision authority exists;
- no multi-position behavior is introduced;
- ScenarioId remains part of execution identity.

Direct broker calls to audit include at minimum:

- `ExecuteMarketOrder`;
- `ExecuteMarketRangeOrder`;
- `PlaceStopOrder`;
- `PlaceLimitOrder`;
- `ModifyPosition`;
- `ModifyPendingOrder`;
- `ClosePosition`;
- `CancelPendingOrder` and supported asynchronous variants where present in the target API;
- direct position protection mutation calls such as `Position.ModifyStopLossPrice`, `Position.ModifyTakeProfitPrice`, `Position.ModifyTakeProfit(...)`;
- trading-permission mutation/request calls;
- order/position collection reads that are used to make execution-authoritative decisions.

### Contract

Test:

- valid/invalid signal;
- expired signal;
- revision change;
- duplicate signal;
- duplicate attempt;
- scenario collision;
- different timeframe scenarios;
- invalid Entry/SL/TP geometry;
- zero/negative/non-finite values;
- contract-version rejection.

### Execution

Test:

- normal BUY/SELL Market;
- Aggressive;
- Pending Stop;
- Pending Limit;
- broker rejection;
- null result;
- accepted-but-unconfirmed;
- fill mismatch;
- broker protection missing;
- partial close;
- full close;
- target progression;
- reconnect/restart.

### Parity

Before cutover, compare the old Indicator execution path against the new cBot path using identical signal inputs.

The comparison must show:

- same accepted/rejected analytical intents;
- same Entry/SL/TP intent;
- same ScenarioId;
- same execution mode;
- differences only where the cBot correctly applies broker/account constraints.

No profitability claim is inferred from this parity test.

### Runtime

Target-terminal tests must cover:

- Indicator alone;
- Indicator + cBot;
- cBot startup before/after Indicator initialization;
- disabled auto trading;
- expired signals;
- multiple scenarios;
- rapid quotes;
- pending fill;
- broker rejection;
- restart;
- reconnect;
- protection recovery.

---

## 12. Safety invariants during migration

Until CBOT-7 is complete:

- never allow both Indicator and cBot to submit the same live order;
- use shadow mode first;
- keep one explicit execution owner at each test stage;
- keep broker-side SL/TP protection whenever available;
- fail closed on contract or identity uncertainty;
- do not infer broker state from desired plan data;
- do not loosen existing RR, risk or execution gates merely to make the split easier;
- do not promote independent timeframe scenarios to broker mutation;
- do not increase position capacity;
- when the cBot is unavailable, stale, disconnected or contract-incompatible, the Indicator remains analytical only and automatic execution is disabled;
- after cutover, absence of cBot capability can never re-enable an Indicator-side fallback executor.

---

## 13. Migration sequence

Recommended order:

1. Freeze analytical behavior and public signal semantics.
2. Complete CBOT-0 repository inventory and dependency/parameter ownership audit.
3. Complete CBOT-Preflight on the target cTrader environment.
4. Create Contracts.
5. Expose the read-only Indicator signal surface.
6. Create cBot in shadow mode.
7. Verify one-to-one plan/intent parity against a repository snapshot of the legacy path; never keep the legacy path as a second live engine.
8. Move Market execution.
9. Move Aggressive execution.
10. Move Pending Stop/Limit.
11. Move close/partial/protection and server-side TP mutation.
12. Move lifecycle/recovery.
13. Move account-dependent execution risk and permission enforcement.
14. Move/replace all execution-side UI control semantics so the cBot is authoritative.
15. Remove every obsolete Indicator broker-mutation file/method and prove zero direct mutation remains.
16. Run final architecture/runtime/restart/reconnect/multi-scenario gates.
17. Only after CBOT-7 acceptance resume previously blocked feature development.

This order deliberately puts the structural split before further execution-feature expansion.

---

## 14. Cloud future-proofing: only the minimum

Nothing Cloud-specific is implemented now.

To preserve a future path, only these rules are mandatory:

- Contracts must not reference cTrader types;
- Signal/management messages must be versioned;
- identifiers must be deterministic;
- execution results must be separable from analysis;
- the Indicator must not depend on local broker mutation to define its analytical result.

Later, a Cloud host could consume the same contracts, but that is outside the current local product.

---

## 15. Definition of Done

The separation is complete only when all are true:

1. `CFIP.Contracts` exists and is platform-neutral.
2. `CFIP.cBot` exists and builds under the target cTrader/.NET environment.
3. Indicator exposes a structured, read-only signal/plan surface.
4. All actual broker mutations are owned by cBot.
5. Indicator contains no duplicate live execution engine.
6. cBot owns one managed broker identity.
7. Scenario identity survives Indicator → cBot → broker telemetry.
8. Broker confirmation remains authoritative.
9. Restart/reconnect reconciliation remains deterministic.
10. Single-position capacity remains unchanged.
11. Market/Aggressive/Pending behavior has contract and runtime coverage.
12. Protection/partial-close/recovery behavior has contract and runtime coverage.
13. Full source/architecture audit is green.
14. Runtime Acceptance is green.
15. cTrader Compile/Build is green.
16. Target-terminal replay has passed the mandatory local execution matrix.
17. The Indicator production tree contains zero direct broker mutations and zero hidden/disabled duplicate execution paths.
18. The execution-control status shown by the Indicator is consistent with the cBot's authoritative live state.
19. A missing/stale/unavailable cBot or signal provider fails closed without fabricating state.
20. Only then may the next master development phase resume.

---

## 16. Explicit blocking rule

This roadmap is a **blocking architectural gate**.

After Phase 11.5, the project must not add more live broker-execution features to the Indicator.

The next implementation work is:

`CBOT-0 → CBOT-1 → CBOT-2 → CBOT-3 → CBOT-4 → CBOT-5 → CBOT-6 → CBOT-7`

Only after CBOT-7 is accepted should the previous analytical/execution certification queue resume.

The existing Phase 11.5 rule about formal scenario execution policy is retained, but any broker-execution promotion of additional scenarios must now be designed through the cBot boundary rather than by expanding the Indicator's broker authority.

---

## 17. Operator rule

This roadmap is documentation-only until CBOT-0 begins.

No Cloud setup is required.

No user-side pull is required for the roadmap's conceptual design itself; once the documentation commit is merged to `main`, the operator should pull `main` before beginning CBOT-0 so the local checkout contains the authoritative roadmap.



---

## 18. Roadmap hardening revision — 2026-09-30

Revision 2.1 closes the pre-implementation gaps found during repository re-audit.

Added/clarified:

- mandatory CBOT-Preflight before Contracts;
- exact repository path correction for `Trading/Execution/ExecutionPlanPreparation.cs`;
- explicit mixed-class rule for `BrokerProtectionCoordinator`;
- one-way project dependency and packaging rules;
- method/helper/field dependency-closure review;
- signal freshness, revision ordering, producer instance scope and correlation requirements;
- execution-control/UI authority boundary;
- repository-snapshot parity rule so the old executor is never retained as a second live engine;
- explicit fail-closed behavior when Indicator/cBot connectivity, initialization or status is unavailable;
- final zero-broker-mutation source gate in the Indicator;
- stronger Definition of Done and final runtime acceptance requirements.

This revision is documentation-only. No production C# behavior changes are authorized by this revision.


---

## 19. Roadmap final-audit revision — 2026-09-30

Revision 2.2 closes the remaining documentation gaps found in the second pre-implementation audit.

Added/fixed:

- corrected section numbering and removed duplicate CBOT-0 heading;
- aligned the mandatory order so CBOT-Preflight is explicitly the blocking gate;
- made solution/project/build metadata part of the architecture gate;
- added a complete execution-parameter ownership/migration requirement;
- established the rule that analytical parameters remain with Indicator while broker/account/live-management parameters move to cBot, with no duplicate controls;
- clarified that broker-normalized final volume is cBot-owned;
- broadened direct broker API scanning to include supported asynchronous mutation variants;
- added clean-checkout/package reproducibility to cBot host acceptance;
- added a final cross-document roadmap consistency gate.

No production C# behavior is changed by this revision.


---

## 21. Final readiness correction — 2026-09-30

Final audit correction: CBOT-0 is the first implementation phase and can begin from repository truth. CBOT-Preflight is a blocking no-trade capability gate that must pass before CBOT-1 Contracts are implemented. This keeps the one-phase workflow intact while ensuring no unproven cTrader handoff is used for the actual migration.

No production C# behavior is changed by this documentation correction.


## Active parallel schedule — 2026-10-02

The active execution-separation schedule is CBOT-P0 → P1 → P2 → P3 → P4 → P5 → P6 → P7 → P8, running in parallel with M2 onward. The historical M29–M38 sections remain as detailed ownership/reference material and are not a second authority.

Hard rule: a migrated execution path is physically removed from the Indicator only after its cBot replacement, caller migration, deterministic parity and source gate pass. This prevents both broken functionality and a permanent dual executor.
