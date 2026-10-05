# CI-10 — Trigger and Trigger-Lifecycle Audit

Date: 2026-10-02

## Status

**VERIFIED COMPLETE — PR #165 merged to `main`; merge commit `ed8fadb2e8af2ca5e0250c72de955e5659d8bdaa`.**

Final verification on implementation head `699cc1dd9c6e58a6df9bbd730b75ee37f7ce93f7`:
- Source / Architecture #2571: **PASS**
- Runtime Acceptance Contracts #2380: **PASS**
- cTrader Compile / Build #2564: **PASS**

The implementation head was verified before merge; the merge commit is the repository integration point.

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

Verification is complete for the exact implementation head listed above. Target-terminal timing, intrabar/replay behavior, chart/panel synchronization and empirical signal quality remain manual acceptance boundaries.

Target-terminal timing, intrabar/replay behavior, chart/panel synchronization and empirical signal quality remain manual acceptance boundaries.

## Next phase

After CI-10 is verified and merged, the authoritative next phase is CI-11 — Entry geometry and signal-timing audit.
