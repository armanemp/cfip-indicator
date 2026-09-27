// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PositionAction
        {
            public string BrokerPositionId { get; private set; }
            public CFIPClean89PositionActionKind Kind { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public string Reason { get; private set; }
    
            public CFIPClean89PositionAction(
                string brokerPositionId,
                CFIPClean89PositionActionKind kind,
                double? stopLoss,
                double? takeProfit,
                string reason)
            {
                BrokerPositionId = brokerPositionId ?? string.Empty;
                Kind = kind;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                Reason = reason ?? string.Empty;
            }
        }
}
