# CR7.6b / G6B — Execution-control truth and single UI authority

Date: 2026-10-01

Status: **VERIFIED COMPLETE — implementation head `7832f47c05117f66686adf659fd92670dcb14ba8` passed all three repository gates.**

## Finding

The repository contained a later regression against the established execution-control
presentation contract.

The 2026-09-29 chart-lines/execution-status hotfix explicitly made AUTO TRADE and
AUTO ORDERS status-only surfaces because cTrader Indicator parameters are the
authoritative execution settings. The current panel implementation had nevertheless
reintroduced click handlers that mutated private runtime flags, creating a second
in-panel execution state.

That behavior could allow the visual control state to diverge from the configured
Indicator settings and from the documented execution authority.

## G6B correction

- removed `ExecutionToggleHandlers.cs`, eliminating the in-panel execution mutation path;
- kept the compact modern `ToggleButton` presentation, but explicitly disables interaction;
- centralized status text and interaction policy in
  `Core/Math/ExecutionControlPresentationRule.cs`;
- made the synchronizer re-apply the read-only interaction state as well as the displayed
  ON/OFF value;
- preserved `EnsureExecutionRuntimeState()` as the settings-to-runtime synchronization
  boundary;
- retained runtime execution state internally for fail-closed safety, but the panel cannot
  mutate it directly;
- accumulated dedicated Runtime Acceptance and Source/Architecture coverage.

## Preserved invariants

- public `EnableAutoTrading` / `EnableAutomaticOrders` parameters remain the configuration
  authority;
- no decision, broker-mutation or execution authority was added;
- no trading threshold, RR, confidence, entry, SL, TP or risk value was retuned;
- no new broker enumeration or unbounded cache was introduced;
- panel telemetry remains presentation-only.

## Deterministic contracts

`VerifyExecutionControlPresentationG6B()` verifies:

- the canonical execution-control presentation rule is explicitly non-interactive;
- AUTO TRADE/AUTO ORDERS status text is deterministic for ON/OFF states;
- blank captions normalize deterministically.

`tools/audit_phase_7_6b.py` additionally enforces:

- no legacy interactive execution-toggle handler file;
- no AUTO TRADE/AUTO ORDERS click handlers in the factory;
- explicit disabled controls;
- canonical presentation-rule consumption;
- settings/runtime synchronization ownership;
- Runtime Contracts and Source/Architecture wiring.

## Manual cTrader boundary

Still required:

- confirm the status cards cannot change execution state when clicked;
- confirm ON/OFF follows the configured cTrader parameters after startup/reload;
- confirm visual styling remains readable while disabled;
- confirm no responsiveness regression from the synchronization boundary;
- confirm reconnect/reload behavior on the target terminal.

Repository CI cannot claim these target-terminal behaviors.

## Verification

- Source/Architecture: **PASS** — run #2339;
- Runtime Acceptance Contracts: **PASS** — run #2148;
- cTrader Compile: **PASS** — run #2332;
- accumulated historical audits through G6A remained green on the final source gate.
