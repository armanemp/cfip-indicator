# CFIP-TRADE — Canonical Trade-Chain Roadmap

## Status

**STATUS: ACTIVE / CANONICAL FOR THE TRADE CHAIN**

This document is the dedicated roadmap for the complete CFIP trading chain:

**Market Data → Canonical Context → MTF → Analysis → Evidence → Decision → Actionability → Trigger → Opportunity → Plan → Risk → Contract → cBot → Broker → Confirmation → Protection → Lifecycle → Outcome → History → Calibration**

It is subordinate to the repository-wide control plane for acceptance gates, but authoritative for the sequencing and completeness of the **trade-chain audit itself**.

This roadmap exists so that the core trading path is completed and certified **before non-critical UI, cosmetic, repository-cleanup or secondary-feature work is allowed to become the main focus**.

---

# 1. Mission

The objective is not merely to make the cBot capable of submitting an order.

The objective is to prove that every trade produced by CFIP is the result of one deterministic, internally consistent chain in which:

1. market observations are valid and time-consistent;
2. all supported timeframe contexts are correctly built;
3. analytical evidence has provenance and is not double-counted;
4. one canonical decision owner determines direction and quality;
5. actionability is explicitly distinguished from prediction/future opportunity;
6. M5 performs entry timing/precision rather than becoming a competing decision clock;
7. M1 can only provide optional confirmation/precision;
8. the plan contains coherent executable Entry/Trigger/SL/TP and reward-path geometry;
9. risk and volume sizing are derived from the real account and executable broker constraints;
10. every opportunity has stable identity;
11. the Indicator publishes one canonical execution intent;
12. the cBot validates the intent without rebuilding analysis;
13. only the cBot can mutate broker state;
14. broker confirmation becomes the authoritative execution truth;
15. protection and management reduce risk without silently widening it;
16. restart/reconnect/retry cannot duplicate or corrupt execution;
17. the lifecycle reaches one broker-confirmed outcome;
18. outcome/history/calibration remain observational and cannot silently rewrite live policy.

A phase is not complete because code compiles. It is complete only when its semantic contract, ownership, implementation, tests and applicable terminal/broker evidence agree.

---

# 2. Non-negotiable trade-chain laws

## 2.1 Single semantic owner

One concept has one owner.

Examples:

- Decision direction → one Decision authority.
- Actionability → one Actionability authority.
- Entry/SL/TP geometry → one Plan authority.
- Volume/risk policy → one Risk authority.
- Scenario identity → one Identity/Contract authority.
- Broker submission → one cBot execution owner.
- Broker-confirmed state → broker truth, consumed read-only.
- Lifecycle transition → one lifecycle authority.
- Outcome → one outcome authority.

Consumers must not silently recalculate or reinterpret the owner's result.

## 2.2 No second strategy engine

The cBot must never reconstruct the Indicator's analysis.

The Indicator must never create an alternate execution engine.

The Contracts project must transport semantic intent, not invent strategy decisions.

## 2.3 Broker truth law

These are distinct states:

**Candidate → Decision → Plan → Intent → Submission → Acceptance → Fill/Creation → Confirmation → Protection → Lifecycle → Outcome**

Submission is not confirmation.

A successful API return is not permission to invent broker state.

## 2.4 Timeframe law

Production analytical timeframes are exactly:

**M1 / M5 / M15 / M30 / H1 / H4 / D1 / W1**

- **M15** = canonical decision/reference and execution-planning center.
- **M5** = trigger/retest/breakout and entry-precision layer.
- **M1** = optional precision/confirmation; it cannot create directional consensus alone.
- **M30/H1/H4** = higher context.
- **D1/W1** = broader optional context.
- **M2 is permanently forbidden** as provider, source, cache, panel item, parameter, contract field, fallback or execution clock.

## 2.5 Closed-bar law

Confirmed decision analysis uses the canonical fully-closed bar context.

Intrabar/reaction data must be explicitly classified as reaction data and must never be allowed to masquerade as closed-bar confirmation.

No look-ahead, future-bar contamination or open/closed index substitution is permitted.

## 2.6 Executable-price law

Entry, Stop and Target validity must be evaluated against the actual executable side:

