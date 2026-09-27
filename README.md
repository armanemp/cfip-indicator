# CFIP Indicator

Clean, modular cTrader indicator source.

## Architecture

The implementation is organized by responsibility:

- Core and state
- Market and analysis
- Planning
- Trading and risk
- Runtime/lifecycle
- Chart and presentation

The execution path is automatic. Manual BUY/SELL entry controls are not included. Safety controls for closing managed positions and cancelling managed pending orders remain available.

## Build

The project targets .NET 6 and references the cTrader Automate API DLL supplied by cTrader.

Set the MSBuild property `CFIP_CTRADER_API` to the full path of `cAlgo.API.dll`, then build the solution.

The project does not add third-party NuGet dependencies.

## Source baseline

Behavior is maintained against the complete reference implementation while the source is split into responsibility-based files. The split does not introduce a compatibility layer or parallel execution engine.
