# Phase 11.1 — Auto Execution, Indicator Identity, Level Presentation and History Discoverability

Date: 2026-09-29

## Scope

This phase addresses five operator-reported defects after Phase 11:
1. The compiled custom indicator did not explicitly register its cTrader display name.
2. Plan level labels were drawn at the same price as their horizontal lines.
3. Automatic market execution could miss a valid same-bar opportunity because plan creation was latched after the first failed actionability check.
4. The Quick Auto Trading control did not participate explicitly in the runtime fault-state re-arm contract.
5. The History directory had no operator-visible location marker or startup diagnostic.

## Implementation

### Indicator identity

`CFIPIndicator` now uses the explicit cTrader IndicatorAttribute name `CFIPIndicator`, and the assembly name is aligned to `CFIPIndicator`.

The cTrader indicator-list name is controlled by IndicatorAttribute; assembly naming alone is not the registration mechanism.

Keeping the exact name `CFIPIndicator` preserves the designated storage root already used by the persistent-history design.

### Same-bar automatic execution

`TryEnsureAutomaticPlan()` no longer treats `_lastAutoPlanAttemptM5` as a per-M5 actionability latch.

Live actionability depends on the current quote and may change while the same closed M5 candle remains active. The plan coordinator already owns idempotent plan/capacity/trigger/actionability gates, so it is safe to re-evaluate until a plan exists.

The broker path still requires permission, runtime health, single-plan capacity, daily-loss protection, market/session/news suitability, indicator quality/conflict checks, confidence/smart-quality thresholds, spread/stop-risk limits, structural SL/TP geometry, execution-intent validation, submission retry protection and broker confirmation.

No safety gate was removed.

### Runtime fault re-arm

The existing fault-state safety contract remains fail-closed: a live `EntryBlocked` state cannot be bypassed by enabling Auto Trading. The Quick Auto Trading control now participates in the same explicit re-arm contract, so after the runtime supervisor has returned the state to `Healthy`, an operator quick-enable can re-arm the runtime entry gate. Any new runtime failure can block execution again.

### Level-label separation

Plan labels now use a bounded vertical price offset derived from the existing label envelope. Text remains background-free and white. The previously calculated `boxHalfHeight` is now consumed to separate text from the horizontal line.

### History discoverability

Startup creates `History/CFIP_HISTORY_LOCATION.txt` and prints the designated history location to the cTrader log.

The marker records the indicator name, relative History path, designated cTrader storage root, designated History path and UTC creation timestamp.

## History location

With indicator name `CFIPIndicator`, the designated cTrader indicator storage path is:

`Documents/cAlgo/Data/Indicators/CFIPIndicator/History/`

The project writes outcome archives, runtime logs, signal-trace archives, the portable memory snapshot and the location marker under that History directory.

## Verification

Required gates:
- Source / Architecture
- Parameter and semantic audits
- Startup/persistence audit
- Runtime acceptance contracts
- cTrader compile/build
- Project integrity and accumulated phase audits

Target-terminal evidence remains necessary for actual permission behavior, broker acceptance/rejection, live and pending fill timing, final chart spacing and first-run filesystem creation.

## Operator diagnostics

The panel now exposes `AUTO TRADE BLOCK` and `AUTO ORDER BLOCK` when automatic execution is enabled and blocked.

Use those reasons together with `History/CFIP_RuntimeLog_v2_*.csv` to distinguish decision/actionability blocks, runtime blocks and broker submission rejections.

## Continuation

The next phase should use the persistent execution telemetry and outcome archive to measure missed-actionable opportunities, broker rejection causes and calibration before changing strategy defaults.