- BUY entry uses Ask-side execution semantics;
- SELL entry uses Bid-side execution semantics;
- spread is part of entry/protection safety where relevant;
- broker minimum distance, tick size, pip size and symbol precision are respected;
- stale quotes cannot authorize execution.

## 2.7 Risk monotonicity

Automatic execution must not widen risk.

After a protective Stop exists:

- a protection update may preserve or reduce risk;
- it must never silently move the Stop to a less protective location;
- retries must not create a wider-risk duplicate request;
- broker-confirmed protection is authoritative.

## 2.8 Identity law

The chain keeps these identities distinct:

**SignalId → ScenarioId → ExecutionId → Broker position/order identity → Outcome identity**

Retries preserve the same causal identity.

A genuinely new opportunity receives a new ScenarioId.

Stale revisions cannot mutate the current opportunity.

## 2.9 Current/future law

The system must explicitly distinguish:

- **ActionableNow** — can be executed immediately after all gates;
- **FutureOrderReady** — valid future opportunity eligible for a pending order;
- **WATCH/PREDICTION** — informative, not executable;
- **BLOCKED/RESTRICTED** — cannot cause trading side effects.

One state must not be silently converted into another.

## 2.10 Safety isolation

Telemetry, panel, alert, persistence or visual failures must not silently authorize unsafe entry.

Safety-critical broker management must remain available independently where the architecture permits.

---

# 3. Canonical chain

The expected ownership direction is:

```
Market Data
    ↓
Canonical Time/Price Context
    ↓
MTF Closed Context
    ↓
Analytical Stack
    ↓
Evidence / Provenance / Independence
    ↓
Decision
    ↓
Actionability
    ↓
Trigger / Opportunity
    ↓
Trade Plan
    ↓
Risk / Volume / Margin / Spread
    ↓
Scenario + Execution Identity
    ↓
Signal Contract
    ↓
cBot Binding + Preflight
    ↓
Execution Environment Gates
    ↓
Broker Submission
    ↓
Broker Confirmation
    ↓
Protection / Management
    ↓
Lifecycle / Recovery
    ↓
Outcome
    ↓
History / Calibration
```

The direction of authority flows forward. Lower layers may reject an unsafe request, but they must not silently manufacture a new strategy decision.

---

# 4. Phase map

The trade chain is divided into the following dedicated work packages.

| ID | Phase | Primary objective | Exit condition |
|---|---|---|---|
| T0 | Trade Baseline | Freeze actual chain topology and evidence baseline | Every trade-chain owner and boundary identified |
| T1 | Market Data Truth | Validate raw market data and executable quote semantics | Deterministic valid observation contract |
| T2 | Time / MTF / Closed-Bar | Validate all timeframe and temporal semantics | No look-ahead, no M2, deterministic closed context |
| T3 | Analytical Stack | Audit all indicators, structure, zones, liquidity and regime | One owner per detector, provenance intact |
| T4 | Evidence Independence | Prevent double counting and correlated evidence inflation | Independent evidence reaches Decision exactly once |
| T5 | Decision Engine | Audit direction, confidence, score, quality and filters | One canonical decision result |
| T6 | Actionability / Trigger / Opportunity | Separate now/future/watch and validate M5/M1 timing | Only valid opportunities become actionable |
| T7 | Trade Plan | Audit Entry, Trigger, SL, TP1–TP4, reward path and RR | Executable coherent plan |
| T8 | Risk / Sizing / Margin | Audit monetary risk, volume, margin, capacity and spread | Safe broker-compatible size |
| T9 | Identity / Scenario / Contract | Prove lineage and stale/retry semantics | No duplicate or stale execution |
| T10 | Indicator → cBot Handoff | Prove transport, binding, freshness and ownership boundary | cBot receives canonical intent without re-analysis |
| T11 | cBot Preflight | Validate final execution-time conditions | Unsafe requests stop before broker |
| T12 | Broker Submission | Audit market, range, aggressive and pending submissions | One submission path and deterministic retry semantics |
| T13 | Broker Confirmation | Separate accepted/submitted/filled/created states | Broker truth is authoritative |
| T14 | Protection / Management | Stop, TP, BE, profit lock, trailing, partial and exit | Risk never widens; management is broker-confirmed |
| T15 | Lifecycle / Recovery | Restart, reconnect, recovery, adoption and idempotency | No duplicate position/order or lost lifecycle |
| T16 | Outcome / History | Record confirmed result and attribution | One complete broker-confirmed outcome |
| T17 | Calibration / Quality Proof | MAE/MFE, reward distribution, false/missed signals, OOS | Quality evidence separated from live policy |
| T18 | Performance / Resilience | Hot-path, allocations, cache, I/O and failure behavior | Measured performance with semantic equivalence |
| T19 | End-to-End Trade Certification | Prove the complete path in target terminal | Full chain certified with evidence |

