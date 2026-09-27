# CFIP Indicator — Modular Architecture

## Authority chain

Market data -> MTF context -> indicators -> analysis -> decision -> entry/trigger -> trade plan -> risk -> execution -> broker -> lifecycle -> live management -> outcome -> presentation.

## Module rules

The source tree is organized by responsibility and by artifact ownership:

- Every native technical indicator has its own source file.
- Every market/structure/zone/liquidity analyzer has its own source file.
- Every decision concern has its own source file.
- Every execution mode and execution validation concern has its own source file.
- Every lifecycle event handler has its own source file.
- Every chart/panel/popup renderer has its own source file.
- Every domain model has its own source file.
- Every cTrader parameter group has its own source file under `Indicator/Parameters/`.
- No production source file is allowed to become a second monolith.

The cTrader host remains a single `CFIPIndicator` partial type solely for platform compatibility. Partial modules are implementation units, not alternative engines.

## Runtime authority

There is exactly one decision authority, one strategy state, one broker identity and one automatic execution authority.

The presentation layer consumes authoritative state and never decides whether a trade should exist. Panel row composition is split by semantic section; row mutation is isolated in a dedicated writer. Broker mutations are isolated to the trading/execution boundary.

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

## OSS boundary

External analytical libraries are optional adapters/benchmarks. They may extend numerical indicators or provide cross-checks, but they cannot become a second decision or trading engine.

## File-size rule

All production `.cs` files must remain below 64 KiB. Configuration is split by parameter group, so there is no special large-file exception.


## Type ownership

- No nested production helper/model classes are permitted inside the cTrader host.
- Runtime value objects such as the closed-bar MTF context have dedicated files.
- Cache-entry/support types also have dedicated files.
- A partial CFIPIndicator file may contain only the host surface or one cohesive behavior module; it must not hide unrelated helper types.


## Dependency direction

Core models and utilities are platform-neutral. cTrader-specific Bars, native-indicator instances, broker types and chart controls live in their owning analysis/trading/UI layers. A Core model may describe strategy state, but must not require the cTrader API merely to exist.


## Presentation boundary

Chart object creation/removal and visual renderers live only under UI/Chart, UI/Historical, UI/Panel or UI/Popup. Trading and Intelligence modules may request presentation through shared partial methods, but they do not own chart mutations.

Broker mutation calls are restricted to the execution, pending, lifecycle and live-management boundaries. Analysis, planning, intelligence and UI cannot directly mutate broker state.


## Core boundary

Core is platform-neutral. It contains domain models, numeric guards, text helpers and generic time-window parsing only. cTrader-dependent bar/index and price normalization helpers are owned by Market and Trading/Execution respectively.
