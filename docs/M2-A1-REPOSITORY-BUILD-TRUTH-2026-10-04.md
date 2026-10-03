# M2-A.1 — Repository / Build Truth Audit — 2026-10-04

Status: **ACTIVE / NOT CLOSED**

Branch: `phase/M2-repository-hygiene-ownership-current`  
HEAD audited: `e8809f8bfcd0db7687c76201e8b7ed3b6613976e`  
PR: #257

## Scope

This document records the first executable M2-A gate. It audits repository/build truth before semantic calculation work. Static evidence is not treated as runtime/terminal PASS.

## Baseline observations

- Current recursive repository tree at the audited HEAD contains **1122 tracked files**.
- The earlier master baseline recorded 1113 files; the delta is therefore **+9 files** and must be reconciled before M2-A closure.
- Production projects in the primary solution are exactly:
  - `src/CFIP.Indicator/CFIP.Indicator.csproj`
  - `src/CFIP.Contracts/CFIP.Contracts.csproj`
  - `src/CFIP.cBot/CFIP.cBot.csproj`
- The solution does **not** directly include the contract harness projects, preflight projects, benchmark project, or shadow-test project.
- Indicator and cBot both target net6.0 and depend on `cTrader.Automate 1.0.21`; only Indicator also depends on `Skender.Stock.Indicators 2.7.3`.
- `CFIP.Contracts` is net6.0, has no cTrader package/reference, and has nullable enabled.

## M2-A.1 findings

### M2-A.1-001 — Primary solution build graph does not include verification projects

**Classification:** build/verification architecture finding  
**Severity:** Medium  
**Status:** OPEN

The primary solution contains only the three production projects. The repository also contains executable contract/preflight/benchmark verification projects that are outside this solution graph.

Impact: a normal solution build can succeed while one or more repository-level contract harnesses or preflight projects are not compiled. This creates a distinction between “production solution builds” and “repository verification graph”.

Root cause: verification projects evolved as separate tools but were not represented in the primary solution/build orchestration.

Required remediation: define one authoritative build manifest (without making test/harness code part of production assemblies) that explicitly builds the production graph plus all mandatory verification graphs. Prefer a solution or CI orchestration layer that references each project rather than relying on scattered script knowledge.

No remediation is marked complete yet.

### M2-A.1-002 — Platform-neutral production rules are compiled directly into multiple verification assemblies

**Classification:** architecture/build graph finding  
**Severity:** High  
**Status:** OPEN

The following verification projects compile source files directly from `src/CFIP.Indicator`:

- Runtime Contracts: **168** explicit Compile items
- Decision Contracts: **55**
- Planning Contracts: **33**
- Execution Contracts: **2**

Across these four projects there are **201 unique explicit source includes**, of which **52 source files are compiled into more than one verification assembly**.

Examples of shared sources include:
- `CanonicalTimeRule.cs`
- `OpportunityLane.cs`
- `ExecutionMode.cs`
- `NumericGuards.cs`
- `MarketRegimeClassifier.cs`
- `TopDownCalibrationRule.cs`
- `RiskRewardMathRule.cs`
- `EntryGeometryRule.cs`
- `TargetProgressionRule.cs`
- `BrokerConfirmationPolicy.cs`
- `LifecycleEventIdempotencyGuard.cs`

This is not duplicate source code: the physical source file remains single-owner. However, it creates **multiple compilation identities of the same production type**. A change can therefore affect several test assemblies through file inclusion rather than through a normal dependency graph.

Architectural risk:
1. build properties/symbols can diverge between harnesses;
2. references can silently differ;
3. source relocation becomes a multi-project manifest problem;
4. the verification architecture is coupled to the Indicator directory rather than to an explicit platform-neutral domain/core assembly;
5. it weakens the intended `Indicator → Contracts ← cBot` boundary by making test assemblies reach sideways into the Indicator implementation tree.

Required remediation: after the full M2-A audit establishes the exact platform-neutral surface, extract the truly platform-neutral rules/models used by multiple layers into an explicit shared project (or a small set of cohesive shared projects), then make verification projects reference that project normally. Do **not** perform a blind mass move during M2-A. The extraction must preserve one source/one owner and must be dependency-driven.

