# CFIP Indicator

Standalone multi-file C# implementation of the CFIP cTrader trading system.

This repository is a clean architectural migration from the frozen CFIP-PRO v89 reference implementation. v89 is a behavioral/reference source, not the target file layout.

## Principles
- Preserve validated behavior during migration.
- One authoritative owner for each trading concept.
- Analysis has no broker side effects.
- Decision is not execution.
- TradePlan is not broker state.
- BrokerGateway is the only broker mutation boundary.
- Lifecycle follows broker reality.
- UI is downstream.
- Automatic trading and automatic pending orders are the product model; no manual entry/order buttons.
- BUY/SELL logic must remain symmetric.
- No silent fallbacks.
- Every public parameter has a disposition.

## Toolchain
- Windows 11
- .NET SDK 6.0.428
- cTrader API: %USERPROFILE%\Documents\cAlgo\API\cAlgo.API.dll
- VS Code + C# Dev Kit

## Current migration status

The v89 implementation surface is now decomposed into 107 C# files: 101 top-level declarations plus seven cTrader host partials. All 512 public parameters are preserved, and the nine v89 service interfaces have been extracted. Core domain types are isolated under `CFIP.Indicator.Core`.

Runtime acceptance is intentionally still pending. The project must pass real cTrader compilation and controlled broker scenarios before a release is declared.

## Documents
- docs/ROADMAP.md
- docs/ARCHITECTURE.md
- docs/WORKFLOW.md
- docs/MIGRATION.md
