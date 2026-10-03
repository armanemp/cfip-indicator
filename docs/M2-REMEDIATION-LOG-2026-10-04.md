# M2 remediation log — 2026-10-04

## RuntimeInitialization module-size correction

CI identified `Runtime/Initialization/RuntimeInitialization.cs` above the repository's enforced 20KB production-module limit.

The startup/lifecycle tail was moved into `RuntimeInitializationLifecycle.cs` under the same partial `CFIPIndicator` owner. No logic was duplicated and no second lifecycle owner was introduced.

Evidence: commit `53fa172b68ce1022395ef43098b5b4cb1513a41b`.

The cTrader compile gate had passed on the preceding CI head; the new source/architecture and runtime gates must be rerun on the latest head before M2 acceptance is claimed.

## M2-A.1 — Repository/build truth findings

Current exact tree at HEAD `e8809f8bfcd0db7687c76201e8b7ed3b6613976e`: 1122 files, 82 directories, 734 C#, 669 Indicator C#, 22 cBot C#, 23 Contracts C#, 210 Markdown, 155 Python.

### Open findings
- **M2.189 — branch divergence:** M2 is 138 commits ahead and 67 commits behind `main`, with merge base `a0f5ab1e6d7711d980320f8bb83ecbd4997a148b`. This blocks final-main closure claims until the histories are explicitly reconciled and re-audited.
- **M2.191 — build-harness source duplication:** 52 exact source-path overlaps exist across Decision/Planning/Execution/Runtime contract harness projects. This is retained as an acceptance-harness architecture risk, not a production duplicate owner, pending source-isolation/build-graph review.
- **M2.196 — solution-vs-CI coverage:** CI builds the production components through individual projects/harnesses rather than the solution file. Coverage is not assumed; exact solution-to-CI mapping must be proven.

### Verified/retained
- **M2.192:** nested .NET 8 SDK pin for the isolated OSS benchmark is intentional.
- **M2.193:** four unreferenced Python files are observational/manual analyzers or benchmarks, not production dead paths.
- **M2.194:** Indicator and Contracts `ExecutionIntent` types are distinct layer-specific models; no duplicate execution authority.
- **M2.195:** current Contracts source/project boundary is platform-neutral.

M2-A.1 remains open. No final PASS is claimed.
