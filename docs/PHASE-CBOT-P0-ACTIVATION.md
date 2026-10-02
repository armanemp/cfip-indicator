# CBOT-P0 — Activation / Boundary Lock

Date: 2026-10-02

Status: **IMPLEMENTATION COMPLETE — verification pending on branch.**

Branch: `phase/cbot-parallel-separation-start`

## Objective

Activate the cBot separation as a mandatory parallel track from the current roadmap point so execution/account/lifecycle migration no longer waits until M29–M38.

## Completed

- Added the active parallel CBOT-P0→P8 schedule to `docs/ROADMAP.md`.
- Aligned `docs/CBOT-SEPARATION-ROADMAP.md` so the detailed M29–M38 material is retained as reference while the active migration starts now.
- Created `src/CFIP.Contracts` as a platform-neutral project boundary.
- Created `src/CFIP.cBot` as an independent cTrader Robot project.
- Registered both projects in `CFIP.Indicator.sln`.
- Added a fail-closed cBot host with no broker mutation APIs in P0.
- Added Contracts/cBot builds to the permanent cTrader compile workflow.
- Explicitly froze the Indicator against adding any new broker mutation authority.

## Current ownership

Indicator:
analysis, evidence, decision, trigger, scenario, plan, panel, chart and alert presentation.

Contracts:
immutable/data-only cross-boundary types.

cBot:
broker execution, account risk, broker-confirmed protection, lifecycle and recovery — to be physically migrated through CBOT-P4/P5/P6/P7/P8.

## Important safety rule

Existing Indicator broker mutation remains temporarily only while each replacement is being built. For every migrated path the required order is:

replacement → caller migration → deterministic parity → source gate → physical deletion of the old Indicator owner.

This prevents both functionality loss and a permanent dual executor.

## Verification

Required:
- Source / Architecture
- Runtime Acceptance Contracts
- cTrader Compile, including the new Contracts and cBot project builds

No real-account execution is enabled by P0.

## Next

**CBOT-P1 — Platform-Neutral Contracts**: define the complete immutable Signal/Plan/Execution/Management/Broker/Lifecycle contract set and begin the read-only Indicator provider boundary.
