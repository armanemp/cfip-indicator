# CFIP Indicator — Architecture

## Authority chain

Market -> MTF -> Analysis -> Decision -> Entry/Trigger -> Trade Plan -> Risk -> Execution -> Broker -> Lifecycle -> Live Management -> Outcome -> Presentation.

## Ownership

**Core**: enums, arithmetic, direction and price invariants.

**Indicator**: cTrader entry point, parameters and state carrier.

**Runtime**: initialization, closed-bar context and calculation orchestration.

**Analysis**: indicators, market frame, structure, zones, liquidity and live reaction.

**Planning**: entry selection, filters, trade plan, structural stop and target ladder.

**Trading**: execution, broker mutation coordination, validation, active management and alerts.

**UI**: chart, panel, popup and historical rendering.

## Rules

There is exactly one indicator host and one automatic execution path.

The presentation layer consumes authoritative state. It never decides whether a trade should exist.

Broker mutations remain isolated to the broker/execution boundary.

The strategy plan and broker lifecycle are different states. Submission is not fill confirmation.

No module creates an alternate trade-entry engine.
