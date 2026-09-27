# CFIP Indicator

A clean, modular cTrader indicator implementation.

## Architecture

Market -> MTF -> Analysis -> Decision -> Entry -> Trade Plan -> Risk -> Execution -> Broker -> Lifecycle -> Live Management -> Outcome -> Presentation.

Automatic trading and automatic pending orders are supported. Manual trade-entry/order-placement controls are not part of the product.

## Development

- .NET 6 SDK
- cTrader Automate API
- VS Code + C# tooling

## Documentation

See the files under `docs/` for the roadmap, architecture, workflow, safety gates, editing guide and reference coverage.
