# CFIP Indicator — Modular Architecture

## Authority chain

Market data -> MTF context -> indicators -> analysis -> decision -> entry/trigger -> trade plan -> risk -> execution -> broker -> lifecycle -> live management -> outcome -> presentation.

## Module rules

The source tree is organized by responsibility and artifact ownership.

- Every native technical indicator has its own source file.
- Every external/OSS indicator adapter has its own source file.
- Every market, regime, structure, zone, liquidity and reaction analyzer has a dedicated owner.
- Every decision concern has a dedicated owner.
- Every execution mode and execution validation concern has a dedicated owner.
- Every lifecycle event handler has its own source file.
- Every renderer has its own source file.
- Every domain model has its own source file.
- Every enum has its own source file.
- Every public cTrader parameter group has its own source file under `Indicator/Parameters/`.
- Production source files do not contain historical version identifiers.

The cTrader host remains a single `CFIPIndicator` partial type solely for platform compatibility. Partial files are implementation units, not alternative engines.

### Primitive indicator mathematical ownership

Standard ATR, ADX/DMI, EMA and RSI calculations are delegated to the cTrader
native indicator boundary; production code does not maintain a competing
second implementation of those standard formulas.

CFIP-specific primitive transformations have explicit pure owners under
Core/Math:

- DmiBiasRule;
- MacdBiasRule;
- RangeEfficiencyRule;
- ChoppinessIndexRule;
- VwapBiasRule;
- VolumeExpansionRule.

Analyzers perform only data-window selection and platform data access, then
delegate arithmetic/normalization to those owners. This keeps measurement
logic separate from platform adapters and prevents a second formula authority
from appearing in downstream decision or execution code.


### OSS numerical and cache ownership

Production Skender 2.7.3 remains the single OSS numerical implementation boundary.
Fixed Skender settings are owned by `OssIndicatorSettings.Default`; configured
RSI/MACD periods and minimum-history semantics remain in
`OssIndicatorParameters`.

`OssIndicatorWarmupPolicy` owns the bounded 768-bar stable window for
path-dependent adapters. The existing 161-bar rolling window remains the
bounded input for window-local adapters.

`OssQuoteWindowRule` owns first-index, window-count and rebuild decisions.
`OssQuoteProjectionRule` owns finite/non-negative quote-volume normalization.
`OssQuoteSeriesCache` owns the adapter-local cache instance, while
HistoryLoaded/Reloaded events are the explicit history-replacement invalidation
boundary.

Stable adapter outputs are allowed documented recursive convergence error after
bounded truncation; CI compares finite outputs, directional classification and
publishes max/mean/RMS error. Window-local adapter outputs are expected to match
the full-prefix last value within the deterministic 1e-12 gate.

Zero-volume source observations remain zero-volume. They are never converted to
an artificial unit weight.



### Indicator fusion and evidence-independence ownership

`IndicatorEvidenceFusionRule` is the single numerical owner for indicator-derived directional bonus, confluence quality and conflict.

`IndicatorEvidenceIndependenceRule` is diagnostic/provenance only. It groups correlated measurements into Trend, Momentum and Context. Divergence is a modifier and aggregate OSS consensus is not an independent vote.

Parallel timeframe scenario enrichment reuses the canonical per-frame evidence owner and must not maintain a second raw boolean counter.

Indicator-group count is provenance only; existing decision quality, confidence and actionability authorities remain unchanged.
### FVG lifecycle ownership

FVG geometry remains owned by `FvgRule`; bounded source age, body/wick mitigation
probe semantics and the full-fill retention/invalidation decision are owned by
platform-neutral `FvgLifecycleRule`. `FvgLifecycleAnalyzer` remains an orchestration
boundary that materializes the managed `Zone` after the canonical lifecycle
transition. All FVG consumers (detection, predictive pending, OB/FVG confluence
and reward-path obstacles) therefore consume the same geometry and lifecycle
semantics rather than reimplementing fill or age rules.

### Structural event and alert-delivery ownership

Canonical swing plateaus are owned by `SwingPlateauRule`. Structure, MSS and
CHOCH consume confirmed swing state through `StructuralEventRule`; a break is
fresh only while no earlier closed bar after swing confirmation has already
crossed the same structural threshold. `StructuralEvidenceRule` collapses
same-causal Structure/MSS/CHOCH labels so they cannot become duplicate evidence
or duplicate user-facing structural alert events.

Liquidity sweeps consume the canonical confirmed swing and
`LiquiditySweepRule.IsActiveUnbrokenLevel`; prior closes can invalidate a
liquidity level before the sweep bar, preventing stale-level reuse.

`AlertDeliveryQueue` is the single bounded runtime queue for audible and panel-alert
delivery. `AlertEngine` creates one event and never directly plays a sound or
renders a popup. Calculation and timer boundaries drain the same queue. The
delivery processor updates the panel alert rail first and then emits the sound cue,
keeping the two user-facing channels on one event boundary.
Platform sound-type resolution occurs only at that cTrader delivery boundary.

