# CFIP Indicator — Roadmap

## Working rule

Each phase is intentionally scoped so it can be completed in one implementation response. A phase is marked complete only after source changes, static checks, and documentation for that phase are updated.

## Baseline

The complete behavioral reference remains the v73 source material used to establish feature and method parity. The production tree no longer uses the historical source as an implementation file.

Baseline guarantees:

- 513 configuration parameters preserved.
- 311 reference methods preserved across the modular source tree.
- One cTrader host.
- One decision authority.
- One broker identity.
- One automatic execution authority.
- Automatic market execution and automatic pending orders retained.
- Manual trade-entry controls absent.
- Production source files constrained below 64 KiB.
- OSS components isolated under `oss/` and admitted only through explicit review.

## Phase 1 — Repository and artifact modularization

Status: complete.

Completed:

- One native technical indicator per source file.
- One market/structure/zone/liquidity analyzer per source file or cohesive analyzer boundary.
- One domain model per source file.
- One enum per source file.
- cTrader parameter groups split under `Indicator/Parameters/`.
- Runtime MTF context and cache-support types extracted.
- Duplicate monolithic source paths removed.
- Production file-size limit established.
- OSS boundary established.

Acceptance:

- No duplicated production engine.
- No versioned production file/class naming.
- One source owner for every declared responsibility.

## Phase 2 — Decision service isolation

Status: complete in this implementation response.

Completed:

- Decision evaluation moved behind an explicit `DecisionEngine` service.
- Decision input construction moved behind `DecisionInputSnapshotFactory`.
- Weighted frame contribution calculation moved behind `DecisionFrameContributionCalculator`.
- Human-readable decision reason construction moved behind `DecisionReasonBuilder`.
- cTrader host now performs only state adaptation and decision filtering around those services.
- Existing decision mathematics and thresholds remain behaviorally equivalent to the reference implementation.

Acceptance:

- Decision services compile without requiring chart controls or broker mutation APIs.
- Decision evaluator remains deterministic for the same immutable input snapshot.
- No second decision authority introduced.

## Phase 3 — Indicator and numerical analysis layer

Status: next.

Scope:

- Keep every indicator implementation in its own file.
- Standardize indicator input/output contracts.
- Add health and availability metadata.
- Build numerical fixture tests for the native and optional OSS indicators.
- Benchmark optional OSS indicator libraries before any production dependency is admitted.
- Keep zero-valued observations distinct from missing observations.
- Add only indicators that improve independent evidence, regime detection, momentum, volatility or trend interpretation.

OSS gate:

- Direct runtime use is allowed only when the library is compatible with the target cTrader/.NET runtime.
- Otherwise use an offline benchmark or source adapter.
- No OSS trading engine is embedded into the indicator.

## Phase 4 — Market, structure and intelligence service isolation

Scope:

- Split market context, regime, structure, FVG, Order Block, liquidity and reaction services into explicit service boundaries.
- Remove remaining large partial-method clusters.
- Replace host field reads with explicit snapshots/contracts where practical.
- Keep CFIP-specific FVG/OB/liquidity semantics authoritative.
- Preserve MTF closed-bar synchronization.

## Phase 5 — Planning and risk isolation

Scope:

- Separate entry selection, trigger gating, execution intent, structural stop, target sources, target classification, target selection and RR validation.
- Separate risk sizing, margin protection, daily loss limits and market-suitability policy.
- Keep requested entry, trigger, actual fill, structural stop and broker protection distinct.
- Add stronger invariant tests for SL/TP monotonicity and reward-path safety.

## Phase 6 — Trading, lifecycle and automatic execution isolation

Scope:

- Separate automatic market execution, aggressive execution, pending placement, broker mutation coordination, identity, reconciliation, lifecycle handlers and live management.
- Keep exactly one broker mutation boundary.
- Verify partial close, break-even, dynamic target progression, reversal, invalidation, protection recovery and reconnect behavior.
- Preserve the rule that broker-confirmed state is authoritative.

## Phase 7 — Presentation isolation

Scope:

- Separate chart object cleanup, signal rendering, plan lines, labels, prediction rendering, pending-order rendering, outcome markers, historical rendering, panel sections, panel layout/theme, popup and execution controls.
- Presentation consumes authoritative state only.
- No UI component may create or cancel trades.

## Phase 8 — Verification and scenario hardening

Scope:

- Static architecture verification.
- Full source-parity verification against the behavioral reference.
- Deterministic unit and contract tests.
- Controlled scenario tests for every automatic trading path.
- Negative-path tests for rejection, slippage, duplicate events, missing protection and invalid state transitions.
- Resource and allocation review.

## Phase 9 — cTrader acceptance

Scope:

- Compile against the target installed cTrader Automate API.
- Validate the target cTrader build.
- Verify all timeframes and MTF behavior.
- Verify panel, chart and popup rendering.
- Verify automatic market and pending execution.
- Verify broker lifecycle and recovery in real cTrader scenarios.

## Phase 10 — Final hardening

Scope:

- Remove proven inefficiencies only.
- Freeze source boundaries.
- Verify OSS attribution and license records.
- Ensure no historical version residue remains in production source.
- Keep historical reference/version details only in roadmap/workflow documentation.
- Produce final operator and maintenance documentation.
