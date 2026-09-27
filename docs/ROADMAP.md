# CFIP Indicator — Roadmap

## Goal

Deliver the complete indicator from the complete behavioral reference without dropping features and without creating duplicate engines or compatibility layers.

## Completed in the clean baseline

- All reference parameters are preserved: 513/513.
- Reference method parity is complete across the split modules.
- The single cTrader host is normalized to `CFIPIndicator`.
- Versioned class, file, namespace, identity and migration naming has been removed.
- The implementation is split into responsibility-based partial modules.
- Automatic market execution and automatic pending-order logic are retained.
- Smart structural SL/TP, lifecycle, live management, prediction, alerts, panel, popup and historical rendering are retained.
- No manual BUY/SELL/order-entry controls are introduced.

## Remaining acceptance gates

1. Compile the solution against the actual cTrader Automate API DLL installed with the target cTrader build.
2. Run controlled cTrader scenarios for market execution, pending orders, rejection, slippage, protection recovery, partial close, close confirmation, restart/reconnect reconciliation, reversal, invalidation and end-of-day handling.
3. Verify chart/panel/popup rendering on the target cTrader build.
4. Review runtime resource usage and remove only proven inefficiencies.

## Non-negotiable behavior

- Automatic trading and automatic pending orders share the same strategy state and broker identity.
- Smart SL/TP are strategy-generated.
- Entry, trigger, requested entry and actual fill remain distinct.
- Broker state is authoritative after mutations.
- Lifecycle follows broker reality.
- Partial close and close are confirmed by broker state before their state is consumed.
- SL moves only in the protective direction.
- Analytical decisions use closed-bar references.
- BUY and SELL remain symmetric.
- No hidden fallback changes semantics.
