// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89BrokerPositionSnapshot
        {
            public string BrokerPositionId { get; private set; }
            public string Label { get; private set; }
            public string Symbol { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public double EntryPrice { get; private set; }
            public double VolumeInUnits { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public double NetProfit { get; private set; }
            public string Comment { get; private set; }
            public bool IsOpen { get; private set; }
    
            public CFIPClean89BrokerPositionSnapshot(
                string brokerPositionId,
                string label,
                string symbol,
                CFIPClean89Direction direction,
                double entryPrice,
                double volumeInUnits,
                double? stopLoss,
                double? takeProfit,
                double netProfit,
                string comment,
                bool isOpen)
            {
                BrokerPositionId = brokerPositionId ?? string.Empty;
                Label = label ?? string.Empty;
                Symbol = symbol ?? string.Empty;
                Direction = direction;
                EntryPrice = entryPrice;
                VolumeInUnits = volumeInUnits;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                NetProfit = netProfit;
                Comment = comment ?? string.Empty;
                IsOpen = isOpen;
            }
        }
}
