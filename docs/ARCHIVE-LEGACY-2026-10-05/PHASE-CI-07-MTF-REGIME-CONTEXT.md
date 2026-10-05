# CI-07 — MTF / Regime / Market Context

Date: 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` in PR #162, merge commit `73511c84ff3072cdbdab8487b0d4331b52c789b1`. Final automated gates passed: Runtime Acceptance #2333, cTrader compile #2517, Source/Architecture #2524.**

## Scope

CI-07 audits and hardens:

- M1/M5/M15/M30/H1/H4/D1/W1 closed-bar mapping;
- exact closed-bar/reference alignment;
- timeframe state stability;
- top-down context provenance;
- regime classification and regime transitions;
- trend/range/compression/expansion/high-volatility semantics;
- premium/discount context;
- session context.

## Root causes found

### 1. MTF cache could return an old reference

MtfClosedContextCache was keyed by Bars identity and bar counts only.
Within an unchanged MTF bar, a cache hit returned the original
MtfClosedContext.Reference. Downstream decision construction requires the
context reference to equal the current calculation reference, so the cached
object could become stale even though its closed indices were still valid.

### 2. Regime transitions were implicit

The classifier emitted the current regime, while transition from the previous
regime was not an explicit state in MarketRegimeSnapshot/Frame. This made it
hard to audit regime changes separately from the TRANSITION regime category.

### 3. Context consumers reconstructed pieces independently

Decision and suitability consumed M5 regime, premium/discount, session and
daily closed-index information through separate paths. This increased the
chance of reference drift and duplicate MTF resolution.

## Implementation

### Canonical market-state snapshot

Added a platform-neutral MarketStateSnapshot containing all eight timeframe
state snapshots plus:

- exact calculation reference;
- closed index per timeframe;
- direction/quality;
- normalized regime and regime quality/stability;
- previous regime and explicit regime-transition state;
- premium/discount bias;
- canonical session-open state.

ProcessNewClosedBar materializes this snapshot once after all canonical
timeframe frames are built and validates reference/index identity before
decision evaluation.

Decision input now carries and validates the same snapshot. Decision regime,
regime quality and premium/discount values are read from that canonical state.

Suitability reuses canonical session state, MTF direction state and the D1
closed index from MtfClosedContext.

### MTF cache reference hardening

The MTF cache now:

- receives the current reference;
- refuses reuse when reference moves backward;
- refuses reuse when any cached timeframe has reached its next bar;
- returns a reference-updated context with the same verified closed indices.

This preserves fast same-bar reuse without allowing a stale context reference.

### Regime transition ownership

Added MarketRegimeTransitionRule as the single pure transition owner.

Every analyzed timeframe now records:

- current normalized regime;
- previous normalized regime;
- STABLE, CHANGED, INITIAL or UNKNOWN transition state.

No existing regime threshold, score, RR, SL/TP, confidence or execution policy
was tuned.

## Verification contracts

Added deterministic CI-07 Runtime Acceptance coverage for:

- exact market-state reference identity;
- all eight canonical MTF index mappings;
- rejection of a misaligned M5 index;
- regime transition classification;
- premium/discount and session state carried in the same snapshot;
- normalization/fail-closed behavior for invalid frame state.

Added tools/audit_phase_ci_07.py and accumulated it immediately after
tools/audit_phase_ci_06.py.

## Safety and performance

- No public cTrader parameter name/type/default was changed.
- No trading threshold, confidence, RR, SL/TP, risk or execution threshold was tuned.
- No new decision/execution/broker-mutation authority was introduced.
- Same-bar MTF cache reuse remains bounded and avoids per-tick recreation of
  the underlying closed-index mapping.
- Regime transition calculation is limited to the previous bar for each newly
  materialized non-M5 frame and uses the existing M5 core cache for M5.
- The new snapshot is data-only and platform-neutral.

## Manual acceptance boundary

Still requires target-terminal validation of:

- live MTF boundary timing under real cTrader callbacks;
- reconnect/history reload behavior;
- panel/chart responsiveness;
- session-state visual timing;
- empirical regime transition frequency and signal/outcome quality.
