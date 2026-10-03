# cBot Multi-Scenario Capacity Alignment — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

## Scope

Align demo cBot defaults with the already-certified bounded multi-scenario architecture so the cBot can actually use more than the old three-execution demonstration ceiling.

Changed:
- Max Concurrent Scenarios default: 5, absolute maximum remains 10.
- Max Demo Executions Per Session default: 10, absolute maximum remains 20.

Safety:
- live accounts remain fail-closed;
- concurrency remains explicitly bounded;
- same ScenarioId idempotency remains enforced;
- per-scenario broker reconciliation and protection remain active;
- Indicator remains broker-mutation-free;
- no signal, confidence, RR or risk threshold was lowered.

Reason:
The realtime opportunity engine now separates execution candidates from display limits and can publish multiple independent current/future scenarios. The previous demo defaults of 3 concurrent and 3 total session executions unnecessarily limited that architecture during demo validation.

Verification:
- Source/Architecture;
- Runtime Acceptance;
- cTrader Compile/Build;
- cBot capacity regression audit;
- target-terminal validation of multiple simultaneous ScenarioIds and session-cap behavior.

Operator action after verified merge: git pull --ff-only.