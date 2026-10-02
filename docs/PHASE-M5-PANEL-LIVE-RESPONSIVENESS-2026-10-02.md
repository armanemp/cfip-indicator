# M5 — Panel Live Content / Responsiveness — 2026-10-02

## Goal

Make the panel a live presentation surface that remains synchronized with canonical analysis state and the actual cBot execution state, without rebuilding the full layout on every refresh and without silently dropping important rows.

## Auto-trader / cBot truth correction

The user-visible execution status previously exposed Indicator-retained execution flags and could therefore show an "OFF" state even when the actual execution authority belonged to the cBot.

M5 changes the panel execution surface to read the cBot heartbeat as the authoritative execution state:
- market execution state;
- pending-order execution state;
- management/protection execution state;
- cBot runtime state and block reason;
- cBot heartbeat freshness.

Indicator execution parameters remain retained configuration inputs during the staged cBot migration and are no longer presented as the broker execution authority.

## Panel live refresh

Implemented:
- one current `SignalVisualSnapshot` per panel content refresh;
- fresh cBot heartbeat read before the live panel snapshot is rendered;
- centralized effective panel width/content width through `PanelDimensionRule`;
- observable row-capacity overflow instead of silent row loss;
- stale live/position/exit rows are explicitly cleared when the active plan disappears;
- full panel renderer and live content renderer now share the same effective width owner;
- execution rows explicitly identify cBot authority.

## Full-chain coherence audit

The M5 audit rechecks the existing complete chain:

`pre-analysis/context -> indicator evidence -> MTF -> decision -> actionability -> plan -> visual signal -> popup/sound event -> provider envelope -> cBot -> broker execution -> position/protection -> outcome/history`

M3 and M4 accumulated audits remain in the same Source/Architecture CI workflow.

## Deterministic contracts

Added `M5PanelContracts` covering panel width/content-width behavior at 220/430/700 and pathological padding/border inputs.

## Manual target-terminal boundary

Repository contracts can prove source ownership and deterministic behavior, but target cTrader verification is still required for:
- actual panel refresh latency and no-flicker behavior;
- actual cBot ON/OFF heartbeat presentation on the attached chart;
- panel readability at multiple widths;
- live chart/panel/alert timing;
- indicator/cBot restart/reconnect transitions.
