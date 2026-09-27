// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89BrokerPendingOrderSnapshot
        {
            public string BrokerOrderId { get; private set; }
            public string Label { get; private set; }
            public string Symbol { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public CFIPClean89ExecutionKind Kind { get; private set; }
            public double RequestedEntry { get; private set; }
            public double VolumeInUnits { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public bool IsActive { get; private set; }
    
            public CFIPClean89BrokerPendingOrderSnapshot(
                string brokerOrderId,
                string label,
                string symbol,
                CFIPClean89Direction direction,
                CFIPClean89ExecutionKind kind,
                double requestedEntry,
                double volumeInUnits,
                double? stopLoss,
                double? takeProfit,
                bool isActive)
            {
                BrokerOrderId = brokerOrderId ?? string.Empty;
                Label = label ?? string.Empty;
                Symbol = symbol ?? string.Empty;
                Direction = direction;
                Kind = kind;
                RequestedEntry = requestedEntry;
                VolumeInUnits = volumeInUnits;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                IsActive = isActive;
            }
        }
}
