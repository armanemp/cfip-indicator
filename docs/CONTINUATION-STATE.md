## 2026-10-03 — cBot Effective Lifecycle State

Status: implementation complete, verification pending.

Closed a state-semantics mismatch: broker reconciliation emits family states such as ACTIVE / RECONCILED and ACTIVE / MULTI-SCENARIO, while the publisher previously required exact lifecycle strings before setting effective execution flags.

Correction:
- CbotExecutionLifecycleRule is now the single lifecycle eligibility owner;
- descriptive READY/ACTIVE/PENDING variants remain executable when recovery is false;
- UNKNOWN and RECOVERY REQUIRED remain fail-closed;
- publisher and behavioral tests consume the same rule.

No execution/risk/quality threshold was lowered.

Verification required:
Source/Architecture, Runtime Acceptance, cTrader Compile/Build, dedicated lifecycle audit and target-terminal panel/state validation.

Phase record: docs/PHASE-CBOT-EFFECTIVE-LIFECYCLE-STATE-2026-10-03.md.

Operator action after merge: git pull --ff-only.
