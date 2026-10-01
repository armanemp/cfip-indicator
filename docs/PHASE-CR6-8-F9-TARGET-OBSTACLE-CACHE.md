# CR6.8 / F9 — Target-obstacle scan performance and cache reuse

Date: 2026-10-01

Status: **VERIFIED COMPLETE — Source/Architecture #2246, Runtime Acceptance #2055 and cTrader Compile #2239 all passed on the final F9 HEAD.**

CI correction log: the first F9 compile attempt exposed an `Entry` construction reference left behind after the architecture refactor; it was corrected to `TargetObstacleScanCacheEntry` without changing behavior.

## Required phase confirmation

تأیید می‌کنم — `Trading/Validation/TargetObstacleValidator.cs` was re-read before implementation at `EvaluateTargetObstacle` (around line 52), `GetTargetObstacleScanSnapshot` (around line 174), `BuildTargetObstacleScanSnapshot` (around line 10) and `IsTargetObstacleSwing` (around line 171). The new Core cache identity is in `Core/Math/TargetObstacleCachePolicy.cs`, and the bounded runtime cache is in `Trading/Validation/TargetObstacleScanCache.cs`.

## Verified finding

F8 consolidated obstacle evaluation with telemetry, but the M5 swing/equality structural scan was still rebuilt for each target candidate. `SelectTargets` remains a single owner and is called by the canonical plan path and the parallel preview path. The existing F1 opposing-zone candidate cache is already reused and is not duplicated by F9.

## Implementation

- Added a platform-neutral `TargetObstacleCacheKey` containing Bars/history/index, direction and every scan input that can materially change the structural snapshot.
- Added a bounded 16-entry `TargetObstacleScanCache` with hit/miss/build/eviction counters.
- Invalidated same-Bars cache entries when history size, closed index or closed-bar open time changes.
- Attached `HistoryLoaded` and `Reloaded` invalidation to the affected Bars series.
- Moved M5 swing and equal-level extraction behind one `TargetObstacleScanSnapshot` per cache context.
- Preserved candidate-specific Entry/Target/clearance comparisons outside the cache, so different targets cannot reuse a stale boolean rejection.
- Preserved the canonical equal-level semantics by materializing canonical liquidity swing pairs and resolving the appropriate BUY/SELL level at evaluation time.
- Kept `HasOpposingZonePathObstacle` on the existing F1 zone cache and `HasHigherTfZonePathObstacle` on that same owner.

## Deterministic verification

- Planning Contracts cover bounded capacity, supported directions, identical-key reuse, closed-index identity, history-size identity, BUY/SELL identity, equality-tolerance identity and new-bar invalidation.
- `audit_phase_6_8.py` is wired immediately after `audit_phase_6_7.py` in Source/Architecture.
- The reference benchmark compares repeated structural scanning with one snapshot reused across many target candidates.

## Behavior and safety boundary

- No public `[Parameter]` name, type or `DefaultValue` changed.
- No obstacle, RR, confidence, SL/TP or execution threshold was retuned.
- No second target-selection authority was introduced.
- No broker mutation path was added or changed.
- No intended user-visible signal criterion was changed.

## Project-wide routine audit / optimization

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was rechecked for ownership impact. F9 remains planning/validation-only; execution, protection and outcome authorities are unchanged.

Performance/code-cleanliness audit confirms the optimization is bounded, invalidation is explicit, target-specific geometry stays out of the cache, and no unrelated rewrite was introduced.

## Verification boundary

Repository CI verification is complete: Source/Architecture #2246, Runtime Acceptance #2055 and cTrader Compile #2239 passed on the final F9 HEAD. The reference benchmark is platform-neutral and is not a claim about cTrader terminal latency.

Manual cTrader acceptance remains required for target-terminal timing/presentation, replay around new-bar and history-reload events, real CPU/memory behavior, and confirmation that broker lifecycle/execution behavior is unchanged.

## Out-of-scope findings

No new out-of-scope production bug was fixed in F9. Prompt 7 G1–G6 remains a separate remediation sequence and is not implicitly closed.

## Next phase

**CR6.9 / F3 — Orphaned managed-position protection.**

