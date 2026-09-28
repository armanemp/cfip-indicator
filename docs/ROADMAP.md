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
- Bound partial take-profit mutation to the active plan position instead of an arbitrary managed position.
- Added explicit break-even rejection recovery after a confirmed partial close; invalid broker distance is not misclassified as a rejection.
- Made live broker protection validate managed stops against current market price, allowing safe post-entry profit-lock/trailing stops and retrying unresolved protection mutations.

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

Status: complete in this response.

Goal: freeze the architecture without freezing legitimate future extension.

Completed:

- Removed the obsolete empty broker mutation coordinator stub.
- Removed the live SL accessor fallback that could substitute the desired plan stop for missing broker state.
- Hardened live-plan SL hit detection so a missing or unconfirmed broker SL cannot create a synthetic broker exit.
- Made broker-state synchronization validate SL against the current market and TP against entry direction before exposing it as authoritative state.
- Hardened position-open and position-modified lifecycle handlers to classify invalid broker protection as recovery state.
- Hardened live broker protection reconciliation to inspect actual broker SL/TP validity before clearing recovery.
- Reduced redundant broker stop/target mutations during protection reconciliation.
- Added architecture gates that enforce broker-state authority, protection validation, recovery handling and removal of the obsolete stub.
- Updated the architecture and trading-safety documentation with the hardened broker-state rules.
- Added the operator/maintenance guide for repository build, cTrader acceptance and maintenance boundaries.
- Hardened aggressive-entry fill-envelope rejection so an invalid fill requests closure and enters recovery only when the broker close is rejected.
- Made successful execution with failed broker protection visibly enter an explicit `RECOVERY` auto-trading state instead of presenting a clean `EXECUTED` state.

Acceptance:

- Repository source, architecture checks and modular ownership remain intact.
- Current main commit passes the source/architecture, cTrader compile and runtime acceptance workflows.
- Phase 9 remains explicitly open only for hands-on cTrader terminal/broker validation; Phase 10 repository hardening is complete.
## Phase 11 — Deep runtime decomposition and oversized-module audit

Status: complete.

Goal: remove remaining dense runtime orchestration while preserving behavior and enforcing the current production-module ceiling.

Completed:

- Reduced Runtime/Calculation/CalculationCycle.cs to a thin orchestration entrypoint.
- Isolated calculation preparation and early-exit state handling in CalculationPreparation.cs.
- Isolated newly-closed-bar MTF analysis, decision construction, prediction refresh and historical-signal synchronization in CalculationClosedBar.cs.
- Isolated high-confidence, restriction and smart-decision alert state handling in CalculationDecisionAlerts.cs.
- Isolated tick-level reaction, recovery, automatic-plan retry, execution-model refresh, broker synchronization, execution/lifecycle processing and final presentation in CalculationLiveCycle.cs.
- Added static ownership checks requiring the thin Calculate() boundary and its extracted runtime modules.
- Audited all 333 production C# files against the current 20 KiB ceiling; no production file exceeds the limit.
- Corrected architecture documentation so the documented module ceiling matches the enforced verifier ceiling.

Acceptance:

- Calculation-cycle early-return behavior remains preserved, including the MTF-data early exit.
- Calculate() owns orchestration only.
- Extracted calculation owners are explicit and regression-protected by static verification.
- No production .cs file exceeds 20 KiB.

Next: continue the deep oversized-module audit from the remaining near-ceiling planning, analysis, trading and UI files, prioritizing modules whose single method still carries multiple state transitions or concerns.


## Phase 12 — Market-frame decomposition

Status: complete.

Goal: separate market-frame evidence collection from scoring/quality calculation while keeping the market-frame analyzer as a thin composition boundary.

Completed:

- Split market-frame evidence construction from score, direction and quality calculation.
- Kept the existing indicator, structure, liquidity, zone, market-context and OSS evidence formulas unchanged.
- Preserved BUY/SELL symmetry and the existing score weights, thresholds and quality formula.
- Reduced MarketFrameAnalyzer.cs to a thin orchestration owner.
- Added static ownership checks for the three-part market-frame boundary.
- Updated the editing guide and architecture contract to reflect the new ownership.

Acceptance:

- Evidence construction and scoring have separate source-file owners.
- MarketFrameAnalyzer contains orchestration only.
- Existing score/direction/quality behavior remains in the scoring owner.
- No production file exceeds the enforced 20 KiB ceiling.

Next: continue with the largest remaining planning/trading modules, prioritizing PlanBuilder and TargetSelector where selection, validation and state construction are still concentrated.


## Phase 13 — Trade-plan construction decomposition

Status: complete.

Goal: isolate entry/stop/risk preparation, target validation and final Plan materialization without changing trade-plan semantics.

Completed:

- Reduced PlanBuilder.cs to an orchestration owner.
- Isolated execution/entry validation, structural-stop selection, fallback stop handling and risk bounds in PlanInputPreparation.cs.
- Isolated fixed TP1..TP4 stage selection, RR validation, HTF reward requirements and TP1 obstacle checks in PlanTargetPreparation.cs.
- Isolated Plan field construction, RR derivation and target metadata enrichment in PlanMaterialization.cs.
- Preserved fixed stage-slot alignment and existing target/RR validation semantics.
- Added static ownership checks for the trade-plan construction boundary.
- Updated editing ownership documentation.

