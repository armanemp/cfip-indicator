## Permanent Development Rule — Canonical Owners / No Patches

For every future phase, modify the existing canonical production owner directly. Do not create parallel hotfix files, duplicate executors, compatibility wrappers, alternate calculation paths, alternate identity formatters, or detached patch subsystems when the existing owner can be corrected. Any obsolete owner created by an extraction must be deleted in the same phase, and all audits/docs must point to the single surviving owner.

## Build Warning / Panel Height Integrity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #198 as `396c72513fc5043bd348e5ceb5c74894da72c395`.**

The locally reported nine Release-build warnings are resolved: transport nullability is explicit,
dead Indicator state is removed, panel geometry no longer reads `Chart.Height`, and overlay
`AutoRescale` is disabled so the invisible provider heartbeat cannot affect price scaling.

Verification on the feature head:
- cTrader Compile/Build workflow #3064: **PASS**;
- Runtime Acceptance workflow #2880: **PASS**;
- reported warning files: **no warning/error diagnostics** in the compile log;
- CBOT-P4B/P4C/P4D/P4E plus accumulated architecture/parameter/UI audits: **PASS**.

The broader Planning Contracts project still has pre-existing `CS0649` warnings in
`TradeOpportunityCandidate.cs`; they are separate from the nine warnings reported locally
and remain outside this phase.

Target-terminal chart/panel verification remains manual.

Phase record: `docs/PHASE-BUILD-WARNING-PANEL-HEIGHT-INTEGRITY-2026-10-02.md`.

Next staged phase: **CBOT-P5 — Protection / Lifecycle / Recovery completion**.

Scope:
- eliminate the 9 locally observed Release-build warnings without null-suppression or artificial field references;
- make nullable transport initialization explicit in the shared Contracts project;
- remove dead last-pending-signal state;
- remove the panel's remaining Chart.Height-derived geometry feedback path;
- disable Indicator AutoRescale because the transparent Provider Heartbeat writes a non-price provider revision series;
- preserve the canonical overlay panel, bounded ScrollViewer, 50px bottom clearance and cBot execution boundary;
- add a dedicated acceptance audit covering warning cleanup and chart-height safety.

Root cause:
- CFIPReadOnlyProvider writes the provider revision to a transparent output series;
- cTrader documents AutoRescale as chart auto-rescaling and its default as enabled; the revision is metadata, not price data;
- panel maximum-height resolution was still reading Chart.Height, so a transient chart viewport could influence the next panel geometry calculation.

Invariant:
Panel geometry and indicator chart scaling must never be driven by provider metadata or a transient chart viewport.

Phase record: docs/PHASE-BUILD-WARNING-PANEL-HEIGHT-INTEGRITY-2026-10-02.md.

Next staged phase after acceptance: CBOT-P5 — Protection / Lifecycle / Recovery completion; signal-quality/target-quality work remains a separate evidence-driven phase.
## CBOT-P4E — Management Command + Remaining Broker Mutation Authority — 2026-10-02

Status: **VERIFIED COMPLETE — merged to `main` via PR #197 as `996cd9cb4872608674269c205b7a03b33ab9e812`.**

The Indicator's remaining broker mutation paths are command-only; `ManagementExecutionCoordinator` in the cBot is the single owner for cancellation, full/partial close, SL, absolute TP, TP-by-pips and server TP ladder mutation.

Verification on final merged implementation:
- Source / Architecture workflow #3064: **PASS**;
- Runtime Acceptance workflow #2873: **PASS**;
- cTrader Compile/Build workflow #3057: **PASS**;
- CBOT-P4E audit: **PASS**;
- accumulated architecture/execution/UI/identity audits: **PASS**.

Target-terminal broker execution verification remains manual. No profitability claim is made from this structural migration.

Phase record: `docs/PHASE-CBOT-P4E-MANAGEMENT-AUTHORITY-2026-10-02.md`.

Next staged phase: **CBOT-P5 — Protection / Lifecycle / Recovery completion**.

## CBOT-P4D — Pending Limit Authority + Signal/Popup Continuity — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #196 as 352e6229adcff8a4ebb6ee6e5c71a0e0397dc70b.**

Scope:
- move Pending Limit broker mutation completely to the existing cBot pending execution owner;
- remove the obsolete Indicator Pending Limit broker owner;
- keep Indicator responsible for analysis, scenario, intent and absolute lifecycle snapshot only;
- expose a dedicated cBot Pending Limit arm while preserving demo-only/fail-closed execution;
- restore arrow-only direction visibility for early/watch/confirmed/strong states using three directional intensity colors;
- keep popup at BottomRight, persistent until next alert/manual close, and restrict popup delivery to important canonical alert families;
- preserve M15 internal execution independence from host Chart TF.

No new strategy engine, duplicate signal engine, duplicate broker owner, or alternate label formatter is introduced.

Repository verification on final P4D implementation head 22d7af189d7037237b27a3df09a42a9cdda7f252:
- Source / Architecture: **PASS**;
- Runtime Acceptance Contracts: **PASS**;
- cTrader Compile/Build: **PASS**;
- CBOT-P4D acceptance audit: **PASS**;
- accumulated execution/UI/identity audits: **PASS**.

Target-terminal acceptance remains manual. No profitability claim is made from this structural/UI phase alone.

Next staged execution phase: **CBOT-P4E — full remaining broker execution authority consolidation (cancellation, protection, partial TP, break-even, trail, close/recovery) in the cBot, with no duplicate Indicator mutation owners.**

## CBOT-P4C — Pending Stop Authority + Host-Timeframe Independence — 2026-10-02

Status: **VERIFIED COMPLETE — merged to main via PR #195 as 7b8648091bde26753b1e0fcff3d75984a1f1b9eb.**

Completed:
- M15 is the internal execution clock; Chart timeframe is host/presentation-only;
- Indicator/cBot runtime no longer rejects or branches on host Chart TF for execution;