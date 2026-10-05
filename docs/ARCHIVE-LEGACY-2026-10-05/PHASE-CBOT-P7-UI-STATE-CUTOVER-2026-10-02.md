# CBOT-P7 — UI / State Cutover — 2026-10-02

Status: IMPLEMENTATION COMPLETE — verification pending.

Canonical execution architecture:
- Indicator: pre-analysis, M15 decision, M5 trigger/tuning/entry precision, M1 optional confirmation, H1+ context/reward, signal, chart, alerts and presentation.
- cBot: broker execution, account/execution risk, lifecycle, protection, recovery and effective execution state.
- Contracts: immutable Indicator→cBot boundary.
- Chart timeframe: presentation/host only; execution remains M15-based.

Completed in P7:
- Added effective Auto Trade / Auto Orders fields to CbotExecutionStateSnapshot.
- cBot publisher derives effective states from bound Indicator settings + cBot execution capability + runtime/lifecycle/recovery state.
- Indicator quick controls are status-only and read only the cBot snapshot.
- Indicator panel separates cBot connection, configuration-off, cBot-disarmed, blocked and recovery states instead of presenting a generic OFF.
- Added event-driven Indicator refresh for cBot attach/remove/modify/start/stop through ChartRobots.
- Kept LocalStorage heartbeat freshness as execution liveness proof, but no longer uses heartbeat absence as sole physical connection proof.
- Removed execution-runtime synchronization from the status-control synchronizer to prevent UI from becoming a hidden execution authority.
- Bounded cBot state refresh cadence to preserve panel responsiveness.

End-to-end audit performed in this phase:
pre-analysis → M15 decision → M5 trigger/tuning → entry geometry → signal → popup/sound → contract → cBot connection/binding → cBot execution/risk → broker mutation → broker-confirmed lifecycle/protection → panel reflection.

Important UI truth:
- AUTO TRADE/AUTO ORDERS controls cannot mutate broker state.
- Their ON/OFF status comes from the effective cBot snapshot, not a private Indicator toggle.
- A connected-but-disarmed cBot is displayed as connected + disarmed, not as disconnected.

Verification:
- Source / Architecture
- Runtime Acceptance
- cTrader Compile
- dedicated P7 audit
- accumulated architecture audits
- target-terminal manual validation still required for actual chart attachment, start/stop, parameter modification, restart/reload, live visual panel refresh and observed broker lifecycle.

After merge: git pull --ff-only
