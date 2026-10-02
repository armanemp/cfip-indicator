# MTF-P1 — Primary M15/H1 Signal Layer + Panel Separation

Date: 2026-10-02

## Scope

Implement the requested signal hierarchy without creating a second decision or execution authority:

- M15 and H1 are the two primary visible signal sources.
- Closed M5 is the local tuning/calibration layer.
- Closed M1 confirmation is additional timing/tuning when the existing M1 trigger feature is enabled.
- M15 and H1 signals may coexist simultaneously, including opposite directions.
- Existing H1+/M30/M15/M5/M1 top-down context remains available to the canonical decision path.
- Existing parallel-timeframe execution policy remains observe-only for non-M5 candidates; this phase does not authorize broker execution from a new signal candidate.
- Source-timeframe OB/FVG evidence remains attached to M15/H1 candidates and participates in deterministic display priority through the existing location-confluence field.
- Candidate Entry/SL/TP geometry remains the canonical M5 planning projection for now. It is not presented as an independent M15/H1 execution plan.

## Implementation

### Primary signal semantics

Added PrimaryTimeframeSignalRule as the single pure owner of the new signal-role semantics. It recognizes only M15/H1 as primary sources and records primary-source validity, M5 directional alignment, M1 closed-trigger confirmation and deterministic presentation state.

M5/M1 tuning does not erase a valid M15/H1 source signal. This preserves the case where a higher-timeframe signal is visible while lower-timeframe confirmation is pending or temporarily opposite.

TimeframeScenarioBuilder now creates primary candidates only from M15 and H1. M30/H4/D1/W1 continue to serve as context rather than becoming additional visible primary signals.

### Simultaneous presentation

Primary candidates receive explicit state on TradeOpportunityCandidate and are prioritized in ParallelScenarioSelectionRule. M15 and H1 retain distinct scenario identities, so both can remain visible at the same time.

Chart labels identify the role as PRIMARY M15 BUY/SELL or PRIMARY H1 BUY/SELL. The panel lane summary exposes the M5/M1 tuning state.

### Important-level evidence

M15/H1 candidate enrichment continues to consume the source frame FVG/OB location evidence. No new numeric OB/FVG threshold was introduced and no existing trading threshold was retuned.

This phase strengthens source-timeframe signal identity and evidence provenance without inventing a new scoring system.

### Panel separation

The in-chart AUTO TRADE / AUTO ORDERS quick-control row has been removed from the Indicator panel. Canonical status/diagnostic rows remain available inside the panel.

The panel keeps a fixed 100px bottom clearance for BottomLeft and BottomRight positions so the separate cBot surface can occupy the lower chart area without being covered by the Indicator panel.

## Safety / authority

No broker mutation, account-risk authority, or execution path was added.

The existing cBot separation remains intact. The new primary signal candidates are analysis/presentation candidates and remain observe-only until later cBot provider/execution phases explicitly define and verify their consumption.

No public parameter was added. No confidence, RR, Entry, SL, TP, risk or execution threshold was tuned.

## Verification

Required repository verification:

- Source/Architecture accumulated audit;
- Runtime Acceptance Contracts for primary M15/H1 semantics;
- cTrader Compile/Build.

Target-terminal validation remains manual for actual M15/H1 signal timing, simultaneous M15 + H1 visual coexistence, M5/M1 tuning refresh timing, panel/cBot spatial separation and responsiveness, and empirical false-signal/missed-opportunity behavior.

## Operator action

After merge to main, run: git pull --ff-only

Then reload/re-attach the Indicator and cBot in the target cTrader chart for manual acceptance.
