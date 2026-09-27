using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CFIP.Indicator;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
                        protected override void Initialize()
                {
                    _m1Bars = MarketData.GetBars(TimeFrame.Minute);
                    _m5Bars = MarketData.GetBars(TimeFrame.Minute5);
                    _m15Bars = MarketData.GetBars(TimeFrame.Minute15);
                    _m30Bars = MarketData.GetBars(TimeFrame.Minute30);
                    _h1Bars = MarketData.GetBars(TimeFrame.Hour);
                    _h4Bars = MarketData.GetBars(TimeFrame.Hour4);
                    _d1Bars = MarketData.GetBars(TimeFrame.Daily);
                    _w1Bars = MarketData.GetBars(TimeFrame.Weekly);
        
                    _configuration =
                        ConfigSnapshot.Build(
                            this,
                            "CFIP");
        
                    _runtimeAuthority =
                        new RuntimeAuthority();
        
                    _runtimeAuthority.InitializeFromConfiguration(
                        _configuration);
        
                    _marketModelBuilder =
                        new MarketModelBuilder(
                            Indicators);
        
                    _structureBuilder =
                        new StructureLedgerBuilder();
        
                    _decisionEngine =
                        new DecisionEngine();
        
                    _entryTriggerEngine =
                        new EntryTriggerEngine();
        
                    _tradePlanBuilder =
                        new TradePlanBuilder();

                    _predictionEngine =
                        new PredictionEngine();

                    _suitabilityEngine =
                        new MarketSuitabilityEngine();
        
                    _executionPolicy =
                        new ExecutionPolicy();
        
                    _executionPlanner =
                        new ExecutionPlanner();
        
                    _brokerGateway =
                        new CTraderBrokerGateway(this);
        
                    _brokerStateReader =
                        new CTraderBrokerStateReader(this);
        
                    _pendingOrderLifecycle =
                        new PendingOrderLifecycleManager(
                            _configuration.Get(
                                "AutoTradeLabel",
                                "CFIP-SMART"));
        
                    _positionLifecycle =
                        new PositionLifecycleManager(
                            _configuration.Get(
                                "AutoTradeLabel",
                                "CFIP-SMART"));
        
                    _livePositionManager =
                        new LivePositionManager();
        
                    _state = new EngineState();
                    _lifecycle = new LifecycleManager();
        
                    PendingOrders.Created += PendingOrders_Created;
                    PendingOrders.Modified += PendingOrders_Modified;
                    PendingOrders.Filled += PendingOrders_Filled;
                    PendingOrders.Cancelled += PendingOrders_Cancelled;
                    Positions.Opened += Positions_Opened;
                    Positions.Modified += Positions_Modified;
                    Positions.Closed += Positions_Closed;
        
                    foreach (var position in Positions)
                    {
                        _positionLifecycle.RegisterOpened(
                            position,
                            DateTime.MinValue,
                            position.StopLoss,
                            position.TakeProfit);
        
                        _livePositionManager.Register(
                            position,
                            _state != null ? _state.Plan : null);
                    }
        
                    foreach (var order in PendingOrders)
                        _pendingOrderLifecycle.RegisterExisting(
                            order,
                            DateTime.MinValue);
        
                }
        
                public override void Calculate(int index)
                {
                    DateTime serverUtc =
                        TimeInUtc;
        
                    DateTime userLocalTime =
                        serverUtc +
                        Application.UserTimeOffset;
        
                    _state.ResetCycleOutputs();
        
                    _state.SetRuntime(
                        new RuntimeSnapshot(
                            serverUtc,
                            SymbolName,
                            Math.Max(0, Symbol.Bid),
                            Math.Max(0, Symbol.Ask),
                            Math.Max(0, Symbol.PipSize),
                            Symbol.PipSize > 0
                                ? Math.Max(
                                    0,
                                    (Symbol.Ask - Symbol.Bid) /
                                    Symbol.PipSize)
                                : 0,
                            Server.IsConnected &&
                            Symbol.IsTradingEnabled,
                            Math.Max(0, Account.Equity),
                            Math.Max(0, Account.FreeMargin),
                            Math.Max(0, Account.Balance),
                            Math.Max(0, Account.Margin),
                            Account.MarginLevel.HasValue
                                ? Math.Max(0, Account.MarginLevel.Value)
                                : 0,
                            CalculateDailyRealizedNetProfit(serverUtc.Date),
                            serverUtc.Date,
                            new BrokerConstraints(
                                Math.Max(0, Symbol.VolumeInUnitsMin),
                                Math.Max(0, Symbol.VolumeInUnitsStep),
                                ConvertMinimumDistanceToPips(
                                    Symbol.MinStopLossDistance),
                                ConvertMinimumDistanceToPips(
                                    Symbol.MinTakeProfitDistance)),
                            CountManagedPositions(),
                            CountManagedPendingOrders()));
        
                    MtfSnapshot mtf =
                        MtfSnapshotBuilder.Build(
                            serverUtc,
                            userLocalTime,
                            Bars,
                            TimeFrame.ToString(),
                            _m1Bars,
                            _m5Bars,
                            _m15Bars,
                            _m30Bars,
                            _h1Bars,
                            _h4Bars,
                            _d1Bars,
                            _w1Bars,
                            30);
        
                    _state.SetMtf(mtf);
        
                    if (mtf.IsReferenceValid &&
                        (
                            mtf.ReferenceUtc !=
                            _lastMarketReferenceUtc ||
                            !mtf.IsPrimaryDecisionReady
                        ))
                    {
                        MarketModel market =
                            _marketModelBuilder.Build(
                                _state.Runtime,
                                mtf,
                                _configuration,
                                Bars,
                                _m1Bars,
                                _m5Bars,
                                _m15Bars,
                                _m30Bars,
                                _h1Bars,
                                _h4Bars,
                                _d1Bars,
                                _w1Bars);
        
                        _state.SetMarket(market);
        
                        StructureSnapshot structure =
                            _structureBuilder.Build(
                                mtf,
                                market,
                                _configuration,
                                _m5Bars,
                                _m15Bars,
                                _m30Bars,
                                _h1Bars,
                                _h4Bars,
                                _d1Bars,
                                _w1Bars);
        
                        _state.SetStructure(structure);
        
                        _lastMarketReferenceUtc =
                            mtf.ReferenceUtc;
                    }
                    else if (!mtf.IsReferenceValid)
                    {
                        _state.SetMarket(null);
                        _state.SetStructure(null);
                        _lastMarketReferenceUtc =
                            DateTime.MinValue;
                    }
        
                    if (mtf.IsPrimaryDecisionReady &&
                        _state.Market != null &&
                        _state.Structure != null)
                    {
                        _state.SetSuitability(
                            _suitabilityEngine.Evaluate(
                                _state.Runtime,
                                _state.Market,
                                _configuration));

                        _state.SetDecision(
                            _decisionEngine.Evaluate(
                                _state.Runtime,
                                mtf,
                                _state.Market,
                                _state.Structure,
                                _configuration));
        
                        _state.SetEntry(
                            _entryTriggerEngine.Evaluate(
                                _state.Decision,
                                _state.Runtime,
                                mtf,
                                _state.Market,
                                _state.Structure,
                                _configuration));
        
                        _state.SetPlan(
                            _tradePlanBuilder.Build(
                                _state.Decision,
                                _state.Entry,
                                _state.Market,
                                _state.Structure,
                                mtf,
                                _state.Runtime,
                                _configuration));
        
                        TryExecuteCurrentCycle();
                    }
        
                    ReconcileBrokerState();
                    ProcessPendingOrderLifecycle();
                    ProcessLivePositionManagement();
                    ProcessPositionLifecycle();
                }
        
        
    }
}
