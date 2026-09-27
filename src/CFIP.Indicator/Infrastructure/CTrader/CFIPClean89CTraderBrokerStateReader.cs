// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89CTraderBrokerStateReader : ICFIPClean89BrokerStateReader
        {
            private readonly CFIP_MTF_LiveEntryEngine_Clean_v87 _host;
    
            public CFIPClean89CTraderBrokerStateReader(
                CFIP_MTF_LiveEntryEngine_Clean_v87 host)
            {
                _host = host ?? throw new ArgumentNullException("host");
            }
    
            public CFIPClean89BrokerStateSnapshot ReadManagedState(
                string symbol,
                string strategyId)
            {
                string label =
                    _host.Configuration.Get(
                        "AutoTradeLabel",
                        "CFIP-SMART-CLEAN89");
    
                var positions = new List<CFIPClean89BrokerPositionSnapshot>();
                foreach (var position in _host.Positions)
                {
                    if (!string.Equals(position.Label, label, StringComparison.Ordinal) ||
                        !string.Equals(position.SymbolName, symbol, StringComparison.Ordinal) ||
                        !HasCurrentIdentity(position.Comment))
                        continue;
    
                    positions.Add(
                        new CFIPClean89BrokerPositionSnapshot(
                            position.Id.ToString(),
                            position.Label,
                            position.SymbolName,
                            position.TradeType == TradeType.Buy
                                ? CFIPClean89Direction.Buy
                                : CFIPClean89Direction.Sell,
                            position.EntryPrice,
                            position.VolumeInUnits,
                            position.StopLoss,
                            position.TakeProfit,
                            position.NetProfit,
                            position.Comment,
                            true));
                }
    
                var orders = new List<CFIPClean89BrokerPendingOrderSnapshot>();
                foreach (var order in _host.PendingOrders)
                {
                    if (!string.Equals(order.Label, label, StringComparison.Ordinal) ||
                        !string.Equals(order.SymbolName, symbol, StringComparison.Ordinal) ||
                        !HasCurrentIdentity(order.Comment))
                        continue;
    
                    var kind =
                        order.OrderType == PendingOrderType.Stop
                            ? CFIPClean89ExecutionKind.Stop
                            : order.OrderType == PendingOrderType.Limit
                                ? CFIPClean89ExecutionKind.Limit
                                : CFIPClean89ExecutionKind.None;
    
                    orders.Add(
                        new CFIPClean89BrokerPendingOrderSnapshot(
                            order.Id.ToString(),
                            order.Label,
                            order.SymbolName,
                            order.TradeType == TradeType.Buy
                                ? CFIPClean89Direction.Buy
                                : CFIPClean89Direction.Sell,
                            kind,
                            order.TargetPrice,
                            order.VolumeInUnits,
                            order.StopLoss,
                            order.TakeProfit,
                            true));
                }
    
                return new CFIPClean89BrokerStateSnapshot(
                    positions,
                    orders);
            }
    
            private bool HasCurrentIdentity(string comment)
            {
                return
                    comment != null &&
                    comment.IndexOf(
                        "CFIP89|",
                        StringComparison.Ordinal) >= 0;
            }
        }
}
