# CFIP Indicator — Implementation Roadmap

## Working rule

One implementation response completes exactly one phase. A phase is complete only when its source changes, static verification, documentation, and available CI checks are updated.

When work resumes in a new chat, read this file first, then `docs/ARCHITECTURE.md` and `docs/WORKFLOW.md`. Continue from the first phase marked `next`; do not repeat completed phases.

## Behavioral baseline

The complete historical behavioral reference is the v73 source material used for parity. It is reference material only; no historical source file or versioned production identifier belongs in the production tree.

Current baseline guarantees:

- 513 behavioral configuration parameters are preserved.
- 311 reference methods are preserved across the modular source tree.
- One cTrader host.
- One decision authority.
- One strategy state.
- One managed broker identity.
- One automatic execution authority.
- Automatic market execution and automatic pending orders are retained.
- Manual trade-entry controls are absent.
- Production source files remain below the repository file-size limit.
- OSS components live under `oss/` and are admitted only through explicit compatibility, license, and benchmark gates.

## Phase 1 — Canonical source hygiene and architecture contract

Status: complete in this response.

Deliverables completed:

- Remove versioned product/source identifiers from production code.
- Replace versioned execution comments with a centralized, version-neutral trade metadata owner.
- Align architecture verification with the canonical managed trade label.
- Add strict production-source residue detection for version-style identifiers.
- Keep historical version details restricted to this roadmap/workflow documentation.
- Preserve the separate `oss/` boundary.
- Keep the CI source/architecture gate as the first protection against regression.

Acceptance:

- No `v<number>`, `Clean<number>`, versioned CFIP strategy identifier, or numbered trade comment remains in production source.
- Broker identity label is stable and version-neutral.
- Architecture verification matches the current source tree.

## Phase 2 — Atomic indicators and analysis modules

Status: complete.

Goal: every indicator, analyzer, scorer, detector, model, and helper has one clear file owner and one responsibility.

Work:

1. Audit every `Analysis/Indicators` file and keep exactly one indicator implementation per file.
2. Audit native and optional OSS indicator adapters separately.
3. Split any remaining multi-concern market analyzers into:
   - market context;
   - regime detection;
   - volatility;
   - trend;
   - momentum;
   - volume;
   - indicator confluence;
   - decision inputs.
4. Split structure into explicit owners for:
   - swing structure;
   - BOS;
   - MSS/CHOCH;
   - displacement;
   - liquidity;
   - equal highs/lows;
   - FVG;
   - Order Block;
   - supply/demand;
   - mitigation;
   - zone lookup/selection.
5. Preserve CFIP-specific semantics and closed-bar MTF behavior exactly.
6. Remove helper clusters that only exist because of the old monolithic source.
7. Add deterministic numerical fixtures and symmetry checks for BUY/SELL.

Completed in this phase:

- Split market context behaviors into dedicated analysis owners for volume expansion, MACD bias, VWAP bias, healthy volatility, premium/discount and live bias.
- Split market-frame scoring from frame construction and moved decision reason formatting into the decision boundary.
- Split liquidity analysis into liquidity-sweep, swing-point and equal-level owners.
- Split FVG detection/selection from FVG lifecycle/mitigation behavior.
- Split Order Block confluence helpers from the primary Order Block analyzer.
- Removed the obsolete monolithic analyzer files and preserved the same method semantics through partial-host ownership.

Acceptance:

- One artifact/responsibility per file.
- No analyzer owns unrelated analysis domains.
- No duplicate indicator or structure logic.
- All source owners are recorded in the editing guide.

## Phase 3 — Decision and intelligence services

Status: complete.

Goal: turn analysis outputs into immutable decision inputs and deterministic decision services.

Work:

- Separate evidence collection, weighting, consensus, confidence, edge, regime quality, adaptive thresholds, and decision filtering.
- Keep prediction separate from confirmed decision.
- Keep outcome telemetry and calibration as observation services, not decision authorities.
- Add explicit immutable input/output contracts.
- Add deterministic fixture tests and contradiction tests.

Completed in this phase:

- Replaced callback-bearing decision inputs with an immutable directional evidence snapshot.
- Split decision score, consensus/share conversion, quality and confidence calculations into dedicated deterministic services.
- Isolated empirical confidence calibration and higher-timeframe conflict penalties.
- Split decision evidence collection into timeframe agreement, independent evidence, structural confirmation and direction acceptance owners.
- Split decision filtering into threshold, confirmation, smart, structure, market and lifecycle gate owners.
- Split intelligence helpers into fresh-trigger, structural-sequence, entry-location, expected-value and no-trade-regime owners.
- Removed the multi-concern decision feature module and duplicate reason-formatting implementation.
- Added deterministic C# contract fixtures and CI execution for them.