### M2-A.1-003 — Runtime contract harness is a large explicit source manifest

**Classification:** build maintainability finding  
**Severity:** Medium  
**Status:** OPEN

`CFIP.Runtime.Contracts.csproj` contains 168 explicit Compile entries across 183 lines. This is deterministic today, but it is a high-maintenance manually curated build surface.

Impact: adding/renaming/removing a canonical rule requires synchronized project-manifest edits; omission can produce false confidence because a contract harness may simply stop compiling a source it was intended to verify.

Required remediation: reduce the explicit source manifest by moving platform-neutral production code behind a normal project reference. If an explicit allow-list remains necessary, generate/validate the manifest mechanically and fail CI on drift.

### M2-A.1-004 — CI mirror project compiles the entire Indicator source tree separately

**Classification:** build duplication / verification architecture  
**Severity:** Medium  
**Status:** OPEN

`tools/CFIP.Indicator.CI/CFIP.Indicator.CI.csproj` disables default Compile items and explicitly glob-compiles `../../src/CFIP.Indicator/**/*.cs`, excluding bin/obj.

This is intentionally a CI mirror rather than a second production owner, but it creates a second compilation surface for the same Indicator sources.

Required remediation: retain only if its purpose is proven distinct from the primary cTrader compile gate. Otherwise converge on a single authoritative production compilation graph plus explicit probe/preflight projects. If retained, document the exact unique invariant it catches and ensure CI always runs it.

### M2-A.1-005 — Build graph uses mixed framework generations

**Classification:** build architecture finding  
**Severity:** Low/Medium  
**Status:** OPEN

Production Indicator/cBot/Contracts and most harnesses target net6.0, while `tools/CFIP.StockIndicators.Benchmark` targets net8.0.

This is not inherently wrong; benchmark isolation can justify a newer runtime. It becomes a problem only if benchmark results or helper code are treated as production-runtime evidence.

Disposition: **retain provisionally**, but explicitly isolate benchmark runtime from production compatibility claims and ensure CI labels its result as research/benchmark evidence.

### M2-A.1-006 — Contracts platform neutrality is currently structurally clean but verification is incomplete

**Classification:** architecture verification  
**Status:** OPEN — verification incomplete

`src/CFIP.Contracts/CFIP.Contracts.csproj` has no cTrader package/reference and no direct `cAlgo.API` usage was found by the targeted repository search.

This is positive evidence, not closure. The full M2-A dependency-direction audit still has to trace every contract type and every consumer.

## Architecture target established by M2-A.1

The modern target is:

`CFIP.Contracts`  
↕ immutable cross-boundary transport contracts

`CFIP.Core / CFIP.Domain` *(platform-neutral calculation/rule/model owners, exact split to be determined by dependency audit)*

↙                         ↘  
`CFIP.Indicator`          `CFIP.cBot`

Indicator owns analysis/signal/presentation.  
cBot owns broker mutation/execution.  
Contracts own transport schemas, not business calculations.

Verification projects reference the platform-neutral owners normally; they do not compile arbitrary Indicator implementation files by path.

This target is a **design constraint**, not yet an implemented refactor.

## Verification state

- Repository tree inspected at exact HEAD: **PASS for inventory collection**.
- Primary solution structure inspected: **PASS for factual inventory**.
- Contracts project targeted platform-dependency scan: **no cTrader dependency found**.
- Shared-source compilation scan: **52 duplicated compile inclusions identified**.
- Runtime/CI/terminal build: **NOT CLAIMED**.
- Full M2-A: **OPEN**.
- M2-B onward: **NOT STARTED as closure gates**.

## Next exact work item

Continue M2-A.2 from this baseline:
**duplicate/near-duplicate files, partial classes, conditional compilation, generated/obsolete/unreachable artifacts, and build reachability reconciliation.**

No feature tuning or signal/visual changes should begin until the repository/build truth gate has a complete disposition.
