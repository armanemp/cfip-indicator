# CFIP Indicator — Editing Guide

Edit the smallest authoritative module that owns the behavior.

| Concern | Owner |
|---|---|
| Public cTrader parameters | `Indicator/Parameters/*.cs` — one file per parameter Group |
| Domain models | `Core/Models/*.cs` — one type per file |
| Enums | `Core/Enums/*.cs` — one enum per file |
| Native indicators | `Analysis/Indicators/*.cs` |
| Market-frame construction | `Analysis/Market/MarketFrameAnalyzer.cs` |
| Market-frame scoring | `Analysis/Market/MarketFrameScoring.cs` |
| Market context: volume | `Analysis/Market/VolumeExpansionAnalyzer.cs` |
| Market context: MACD | `Analysis/Market/MacdBiasAnalyzer.cs` |
| Market context: VWAP | `Analysis/Market/VwapBiasAnalyzer.cs` |
| Market context: volatility | `Analysis/Market/HealthyVolatilityAnalyzer.cs` |
| Market context: premium/discount | `Analysis/Market/PremiumDiscountAnalyzer.cs` |
| Market context: live bias | `Analysis/Market/LiveBiasAnalyzer.cs` |
| Decision | `Analysis/Market/Decision/*.cs` |
| Decision reason formatting | `Analysis/Market/Decision/DecisionReasonFormatter.cs` |
| Reaction | `Analysis/Reaction/*.cs` |
| Liquidity sweep | `Analysis/Structure/LiquiditySweepAnalyzer.cs` |
| Swing points | `Analysis/Structure/SwingPointAnalyzer.cs` |
| Equal highs/lows | `Analysis/Structure/EqualLevelAnalyzer.cs` |
| FVG detection / selection | `Analysis/Structure/Zones/FvgDetectionAnalyzer.cs` |
| FVG lifecycle / mitigation | `Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs` |
| Order Block analysis | `Analysis/Structure/Zones/OrderBlockAnalyzer.cs` |
| Order Block confluence | `Analysis/Structure/Zones/OrderBlockConfluenceAnalyzer.cs` |
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


| Outcome telemetry | Trading/Intelligence/OutcomeTelemetryEngine.cs |
| Prediction calculations | Trading/Intelligence/Prediction/ |
| Prediction rendering | UI/Chart/PredictionRenderer.cs |
| Pending-order chart rendering | UI/Chart/PendingOrderRenderer.cs |
| Outcome chart markers | UI/Chart/OutcomeMarkerRenderer.cs |

Presentation methods should be added under UI even when their callers are in Trading. Broker mutation methods must remain in their execution/lifecycle ownership directories.


| Market index/range helpers | Analysis/Market/Math/IndexMath.cs |
| Price normalization/protection math | Trading/Execution/PriceMath.cs |
| Platform-neutral numeric guards | Core/Math/NumericGuards.cs |
