# Track 19 — OSS Numerical Benchmark

## Purpose

Track 19 validates numerical OSS candidates without changing the production
authority boundary.

The production numerical dependency remains Skender.Stock.Indicators 2.7.3.
FacioQuo.Stock.Indicators 3.0.1 remains isolated research material under the
net8 benchmark project. Numerical parity never by itself authorizes promotion
into the net6 cTrader production assembly.

## Phase 19.1 — FacioQuo comparison

Status: complete.

Completed:

- Preserved the production/research package boundary.
- Rebuilt the benchmark around a shared deterministic OHLCV fixture source.
- Expanded the fixture to 800 bars per scenario.
- Added TREND_UP, TREND_DOWN, RANGE and REGIME_SHIFT scenarios.
- Added complete-series count and timestamp alignment checks.
- Added warm-up-aware full-series numerical comparison.
- Added maximum absolute error, mean absolute error and RMS error reporting.
- Added strict finite-value coverage requirements after warm-up.
- Covered all 10 OSS indicator families used by the production confluence
  adapter, including Bollinger width and Stochastic %K/%D.
- Added repeatable batch timing and per-iteration allocation measurements.
- Added a Markdown benchmark report and GitHub Actions job-summary output.
- Added verifier gates for the Track 19.1 benchmark structure and package
  isolation.
- Kept v3 promotion blocked by target-runtime compatibility regardless of the
  numerical parity result.

Acceptance:

- Four deterministic scenarios execute for both package generations.
- Every benchmarked output has complete-series coverage.
- Timestamps align one-to-one.
- Every post-warm-up comparable pair is finite.
- All configured numerical tolerances pass.
- Performance measurements are emitted for both package generations.
- Production package pins remain unchanged.
- FacioQuo remains absent from production source.

## Phase 19.2 — TA-Lib.NETCore cross-check

Status: planned.

Scope:

- benchmark only;
- verify API/output feasibility for the indicators that overlap CFIP;
- record native/runtime dependencies and licensing constraints;
- compare numerical behavior on the same deterministic fixture set;
- do not add TA-Lib to production until distribution and license implications
  are explicitly cleared.

Acceptance:

- reproducible numerical results;
- explicit native dependency inventory;
- license/distribution gate recorded;
- no production boundary change.

## Phase 19.3 — QuantConnect LEAN numerical/reference comparison

Status: planned.

Scope:

- use LEAN as an offline reference where its indicator implementations
  overlap CFIP;
- compare formulas/output conventions rather than importing its trading engine;
- keep LEAN out of the production assembly and live execution path.

Acceptance:

- reference-only results are reproducible;
- semantic differences are documented;
- no second trading/decision engine is introduced.

## Phase 19.4 — OSS benchmark consolidation

Status: planned.

Scope:

- consolidate package/version/indicator/fixture/tolerance/timing evidence;
- detect numerical regressions across package upgrades;
- make benchmark output machine-readable and human-readable;
- bind promotion decisions to explicit compatibility and authority gates.

Acceptance:

- one canonical numerical benchmark contract;
- package upgrades fail closed on unreviewed numerical drift;
- production/research boundaries are mechanically verified.

## Phase 19.5 — OSS admission decision

Status: planned.

Scope:

- decide whether any research package is eligible for production promotion;
- require runtime compatibility, license/distribution review, numerical parity,
  performance evidence and regression coverage;
- preserve CFIP-specific indicator and strategy semantics when adopting any
  external numerical implementation.

Acceptance:

- promotion or rejection is documented per package;
- production dependency set is minimal and explicit;
- no OSS package becomes the CFIP decision, risk, execution or broker authority.
