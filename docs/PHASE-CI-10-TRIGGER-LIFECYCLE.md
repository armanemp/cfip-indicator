# CI-10 — Trigger and Trigger-Lifecycle Audit

Date: 2026-10-02

## Status

Implementation complete on branch phase/ci-10-trigger-lifecycle-audit; repository verification pending.

This phase continues the full-stack calculation integrity track immediately after CI-09. It audits trigger mathematics and the causal lifetime of an M1 confirmation relative to the closed-M5 decision that owns it.

## Scope completed

### Canonical threshold semantics

TriggerThresholdRule is now the single Core owner for resolving the existing Live/Precision trigger-score policy, rejecting invalid score inputs fail-closed, and evaluating triggerScore >= requiredTrigger at one explicit boundary.

The M5 closed-bar evaluator, M1 closed-bar evaluator, M1 runtime updater and platform-neutral M1 trigger rule now reuse this owner. No public trigger parameter or default was changed.

### Causal M1 lifecycle

TriggerLifecycleRule is now the single Core owner for runtime reset identity (DecisionM5 + Direction), determining whether a live M1 confirmation belongs to the currently-forming M5 after the decision M5, recording a new confirmation only once per causal M1 bar, and propagating confirmed TriggerReady from M5 readiness plus optional M1 latch.

The live runtime no longer treats an M1 bar whose parent is the already-closed decision M5 as a valid live confirmation. A live M1 confirmation must belong to the current forming M5 and the M1 candle must be fully closed at the calculation reference.

### Revision and reset semantics

TriggerRuntimeState.ConfirmationRevision records a monotonic runtime revision whenever a new causal M1 confirmation is established.

A new decision-M5 or direction resets the prior M1 confirmation identity while preserving the revision counter. The lifecycle is therefore explicit as WAITING → M1 confirmation candidate → M1 confirmed/latch → decision-M5 expiry/reset.

A later weak M1 candle cannot revoke an already-latched confirmation for the same decision-M5.

### Fresh evidence and displacement override

Existing FreshTriggerEvidence remains prior-bar based. The direct-displacement override remains scoped to the existing fresh-M5 evidence check; it does not bypass the canonical trigger-score or M5 breakout gate.

No trigger threshold, displacement ATR, fresh-evidence threshold or direct-override parameter was retuned.

## Deterministic coverage

Runtime acceptance contracts now cover exact live M1-in-current-M5 containment, rejection of stale/expired M1 confirmations, rejection of an unclosed M1 bar, runtime reset on M5/direction identity changes, one-time confirmation recording, M5 + optional M1 TriggerReady propagation, fail-closed threshold boundaries, and preservation of the monotonic confirmation revision across reset.

## Architecture / performance audit

The runtime M1 confirmation remains on the live calculation path before pre-trade plan synchronization and automatic plan creation. No new broker read, network I/O, unbounded cache or second decision authority was introduced.

The runtime updater also now retains the previously observed closed-M1 index before updating state, avoiding a false self-comparison that could suppress the intended M1 frame refresh.

## Safety boundary

No public parameter name, type or DefaultValue was changed.

No RR, confidence, risk, Entry, SL, TP or execution threshold was tuned.

No second decision, plan or broker-mutation authority was introduced.

## Verification boundary

This phase is not marked VERIFIED until the exact branch head passes Source / Architecture, Runtime Acceptance Contracts, and cTrader Compile / Build.

Target-terminal timing, intrabar/replay behavior, chart/panel synchronization and empirical signal quality remain manual acceptance boundaries.

## Next phase

After CI-10 is verified and merged, the authoritative next phase is CI-11 — Entry geometry and signal-timing audit.