Acceptance:

- One decision authority.
- Same input snapshot produces the same decision.
- No UI/broker dependency in pure decision services.

## Phase 4 — Planning and risk

Status: complete.

Goal: isolate trade planning from execution and risk side effects.

Work:

- Entry zone selection.
- Trigger logic.
- Execution intent.
- Structural invalidation.
- Structural SL.
- Target source interfaces.
- Target aggregation.
- Target classification.
- Target progression.
- RR validation.
- Risk sizing.
- Margin safety.
- Daily loss guard.
- Market suitability.
- Spread/session/event/volatility guards.

Completed in this phase:

- Split plan construction, plan integrity and target progression into explicit owners.
- Split structural stop planning and minimum required RR.
- Split target-level construction, candidate merging, target selection, stage selection, target metadata and HTF source classification.
- Split closed-bar trigger readiness and bullish/bearish trigger scoring.
- Split risk controls into daily loss, risk policies, margin safety, volume sizing, position counting and auto-plan validation.
- Split suitability/session/risk-scaling guards into dedicated owners.
- Added pure target progression, risk percent, risk amount and margin usage policies.
- Added deterministic planning/risk contract fixtures to CI.
- Removed empty legacy planning/risk parent modules.

Acceptance:

- Requested entry, trigger, actual fill, SL, TP and broker protection are distinct values/states.
- SL is protective-only.
- Target progression is monotonic and path-safe.

## Phase 5 — Automatic trading, pending orders and lifecycle

Status: complete.

Goal: isolate every broker-facing behavior while keeping one mutation boundary.

Work:

- Automatic market execution.
- Aggressive execution.
- Continuation stop placement.
- Reversal limit placement.
- Broker mutation coordinator.
- Broker identity.
- Position/pending reconciliation.
- Lifecycle handlers.
- Missing-protection recovery.
- Partial close.
- Break-even.
- Dynamic target progression.
- Reversal/exhaustion/invalidation handling.
- Restart/reconnect adoption.

Completed in this phase:

- Centralized market-order, pending-order, stop-loss, take-profit, position-close and protection mutations into explicit broker mutation owners under Trading/Execution.
- Removed direct broker mutation calls from market execution, aggressive execution, pending placement and lifecycle orchestration callers.
- Moved executable plan reconstruction out of the broker mutation boundary.
- Split execution runtime state into auto-trading state, lifecycle state, target-stage state and trade-label formatting owners.
- Split broker reconciliation and lifecycle recovery helpers into dedicated modules.
- Split pending-fill plan construction from pending-fill protection recovery and event orchestration.
- Added deterministic broker confirmation/adoption policy and execution contract fixtures.
- Added static CI enforcement that broker mutation APIs cannot escape the approved mutation owners.

Acceptance:

- Broker-confirmed state is authoritative.
- Rejected mutations never become synthetic state.
- Pending order is never treated as a position before broker confirmation.
- Exactly one broker mutation boundary exists.

## Phase 6 — Presentation and UI

Status: complete.

Goal: presentation becomes a pure consumer of authoritative state.

Work:

- Chart object cleanup.
- Signal rendering.
- Plan lines.
- Plan labels.
- Prediction rendering.
- Pending-order rendering.
- Outcome markers.
- Historical rendering.
- Panel layout.
- Panel semantic sections.
- Theme/visual settings.
- Popup.
- Execution controls.

Completed in this phase:

- Split plan-line rendering into coordinator, line renderer, object clearer/remover owners.
- Split plan-label rendering into coordinator, anchor calculator, renderer and remover owners.
- Split popup rendering, expiration cleanup and removal into explicit owners.
- Moved execution controls under the UI boundary.
- Kept chart/panel/popup modules free of broker mutation and decision-gate authority.
- Added static CI enforcement for UI authority boundaries.
- Preserved the existing semantic panel sections and chart render responsibilities without introducing a second source of trading truth.

Acceptance:

- UI never decides whether a trade should exist.
- UI never mutates broker state directly.
- Each renderer has one file owner and one rendering responsibility.

## Phase 7 — OSS research, adapters and benchmarks

Status: complete.

Goal: use strong OSS where it materially improves numerical analysis without
importing a second trading engine.

Completed in this phase:

