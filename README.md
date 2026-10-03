# CFIP Indicator

Clean, modular cTrader indicator with a single execution authority.

## Current state

- 547 configuration parameters are currently exposed; the count is machine-verified from the parameter source tree by `python tools/audit_parameter_count.py`.
- Strategy behavior decomposed into responsibility-isolated source modules.
- Automatic market execution and automatic pending orders retained.
- Manual BUY/SELL/order-entry controls are absent.
- Execution capacity is intentionally single-plan: `Maximum Open Positions` is constrained to `1` (`MinValue=1`, `MaxValue=1`).
- .NET 6 production target.
- GitHub CI verifies architecture and compiles against the `cTrader.Automate` package.

## Source organization

- Core: math, text and time primitives.
- Analysis: one native indicator per file plus market, decision, reaction, structure, zone and liquidity analyzers.
- Planning: entry, trigger, filters, execution model, trade plan and target engines.
- Runtime: initialization, MTF context and calculation cycle.
- Trading: identity, pending orders, execution, lifecycle, live management, risk, validation and intelligence.
- UI: chart, panel, historical and unified alert-rail presentation.

See `docs/ARCHITECTURE.md`, `docs/EDITING-GUIDE.md`, `docs/ROADMAP.md`, `docs/PHASE-CR1-9-MINOR-CLEANUP.md` and `docs/OSS-COMPONENT-REGISTER.md`.