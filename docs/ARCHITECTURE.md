# CFIP Indicator — Modular Architecture

## Authority chain

Market data -> MTF context -> indicators -> analysis -> decision -> entry/trigger -> trade plan -> risk -> execution -> broker -> lifecycle -> live management -> outcome -> presentation.

## Module rules

The source tree is organized by responsibility and artifact ownership.

- Every native technical indicator has its own source file.
- Every external/OSS indicator adapter has its own source file.
- Every market, regime, structure, zone, liquidity and reaction analyzer has a dedicated owner.
- Every decision concern has a dedicated owner.
- Every execution mode and execution validation concern has a dedicated owner.
- Every lifecycle event handler has its own source file.
- Every renderer has its own source file.
- Every domain model has its own source file.
- Every enum has its own source file.
- Every public cTrader parameter group has its own source file under `Indicator/Parameters/`.
- Production source files do not contain historical version identifiers.

The cTrader host remains a single `CFIPIndicator` partial type solely for platform compatibility. Partial files are implementation units, not alternative engines.

## Runtime authority

There is exactly one decision authority, one strategy state, one managed broker identity and one automatic execution authority.

The presentation layer consumes authoritative state and never decides whether a trade should exist. Broker mutations are isolated to the trading boundary.

## Non-negotiable invariants

- Analytical decisions use fully closed-bar references at one UTC reference.
- Entry, trigger, requested entry and actual fill remain distinct.
- Broker state is authoritative after mutations.
- Automatic market execution and automatic pending orders share one strategy identity.
- Manual BUY/SELL/order-entry controls do not exist.
- SL changes are protective-only.
- Partial closes and closes are consumed only after broker confirmation.
- Telemetry timeout never transfers or ends broker/lifecycle ownership.
- CFIP-specific structure, confluence, risk and execution semantics remain authoritative.
- Historical version/source identifiers are documentation-only and never production runtime identifiers.

## OSS boundary

External analytical libraries are optional adapters and benchmarks. They may extend numerical indicators or provide cross-checks, but they cannot become a second decision or trading engine.

All OSS assets, adapter notes, license information and benchmark material belong under `oss/`. Production adoption requires target-runtime compatibility and explicit verification.

## Dependency direction

Core models and utilities are platform-neutral. cTrader-specific Bars, native-indicator instances, broker types and chart controls live in their owning analysis/trading/UI layers.

Core models may describe strategy state, but must not require the cTrader API merely to exist.

Analysis may consume market data and configuration, but must not mutate broker state.

Planning may create plans/intents, but must not submit or mutate broker objects.

Trading may execute broker mutations, but must not invent alternative analytical authority.

Presentation consumes state and renders it; it does not decide or trade.

## Decision service boundary

Decision input snapshots are immutable data carriers. Pure decision services perform no broker mutation, chart mutation or executable callback dispatch. Direction-specific evidence is captured for both BUY and SELL before consensus selection so neutral states cannot inherit directional evidence. Decision filtering is an ordered pipeline of narrow gate owners; the pipeline itself only orchestrates gate results.

## File ownership

A source file should contain one primary production artifact and one coherent responsibility. Domain types, enums, indicators, analyzers, execution policies, lifecycle handlers, renderers and adapters are separated into dedicated files.

The remaining cTrader partial host files are permitted only for platform callbacks or a single cohesive behavior boundary. They must not hide nested helper types or unrelated responsibilities.

## Production source hygiene

Production `.cs` files must not contain:

- release/version suffixes;
- historical source class names;
- migration aliases;
- compatibility names for obsolete versions;
- numbered execution comments/identifiers.

Historical baseline/version information is allowed only in `docs/ROADMAP.md` or workflow material when it is necessary for continuity.

## File-size rule

All production `.cs` files must remain below 64 KiB.

## Presentation boundary

Chart object creation/removal and visual renderers live only under UI modules. Trading and intelligence modules may request presentation through authoritative state, but they do not own chart mutations.

## Broker boundary

Broker mutation calls are restricted to the execution, pending, lifecycle and live-management boundaries.

## Decision ownership

- Input/evidence snapshot: Analysis/Market/Decision/DecisionInput*.cs and DecisionEvidenceSnapshot.cs.
- Score, consensus, quality and confidence: dedicated Decision*Calculator.cs services.
- Decision threshold and smart consensus: dedicated pure filter evaluators.
- Confirmation, smart, structure, market and lifecycle gates: dedicated gate modules.
- Reason formatting: DecisionReasonFormatter.cs; DecisionReasonBuilder.cs only composes the formatter.
- Intelligence helpers remain single-purpose modules and do not become a second decision authority.

## Core boundary

Core contains domain models, enums, numeric guards, text helpers and generic time-window parsing only. cTrader-dependent index and broker-price helpers live outside Core.
