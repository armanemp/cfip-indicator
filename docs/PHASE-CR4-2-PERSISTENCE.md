# CFIP — CR4.2 File/Archive Path and Persistence Observability

## Status

COMPLETE — PR #103 merged to main; target-terminal verification remains mandatory.

## Own verification

| ID | Review claim | Result |
|---|---|---|
| D2.1 | relative History path may depend on cwd | **Not confirmed as a cTrader defect**; official .NET 6 cTrader file-operations contract requires a relative path inside the algo-designated folder for restricted-access algos |
| D2.2 | multiple persistence paths use duplicated file-write ownership | **Confirmed** for marker/snapshot; fixed by routing file writes through BufferedArchivePersistence |
| D2.3 | archive writes can silently fail | **Confirmed as observability gap**; fixed with write/read failure counters, last error and startup probe |
| D2.4 | signal trace is outside account-scoped persistence identity | **Confirmed** in current main; fixed and cache invalidated on account switch |
| D2.5 | hot path performs synchronous archive writes | **Not confirmed**; existing archive records are queued and flushed by timer with a bounded line budget, preserved |

## Implementation

- `BufferedArchivePersistence` is the single production file-write owner.
- `OutcomeHistoryArchiveStore`, `RuntimeLogPersistence`, and `SignalEvaluationTraceArchivePersistence` continue using buffered enqueue/flush.
- `PortableMemorySnapshotStore` marker/snapshot writes and reads use the same bounded persistence owner.
- `History` remains a relative path. cTrader documents that .NET 6 restricted-access algos can use System.IO only inside the designated algo folder and should use relative paths there. citeturn537789view0turn187975search1
- startup performs a marker write/read round-trip and records probe health.
- panel displays `PERSISTENCE • READY/DEGRADED/UNVERIFIED` plus pending/read/write failure counts.
- Signal Trace archive prefix is account-scoped and refreshed when the account switches.

## Behavioral changes

- **Yes:** persistence writes are now owned by one bounded persistence service.
- **Yes:** persistence health is visible in the diagnostic panel.
- **Yes:** Signal Trace files are separated by account identity.
- **No:** `AccessRights.None` was not changed.
- **No:** public `[Parameter]` names, types or `DefaultValue` values were changed.
- **No:** trading thresholds, RR values or confidence defaults were tuned.

## Deterministic tests

- existing `VerifyBufferedArchivePersistence()` now exercises real temp-file write/flush/readback plus write/read health counters;
- new `VerifyPersistenceHealthSemantics()` validates sandbox-relative path rules and health-state semantics;
- `tools/audit_phase_4_2.py` checks centralized write ownership, account-scoped trace identity, panel observability and CI wiring.

## Needs manual cTrader verification

1. Exact cTrader desktop/build and OS.
2. Start CFIP with `AccessRights.None` and confirm no security exception.
3. Confirm actual folder is the cTrader designated Indicator folder with `History` beneath it; expected Windows pattern is `Documents/cAlgo/Data/Indicators/CFIPIndicator/History`. citeturn537789view0
4. Confirm the startup marker write/read probe reports PASS.
5. Confirm outcome archive, runtime log, signal trace and portable snapshot files are created under the same History tree.
6. Restart the indicator and confirm data remains readable.
7. Switch account and confirm new archive/trace identity is isolated from the previous account.

## New bugs observed but not fixed

- Exact filesystem root and account-switch event timing remain terminal-only acceptance evidence.
- LocalStorage persistence itself remains governed by cTrader local-storage semantics; D2 does not change the existing LocalStorage scope.

## Completion boundary

Repository implementation is complete after this phase PR merge. CI execution and target-terminal manual evidence must still be recorded honestly; no unverified PASS is claimed.

Main merge: PR #103, commit `05a91cb9764f7ee86fcaaba0d7dede540c4b6e1c`.
