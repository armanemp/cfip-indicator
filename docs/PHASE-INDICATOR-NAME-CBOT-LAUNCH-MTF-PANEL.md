# Indicator Naming + cBot Launch + MTF Panel Direction Correction

Date: 2026-10-02

Status: **IMPLEMENTATION IN PROGRESS — branch `phase/indicator-name-cbot-launch-mtf-panel-2026-10-02`.**

## Scope

This phase handles three related operator-facing corrections while keeping the core
analysis/trading architecture unchanged:

1. Give the Indicator a stable cTrader display name: **CFIP Smart Indicator**.
2. Give the cBot a stable cTrader display name: **CFIP Smart Execution Bot**, with M5 as
   its default host timeframe so the already-compilable shadow host is straightforward to
   add to a chart.
3. Fix the MTF panel wording so an unresolved timeframe with clearly bullish/bearish
   directional evidence is not misleadingly displayed as **NEUTRAL**.

## MTF panel semantics

The panel now distinguishes:
- **BUY / SELL** = the frame's resolved directional state;
- **BULL BIAS / BEAR BIAS** = directional evidence exists, but the frame has not met the
  canonical resolved-direction threshold;
- **NEUTRAL** = the frame is genuinely balanced/unresolved.

This is presentation-only. It does not modify the Frame Direction resolver or the Decision
engine, and does not turn a bias into an actionable signal.

The displayed bias uses existing frame BullScore/BearScore and TrendBull/TrendBear values.
No new score or threshold is introduced.

## cBot initial launch status

The repository already contains a separate cTrader Robot project and its CI compile gate is
green. This phase makes its cTrader metadata explicit so the compiled robot appears under
the stable name **CFIP Smart Execution Bot** and defaults to M5 when instantiated.

The current cBot remains **SHADOW / broker mutation DISARMED**. This phase does not activate
live execution and does not move broker mutation ownership.

## Cleanup

The panel render path no longer reserves the obsolete Quick Execution row height and no longer
runs the obsolete quick-control synchronization during panel rendering. The underlying legacy
execution modules remain untouched until their planned broker-migration phase so functionality
is not orphaned.

## Safety

No strategy, threshold, Decision, Plan, RR, Entry, SL, TP, confidence, risk, provider contract
or broker execution authority changes.

Public parameter count remains 568.

## Verification

Required:
- accumulated Source / Architecture;
- Runtime Acceptance Contracts;
- cTrader Compile/Build;
- accumulated MTF/provider/panel audits;
- this phase's naming/launch/panel-bias audit.

Manual cTrader boundary remains:
- verify the exact displayed Indicator and cBot names in the Algo list;
- compile the cBot from the cTrader editor and add it to an M5 chart;
- verify the MTF panel shows BULL BIAS/BEAR BIAS where directional evidence is present but
  the canonical Frame.Direction is still unresolved.

## Operator action after merge

Run: `git pull --ff-only`.