No phase may bypass an earlier unresolved safety/ownership defect merely because a later layer appears functional.

---

# 5. Detailed phase contracts

## T0 — Trade Baseline

### Scope

Establish the real current trade graph from source, not old documentation.

Inventory:

- Indicator entry point;
- market-context builders;
- MTF providers/caches;
- indicator/OSS adapters;
- structure/liquidity/FVG/OB detectors;
- evidence/confluence;
- decision;
- actionability;
- trigger/retest;
- opportunity lanes;
- plan construction;
- risk/sizing;
- execution intent;
- scenario/identity;
- provider/transport;
- cBot binding;
- preflight;
- execution owners;
- broker reconciliation;
- protection/management;
- lifecycle;
- history/outcome;
- calibration.

### Required evidence

- exact branch and commit;
- current file inventory;
- call/consumer graph for all critical concepts;
- direct broker mutation scan;
- M2 scan;
- parameter ownership graph;
- trade-state ownership graph.

### Exit

A complete owner map exists and no critical trade-chain concept is unaccounted for.

---

## T1 — Market Data Truth

### Audit

- symbol identity;
- Bid/Ask;
- Mid/reference price semantics;
- tick size;
- pip size;
- digits/precision;
- minimum executable distance;
- spread;
- quote freshness;
- history availability;
- missing bars;
- gaps;
- session state;
- market status;
- data source provenance;
- observation timestamps;
- history replacement events;
- cache invalidation.

### Required invariants

- BUY and SELL use correct executable sides.
- Spread cannot disappear merely because a plan was built from mid/close data.
- Invalid or stale price values fail closed.
- No downstream stage invents missing market values.

### Exit

Every execution-relevant price has explicit provenance and executable-side semantics.

---

## T2 — Time / MTF / Closed-Bar Integrity

### Audit

- M1/M5/M15/M30/H1/H4/D1/W1 loading;
- closed indexes;
- OpenTime boundaries;
- UTC and DST;
- next-bar semantics;
- reference timestamp;
- cross-timeframe mapping;
- history warm-up;
- cache refresh;
- MTF freshness;
- bar replacement;
- reaction/open-bar isolation;
- M2 absence.

### Special contract

M15 remains the canonical decision/reference center.

M5 provides trigger/entry precision and cannot become an alternate decision clock.

M1 is optional precision/confirmation only.

### Exit

A decision can be reconstructed deterministically from the same closed context at the same reference time.

---

## T3 — Analytical Stack

### Audit

- trend/momentum;
- native indicators;
- OSS adapters;
- EMA/ADX/DMI/RSI/MACD and other calculations;
- volatility/range/choppiness;
- swing points;
- structure;
- BOS/MSS/CHOCH;
- liquidity sweeps;
- equal levels;
- FVG detection/lifecycle/mitigation;
- order blocks and mitigation;
- retests;
- divergence;
- WaveTrend;
- VWAP/volume/volume-profile components;
- higher-timeframe context;
- market regime.

### Owner rule

Each detector has one canonical calculation/lifecycle owner.

Adapters may translate; they must not recalculate a second version of the same semantic result.

### Exit

Analytical results are deterministic, provenance-preserving and free of competing detector logic.

---

## T4 — Evidence Independence

### Audit

- evidence grouping;
- independence;
- correlation;
- duplicate structure evidence;
- duplicate OB/FVG evidence;
- trend/momentum double counting;
- repeated timeframe representations;
- context duplication;
- conflict handling;
- contribution weights;
- negative/conflicting evidence.

### Required invariant

Multiple representations of the same underlying fact count as one semantic fact.

Independent evidence may contribute separately.

### Exit

Decision input can explain exactly which independent evidence groups contributed and why.

---

## T5 — Decision Engine

### Audit

