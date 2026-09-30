# CR1.8 — Managed Identity Boundary

Date: 2026-09-30

## Review confirmations

تأیید می‌کنم — A11 / identity collision: baseline BrokerIdentity.cs used symbol + label only; IsManagedPosition(Position position) began at line 94.

تأیید می‌کنم — A11 / broad unmanaged ownership: baseline BrokerIdentity.cs returned true when ManagedActionsOnly=false at line 100, allowing same-symbol manual/foreign positions into automatic protective mutation.

تأیید می‌کنم — A11 / hidden reversal profit rule: baseline ReversalCloseGuard.cs used position.NetProfit <= 0 at line 48, so the minimum profit threshold was a hidden constant.

## Implementation

Identity boundary:
- Added ManagedIdentityRule as the single platform-neutral identity formatter.
- Canonical label: <AutoTradeLabel>|CFIP-I:<InstanceId>.
- Automatic Market and Aggressive Market broker mutations now receive ManagedExecutionLabel().
- Pending Stop and Pending Limit use PendingOrderLabel(), which derives from the same managed identity and appends -PENDING.
- IsManagedPosition requires the exact instance-scoped label regardless of ManagedActionsOnly.
- ManagedActionsOnly=false is retained for compatibility but cannot widen ownership; its safety notice is emitted once, not per scan.

Reversal close threshold:
- Added Reversal Close Minimum Net Profit with default 0.0, minimum 0, maximum 100000, step 0.01.
- Added ReversalProfitThresholdRule as the single finite/non-negative/strict-lower-bound owner.
- ReversalCloseGuard now consumes that rule instead of the hard-coded > 0 check.
- Default behavior remains strictly positive net profit; higher configured thresholds require correspondingly higher net profit.

## Current source locations after implementation

- BrokerIdentity.cs: ValidateTradeIdentityConfiguration line 70; IsManagedPosition line 106; pending identity lines 138–143.
- TradeLabelFormatter.cs: ManagedExecutionLabel line 22 and InstanceId binding line 29.
- AutomaticMarketBrokerExecution.cs: instance-scoped labels at lines 18, 95 and 100.
- AggressiveBrokerExecution.cs: instance-scoped labels at lines 124 and 135.
- ReversalCloseGuard.cs: canonical profit rule at line 48.
- 13_auto_trading.cs: new safety parameter at line 43.

## Verification contract

Deterministic Runtime Contracts cover stable identity, distinct cross-instance identities, deterministic sanitization, missing-input fail-closed behavior, strict reversal-profit boundaries, non-finite inputs and invalid negative thresholds.

A dedicated tools/audit_phase_11_8.py is wired into Source/Architecture CI and checks identity propagation through all four broker-submission families plus the reversal threshold contract.

## Routine project-wide audit

Analysis → Decision → Signal → Alert → Execution → Broker confirmation → Protection/Lifecycle → Outcome → Learning was reviewed for this phase.

No analysis threshold, signal threshold, RR floor, TP/SL geometry, news policy or scenario decision authority was tuned in CR1.8. The change is isolated to broker-ownership identity and the reversal-close safety invariant. The one-shot warning avoids repeated log output in the hot path.

## Verification boundary

CI can verify compile/source/runtime contracts. Target-terminal replay is still required before claiming empirical broker identity/restart/reconciliation behavior.

## Next

CR1.9 — Minor cleanup and documentation.
