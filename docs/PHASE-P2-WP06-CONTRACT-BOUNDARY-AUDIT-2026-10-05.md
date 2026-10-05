# P2 / WP-06 — Contract Boundary Audit

Date: 2026-10-05
Baseline: `dabba8629480aff82742c159714a8a573e616457`
Branch: `audit/p2-wp06-contract-boundary-2026-10-05`

## Status

**BLOCKED — implementation applied on the audit branch; target-terminal evidence still blocks WP-06 closure.**

WP-06 remains blocked by WP-05 target-terminal evidence (0/13 required scenarios at the current control-plane state). This document records the contract-boundary findings discovered before implementation.

## Scope

Audited:
- `src/CFIP.Contracts` (23 C# sources on the baseline)
- contract schema audit `tools/audit_cbot_contract_schema.py`
- canonical gate/roadmap state
- available producer/consumer references for SignalEnvelope, PlanSnapshot, ExecutionIntent and AlertEnvelope
- runtime evidence supplied for 2026-10-05 11:47–11:50

## PASS findings

1. `CFIP.Contracts` is a standalone .NET 6 boundary with no cTrader/cAlgo dependency.
2. Canonical contract families exist: ContractIdentity, PlanSnapshot, SignalEnvelope, ExecutionIntent, ManagementCommand, BrokerExecutionReport, LifecycleEvent and ContractVersion.
3. Contract records are immutable; the schema audit rejects setters.
4. Broker mutation is not implemented inside Contracts.
5. Transport-only helpers/codecs are explicitly separated from data contracts.
6. Runtime alert identity currently carries SignalId → ScenarioId → PlanId, SourceTimeframe and Revision information.
7. Supplied runtime evidence shows canonical `M15` identity and successful single delivery for the observed BOS/EARLY events.

## Findings

### CFIP-WP06-001 — Contract version is not a general decode invariant
**Severity:** P1
**Owner:** WP-06 / canonical contract codec boundary
**Status:** IMPLEMENTED — verification pending

`ContractVersion.Current` exists, but the codec layer does not establish a single general rule that an incoming payload must carry an accepted contract version before it is materialized.

Required resolution:
- define the accepted-version policy at the canonical codec boundary;
- reject unsupported schema versions deterministically;
- do not add per-consumer version checks.

### CFIP-WP06-002 — ExecutionIntent compatibility path permits null MarketProfile
**Severity:** P1
**Owner:** WP-06
**Status:** IMPLEMENTED — verification pending

`ExecutionIntent` currently disables nullable analysis and has a compatibility constructor path that can pass null for `MarketProfile`.

Required resolution:
- make the actual nullability contract explicit;
- remove or migrate the compatibility path through the canonical owner;
- preserve one canonical construction path.

### CFIP-WP06-003 — SourceTimeframe is an unconstrained string at the shared boundary
**Severity:** P1
**Owner:** timeframe owner / WP-59 + WP-06 boundary integration
**Status:** OPEN

Runtime evidence currently shows canonical `M15`, but the contract type itself cannot reject `M2` or non-canonical spellings.

Required resolution:
- do not introduce a second timeframe engine inside Contracts;
- enforce canonical MTF semantics at the existing timeframe owner;
- make the shared boundary consume that canonical representation;
- WP-59 must provide the global M2 regression guard.

### CFIP-WP06-004 — cBot execution-setting compatibility state remains exposed
**Severity:** P0/P1 architecture boundary
**Owner:** WP-08
**Status:** ALREADY TRACKED

Fields such as IndicatorAutoTradingEnabled / IndicatorAutomaticOrdersEnabled / EffectiveAutoTradingEnabled remain in cBot execution-state compatibility paths.

This is not duplicated as a new defect. It remains DEF-P0-003 and must be resolved by WP-08, where execution authority is cut over to cBot/Contracts.

### CFIP-WP06-005 — Domain-state strings require classification
**Severity:** P2
**Owner:** WP-06 with downstream state owners
**Status:** OPEN / CLASSIFICATION REQUIRED

Several runtime-state fields use strings. Before changing them, each field must be classified as domain state, diagnostic text, or presentation text. Only domain state should receive a constrained canonical type.

## Runtime evidence correlation

The supplied 2026-10-05 runtime evidence confirms:
- Indicator loaded on XAUUSD M15.
- History persistence probe PASS.
- Semantic alert audio delivered successfully.
- BOS and EARLY events carry a shared Signal/Scenario/Plan identity chain.
- 11:50 EARLY event has a new canonical event identity rather than reusing the prior 11:47 event.
- No duplicate delivery is visible in the supplied window.
- Economic-news shared snapshot adoption succeeded.

This is runtime evidence for identity/lifecycle behavior only. It does not close target-terminal acceptance and does not override WP-05 blocking status.

## Ownership decisions

Do **not**:
- add a second timeframe validator to Contracts;
- add a second execution-authority switch;
- add consumer-specific ContractVersion checks;
- duplicate alert identity generation;
- change PlanLabelRenderer/PlanLabelAnchorCalculator geometry.

The existing Left label placement is explicitly outside this work package and must remain unchanged.

## Exit criteria for WP-06 implementation

Before PASS:
1. WP-05 target-terminal evidence is complete.
2. Version acceptance/rejection is centralized in the canonical codec boundary.
3. ExecutionIntent nullability/compatibility construction is canonical.
4. Timeframe representation is proven against the existing canonical MTF owner and WP-59 guard.
5. State-string classification is complete.
6. Full producer/consumer graph is audited.
7. Release build and contract audit pass.
8. CFIP_GATE and CFIP-ROADMAP record exact evidence and next package.
