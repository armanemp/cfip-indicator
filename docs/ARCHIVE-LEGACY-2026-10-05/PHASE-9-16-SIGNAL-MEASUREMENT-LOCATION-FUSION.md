# Phase 9.16 — Signal Measurement, OB/FVG Location Fusion & Execution Safety

Date: 2026-09-29

## Status

Implementation complete pending final CI verification on PR #58.

Branch:
`phase/9-16-signal-measurement-location-fusion`

PR:
#58

Scope:
- identify where directional opportunities are lost instead of adding blind filters;
- strengthen location evidence with a bounded OB/FVG hierarchy;
- make OB+FVG the strongest single location feature while keeping structure, liquidity,
  trigger, risk and reward-path authorities intact;
- record one closed-M5 signal/decision trace per bar;
- persist traces in append-only 90-day files for offline replay analysis;
- expose the latest gate/reason in the panel;
- add a final automatic-market plan-geometry revalidation before broker mutation;
- preserve BUY/SELL symmetry, source ownership and public-parameter compatibility.

## Why this phase exists

The remaining quality problem is not safely solved by simply raising or lowering one
confidence threshold. A directional opportunity can disappear at several distinct
stages:

Analysis -> Decision -> Signal -> Actionability -> Execution.

Phase 9.16 therefore adds measurement before further tuning. Every closed-M5 canonical
decision can now report the stage that stopped it:

- CONSENSUS
- DECISION-FILTER
- TRIGGER
- ACTIONABILITY
- ACTIONABLE

The trace contains the important contextual inputs required to compare accepted and
rejected opportunities later, including M5 structure/location evidence, FVG/OB quality,
OB+FVG confluence, indicator-fusion quality/conflict, WaveTrend, divergence, MTF/top-down
state, confidence, RR, entry mode and the exact block/actionability reasons.

Trace recording is observational. It never feeds the live directional decision and it
cannot submit, cancel or modify broker orders. It is captured for each canonical
closed-M5 decision independently of the outcome-telemetry switch.

## OB/FVG location hierarchy

A canonical `LocationEvidenceRule` now owns FVG/Order Block location scoring.

The hierarchy is:

1. Order Block gets a stronger base contribution than a standalone FVG.
2. FVG quality and OB quality increase their bounded individual contributions.
3. OB+FVG overlap receives the strongest single location synergy.
4. The synergy is quality-tiered and bounded.
5. The correlated FVG/OB group contributes one evidence unit, not two independent
   evidence units, to prevent double-counting.

This deliberately makes OB+FVG the strongest *single location feature*, not the overall
signal authority. A high-quality confluence cannot bypass structural conflict, MTF
conflict, trigger requirements, trap risk, reward-path validation, spread/risk checks,
broker capacity or protection rules.

## Signal missed-opportunity measurement

`tools/analyze_signal_trace.py` reads the append-only trace files and performs a
forward-window diagnostic on the recorded M5 bars.

The analyzer reports:
- gate counts;
- exact actionability/filter reasons;
- lane counts;
- OB+FVG confluence versus non-confluence counts;
- forward maximum favorable excursion (MFE) and adverse excursion (MAE) in units of
  each trace's frozen Entry->Stop risk;
- a bounded "potential missed" cohort.

"Potential missed" is intentionally observational. It means the setup was not
actionable at observation time but later moved at least the configured forward MFE
threshold in its original direction without first hitting the recorded stop. It is
not a backtest trade, a profitability claim or a causal proof.

Default diagnostic window:
- 12 subsequent closed M5 bars;
- 1.0R MFE threshold.

These defaults are analyzer settings, not live trading parameters.

## Automatic execution hardening

Immediately before automatic market execution returns eligible, the current market
entry is revalidated against the frozen plan Stop/TP1 geometry through
`ExecutionPlanGeometryRule`.

The rule rejects:
- invalid direction/level geometry;
- wrong-side protective stop;
- wrong-side TP1;
- RR below the regime-specific execution floor.

This is a final broker-entry safety check against stale or malformed plan geometry. It
does not replace broker confirmation or protection ownership.