- direction;
- score;
- confidence;
- quality;
- smart quality;
- consensus;
- filters;
- structural gates;
- market gates;
- confirmation gates;
- higher-timeframe penalties;
- range/chop handling;
- restriction states;
- block reasons;
- calibration modifiers;
- stale decision invalidation.

### Required states

At minimum, distinguish:

**NONE / WATCH / CONFIRMED / READY / BLOCKED / RESTRICTED**

Where applicable, the exact repository contract may add explicit sub-states without collapsing these meanings.

### Exit

One decision object is authoritative and all downstream consumers use it.

---

## T6 — Actionability / Trigger / Opportunity

### Audit

- ActionableNow;
- FutureOrderReady;
- WATCH/prediction;
- M5 trigger;
- retest;
- breakout;
- trigger buffer;
- entry trap risk;
- M1 optional confirmation;
- opportunity lanes;
- strategic/tactical/micro distinctions;
- counter-HTF cases;
- weak/range suppression;
- opportunity magnitude and quality.

### Required invariant

No prediction, watch signal or restricted candidate may accidentally reach broker execution.

### Exit

Each opportunity has an explicit state and executable timing contract.

---

## T7 — Trade Plan

### Audit

- requested entry;
- ideal entry;
- executable entry;
- trigger;
- zone low/high;
- invalidation;
- structural stop;
- Stop buffer;
- TP1/TP2/TP3/TP4;
- target source/provenance;
- obstacles;
- liquidity path;
- HTF reward path;
- target progression;
- minimum reward distance;
- RR;
- price normalization;
- spread impact;
- broker distance constraints.

### Mandatory checks

- BUY targets are above entry where required.
- SELL targets are below entry where required.
- Stop is on the correct risk side.
- Risk > 0.
- RR is calculated from the canonical plan.
- RR is not distorted by premature rounding.
- Tiny stagnant-market targets are rejected where contract requires it.
- Reward path is structurally valid rather than merely numerically distant.
- No generic fixed 1:2 rule becomes the strategy owner.

### Exit

One canonical plan supplies all execution geometry.

---

## T8 — Risk / Sizing / Margin

### Audit

- risk percent;
- risk amount;
- fixed-volume mode where legitimately supported;
- stop-distance sizing;
- symbol volume constraints;
- minimum/maximum volume;
- normalization step;
- account balance/equity;
- free margin;
- margin level;
- stop-out proximity;
- margin budget;
- concurrent scenario capacity;
- maximum open positions;
- session execution cap;
- daily loss;
- spread-to-risk relationship;
- minimum viable risk;
- maximum viable reward-path requirements.

### Special requirement

The product requires multiple distinct opportunities to be representable. Any temporary capacity restriction must be a deliberate, visible risk policy and must not be an accidental hard-coded single-plan limitation.

### Exit

Requested volume is converted to one safe broker-compatible volume without bypassing account risk or margin safety.

---

## T9 — Identity / Scenario / Contract

### Audit

- SignalId;
- ScenarioId;
- ExecutionId;
- Broker identity;
- Outcome identity;
- revision;
- timestamps;
- expiry;
- idempotency key;
- scenario lineage;
- stale request rejection;
- retry preservation;
- market vs aggressive vs pending identity;
- codec/schema/version compatibility;
- identity mismatch rejection.

### Required invariant

A retry may repeat an attempt, but never create an unrelated execution identity for the same causal scenario.

### Exit

Every execution can be traced backward to its originating decision/plan and forward to broker/outcome identity.

---

## T10 — Indicator → cBot Handoff

### Audit

- signal transport;
- scenario-batch transport;
- provider freshness;
- chart binding;
- instance identity;
- symbol matching;
- restart/rebind;
- missing Indicator;
- stale Indicator;
- contract-version compatibility;
- payload validation;
- cBot does not rebuild analysis.

### Exit

The cBot consumes exactly the Indicator's canonical intent and rejects invalid/stale/mismatched payloads deterministically.

---

## T11 — cBot Preflight

### Audit

- master auto-trading state;
- market/pending/aggressive/management path arms;
- account Demo/Live state;
- symbol;
- session;
- spread;
- daily loss;
- margin;
- stop-out;
- volume;
- execution label;
- capacity;
- duplicate identity;
- current quote;
- current plan refresh where required;
- market-hours state;
- broker constraints;
- fail-closed behavior.

### Exit

