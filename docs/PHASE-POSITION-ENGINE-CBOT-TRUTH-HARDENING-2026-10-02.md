# Position Engine / cBot Truth Hardening — 2026-10-02

## Status

Implementation complete on branch `phase/position-engine-cbot-truth-hardening-2026-10-02`; repository verification runs through the pull-request gates.

## Scope

This phase stays concentrated on:

`pre-analysis → M15 decision → M5 trigger/tuning → execution-zone discovery → structural SL → reward-path TP → actionability → alert/message → cBot binding → broker-object discovery`.

## Position discovery

The execution-zone selector is now a canonical scored comparison rather than a fixed-priority chain.

Compared families:

- M5 FVG
- M5 Order Block
- M15 FVG
- M15 Order Block
- M5 FVG+OB
- M15 FVG/OB overlap
- M15/H1 structural levels

Zone candidates are re-scored with downstream structural-stop quality and best attainable TP1 RR. A candidate with no valid reward path is not selected as the execution zone. Existing actionability, RR, risk and broker-safety gates remain intact.

Structural-stop and forward-target FVG collection are allowed to evaluate valid historical/unretested zones instead of requiring a current-bar retest at the discovery boundary.

## cBot truth and broker-object discovery

The cBot now publishes a symbol-scoped presence heartbeat immediately on STARTING, RUNNING and STOPPED states.

Indicator execution capability still requires the exact fresh Indicator-instance heartbeat. Presence only tells the panel that a compatible cBot exists on the symbol; it never grants execution authority.

Managed broker objects are matched through one cBot identity rule. Exact execution label remains preferred, while the stable `|CFIP-I:<IndicatorInstanceId>` scope allows existing positions/pending orders to remain discoverable after the user changes the base `AutoTradeLabel`.

Reconciliation also reruns immediately when that managed label changes instead of waiting for the periodic cadence.

## Panel

The alert rail now uses:

- left-aligned text;
- larger readable font (minimum effective size 10);
- 20 px row height;
- the same bounded canonical alert queue used by sound delivery.

## Safety boundary

M15 remains the trade-decision/execution reference timeframe. M5 remains trigger/tuning/entry precision. M1 is optional confirmation and H1+ provides context/reward support.

This phase does not lower public quality/RR/risk thresholds and does not remove the single-plan broker-capacity gate. Parallel scenario presentation remains possible; broker execution capacity is a separate lifecycle/reconciliation phase.

## Verification

The dedicated source regression gate is:

`tools/audit_phase_position_engine_cbot_truth_hardening_2026_10_02.py`

Target-terminal checks still required:

- indicator + cBot startup in either order;
- cBot renamed instance still recognized;
- cBot stop/restart reflected as DETECTED/STOPPED/CONNECTED states;
- existing managed position found after AutoTradeLabel change;
- Entry/SL/TP geometry and RR on live M15/M5 data;
- simultaneous visible scenarios;
- sound/message alignment.

No empirical profitability claim is made by repository verification.

## Operator action after merge

Run:

`git pull --ff-only`

on local `main`.
