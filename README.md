# CFIP Indicator

Clean, modular cTrader indicator with a single execution authority.

## Current state

- 533 configuration parameters are currently exposed (530 baseline + 3 OSS extension parameters).
- Strategy behavior decomposed into responsibility-isolated source modules.
- Automatic market execution and automatic pending orders retained.
- Manual BUY/SELL/order-entry controls are absent.
- .NET 6 production target.
- GitHub CI verifies architecture and compiles against the `cTrader.Automate` package.

## Source organization

- Core: math, text and time primitives.
- Analysis: one native indicator per file plus market, decision, reaction, structure, zone and liquidity analyzers.
- Planning: entry, trigger, filters, execution model, trade plan and target engines.
- Runtime: initialization, MTF context and calculation cycle.
- Trading: identity, pending orders, execution, lifecycle, live management, risk, validation and intelligence.
- UI: chart, panel, historical and popup renderers.

See `docs/ARCHITECTURE.md`, `docs/EDITING-GUIDE.md`, `docs/ROADMAP.md` and `docs/OSS-COMPONENT-REGISTER.md`.