Every unsafe or stale request is stopped before broker mutation.

---

## T12 — Broker Submission

### Market

Audit:

- market order;
- market-range order;
- aggressive market;
- range semantics;
- requested entry;
- stop/target distances;
- volume;
- label;
- idempotency.

### Pending

Audit:

- Stop order;
- Limit order;
- correct side of current executable quote;
- expiry;
- distance;
- stop/target;
- capacity;
- identity.

### Retry

One retry/backoff policy must exist.

Retry must not become an alternate execution path.

### Exit

Each execution mode has one cBot broker owner and one submission lifecycle.

---

## T13 — Broker Confirmation

### Audit

Distinguish:

1. request created;
2. broker submission returned;
3. broker accepted;
4. Position actually exists;
5. PendingOrder actually exists;
6. broker-provided entry;
7. broker-provided SL;
8. broker-provided TP;
9. fill/slippage;
10. error/rejection;
11. unknown/recovery-required state.

### Exit

Only broker-confirmed state is promoted to authoritative execution truth.

---

## T14 — Protection / Management

### Audit

- initial SL;
- initial TP;
- broker protection;
- break-even;
- profit lock;
- trailing;
- partial take profit;
- target progression;
- reversal protection;
- structural invalidation exit;
- smart exit;
- pending cancellation;
- position close;
- stale management commands;
- command dedup;
- broker confirmation.

### Safety invariants

- Protection cannot silently widen risk.
- Management acts on the active live broker object, not only the plan.
- Partial close cannot accidentally exceed position volume.
- Break-even after partial close must use broker-confirmed remaining volume/state.
- Target advancement cannot regress.
- Management commands remain identity-bound.

### Exit

Protection/management follows the live broker object and is fully reconciled.

---

## T15 — Lifecycle / Recovery

### Audit

- created;
- submitted;
- active;
- modified;
- partially closed;
- fully closed;
- pending created;
- pending modified;
- pending filled;
- pending cancelled;
- rejected;
- expired;
- orphan;
- recovery required;
- recovered;
- restart;
- reconnect;
- cBot restart;
- Indicator restart;
- instance replacement;
- provider loss;
- broker state adoption.

### Required invariant

Restart/reconnect must reconcile broker truth before treating local state as authoritative.

### Exit

Lifecycle remains coherent through all recoverable disruptions and remains idempotent.

---

## T16 — Outcome / History

### Audit

- broker-confirmed close;
- realized P/L;
- R outcome;
- entry/exit provenance;
- MAE;
- MFE;
- time in trade;
- target reached;
- Stop reached;
- manual/intervening close classification;
- pending cancellation/expiry;
- rejected execution;
- duplicate suppression;
- persistence;
- archive;
- account/symbol/strategy identity.

### Exit

Every executed scenario ends in one attributable confirmed outcome or an explicitly unresolved recovery state.

---

## T17 — Calibration / Quality Proof

### Audit

- historical outcome attribution;
- MAE/MFE distributions;
- confidence calibration;
- win/loss distribution;
- reward distribution;
- false signals;
- missed opportunities;
- rejection reasons;
- regime-specific quality;
- timeframe-specific behavior;
- OOS/walk-forward;
- ablation of evidence groups;
- parameter sensitivity.

### Critical rule

Calibration may produce evidence or governed policy inputs.

It must never silently mutate live trading policy.

### Exit

Predictive quality is measured independently from execution mechanics and from cosmetic presentation.

---

## T18 — Performance / Resilience

### Measure

- startup;
- history warm-up;
- Calculate;
- closed-bar rebuild;
- MTF cache;
- zone cache;
- decision build;
- plan build;
- persistence;
- LocalStorage;
- cBot polling/timer;
- event handlers;
- broker reconciliation;
- chart object churn;
- memory;
- allocations;
- exception frequency.

### Required invariant

Optimization is permitted only after semantic equivalence is proven.

### Exit

Measured performance is acceptable, failure handling is bounded, and no optimization introduces a second semantic path.

---

## T19 — End-to-End Trade Certification

### Mandatory terminal scenarios

At minimum, cover:

