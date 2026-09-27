# CFIP Indicator — Editing Guide

Edit the smallest authoritative module that owns the behavior.

| Concern | Owner |
|---|---|
| Public cTrader parameters | `Indicator/Parameters/*.cs` — one file per parameter Group |
| Domain models | `Core/Models/*.cs` — one type per file |
| Enums | `Core/Enums/*.cs` — one enum per file |
| Native indicators | `Analysis/Indicators/*.cs` |
| Market context / frame | `Analysis/Market/*.cs` |
| Decision | `Analysis/Market/Decision/*.cs` |
| Reaction | `Analysis/Reaction/*.cs` |
| Structure / FVG / Order Block / liquidity | `Analysis/Structure/**/*.cs` |
| Entry / trigger | `Planning/Entry/*.cs` |
| Execution model / trigger validation | `Planning/Execution/*.cs` |
| Trade plan / stop / targets | `Planning/TradePlan/*.cs` |
| Runtime / MTF / calculation | `Runtime/**/*.cs` |
| Automatic market execution | `Trading/Execution/AutomaticMarket/*.cs` |
| Aggressive execution | `Trading/Execution/Aggressive/*.cs` |
| Pending orders | `Trading/Pending/**/*.cs` |
| Broker mutation coordination | `Trading/Execution/BrokerMutationCoordinator.cs` |
| Broker identity | `Trading/Identity/*.cs` |
| Risk / suitability | `Trading/Risk/*.cs` |
| Lifecycle events | `Trading/Lifecycle/*.cs` |
| Live management | `Trading/LiveManagement/*.cs` |
| Prediction / intelligence | `Trading/Intelligence/**/*.cs` |
| Alerts | `Trading/Alerts/*.cs` |
| Validation | `Trading/Validation/*.cs` |
| Chart | `UI/Chart/**/*.cs` |
| Panel | `UI/Panel/**/*.cs` |
| Popup | `UI/Popup/**/*.cs` |
| Historical rendering | `UI/Historical/**/*.cs` |
| Shared math/text/time utilities | `Core/{Math,Text,Time}/*.cs` |

Do not add compatibility aliases, duplicate business rules or a second execution path. Update the authoritative owner, migrate callers, remove the old owner, then run static and runtime acceptance.