Explicit entry-restriction notifications are diagnostic alerts, not trade/signal
creation; they remain subject to their configured alert controls. A blocked
candidate still cannot create an actionable signal or trade side effect.

### Canonical calculation-market context

The calculation runtime owns one `CalculationMarketContext` per live calculation
cycle. It composes the existing `MtfClosedContext` with one
`CanonicalPriceSnapshot`.

`CanonicalMarketContextBuilder.cs` is the cTrader-to-core adapter for current
Bid/Ask, executable BUY/SELL prices, spread, pip/tick scale, digits, broker
minimum-distance metadata and terminal observation time. Downstream planning and
execution consumers must read this context rather than reconstruct the same
quote semantics independently.

The context is refreshed independently of readiness-probe throttling. Therefore
closed-bar/MFT cache stability does not imply quote stability, and a stable
closed-bar decision may still be evaluated against the latest observed
executable quote at the live execution boundary.

The quote observation timestamp is intentionally distinct from the signal
reference timestamp. `Server.TimeInUtc` is recorded as terminal observation
time; it is not represented as an exchange-event timestamp.

## Runtime authority

The calculation entrypoint is a thin orchestration boundary. Calculate() delegates preparation, newly-closed-bar analysis, decision alerts, live-cycle state/execution, and final presentation to dedicated runtime modules. It must not contain analytical, execution, lifecycle, or rendering business logic directly.

There is exactly one decision authority, one strategy state, one managed broker identity and one automatic execution authority.

The presentation layer consumes authoritative state and never decides whether a trade should exist. Broker mutations are isolated to the Trading/Execution boundary. Live broker protection state is accepted only after direction/current-market validation; a desired plan SL never substitutes for a missing or invalid broker SL. Direct cTrader mutation APIs are permitted only in explicit broker mutation owner files; execution, pending and lifecycle modules consume those owners and broker-confirmed results.

## Non-negotiable invariants

- Analytical decisions use fully closed-bar references at one UTC reference.
- Entry, trigger, requested entry and actual fill remain distinct.
- Broker state is authoritative after mutations.
- Automatic market execution and automatic pending orders share one strategy identity.
- M1 trigger evidence is confirmation-only and cannot create directional consensus.
- When enabled, M1 TriggerReady is evaluated from a fully closed M1 bar inside the exact selected closed M5 window and is combined with the canonical M5 trigger.
- Execution capacity is explicitly single-plan: a managed open position blocks creation of another managed plan.
- Execution capacity is explicitly single-plan: `ExecutionCapacityRule` is the semantic owner and `ExecutionCapacityGuard` maps broker state to that rule.
- No configurable multi-position mode is advertised while the execution architecture remains single-plan.
- Manual BUY/SELL/order-entry controls do not exist.
- SL changes are protective-only.
- Partial closes and closes are consumed only after broker confirmation.
- Telemetry timeout never transfers or ends broker/lifecycle ownership.
- CFIP-specific structure, confluence, risk and execution semantics remain authoritative.
- Historical version/source identifiers are documentation-only and never production runtime identifiers.

## OSS boundary

External analytical libraries are optional adapters and benchmarks. They may extend numerical indicators or provide cross-checks, but they cannot become a second decision or trading engine.

All OSS assets, adapter notes, license information and benchmark material belong under `oss/`. Production adoption requires target-runtime compatibility and explicit verification.

## Dependency direction

Core models and utilities are platform-neutral. cTrader-specific Bars, native-indicator instances, broker types and chart controls live in their owning analysis/trading/UI layers.

Core models may describe strategy state, but must not require the cTrader API merely to exist.

Analysis may consume market data and configuration, but must not mutate broker state.

Planning may create plans/intents, but must not submit or mutate broker objects. Plan construction, target selection, stop planning and trigger evaluation are separate owners. Risk policy, sizing, margin safety and market suitability are separate from plan construction and broker mutation.

The broker mutation boundary owns only broker API mutations. Executable plan preparation and structural target/stop rebuilding are outside that boundary. Broker-confirmed positions and pending orders are authoritative; rejected or incomplete mutation results are never adopted as live broker state.

Trading may execute broker mutations, but must not invent alternative analytical authority.

Presentation consumes state and renders it; it does not decide or trade.

## Decision service boundary

Decision input snapshots are immutable data carriers. Pure decision services perform no broker mutation, chart mutation or executable callback dispatch. Direction-specific evidence is captured for both BUY and SELL before consensus selection so neutral states cannot inherit directional evidence. Decision filtering is an ordered pipeline of narrow gate owners; the pipeline itself only orchestrates gate results.

## File ownership

A source file should contain one primary production artifact and one coherent responsibility. Domain types, enums, indicators, analyzers, execution policies, lifecycle handlers, renderers and adapters are separated into dedicated files.

The remaining cTrader partial host files are permitted only for platform callbacks or a single cohesive behavior boundary. They must not hide nested helper types or unrelated responsibilities.

## Production source hygiene

Production `.cs` files must not contain:

- release/version suffixes;
- historical source class names;
- migration aliases;
- compatibility names for obsolete versions;
- numbered execution comments/identifiers.

