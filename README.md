# CFIP Indicator

Clean, modular cTrader indicator.

## Source organization

The implementation is split by responsibility while remaining one cTrader indicator and one execution authority:

- Core
- Indicator
- Runtime
- Analysis
- Planning
- Trading
- UI

Automatic market execution and automatic pending-order placement are part of the execution path. Manual BUY/SELL/order-entry controls are not included. Close/cancel safety controls remain available for managed broker objects.

## Build

Target: .NET 6.

The project references the cTrader Automate API from cTrader's standard local installation. Set `CFIP_CTRADER_API` when a custom DLL path is required.

No third-party NuGet packages are required.

## Baseline

Behavior is migrated from the complete reference implementation. The reference is treated as behavioral source material; source names, files and internal identifiers are normalized to the clean project vocabulary.
