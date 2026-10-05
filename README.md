# CFIP Indicator

Clean, modular cTrader indicator with a single execution authority.

## Current state

- 513 configuration parameters are currently exposed; the count is machine-verified from the parameter source tree by `python tools/audit_parameter_count.py`.
- Strategy behavior decomposed into responsibility-isolated source modules.
- Automatic market execution and automatic pending orders are owned by the cBot; the Indicator remains the analysis/signal authority.
- Manual BUY/SELL/order-entry controls are absent.
- Multiple opportunities may be represented distinctly; execution capacity remains an explicit risk policy rather than a hidden presentation or fallback rule.
- .NET 6 production target.
- GitHub CI verifies architecture and compiles against the `cTrader.Automate` package.

## Source organization

- Core: math, text and time primitives.
- Analysis: one native indicator per file plus market, decision, reaction, structure, zone and liquidity analyzers.
- Planning: entry, trigger, filters, execution model, trade plan and target engines.
- Runtime: initialization, MTF context and calculation cycle.
- Trading: identity, pending orders, execution, lifecycle, live management, risk, validation and intelligence.
- UI: chart, panel, historical and unified alert-rail presentation.

Canonical project documents: `docs/CFIP-ROADMAP.md`, `docs/CFIP-TRADE.md`, `docs/CFIP-LIST.md`, `docs/CFIP_GATE.md`, `docs/CFIP-PROMPT.md`, and `docs/CFIP-PREPROMPT.md`. These define the active project workflow and boundaries.

Repository hygiene is mandatory: every tracked artifact must be intentional and classified; proven-unused, redundant, generated, temporary, or obsolete files must be removed or explicitly archived after dependency verification.