## Performance

Signal traces are bounded in memory at 256 closed-M5 observations. Each 90-day trace
file is append-only and deduplicated by closed-bar open-time. The file archive is not
trimmed by the indicator.

No new public parameters were introduced.

The offline analyzer performs strict forward-window measurement and never runs in the
live calculation path.

## Long-term history continuity decision

The full raw long-term history remains in the designated `History/` files. Recent
outcome memory remains bounded in LocalStorage and current archive aggregates are used
for calibration.

This phase deliberately does not duplicate unlimited raw history into LocalStorage.
cTrader Type-scope LocalStorage has a finite quota, while the designated indicator
folder is the safe place for persistent file archives. Keeping files canonical makes
the history portable and avoids eventually exhausting LocalStorage.

For a system migration:
- copy the CFIP Type LocalStorage directory to preserve the recent bounded cache;
- copy the indicator `History/` directory to preserve all raw 90-day outcome and signal
  trace archives;
- on a machine where LocalStorage is missing, the archive can still rebuild the
  long-term calibration aggregate after startup.

## Verification boundary

Automated:
- Runtime Acceptance;
- cTrader Compile/Build;
- Source/Architecture;
- accumulated project audits;
- parameter/public-API compatibility;
- Decision Contracts for location hierarchy and execution geometry;
- signal-measurement/location audit.

Empirical target-terminal validation still remains required for:
- whether the trace identifies the user's perceived missed setups correctly;
- false-signal frequency;
- timing of M1 confirmation;
- actual chart/label lifecycle;
- broker slippage/rejections;
- realized SL/TP/R and live protection behavior.

No profitability or win-rate improvement is claimed from source/contract verification.


## Remediation closeout — 2026-09-29

The first CI pass exposed two deterministic engineering defects; both are corrected:
- the BUY/SELL geometry regression fixture used non-mirrored risk/reward levels and is now symmetric;
- the signal-trace recorder exceeded the production-module size budget and is now isolated from the archive-store state.

Persistent-memory behavior is also now explicit:
- closed-M5 signal traces are no longer gated by the outcome-telemetry switch;
- the `History/` directory is created during indicator initialization;
- a visible `CFIP_PortableMemory_<symbol>_<timeframe>_<fingerprint>.txt` snapshot is created at startup;
- when the matching Type-scoped LocalStorage key is absent, the snapshot can restore the bounded recent outcome cache into LocalStorage;
- raw outcome archives and raw signal-trace archives remain the long-term file-of-record and are never deleted by CFIP.

## Next phase

Phase 9.17:
Target-terminal replay of Phase 9.16 traces and evidence-driven gate refinement. The
next refinement must use measured rejection cohorts rather than adding blind global
thresholds.


## Final verification closeout — 2026-09-29

Verified merge commit: `fd6f43594ac1a50a67ccd3007c661a2ba059374f`.

CI:
- Runtime Acceptance #1172: PASS.
- cTrader Compile/Build #1356: PASS.
- Source/Architecture + accumulated audits #1363: PASS.
- Decision Contracts: PASS within cTrader Compile/Build.
- Phase 9.15 startup/persistence audit: PASS.
- Phase 9.16 signal measurement audit: PASS.

Public parameter count remains 552.

Portable-memory clarification:
PortableMemorySnapshotStore already restores the bounded recent outcome cache from the matching History snapshot into Type-scoped LocalStorage when that LocalStorage key is absent. It is intentionally not a full raw-history mirror. The raw 90-day files remain the canonical long-term record; LocalStorage is the compact recent cache. This avoids duplicating unlimited raw data into the finite LocalStorage quota.

Next phase:
Phase 9.17 — target-terminal replay of Phase 9.16 traces and evidence-driven gate refinement.


## Phase 9.16 remediation carried into Phase 9.17 — 2026-09-29

The final Phase 9.16 implementation kept closed-M5 signal traces observational and independent
of outcome telemetry, preserved non-destructive long-term History archives, and added the
portable bounded LocalStorage snapshot/restore path. Phase 9.17 builds on that verified mainline
state rather than replacing it.
