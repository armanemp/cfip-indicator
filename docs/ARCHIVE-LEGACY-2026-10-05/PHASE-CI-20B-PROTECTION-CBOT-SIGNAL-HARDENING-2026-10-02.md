# CI-20B — Protection / cBot / Signal Hardening — 2026-10-02

## Status

VERIFIED COMPLETE — repository gates PASS on final implementation head `5d37563536f8a5171d14dbc09b58cf2affeb10a2`.

## Scope

This phase continues CI-20 without creating a second execution engine:

1. consolidate live SL/BE/trailing reasoning under one pure canonical Indicator rule;
2. harden cBot management-command freshness so stale requests become terminal instead of retrying indefinitely;
3. keep Indicator management commands read-only and broker mutation cBot-owned;
4. improve primary-signal sensitivity to valid M15/H1 aligned pullbacks when M5 is neutral, while still rejecting opposite M5 direction;
5. remove repeated M5 regime snapshot calculation inside one smart-gate decision cycle;
6. add deterministic regression contracts and a dedicated accumulated architecture audit.

## Protection changes

- `IntelligentProtectionRule` is the single pure owner for local BE/structural trailing progression semantics.
- `ProtectionManager` now gathers the analytical inputs once and delegates the combined decision to that rule.
- BE uses the existing canonical `SmartBreakEvenRule`, including TP1-aware trigger/lock limits.
- Existing SL can only move forward in the protective direction.
- structural trailing remains structure-driven and does not chase raw price;
- minimum trail step and broker minimum-distance validation remain active;
- server-owned break-even prevents competing local BE mutation;
- invalid/non-finite protection state fails closed.

## cBot management hardening

- cBot management commands now have an explicit bounded maximum age.
- expired commands produce a terminal `BrokerReportStatus.Expired` report;
- Indicator report reconciliation removes both confirmed and expired commands;
- this prevents stale management commands from remaining permanently executable or starving newer queue entries through repeated retry cycles.
- broker mutation authority remains exclusively in the cBot management owner.

## Signal / analysis hardening

- a dedicated `PrimaryPullbackTuningRule` now permits a neutral M5 only when:
  - selected direction is BUY/SELL;
  - M15 and H1 agree with the selected direction;
  - both primary frame qualities meet the existing primary-quality floor;
  - top-down calibration is eligible.
- an opposite M5 direction remains blocked;
- no confidence, RR, or public strategy threshold was lowered;
- smart-gate regime lookup is computed once and reused within the same decision evaluation.

## Verification

Repository verification on the final head:
- Source / Architecture #3141: **PASS**;
- Runtime Acceptance Contracts #2950: **PASS**;
- cTrader Compile/Build #3134: **PASS**;
- CI20B dedicated protection/cBot/signal audit: **PASS**;
- accumulated architecture/execution/UI audits: **PASS**.

Required:
- accumulated Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- CI20B dedicated protection/cBot/signal audit.

Manual boundary:
- actual cTrader target-terminal signal frequency/quality;
- panel/chart synchronization;
- broker management acceptance/rejection;
- restart/reconnect ordering;
- empirical outcomes.

No profitability claim is made from this structural hardening phase.

## Next

Operator action after merge: `git pull --ff-only` on local `main`.

After merge, continue the remaining cBot lifecycle/recovery and broker-owned protection/target progression work, then use replay/OOS evidence to tune signal quality rather than changing thresholds blindly.