- Formalized Skender.Stock.Indicators 2.7.3 as the current production OSS numerical dependency because its .NET Standard 2.0 asset is compatible with the net6.0 cTrader target.
- Kept FacioQuo.Stock.Indicators 3.0.1 in an isolated .NET 8 benchmark project because the current v3 package line cannot be consumed by the net6.0 production target.
- Added deterministic v2/v3 numerical parity checks for RSI, MACD histogram, Bollinger %B, MFI, Stochastic K/D and SuperTrend.
- Added complete-series coverage checks for all 10 OSS indicators used by the production confluence adapter.
- Added batch-performance measurements to the OSS benchmark.
- Added a source-bar-aware OSS snapshot cache so repeated calculations for an unchanged bar reuse the complete confluence result instead of recalculating the indicator suite.
- Removed the redundant symmetric flag from the OSS directional-vote helper.
- Updated the OSS boundary, package register and benchmark documentation so production/research dependency status is explicit.
- Added verifier checks preventing the research-only v3 package from leaking into production source and requiring both package pins in the benchmark project.

Acceptance:

- Every current production OSS component has an upstream source, exact package version, license/attribution, target-runtime compatibility evidence and an isolated adapter owner.
- Deterministic numerical parity and extended coverage are enforced by the OSS benchmark.
- Performance is measured by the benchmark.
- No OSS trading engine becomes a second decision, risk or execution authority.
- Production cTrader build remains dependency-minimal and net6-compatible.
- v3 migration remains blocked until a maintained package line can target the actual production runtime.

## Phase 8 — Static verification and contract testing

Status: complete.

Goal: make architectural and behavioral drift mechanically detectable.

Completed in this phase:

- Extended the architecture verifier with explicit Phase 8 contract-fixture and analysis/planning boundary checks.
- Added deterministic BUY/SELL consensus symmetry and confidence repeatability checks.
- Extracted platform-neutral SL/TP directionality and minimum-distance invariants into `PriceProtectionRule` and wired production validation to it.
- Added lifecycle transition invariants as an explicit policy and executable contract fixture, including safe same-state idempotency and blocked terminal misuse.
- Added a real one-shot `LifecycleEventIdempotencyGuard` and integrated it into position-open, pending-create, pending-fill, pending-cancel and position-close handlers.
- Kept legitimately repeatable position/pending modification events outside the one-shot guard.
- Reused the existing decision/planning/execution contract projects so the invariants run inside the normal cTrader CI workflow.

Acceptance:

- CI rejects architectural regression before cTrader testing.
- Static checks cover the same boundaries documented in the architecture.
- BUY/SELL symmetry, SL/TP directionality, lifecycle transitions and duplicate-event behavior are executable contracts.

## Phase 9 — cTrader compile and runtime acceptance

Status: repository acceptance complete; live cTrader validation required.

Goal: prove the finished source on the target cTrader environment.

Repository acceptance completed in this phase:

- Extended the cTrader CI workflow with a dedicated runtime-acceptance contract executable.
- Added deterministic contracts for MTF context integrity, market/pending broker confirmation, rejection handling, fill-envelope symmetry, initial SL/TP directionality, managed break-even protection, monotonic target progression, lifecycle flows, and lifecycle event idempotency.
- Separated initial protective-stop validation from post-entry managed-stop validation so break-even/profit-lock stops can move into protected profit while never crossing the current market.
- Fixed normal automatic-market rejection handling so broker error information is preserved instead of being replaced by a misleading null-result state.
- Kept broker-confirmed state as the only accepted execution state.

Hands-on cTrader acceptance remains the only open part of this phase and requires the actual target terminal and broker session. Repository CI cannot reproduce the cTrader chart UI, live broker server, order-fill timing, slippage, reconnection, or terminal resource profile. Those checks remain explicitly required before this phase can be marked complete.

Work:

- Compile against the target installed Automate API.
- Verify all relevant chart timeframes.
- Verify closed-bar MTF synchronization.
- Verify chart/panel/popup rendering.
- Verify automatic market execution.
- Verify pending orders.
- Verify rejection/slippage behavior.
- Verify protection recovery.
- Verify partial close and break-even.
- Verify restart/reconnect reconciliation.
- Verify reversal, invalidation and end-of-day handling.
- Review runtime memory/allocation behavior.

Acceptance:

- No known compile errors.
- No known runtime authority violations.
- All critical trading scenarios pass controlled acceptance.

## Phase 10 — Final hardening

Goal: freeze the architecture without freezing legitimate future extension.

Work:

- Remove only proven inefficiencies.
- Remove dead code and duplicate helpers.
- Freeze module ownership boundaries.
- Verify OSS licenses and attribution.
- Ensure production source contains no historical/version residue.
- Keep historical version details only where required for roadmap/workflow continuity.
- Produce final operator and maintenance documentation.
