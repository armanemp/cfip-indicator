# Corrective Hotfix — Chart Lines and Execution Status UI

Date: 2026-09-29

## User-validated defects

This hotfix addresses two areas that remained incorrect in live cTrader validation despite earlier corrective iterations:

1. Plan-level chart lines could terminate before the latest visible chart candle.
2. AUTO TRADE / AUTO ORDERS controls could drift from the configured cTrader settings because the chart controls mutated private runtime flags rather than editing the public configuration inputs.

The user explicitly requested that unreliable execution controls stop pretending to be buttons. The resulting design is therefore status-only.

## Root cause — incomplete level lines

The compact 40-bar renderer correctly calculated a compact span, but its right edge was subsequently replaced by an M5-to-chart time mapping. On charts whose timeframe did not align one-to-one with the M5 event timestamp, the rendered line could end at an earlier chart candle.

This was a geometry ownership error: an execution/event timestamp was being used as the boundary of a chart presentation object.

### Correction

PlanLineRenderer now owns the chart geometry:

- compact mode is exactly 40 chart bars ending at Bars.Count - 1;
- full-width compatibility starts at bar 0 and also ends at the latest chart candle;
- no M5 event index or MapM5ToChart() call is used by plan-line geometry;
- the right edge therefore reaches the latest chart candle consistently across chart timeframes.

PlanLabelAnchorCalculator no longer maintains a second compact-line-left calculation. Label coordinators reuse the canonical GetPlanLineLeftBar() owner.

Pending-order line rendering also no longer computes and validates a disposable M5 anchor before drawing the pending levels.

## Root cause — unreliable execution controls

The prior chart controls changed the private runtime flags _autoTradingEnabledRuntime and _automaticOrdersEnabledRuntime while the public settings remained EnableAutoTrading and EnableAutomaticOrders.

EnsureExecutionRuntimeState() only synchronized the runtime values when it detected a change in the public setting. Consequently, the chart control could create an independent runtime override without changing the actual cTrader configuration. That is not a reliable settings-synchronized control.

### Correction

AUTO TRADE and AUTO ORDERS are now non-interactive status surfaces.

Each surface:

- displays the current ON/OFF state;
- uses a modern switch-style visual with a rounded track and movable thumb;
- uses the semantic execution color when enabled;
- is synchronized by EnsureExecutionRuntimeState();
- does not register Click, Checked or Unchecked handlers;
- cannot mutate execution authority;
- cannot drift into a second UI-owned runtime setting.

The only enable/disable authority remains the public cTrader settings: EnableAutoTrading and EnableAutomaticOrders.

This deliberately follows the user's requirement: where reliable in-chart mutation cannot be guaranteed, show the authoritative state rather than expose a misleading button.

## Scope and preserved invariants

No changes were made to decision authority, signal thresholds or weights, RR policy, risk sizing, structural SL/TP construction, broker mutation ownership, predictive pending selection, aggressive entry ownership, or lifecycle/broker-confirmation semantics.

Important market levels, future structural pending levels, Order Block quality and holistic signal coordination remain first-class strategy work and are not diluted by this presentation correction.

## Verification hardening

Added tools/audit_runtime_ui.py and wired it into Source / Architecture CI.

The audit rejects M5 anchoring inside the plan-line renderer, legacy compact-line helper duplication, legacy interactive execution-control fields anywhere in the C# source tree, and execution UI runtime-mutation handlers.

The existing Source / Architecture and Runtime Acceptance contracts were updated to verify the new status-only boundary and canonical line geometry.

## Manual validation still required

Automated gates can prove source ownership, runtime contracts and compilation. Actual cTrader validation is still required for visual line termination at the latest chart candle on the target chart timeframes, correct 40-bar span, readable compact switch-style status cards, live synchronization when the cTrader parameter values are changed, and absence of accidental interaction on the status cards.

This hotfix must not be described as hands-on cTrader verified until that validation is actually performed.

## Verification status

Pre-merge automated gates are required:

- Source / Architecture
- Runtime Acceptance Contracts
- cTrader Compile

PR #30 is the implementation vehicle.

After merge, a merge record and final commit will be appended here.

## Merge record — 2026-09-29

PR #30 was merged into main as 3c5afe6f37c8852fb6975bbb5d4004bc1ec95ffe.

Final automated verification for the merged implementation:
- Source / Architecture: PASS;
- Runtime Acceptance Contracts: PASS;
- cTrader Compile: PASS.

The source and runtime audits therefore passed for the status-only execution UI and canonical chart-line geometry. Hands-on cTrader validation is still a separate requirement and has not been claimed here.

Main was advanced by this merge; local pull is required before continuing development.