1. Indicator starts correctly.
2. cBot attaches to the correct Indicator instance.
3. cBot reports correct Demo/Live mode.
4. Analysis reaches READY/valid states.
5. A valid actionable signal is produced.
6. Identity propagates across Signal/Scenario/Execution.
7. cBot receives the canonical intent.
8. Preflight allows a valid request.
9. Broker creates/fills the expected object.
10. Actual broker Position/PendingOrder is observed.
11. Broker-confirmed SL/TP is observed.
12. Rejected request is classified correctly.
13. Duplicate request does not duplicate broker state.
14. Stale request is rejected.
15. Pending order is created with correct expiry.
16. Pending order fills and converts into live lifecycle.
17. Protection modification is broker-confirmed.
18. Break-even/profit lock is broker-confirmed.
19. Trailing never widens risk.
20. Partial close is broker-confirmed.
21. Full close ends the lifecycle.
22. Restart reconciles the existing broker object.
23. Reconnect reconciles without duplicate actions.
24. Final outcome is persisted once.
25. Visual/panel/alert presentation reflects the same authoritative state.

### Exit

No unresolved mandatory target-terminal or broker evidence remains for the completed chain.

---

# 6. Cross-phase invariants

These are checked in every applicable trade-chain phase.

## Direction parity

BUY and SELL must be structurally symmetric except for intentional sign/side differences.

## Price parity

Every consumer must use the same canonical normalized Entry/SL/TP for the same plan unless an explicit broker-confirmed price supersedes planned values.

## State parity

Decision, Actionability, Plan, Intent, cBot state and broker lifecycle may differ only according to their defined state transition, never by accidental contradiction.

## Identity parity

No downstream stage may replace causal identity without an explicit new opportunity.

## Freshness parity

A stale signal cannot become executable merely because the cBot restarted or polled again.

## Safety parity

Demo/live must use the same execution architecture, with environment-specific safety arms rather than separate execution engines.

## Execution-clock parity

M15 remains the reference decision/execution-planning center; M5 refines timing; M1 remains optional precision.

## Presentation parity

Panel/chart/alerts mirror authoritative state; they do not create trading truth.

---

# 7. Trade-chain defect classes

Every discovered defect is classified by root cause:

### DATA
Bad/missing/stale market information.

### TIME
Wrong bar, wrong timestamp, look-ahead, wrong MTF mapping.

### ANALYSIS
Incorrect detector, formula, lifecycle or provenance.

### EVIDENCE
Double counting, correlation, false independence.

### DECISION
Wrong direction/score/confidence/filter/state.

### ACTIONABILITY
Invalid now/future/watch conversion.

### TRIGGER
Bad M5/M1 timing or retest/breakout semantics.

### PLAN
Bad Entry/SL/TP/reward geometry.

### RISK
Bad risk amount, volume, margin or capacity.

### IDENTITY
Duplicate/stale/mismatched scenario or execution.

### CONTRACT
Invalid payload/schema/version/transport.

### PREFLIGHT
Unsafe condition not blocked, or valid condition blocked incorrectly.

### SUBMISSION
Incorrect broker API usage or retry behavior.

### CONFIRMATION
Submission confused with broker truth.

### PROTECTION
SL/TP/BE/trailing/partial behavior incorrect.

### LIFECYCLE
Open/modified/closed/recovery state incorrect.

### OUTCOME
Incorrect or missing attribution.

### PERFORMANCE
Unbounded hot-path work or pathological allocation/I/O.

### RESILIENCE
Restart/reconnect/failure causes semantic divergence.

---

# 8. Evidence standard

Each completed trade phase records:

- exact branch;
- exact commit;
- exact implementation files;
- canonical owner(s);
- callers/consumers audited;
- competing paths removed;
- focused tests;
- static audits;
- Release build;
- runtime evidence;
- terminal evidence;
- broker evidence;
- performance evidence;
- safety impact;
- remaining risks;
- next phase.

Evidence types are:

**SOURCE / TEST / AUDIT / BUILD / RUNTIME / TERMINAL / BROKER / PERFORMANCE / DOC**

Historical evidence cannot close a changed implementation.

A screenshot can support visual evidence, but it cannot substitute for source/contract/broker truth.

---

# 9. Definition of Done for every trade phase

A trade-chain phase can be marked **PASS** only when all applicable items are true:

