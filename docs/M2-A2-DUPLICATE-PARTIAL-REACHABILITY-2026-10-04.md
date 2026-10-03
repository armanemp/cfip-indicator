# M2-A.2 — Duplicate / Partial / Conditional / Reachability Audit — 2026-10-04

Status: **ACTIVE / NOT CLOSED**

Audited branch: `phase/M2-repository-hygiene-ownership-current`

## Findings

### M2-A.2-001 — No suspicious backup/temp artifact names found

A recursive tracked-tree scan found no paths matching common generated/temporary/backup markers such as `bin`, `obj`, `.vs`, `node_modules`, `__pycache__`, `backup`, `old`, `archive`, `temp`, `tmp`, `copy`, `.bak`, `.orig`, `.rej` or `.swp`.

**Disposition:** PASS for this narrow filename/path heuristic. It is not proof that no generated content exists under a legitimate-looking filename.

### M2-A.2-002 — Duplicate basenames are limited and mostly intentional, but ExecutionIntent requires architectural classification

Only four duplicate-basename groups were found:

1. `Program.cs` — six independent executable harnesses/tools.
2. `README.md` — root, OSS and benchmark documentation.
3. `global.json` — repository SDK pin and benchmark SDK pin.
4. `ExecutionIntent.cs` — one cross-boundary `CFIP.Contracts.ExecutionIntent` and one internal Indicator planning model.

The first three are structurally explainable. The fourth is **not a duplicate implementation**: the two types have different namespaces and materially different responsibilities. However, identical type names across the transport boundary are a maintainability hazard because they can be confused during refactoring.

**Disposition:** retain semantics; future shared-core extraction should rename the internal planning type to an intent-specific domain name if dependency tracing confirms it does not itself represent the transport contract.

### M2-A.2-003 — CFIPIndicator is heavily split across partial classes

Repository search shows the Indicator is substantially modularized as many files contributing to the same `CFIPIndicator` partial class. The pattern spans analysis, planning, trading, runtime, panel, chart and parameter files.

This is not automatically bad: it keeps the cTrader entrypoint and platform state in one host while separating physical concerns.

The risk is architectural rather than syntactic: all partial files share private fields/state and therefore bypass explicit dependency boundaries. A renderer can directly reach runtime state; a planning helper can reach platform state; a parameter file can expose or couple to unrelated execution internals.

**Required audit:** classify each partial file into:
- host/lifecycle adapter;
- platform I/O adapter;
- pure calculation/rule;
- application service;
- presentation;
- state owner;
- parameter surface.

Pure calculations/services that do not require cTrader should be candidates for explicit composition and shared projects rather than remaining partial methods on the platform host.

**Disposition:** OPEN. Do not mass-convert partials to classes until M2-B dependency tracing identifies the real boundaries.

### M2-A.2-004 — Conditional compilation appears absent from production source

Targeted GitHub code search returned no production matches for `#if` or `#define`. This is favorable because it reduces hidden build-path divergence.

**Disposition:** PROVISIONAL PASS. Final closure still requires inspecting project files, generated build properties and CI scripts for MSBuild properties/symbols that can alter compile behavior without source preprocessor directives.

### M2-A.2-005 — No NotImplementedException / Obsolete production markers found by targeted search

Targeted search found no production `NotImplementedException` and no `[Obsolete]` usage in the initial repository search. This is favorable but not a reachability proof.

**Disposition:** PROVISIONAL PASS. Continue with call-graph/build-graph reachability rather than deleting anything from search absence.

## Build reachability implications

The strongest current reachability risk is not dead files; it is **files that are live in production but compiled through several verification assemblies**. This was recorded as M2-A.1-002 and remains open.

The next audit must map:
- every project source include;
- every explicit compile exclusion;
- every CI-only project;
- every preflight probe;
- every shadow test;
- every benchmark;
- every script-imported source;
- every reflection/string entry point;
- every cTrader entrypoint and event callback.

## Modern architecture disposition

The project should converge toward explicit composition:

- platform-neutral rules/models/services in normal referenced assemblies;
- Indicator as the analysis + signal + presentation host;
- Contracts as immutable cross-boundary transport schemas;
- cBot as the broker mutation/execution host;
- verification projects referencing canonical owners instead of path-compiling arbitrary Indicator files;
- partial classes retained only where they genuinely represent one host owner's lifecycle/platform surface.

No duplicate implementation or parallel owner is introduced by this audit.

## Verification

- Suspicious tracked-path heuristic: PASS.
- Duplicate basename classification: PASS for current four groups, with ExecutionIntent naming flagged.
- Conditional source scan: provisional PASS.
- Obsolete/not-implemented scan: provisional PASS.
- Full reachability: OPEN.
- Full partial-class ownership classification: OPEN.
- Runtime/compile/terminal PASS: not claimed.

## Next exact work item

**M2-A.3 — complete build reachability and source ownership mapping**, including project compile/exclude rules, CI/preflight/shadow/benchmark graphs, entrypoints, callbacks, reflection/string-based activation, and files that exist but are unreachable.
