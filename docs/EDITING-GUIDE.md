# Editing Guide

Use the file that owns the behavior.

| Behavior | File |
|---|---|
| Enums / indicator entry point | Core/Enums.cs, Indicator/CFIPIndicator.cs |
| Parameters | Indicator/Parameters.cs |
| Runtime / closed-bar cycle | Runtime/Lifecycle.cs |
| Native indicators | Analysis/Indicators.cs |
| Market frame | Analysis/Market.cs |
| Live reaction | Analysis/Reaction.cs |
| Entry / trigger | Planning/Entry.cs |
| Trade plan / stop / targets | Planning/TradePlan.cs |
| Filters | Planning/Filters.cs |
| Active plan management | Trading/ActiveManagement.cs |
| Structural validation | Trading/Validation.cs |
| Broker execution and pending orders | Trading/Execution.cs |
| Chart | UI/Chart.cs |
| Panel | UI/Panel.cs |
| Popup | UI/Popup.cs |
| Alerts | Trading/Alerts.cs |
| Historical rendering | UI/Historical.cs |
| Shared utilities | Core/Utilities.cs |

Do not add compatibility names or a second execution path. Update the authoritative owner and then update callers and tests.
