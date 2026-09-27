# Editing Guide

Change the file that owns the behavior.

| Responsibility | Owner |
|---|---|
| EMA | Analysis/Indicators/ExponentialMovingAverage.cs |
| ATR | Analysis/Indicators/AverageTrueRange.cs |
| RSI | Analysis/Indicators/RelativeStrengthIndex.cs |
| ADX | Analysis/Indicators/AverageDirectionalIndex.cs |
| DMI | Analysis/Indicators/DirectionalMovementIndex.cs |
| Market frame | Analysis/Market/MarketFrameAnalyzer.cs |
| Decision construction | Analysis/Market/Decision/DecisionEngine.cs |
| Decision evidence | Analysis/Market/Decision/DecisionEvidence.cs |
| Decision filters | Analysis/Market/Decision/DecisionFilters.cs |
| FVG | Analysis/Structure/Zones/FvgAnalyzer.cs |
| Order Block | Analysis/Structure/Zones/OrderBlockAnalyzer.cs |
| Liquidity | Analysis/Structure/LiquidityAnalyzer.cs |
| Entry trigger | Planning/Entry/EntryTriggerAnalyzer.cs |
| Execution model | Planning/Execution/ |
| Trade plan | Planning/TradePlan/ |
| Runtime | Runtime/ |
| Risk | Trading/Risk/ |
| Pending orders | Trading/Pending/ |
| Market execution | Trading/Execution/AutomaticMarket/ |
| Broker mutations | Trading/Execution/ |
| Lifecycle | Trading/Lifecycle/ |
| Live management | Trading/LiveManagement/ |
| Prediction / calibration | Trading/Intelligence/ |
| Alerts | Trading/Alerts/AlertEngine.cs |
| Chart | UI/Chart/ |
| Panel | UI/Panel/ |
| Popup | UI/Popup/PopupView.cs |

Do not create a second execution path, second decision engine or compatibility layer.