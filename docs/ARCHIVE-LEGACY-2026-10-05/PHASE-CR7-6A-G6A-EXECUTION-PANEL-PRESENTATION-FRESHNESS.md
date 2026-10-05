# CR7.6a / G6A — Execution panel presentation freshness

Date: 2026-10-01

Status: **VERIFIED COMPLETE — repository gates passed on implementation HEAD `b0b19c1a3e7e65110dc1b64d4f8a3bf555b7c54e`.**

## تأیید می‌کنم

The verified G5 panel-state cache contract is correct for broker/runtime freshness, but
the presentation identity still had an incomplete execution-facing dependency set.

The affected code is:
- `src/CFIP.Indicator/UI/Panel/PanelRenderOptimization.cs`
- `src/CFIP.Indicator/Trading/Execution/State/AutoTradingStateStore.cs`
- `src/CFIP.Indicator/Trading/Intelligence/OutcomeTelemetryEngine.cs`
- `src/CFIP.Indicator/Core/Math/ExecutionPanelPresentationIdentityRule.cs`

## Root cause

G5 correctly stopped `BuildPanelPresentationKey` from forcing broker-state invalidation.
However, the key did not explicitly encode several mutable execution-facing values already
rendered by the panel:

- Auto Trading state and reason;
- latest execution telemetry path/state;
- active execution scenario identity;
- market-suitability score/state/reason;
- break-even diagnostic.

As a result, those presentation changes could leave the panel key unchanged and delay a full
panel render even though no broker re-read was necessary.

A second freshness gap existed in `SetAutoTradingState`: changes to the canonical runtime
state/reason did not invalidate the G4 execution/protection snapshot. That could leave the
derived operational state cached across a state transition.

## Implementation

### Canonical presentation identity

Added `ExecutionPanelPresentationIdentityRule` in Core and made the panel presentation key
consume it.

The owner serializes the exact execution-facing presentation inputs in a culture-invariant,
null-safe and deterministic form.

No broker object is queried by the new Core rule.

### Auto-trading state invalidation

`SetAutoTradingState` now invalidates the canonical execution/protection panel snapshot
when either the state or reason changes.

This preserves G5's rule that authoritative mutation drives cache invalidation, while
keeping unchanged state cacheable.

### Telemetry remains presentation-only

Execution telemetry changes are represented in the presentation identity, but the telemetry
writer does **not** invalidate the broker execution/protection snapshot.

This is deliberate: telemetry changes do not change broker-confirmed protection facts, so
forcing a broker read would regress G5's performance boundary.

## Deterministic verification

Added `VerifyExecutionPanelPresentationIdentityG6A()` to Runtime Acceptance Contracts.

Covered:
- unchanged identity is deterministic;
- Auto Trading state/reason changes change presentation identity;
- telemetry and scenario changes change presentation identity;
- suitability and break-even changes change presentation identity;
- null values normalize deterministically.

Added `tools/audit_phase_7_6a.py` and wired it into Source/Architecture CI.

## Safety boundary

- no public parameter name/type/`DefaultValue` changed;
- no RR, confidence, entry, SL, TP, risk or execution threshold changed;
- no decision authority changed;
- no broker mutation path changed;
- no second execution authority introduced;
- broker-confirmed state remains authoritative.

## Performance/code-cleanliness audit

- the new identity is pure string composition with bounded inputs;
- no market-data scan, broker enumeration, file I/O or network I/O was added;
- telemetry-only changes do not invalidate the broker-state snapshot;
- execution presentation ownership is centralized rather than duplicated in the renderer;
- no new unbounded cache was introduced.

Routine whole-chain audit:
Analysis → Decision → Signal → Alert → Execution → Broker confirmation →
Protection/Lifecycle → Outcome → Learning remains unchanged.

## نیاز به تست دستی در cTrader

Still required:
- visible panel refresh after Auto Trade state transitions;
- visible update after execution/recovery telemetry changes;
- scenario/suitability/break-even status refresh;
- no measurable regression in panel responsiveness;
- reconnect/reload behavior.

## Verification status

Repository gates passed on the functional implementation HEAD:
- Source/Architecture: **PASS** — #2327;
- Runtime Acceptance Contracts: **PASS** — #2136;
- cTrader Compile: **PASS** — #2320.

A documentation-only synchronization commit follows this functional verification.
No target-terminal behavior is claimed from repository CI.