1. Root cause is evidence-based.
2. Canonical owner is identified.
3. Full affected caller/consumer graph is audited.
4. Competing logic is removed or redirected.
5. State transitions are explicit.
6. BUY/SELL symmetry is checked.
7. Time/index semantics are explicit.
8. Price-side/spread semantics are explicit where relevant.
9. Invalid numeric values are handled.
10. Stale/retry/boundary cases are covered.
11. Identity and lineage remain intact.
12. Focused tests/contracts pass.
13. Whole-project integrity gates pass.
14. cTrader Release build passes.
15. Safety implications are assessed.
16. Performance implications are assessed.
17. Required target-terminal/broker scenarios are recorded.
18. `CFIP_GATE.md` is updated.
19. `CFIP-ROADMAP.md` continuity is updated where the trade-chain package changes the repository execution state.
20. Exactly one next trade phase is marked.
21. No hidden workaround, duplicate owner or parallel path remains.

---

# 10. Trade-chain stop conditions

The roadmap is BLOCKED immediately if any of these are introduced or rediscovered:

- Indicator broker mutation;
- second execution engine;
- second decision engine;
- second Plan builder for the same semantic path;
- duplicate risk/volume authority;
- M2;
- look-ahead;
- stale signal accepted as current;
- duplicate Scenario/Execution identity;
- plan state treated as broker truth;
- submission treated as fill;
- protection can widen risk;
- retry can duplicate broker action;
- recovery can create a second position/order;
- cBot invents analysis;
- downstream consumer silently rewrites strategy semantics;
- performance workaround changes semantic results without proof.

---

# 11. Current priority

The dedicated trade roadmap must be treated as the main engineering priority before unrelated cleanup or cosmetic work.

**Priority order:**

**T0 → T1 → T2 → T3 → T4 → T5 → T6 → T7 → T8 → T9 → T10 → T11 → T12 → T13 → T14 → T15 → T16 → T17 → T18 → T19**

However, an urgent safety defect may reopen the earliest affected phase.

Exactly one trade phase may be **NEXT** at a time.

---

# 12. Relationship to the repository master roadmap

`docs/CFIP-ROADMAP.md` remains the repository-wide execution roadmap.

`docs/CFIP_GATE.md` remains the repository-wide acceptance/defect authority.

`docs/CFIP-LIST.md` remains the exhaustive inventory.

**CFIP-TRADE.md** specializes and expands only the complete **analysis-to-trade chain**.

When documents disagree:

1. actual current source and verified evidence establish implementation truth;
2. `CFIP_GATE.md` establishes acceptance truth;
3. `CFIP-ROADMAP.md` establishes repository-wide execution order;
4. `CFIP-TRADE.md` establishes trade-chain scope, sequence and completeness.

Historical documents never override these control-plane rules.

---

# 13. Operator protocol

For every work package:

1. Synchronize the branch.
2. Read the current CFIP-TRADE phase status.
3. Inspect the real code around the owner and all consumers.
4. Make one complete atomic repair package.
5. Run the required local/CI verification.
6. Perform target-terminal testing where required.
7. Record evidence and defects.
8. Mark exactly one next trade phase.
9. Continue from verified repository state.

Operator commands must always be explicit and limited to commands actually required for the current phase.

---

# 14. Initial status

**Current trade-chain roadmap status: ACTIVE**

**Current trade-chain phase: T0 — Trade Baseline**

**T0 objective:** build the verified current trade-chain owner/consumer graph from the actual repository head and use it as the starting point for all later trade phases.

**No later trade phase is considered PASS before its required evidence is completed.**

---

# 15. Final certification target

CFIP-TRADE is fully complete only when the following statement is provable:

> A valid market observation is transformed exactly once into canonical MTF context, analytical evidence, decision, actionability, trigger, opportunity, plan and risk; that intent crosses the Indicator/Contracts/cBot boundary with stable identity; the cBot performs the only broker mutation after final safety validation; broker truth becomes authoritative; protection and lifecycle remain risk-safe through fills, modifications, exits, restart and reconnect; and the resulting outcome is attributed and persisted once, with quality/calibration evidence isolated from live-policy mutation.

That is the definition of a **complete CFIP trading system**.

---

## Revision history

### 2026-10-05 — Initial canonical trade-chain roadmap

Created as the dedicated roadmap for the full analysis-to-trade path. This document deliberately treats trading as one end-to-end system rather than as separate Indicator, cBot, risk, execution and management features.
