# Canonical Trade-Path Geometry — 2026-10-03

Status: IMPLEMENTATION COMPLETE — verification pending.

Goal:
make the live actionability calculation use the same actual-entry SL/TP/RR path that the PlanBuilder and cBot handoff use, instead of evaluating reward geometry from the presentation-oriented IdealEntry preview.

Completed:
- added CanonicalTradePathGeometry as an execution-geometry snapshot;
- added one builder that derives structural stop, risk and TP1..TP4 from ExecutionModel.ActualEntry;
- actionability now validates that its live mode and actual entry agree with the canonical path;
- actionability reward/RR validation now uses the canonical actual-entry stop/TP1;
- bounded cache reuses the path within a small entry/spread tolerance to avoid rebuilding targets on every tick;
- trace and panel state now treat ActionableNow as authoritative before generic TriggerReady presentation.

Quality boundary:
- no confidence, smart-quality, MTF, evidence, structure, RR or risk threshold was lowered;
- M15 remains the canonical trade-decision/execution reference;
- M5 remains the trigger/tuning/entry-precision layer;
- M1 remains optional confirmation;
- presentation preview remains separate from executable geometry.

Full-chain audit:
Pre-analysis -> M15 decision -> M5 tuning/zone -> M1 optional -> Entry geometry -> Structural SL -> TP1..TP4 reward path -> live actionability -> Plan -> SignalEnvelope/Scenario -> cBot preflight -> broker execution -> protection -> outcome/history

Verification:
- canonical trade-path static audit;
- accumulated Source/Architecture;
- Runtime Acceptance;
- cTrader Compile;
- target terminal validation of exact Entry/SL/TP values at proposal and broker handoff.

Operator action after verified merge: git pull --ff-only.

No profitability claim is made by this phase; empirical outcome validation remains required.