Historical baseline/version information is allowed only in `docs/ROADMAP.md` or workflow material when it is necessary for continuity.

## File-size rule

All production `.cs` files must remain below 20 KiB. The runtime Calculate() entrypoint is additionally constrained to a thin orchestration boundary by static verification.

## Presentation boundary

Chart object creation/removal and visual renderers live only under UI modules. Trading and intelligence modules may request presentation through authoritative state, but they do not own chart mutations.

Signal visual lifecycle is owned by the platform-neutral `SignalVisualLifecycleRule`: pre-trade plans and setup previews are bounded by current direction/actionability and a finite closed-M5 age. Legacy alert-mirror text is cleanup-only and cannot become a second signal authority. Blocked/restricted candidates do not produce user-facing alert side effects.

## Broker boundary

Broker mutation calls are restricted to the execution, pending, lifecycle and live-management boundaries.

## Decision ownership

- Input/evidence snapshot: Analysis/Market/Decision/DecisionInput*.cs and DecisionEvidenceSnapshot.cs.
- Score, consensus, quality and confidence: dedicated Decision*Calculator.cs services.
- Decision threshold and smart consensus: dedicated pure filter evaluators.
- Confirmation, smart, structure, market and lifecycle gates: dedicated gate modules.
- Reason formatting: DecisionReasonFormatter.cs; DecisionReasonBuilder.cs only composes the formatter.
- Intelligence helpers remain single-purpose modules and do not become a second decision authority.

## Planning and risk boundary

Planning owns executable trade intent as data, not broker submission. Entry/trigger, target, stop and plan-integrity concerns are isolated. Risk owns exposure policy, volume sizing, margin safety, daily-loss protection and market suitability; it does not create or mutate broker positions/orders.

## Core boundary

Core contains domain models, enums, numeric guards, text helpers and generic time-window parsing only. cTrader-dependent index and broker-price helpers live outside Core.


## Phase 9.17 exit/protection boundary

Exit geometry is centralized in `Core/Math/LiveExitGeometryRule.cs`. Planning creates the
initial ladder, live management may advance only forward, and broker protection remains the
only mutation authority. Actual fills are reconciled through a transactional live-aware
exit reconciler before plan state is committed. Server-side Advanced Protection remains
broker-owned and is synchronized only with forward/progressive targets.


## Active three-project boundary — 2026-10-02

The repository now contains the active target project boundary:

- `src/CFIP.Indicator` — analysis, decision, scenario, plan and presentation;
- `src/CFIP.Contracts` — platform-neutral immutable cross-boundary data only;
- `src/CFIP.cBot` — broker execution, account risk, live protection, lifecycle and recovery.

During migration, existing broker mutation inside the Indicator is temporary compatibility only. Each migrated owner must be replaced and parity-verified before its Indicator implementation is physically removed. The cBot may reference the installed CFIP custom Indicator only through cTrader's supported custom-indicator reference mechanism; it must not access Indicator private state, scrape chart objects, use reflection or introduce a second decision engine.

The active migration schedule is CBOT-P0→P8 and runs in parallel with M2 onward. `docs/CBOT-P0-EXECUTION-DEPENDENCY-CLOSURE.md` is the exact current execution extraction inventory.


## Canonical Indicator ↔ cBot contract ownership — 2026-10-02

`src/CFIP.Contracts` is the single cross-project data boundary. The existing Indicator-side `Plan` and `ExecutionIntent` types are internal production models and are not alternate cross-project contracts.

Mapping for the upcoming provider migration:

- `cAlgo.Plan` → `CFIP.Contracts.PlanSnapshot`
- `cAlgo.ExecutionIntent` → `CFIP.Contracts.ExecutionIntent`
- signal trace / scenario / plan lineage → `CFIP.Contracts.ContractIdentity`
- lifecycle transitions → `CFIP.Contracts.LifecycleEvent`
- broker-confirmed execution facts → `CFIP.Contracts.BrokerExecutionReport`
- requested live management actions → `CFIP.Contracts.ManagementCommand`

P2 must expose these canonical Contracts read-only from the Indicator. cBot must never reconstruct them from chart objects or private Indicator state. After P2/P3 parity, the temporary internal models can be reduced or removed as their callers are migrated.


## M2 micro-precision contract — 2026-10-04

M2 (2-minute) is a dedicated micro-precision evidence layer. cTrader officially exposes `TimeFrame.Minute2` / m2, so CFIP loads it through the same platform boundary as the other native timeframes. citeturn1search0

Ownership is intentionally constrained: M2 has one closed-index source (`MtfClosedContext.M2`), one frame (`_m2Frame`) and one semantic evaluator (`M2PrecisionRule`). It may report micro alignment/conflict against the canonical M5 frame, but it does **not** vote in the directional decision, does not become an execution clock, and does not replace M5 trigger/entry precision ownership. This prevents M2 from becoming a competing decision engine.

The panel exposes M2 through the same `PanelTimeframePresentationState` path as the other timeframe rows and marks its state as MICRO-ALIGNED, MICRO-CONFLICT or WAIT. No second panel renderer was introduced.
