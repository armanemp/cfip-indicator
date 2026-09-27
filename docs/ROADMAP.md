# CFIP Indicator — Roadmap

## Goal

Deliver the complete indicator from the complete behavioral reference without dropping features and without creating duplicate engines or compatibility layers.

## Implementation order

1. Clean repository foundation
2. Complete parameter and behavior parity
3. Market data, time and MTF
4. Indicators and market model
5. Structure, FVG, Order Block and liquidity
6. Decision and confluence
7. Entry and trigger
8. Risk, structural SL and target ladder
9. Unified automatic execution
10. Pending-order lifecycle
11. Position lifecycle and reconciliation
12. Live position management
13. Outcome telemetry and calibration
14. Chart, panel, popup, alerts and historical presentation
15. Performance and cleanup
16. Compile/runtime acceptance

## Non-negotiable behavior

- Automatic trading and automatic pending orders share the same strategy state and broker identity.
- No manual BUY/SELL/order-entry controls.
- Smart SL/TP are strategy-generated.
- Entry, trigger, requested entry and actual fill remain distinct.
- Broker state is authoritative after mutations.
- Lifecycle follows broker reality.
- Partial close and close are confirmed by broker state before their state is consumed.
- SL moves only in the protective direction.
- Analytical decisions use closed-bar references.
- BUY and SELL remain symmetric.
- No hidden fallback changes semantics.
