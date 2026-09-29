# Phase 11.3 — Execution Rejection Forensics, Missed-Actionable Cohorts & Threshold Evidence

Date: 2026-09-30

## Scope

Phase 11.3 turns the telemetry already emitted by CFIP into a repeatable forensic
measurement layer for the complete path:

Analysis -> Decision -> Signal -> Alert -> Execution -> Broker confirmation -> Protection/Lifecycle -> Outcome -> Learning

The phase is deliberately observational. It does not add a second decision engine,
does not add a second execution authority, and does not automatically modify live
thresholds.

## Why this phase follows 11.2

Phase 11.2 added final quote-sensitive ActionableNow refreshes and persistent
signal/runtime telemetry. The next engineering dependency is to determine, from
real observations, where the canonical chain is:

- rejecting technically valid candidates;
- producing potential missed-actionable cases;
- experiencing execution rejection/null-result/unconfirmed cohorts;
- clustering near current threshold boundaries.

A threshold is not changed merely because a single chart example looks attractive.
It must first survive cohort measurement and replay/outcome verification.

## Implementation

Added `tools/analyze_phase_11_3.py`.

The analyzer:

1. reads append-only `CFIP_SignalTrace_*.csv` and `CFIP_RuntimeLog_v2_*.csv`;
2. normalizes base64/plain-text diagnostic fields;
3. classifies execution rejection/failure reasons into stable forensic categories;
4. correlates execution rejection rows with the same-config closed-M5 trace gate;
5. calculates forward MFE/MAE cohorts for non-actionable traces;
6. reports potential missed-actionable cohorts by gate/reason/lane/direction/regime;
7. surfaces OB+FVG, WaveTrend, indicator-fusion and MTF evidence inside those cohorts;
8. identifies traces near the current quality/RR/entry thresholds;
9. accepts an optional threshold snapshot JSON for replaying alternative historical configurations;
10. writes only an offline report and never feeds results back into live execution.

### Forensic categories

The current normalized categories are:

- NEWS
- PERMISSION
- CAPACITY
- DUPLICATE
- SUITABILITY
- SPREAD_RISK
- GEOMETRY
- VOLUME
- RUNTIME
- BROKER_REJECT
- NULL_RESULT
- OTHER

These are analysis labels, not new runtime states.

## Current threshold snapshot

The analyzer's built-in snapshot matches the current public parameter defaults used
for signal-quality/entry/risk boundaries:

| Parameter | Current default |
| --- | ---: |
| MinimumConfidence | 72 |
| MinimumEdge | 15 |
| MinimumSmartQuality | 70 |
| MinimumStructuralConfirmations | 4 |
| MinimumIndependentEvidence | 4 |
| MinimumTimeframeAgreement | 72 |
| SmartMinimumTimeframeAgreement | 72 |
| SmartQualityThreshold | 70 |
| MinimumEntryLocationQuality | 64 |
| MinimumEntryQuality | 72 |
| MaximumEntryDistanceAtr | 0.45 |
| Tp1MinimumRR | 2.00 |
| MinimumStructuralStopQuality | 65 |
| MinimumFreshTriggerEvidence | 3 |

These values are a measurement baseline only.

**No automatic threshold change is made in Phase 11.3.**

The correct next action after real logs exist is to compare near-threshold and
missed-actionable cohorts against replay/outcome evidence. A change is warranted
only after the corresponding cohort shows a repeatable signal-quality or execution
effect without breaking SL/TP geometry, BUY/SELL symmetry or safety gates.

## Whole-chain review

### Analysis / signal

The current production stack already includes:

- closed-bar MTF/top-down context;
- canonical structure, liquidity and sweep analysis;
- explicit FVG and Order Block quality;
- bounded OB+FVG synergy as the strongest single location feature;
- WaveTrend and divergence evidence;
- indicator-evidence fusion with conflict handling;
- regime-aware gating;
- one canonical Decision -> Trigger -> ActionableNow pipeline.

Phase 11.3 does not duplicate these owners. It measures their observed contribution
and the cohorts around their existing gates.

### Entry / SL / TP

The forensic analyzer carries Entry, Stop and TP1..TP4 from the existing signal
trace and evaluates forward geometry in R units. It remains a diagnostic layer and
cannot mutate any live level.

Existing protection invariants remain authoritative:

- BUY stop must remain below Entry and BUY TP1 above Entry;
- SELL stop must remain above Entry and SELL TP1 below Entry;
- later TP progression must remain forward-only/monotonic;
- broker-confirmed protection remains authoritative.

### Auto Trading / Auto Orders

The current live paths retain:

- final runtime/actionability refresh;
- permission and capacity gates;
- suitability and news controls;
- spread/risk and volume checks;
- execution-intent geometry;
- submission identity/duplicate prevention;
- broker confirmation and lifecycle authority.

Phase 11.3 only observes the existing execution telemetry.

## Uploaded Swing reference assessment

The user-provided Swing reference is useful as a conceptual/reference implementation
for swing-strength detection and visual swing plotting, but it is not promoted into
the CFIP runtime.

The production project already owns swing structure through `SwingPointAnalyzer`
and `SwingPlateauRule`, including canonical plateau handling and closed-bar
boundaries. Introducing the raw reference as a second swing authority would risk
different pivot identities, repainting differences and double-counted structure.

The raw reference also uses latest-value series access during its cache-building path
and retroactively paints the just-confirmed swing across prior bars. Those semantics
are not suitable as the authoritative closed-bar decision source.

Conclusion for implementation: keep the reference as a visual/behavioral reference;
do not copy its runtime code. Improvements that pass parity/regression testing should
land only in the existing canonical swing owner.

## Evidence boundary

Without actual cTrader runtime logs/outcomes, this phase can validate the forensic
mechanism but cannot report real false-positive, false-negative, missed-opportunity
or profitability measurements.

In particular, a potential-missed row means:

> a non-actionable observed setup whose original direction later moved by the configured
> forward MFE threshold.

It does **not** prove that an executable order would have filled, survived spread,
news, slippage, broker constraints or produced realized profit.

Execution rejection categories likewise describe observed telemetry and require
terminal/broker context before attributing root cause.

## Operator procedure

1. Copy the generated History files without renaming them.
2. Run:

```
python tools/analyze_phase_11_3.py --history-dir "<cTrader-History>"
```

3. Save the JSON report and correlate it with terminal replay and realized outcomes.
4. Only after sufficient evidence, update the relevant owner and add a deterministic
regression/contract test in a subsequent implementation phase.

## Verification required

- Phase 11.3 audit;
- existing source/architecture accumulated audits;
- runtime acceptance contracts;
- cTrader Compile/Build;
- target-terminal replay for broker rejection behavior and empirical signal quality.

No claim of profitability or prediction accuracy is made by this phase.
