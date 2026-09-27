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

Status: next.

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

Acceptance:

- One artifact/responsibility per file.
- No analyzer owns unrelated analysis domains.
- No duplicate indicator or structure logic.
- All source owners are recorded in the editing guide.

## Phase 3 — Decision and intelligence services

Goal: turn analysis outputs into immutable decision inputs and deterministic decision services.

Work:

- Separate evidence collection, weighting, consensus, confidence, edge, regime quality, adaptive thresholds, and decision filtering.
- Keep prediction separate from confirmed decision.
- Keep outcome telemetry and calibration as observation services, not decision authorities.
- Add explicit immutable input/output contracts.
- Add deterministic fixture tests and contradiction tests.

Acceptance:

- One decision authority.
- Same input snapshot produces the same decision.
- No UI/broker dependency in pure decision services.

## Phase 4 — Planning and risk

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

Acceptance:

- Requested entry, trigger, actual fill, SL, TP and broker protection are distinct values/states.
- SL is protective-only.
- Target progression is monotonic and path-safe.

## Phase 5 — Automatic trading, pending orders and lifecycle

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

Acceptance:

- Broker-confirmed state is authoritative.
- Rejected mutations never become synthetic state.
- Pending order is never treated as a position before broker confirmation.
- Exactly one broker mutation boundary exists.

## Phase 6 — Presentation and UI

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

Acceptance:

- UI never decides whether a trade should exist.
- UI never mutates broker state directly.
- Each renderer has one file owner and one rendering responsibility.

## Phase 7 — OSS research, adapters and benchmarks

Goal: use strong OSS where it materially improves numerical analysis without importing a second trading engine.

Rules:

- All OSS stays under `oss/` or an explicitly named adapter/benchmark boundary.
- Direct runtime dependencies must be compatible with the target cTrader/.NET runtime.
- Incompatible projects are benchmark/reference-only.
- No OSS trading engine becomes the execution authority.
- License and attribution are documented before adoption.
- Numerical parity and performance are measured before promotion.

Priority candidates:

- Technical indicator libraries for numerical cross-checking.
- Lightweight statistical/time-series components that are compatible with the target runtime.
- Research-only algorithmic trading engines for architectural benchmarking, never as a second live execution engine.

Acceptance:

- Every adopted component has an upstream source, license, compatibility result, benchmark result, and isolated adapter owner.
- Production cTrader build remains dependency-minimal.

## Phase 8 — Static verification and contract testing

Goal: make architectural and behavioral drift mechanically detectable.

Work:

- One-type-per-file verification where applicable.
- One-responsibility ownership checks.
- Parameter parity checks.
- Reference-method coverage checks.
- Production version-residue checks.
- Dependency-direction checks.
- UI/broker boundary checks.
- Decision purity checks.
- BUY/SELL symmetry tests.
- SL/TP invariants.
- Lifecycle transition invariants.
- Duplicate-event idempotency tests.

Acceptance:

- CI rejects architectural regression before cTrader testing.
- Static checks cover the same boundaries documented in the architecture.

## Phase 9 — cTrader compile and runtime acceptance

Goal: prove the finished source on the target cTrader environment.

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
