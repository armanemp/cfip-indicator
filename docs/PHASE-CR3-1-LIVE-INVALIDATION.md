# CFIP — CR3.1 Live Invalidation and False-Signal Semantics

Date: 2026-09-30

Status: **VERIFIED COMPLETE**

Review scope: Claude Prompt 3 findings C1, C2 only.

## Implementation

### C1 — Live invalidation stability and broker-close truth

- Added LiveInvalidationRule as the canonical deterministic owner for closed-bar evaluation, directional adverse movement and successful-exit bookkeeping.
- Live invalidation/false-signal evaluation is now gated to one canonical closed M5 bar at a time.
- Soft/false-signal invalidation consumes the closed M5 close rather than a single live tick.
- Replaced the structural invalidation rolling high/low scan with confirmed structural swing candidates from the canonical swing analyzer.
- Broker-close success is represented explicitly by closeAccepted.
- A rejected structural broker close enters RecoveryRequired, returns failure to the caller, and does not advance _lastExitM5.
- _lastExitM5 success recording is centralized and only advances after a successful broker mutation. Broker OnPositionClosed confirmation remains the authoritative lifecycle confirmation.

### C2 — False-signal adverse-R semantics

- Added Enable Soft Adverse-R Invalidation as an explicit safety control with DefaultValue = true, preserving current default behavior.
- Added FalseSignalAdverseRRule as the single semantic owner for configured soft/hard adverse-R validation and BUY/SELL symmetry.
- Existing FalseSignalAdverseR bounds remain 0.25 to 5; configured default remains 1.10.
- When an active broker protective stop is known and lies in the adverse direction, effective software thresholds are capped to the currently protected R envelope so software invalidation cannot silently sit beyond broker protection.
- Existing independent invalidation alerts remain independent of the new soft-guard flag, avoiding an unintended alert-behavior regression.
- Invalid/non-finite threshold inputs fail closed.

## Deterministic verification

Runtime contracts cover:
- closed-bar one-evaluation-per-bar semantics;
- BUY/SELL directional symmetry;
- invalid numeric/direction inputs;
- rejected versus successful exit bookkeeping;
- protected-stop/threshold coherence;
- break-even-or-better protection behavior;
- invalid adverse-R configuration bounds.

Dedicated static gate: tools/audit_phase_3_1.py.

Final implementation branch head before merge:
f9ab0aeb6802c78f4bb97adbde140fff1ed1967e

Merged:
- PR #95
- merge commit f482d2f76cdf37cca87fabc5b11b3d8c0a7edac7

Final PR verification on implementation head:
- Source/Architecture: PASS, run 1808
- Runtime Acceptance: PASS, run 1617
- cTrader Compile: PASS, run 1801
- CR3.1 static audit: PASS
- accumulated CR2.1–CR2.9 audits: PASS
- project-wide routine/optimization audits reached PASS after reconciling the intentional safety-parameter inventory

Production compile result remains: 109 existing warnings, 0 errors.

## Whole-chain routine audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed.

No new execution authority, cBot code, broker mutation owner, multi-position capacity, signal-confidence tuning, RR-floor tuning, OB/FVG logic or WaveTrend logic was introduced.

Rejected broker exits remain recovery states. Actual position closure remains broker-event confirmed.

## Performance and code cleanliness

- Closed-bar invalidation removes repeated intra-bar evaluation for the same M5 result.
- Canonical pure rules isolate arithmetic from the cTrader partial host.
- Structural invalidation no longer scans a rolling min/max window on every evaluation.
- No network/file I/O was introduced into the invalidation hot path.
- No new mutable cache or duplicate business-state authority was introduced.
- The new safety parameter increases the public parameter inventory to 568 and is fully consumed by runtime logic and parameter audits.

## Evidence boundary

CI proves source/build/runtime-contract consistency; it does not prove target-terminal broker timing, live quote/bar synchronization, broker rejection behavior under network conditions, restart/reconnect reconciliation, or profitability/accuracy.

Those remain target-terminal/manual acceptance items.

## Next phase

**CR3.2 — Decision gate and early prediction semantics (C3/C4).**

Track 12A cBot separation remains blocked until CR-FINAL.