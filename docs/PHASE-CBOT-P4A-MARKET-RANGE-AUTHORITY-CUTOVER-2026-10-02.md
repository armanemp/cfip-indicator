# CBOT-P4A — Market / Market Range Authority Cutover — 2026-10-02

## Purpose

Move the Market and Market-Range broker mutation authority out of the Indicator while preserving the canonical analysis/plan chain.

## Completed

- Removed BrokerMarketOrderMutation.cs from the Indicator.
- Removed legacy Indicator automatic-market and aggressive-market broker execution stages from the live calculation cycle.
- Kept analytical plan creation independent of Enable Auto Trading.
- Preserved EntryAllowed, TriggerReady and ActionableNow as separate canonical states.
- Provider publishes a Market intent only when the current quote passes the canonical actionability boundary and the plan mode is executable Market/Retest/Breakout.
- Provider fingerprint now changes when live actionability changes, so the cBot receives the transition from confirmed/waiting to actionable.
- cBot consumes MarketExecutionProfile and owns ExecuteMarketRangeOrder / ExecuteMarketOrder.
- Removed Indicator Close/Cancel broker-action controls from the panel surface; internal lifecycle mutation methods remain for the staged migration.
- Added finite panel bootstrap width/height before Chart.AddControl and reduced the bootstrap cap to 260px; live panel height is capped at 75% of usable chart height to prevent layout collapse.
- Added explicit M15/H1 primary alignment diagnostics using exact resolved source direction, added panel-key invalidation for MTF frame changes, and added distinct observe-only M15/H1 chart arrows.
- Fixed algorithm package naming by defining AlgoName for Indicator and cBot.

## Signal behavior correction

Before this phase, disabling Indicator Auto Trading also prevented canonical plan materialization. That made a signal appear absent in the exact architecture required for cBot execution.

The plan is now an Indicator analysis artifact. The cBot receives an execution intent only when the same canonical live actionability condition is satisfied.

No numeric signal threshold, RR, Entry, SL or TP policy was tuned in this phase.

## MTF note

The M15/H1 display remains a source-frame diagnostic, not a profitability claim. Closed-bar context/index alignment is still enforced by the decision input contract. Directional accuracy itself requires replay/OOS/target-terminal evidence before any numeric tuning is justified.

## Remaining execution migration

The Indicator still contains other broker mutation paths by design of the staged roadmap: Aggressive, Pending Stop, Pending Limit, Cancel, Close/Partial, SL, TP/server-ladder, protection, lifecycle, account risk and recovery.

Those are not silently treated as complete by this phase.

## Verification

Status: **VERIFIED COMPLETE — 2026-10-02.**

Final implementation HEAD: `e51d1bb82db2fec25b91917873ab19ccacbd9859`.

Repository gates:
- Source / Architecture: PASS — run `36981439556`;
- Runtime Acceptance: PASS — run `36981439561`;
- cTrader Compile/Build: PASS — run `36981439570`.

The final verification also passed the accumulated architecture, parameter, calculation-cycle, persistence, MTF/panel, and cBot boundary audits. Target-terminal checks remain manual: new .algo package name, first-load chart area, canonical signal arrow, provider actionability revision, demo cBot execution, live-account fail-closed, and real panel/chart responsiveness.

Target-terminal checks: new .algo package name, first-load chart area, canonical signal arrow, provider actionability revision, demo cBot execution, live-account fail-closed, and zero duplicate Market owner in Indicator.

## Operator action after merge

git pull --ff-only, rebuild Indicator and cBot, then re-attach the newly named algorithms in cTrader.