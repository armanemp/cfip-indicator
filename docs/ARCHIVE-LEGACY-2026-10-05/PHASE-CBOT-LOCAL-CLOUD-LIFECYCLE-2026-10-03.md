# cBot Local/Cloud Lifecycle — 2026-10-03

Status: IMPLEMENTED ON MAIN — automated verification pending; target-terminal verification remains required.

## Objective

Remove the lifecycle path that can repeatedly re-enter cTrader's custom-algorithm resolution when the cBot restarts, reloads or loses its Indicator binding.

## Root cause

The cBot previously tried to recover a missing same-chart Indicator by calling ChartIndicators.Add("CFIP Smart Indicator"). That operation is algorithm resolution/creation, not simple binding. During cBot restart or reload, a missing chart-bound Indicator could therefore re-enter cTrader's algorithm-selection path. Recent explicit Indicator display-name and assembly-identity changes make that lifecycle seam especially important.

The intended ownership is:

Operator attaches local Indicator → cBot observes ChartIndicators.Custom → exact instance binding → heartbeat/state.

Not:

cBot restart → programmatic Indicator creation → platform algorithm resolution.

## Implementation

- Removed ChartIndicators.Add(...) from cBot lifecycle binding.
- Removed obsolete _indicatorAutoAttachAttempted state.
- Missing Indicator now fails closed with an explicit ATTACH LOCAL INDICATOR diagnostic.
- Existing IndicatorAdded/Removed/Modified lifecycle events remain, so a later manual attachment is rebound immediately.
- Stable Indicator/cBot type identities and deterministic assembly/algo names remain unchanged.
- No Cloud transport or cloud-dependent Indicator execution was introduced.

## Local vs Cloud boundary

The intended local setup is a local custom Indicator attached to the same chart as the local cBot. The cloud path is a separate cBot-host concern; the custom Indicator is not treated as a cloud execution dependency.

## Verification

Automated: Source/Architecture, Runtime Acceptance, cTrader Compile/Build, accumulated cBot attachment/identity/lifecycle audits, and the dedicated local/cloud lifecycle audit.

Target-terminal: attach CFIP Smart Indicator manually, start CFIP Smart Execution Bot on the same chart, stop/restart the cBot repeatedly, modify/reload the Indicator, and verify that binding reuses the existing Indicator instance without repeatedly invoking a Local/Cloud selection flow. Removing the Indicator must yield a blocked/fail-closed state rather than creating another instance.

The repository can verify the lifecycle contract, but native cTrader UI prompt behavior still requires terminal evidence.

## Safety and architecture

No strategy threshold, signal quality threshold, Entry/SL/TP/RR rule, risk limit or broker-mutation authority changed.

Execution boundary remains: Indicator analysis/signal/plan/presentation → shared contract → cBot → broker.

Operator action after verified merge: git pull --ff-only