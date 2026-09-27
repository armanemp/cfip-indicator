// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89LivePositionAction
        {
            public string BrokerPositionId { get; private set; }
            public CFIPClean89LivePositionActionKind Kind { get; private set; }
            public double? StopLoss { get; private set; }
            public double? TakeProfit { get; private set; }
            public double VolumeInUnits { get; private set; }
            public CFIPClean89TargetStage TargetStage { get; private set; }
            public string Reason { get; private set; }
            public DateTime CreatedUtc { get; private set; }
    
            public CFIPClean89LivePositionAction(
                string brokerPositionId,
                CFIPClean89LivePositionActionKind kind,
                double? stopLoss,
                double? takeProfit,
                double volumeInUnits,
                CFIPClean89TargetStage targetStage,
                string reason,
                DateTime createdUtc)
            {
                BrokerPositionId = brokerPositionId ?? string.Empty;
                Kind = kind;
                StopLoss = stopLoss;
                TakeProfit = takeProfit;
                VolumeInUnits = Math.Max(0, volumeInUnits);
                TargetStage = targetStage;
                Reason = reason ?? string.Empty;
                CreatedUtc = createdUtc;
            }
        }
}
