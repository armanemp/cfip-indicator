// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        internal sealed class CFIPClean89LivePositionContext
        {
            public string BrokerPositionId;
            public string PlanId;
            public CFIPClean89Direction Direction;
            public CFIPClean89LivePlanSnapshot PlanSnapshot;
            public double InitialEntryPrice;
            public double InitialRiskPrice;
            public double InitialVolumeInUnits;
            public double LastObservedVolumeInUnits;
            public double InvalidationPrice;
            public CFIPClean89TargetStage ActiveTargetStage;
            public double PeakRR;
            public bool Tp1Consumed;
            public bool Tp2Consumed;
            public bool PartialPending;
            public bool ProtectionPending;
            public double? PendingStopLoss;
            public double? PendingTakeProfit;
            public CFIPClean89TargetStage PendingProtectionStage;
            public DateTime PendingProtectionRequestedUtc;
            public DateTime LastTargetRepriceReferenceUtc;
            public CFIPClean89TargetStage PendingPartialStage;
            public double PendingPartialRequestedVolume;
            public double PendingPartialExpectedRemainingVolume;
            public DateTime PartialAcceptedUtc;
            public int PartialRejectAttempts;
            public DateTime NextPartialRetryUtc;
            public bool ManagementRecoveryRequired;
        }
}
