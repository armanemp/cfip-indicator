// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PendingOrderAction
        {
            public string BrokerOrderId { get; private set; }
            public CFIPClean89PendingOrderActionKind Kind { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public string Reason { get; private set; }
    
            public CFIPClean89PendingOrderAction(
                string brokerOrderId,
                CFIPClean89PendingOrderActionKind kind,
                double? stopLoss,
                double? takeProfit,
                string reason)
            {
                BrokerOrderId = brokerOrderId ?? string.Empty;
                Kind = kind;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                Reason = reason ?? string.Empty;
            }
        }
}
