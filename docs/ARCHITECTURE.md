# CFIP Indicator — Modular Architecture

## Authority chain

Market data -> MTF context -> analysis -> decision -> entry/trigger -> trade plan -> risk -> execution -> broker -> lifecycle -> live management -> outcome -> presentation.

## Module rule

One cohesive responsibility owns one source module. Native indicators are isolated one-per-file. FVG, Order Block and liquidity analyzers are separate. Decision evidence/filtering are separate from decision construction. Market execution, aggressive execution, pending-order policy/placement, broker mutations, lifecycle handlers, live-management guards, prediction and UI responsibilities are separately owned.

## Runtime authority

The cTrader host remains a single `CFIPIndicator` partial type for platform compatibility. Partial files are only an implementation shell; they are not alternative engines. There is one decision authority, one strategy state, one broker identity and one automatic execution authority.

## Non-negotiable invariants

- Analytical decisions use closed-bar references.
- Entry, trigger, requested entry and actual fill remain distinct.
- Broker state is authoritative after mutations.
- Automatic market execution and automatic pending orders share one strategy identity.
- No manual BUY/SELL/order-entry controls.
- SL changes are protective-only.
- Partial close and close state are consumed only after broker confirmation.
- Telemetry timeout never ends broker/lifecycle ownership.
- CFIP-specific structure, confluence, risk and execution semantics remain authoritative even when OSS analytics are introduced.

## OSS boundary

External analytical libraries are optional adapters and benchmarking backends. They may enrich the indicator catalog or provide numerical cross-checks, but they must not introduce a second trading engine or broker authority.

## File-size rule

`Indicator/Parameters.cs` is the only intentionally large source file because cTrader exposes the parameter surface as one public configuration contract. All other production files are kept below 64 KiB by CI.