# F1 — Repository / Build / Dependency Truth — 2026-10-04

Status: **IMPLEMENTATION COMPLETE — CI verification pending on the F1 branch.**

## Scope

F1 audits the repository/project structure, solution boundaries, package and target-framework consistency, dependency direction, Core/Contracts platform neutrality, Runtime.Contracts source-link boundary, CI build matrix, and generated-source hygiene.

## Baseline

- main: `8b26c50500cfe980b71a5a5912c3ea25ccdee25a`
- Active production projects: `CFIP.Indicator`, `CFIP.Contracts`, `CFIP.cBot`
- Canonical MTF remains M1/M5/M15/M30/H1/H4/D1/W1. M2 remains forbidden.
- Indicator remains analysis/signal owner; cBot remains broker mutation owner.

## Findings / root cause

### 1. Core platform leakage

`src/CFIP.Indicator/Core/Math/PlanLinePresentationRule.cs` imported `cAlgo.API` only to materialize cTrader `Color`. This violated the Core boundary and caused Source/Architecture CI to fail.

Root cause: a presentation semantic (fixed line alpha) and its platform materialization were placed in the same rule.

### 2. Runtime harness dependency drift

`tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj` linked `PlanLinePresentationRule.cs`. The source was platform-dependent, so the platform-neutral harness could not remain platform-neutral.

PR #272 attempted to remove the linked file, but its existing deterministic `VerifyPlanLineThicknessG3` still referenced the canonical rule, producing five `CS0103` errors. Removing the test would weaken the semantic regression boundary.

### 3. CI matrix truth

The existing compile matrix already builds the source-linked Indicator CI project, Contracts, cBot and Runtime.Contracts. F1 adds a repository-level audit that verifies these boundaries and their project references instead of relying on convention.

## Architectural correction

- `PlanLinePresentationRule` remains the single owner of platform-neutral line presentation semantics: thickness and alpha.
- cTrader `Color.FromArgb` materialization is now owned by the existing `PlanLineRenderer`, the actual chart-rendering boundary.
- Runtime.Contracts continues to compile the same canonical thickness rule and therefore retains its deterministic regression test without importing cTrader API.
- No duplicate renderer, color policy, calculation or fallback path was introduced.

## Repository audit added

`tools/audit_phase_f1_repository_build_dependency_truth.py` verifies:
- solution/project registration and local ProjectReference resolution;
- duplicate ProjectReference detection;
- Indicator/Contracts/cBot dependency direction;
- cTrader and Skender package version consistency;
- target-framework consistency;
- Core/Contracts cTrader API leakage;
- Runtime.Contracts linked-source neutrality;
- required CI build matrix entries;
- generated build-output exclusion.

The audit is part of Source/Architecture CI.

## M2 audit

F1 does not add or restore any M2 path. The existing MTF contract remains M2-free.

## Verification boundary

Repository-side implementation is complete. GitHub Source/Architecture, Runtime Acceptance and cTrader Compile workflows must pass on the final F1 head. Target-terminal behavior is not required for this repository/build phase.

## Performance

No runtime calculation, MTF traversal, chart-object lifecycle or hot-path algorithm was changed. The only runtime-path change is moving cTrader color materialization to the existing renderer owner; allocation/churn characteristics are unchanged.

## Operator action

After merge: `git pull --ff-only`.

## Additional baseline cleanup

The source/architecture gate exposed three dead public display parameters after the repository/dependency audit passed: `LabelLeftOffsetBars`, `ShowEarlyArrow`, and `ShowEarlyWatch`. They had declarations but no production consumer; related audits referenced obsolete owners. They were removed rather than reactivated through compatibility or parallel presentation paths. Current public parameter inventory is 545.