Acceptance:

- BuildPlan does not directly calculate stops, targets or target metadata.
- Entry, stop, target selection and materialization have explicit owners.
- Requested/ideal/actual entry, stop and TP stages remain distinct.
- No production file exceeds 20 KiB.

Next: audit TargetSelector itself and then the remaining large trading/validation modules, with special attention to duplicated target-obstacle and reward-path rules.


## Phase 14 — Target selection decomposition

Status: complete.

Goal: separate stage orchestration from target-selection policy and candidate eligibility/scoring while preserving fixed TP1..TP4 slot semantics.

Completed:

- Reduced TargetSelector.cs to stage orchestration.
- Isolated progressive RR requirements, HTF-reward stage policy and previous-stage lookup in TargetSelectionPolicy.cs.
- Isolated candidate validity, age, HTF quality, RR, extension, spacing, obstacle/path checks and target scoring in TargetCandidateEvaluator.cs.
- Preserved four fixed target slots with null gaps and stage-index trustworthiness.
- Preserved existing candidate score, HTF/liquidity/zone bonuses, hit weighting, nearest bias and stage weighting.
- Added static ownership checks for target selection.
- Updated editing ownership documentation.

Acceptance:

- TargetSelector owns stage iteration only.
- Candidate filtering/scoring is owned by one module.
- Stage policy is owned by one module.
- No target-selection logic is duplicated in PlanBuilder.

Next: audit the remaining near-ceiling structural/planning module OrderBlockCandidateBuilder, then inspect reward-path validation for overlap with target obstacle logic.


## Phase 15 — Order-block candidate decomposition

Status: complete.

Goal: separate order-block geometry, impulse/structure evidence, mitigation and quality calculation while preserving candidate semantics.

Completed:

- Reduced OrderBlockCandidateBuilder.cs to candidate orchestration.
- Isolated displacement and structure-break evidence in OrderBlockEvidenceBuilder.cs.
- Isolated managed-zone mitigation and remaining-width checks in OrderBlockMitigationGuard.cs.
- Isolated order-block quality calculation in OrderBlockQualityCalculator.cs.
- Preserved the caller-supplied ATR for displacement and impulse-quality calculations.
- Preserved liquidity-sweep and FVG-confluence ownership in OrderBlockConfluenceAnalyzer.cs.
- Added static ownership checks for order-block construction.
- Updated editing ownership documentation.

Acceptance:

- Candidate construction has explicit geometry, evidence, mitigation and quality owners.
- Existing direction, break, mitigation, confluence and quality rules remain unchanged.
- No production file exceeds 20 KiB.

Next: inspect Trading/Validation/RewardPathValidation.cs together with target obstacle consumers to remove overlapping reward-path rules without changing trade rejection behavior.


## Phase 16 — Reward-path validation decomposition

Status: complete.

Goal: separate reward-path geometry, local obstacle scanning, target obstacle checks, higher-timeframe path validation and HTF target presence without changing rejection semantics.

Completed:

- Removed the monolithic RewardPathValidation.cs owner.
- Isolated opposing-zone reward-path scanning in RewardPathZoneObstacleScanner.cs.
- Isolated path intersection semantics in RewardPathGeometryRule.cs.
- Isolated swing/equal-level target obstacle checks in TargetObstacleValidator.cs.
- Isolated M15/M30/H1/H4 reward-path traversal in HigherTfRewardPathValidator.cs.
- Isolated HTF-target presence validation in HtfTargetPresenceValidator.cs.
- Preserved the existing obstacle clearance, FVG, Order Block and equal-high/low rules.
- Added static ownership checks and updated editing ownership documentation.

Acceptance:

- Reward-path rules have explicit owners with no monolithic implementation file.
- Target selection consumes the same obstacle authorities rather than duplicating their formulas.
- Existing trade-rejection behavior remains unchanged.
- No production file exceeds 20 KiB.

Next: continue the remaining near-ceiling audit, prioritizing Trading/LiveManagement/TargetProgression.cs and the automatic-market pre-trade/execution modules before the final hands-on cTrader validation pass.


## Phase 17 — Live target progression decomposition

Status: complete.

Goal: separate live target candidate eligibility/scoring from stage orchestration and isolate plan RR recalculation.

Completed:

- Reduced TargetProgression.cs to live target stage/state orchestration.
- Isolated live candidate eligibility, spacing, obstacle checks and scoring in LiveTargetCandidateEvaluator.cs.
- Isolated TP1..TP4 RR recomputation in PlanRiskRewardRecalculator.cs.
- Preserved unhit-target-only behavior, monotonic spacing, maximum RR, HTF/liquidity filters and obstacle rejection.
- Preserved target metadata refresh and HTF target counting after successful repricing.
- Added static ownership checks and updated editing ownership documentation.

Acceptance:

- Live target progression keeps a single orchestration owner.
- Candidate scoring/filtering has one owner.
- Plan RR derivation has one owner.
- No duplicate target progression authority is introduced.
- No production file exceeds 20 KiB.

Next: audit AutomaticMarketPreTrade.cs and AutomaticMarketBrokerExecution.cs as a single execution chain, preserving broker mutation ownership and rejection/recovery semantics.
