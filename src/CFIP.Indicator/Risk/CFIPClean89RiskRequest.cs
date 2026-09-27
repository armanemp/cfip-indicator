// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89RiskRequest
        {
            public double RiskPercentEquity { get; private set; }
            public double RequestedVolumeInUnits { get; private set; }
            public double MaxRiskAmount { get; private set; }
    
            public CFIPClean89RiskRequest(
                double riskPercentEquity,
                double requestedVolumeInUnits,
                double maxRiskAmount)
            {
                RiskPercentEquity =
                    Math.Max(0, riskPercentEquity);
                RequestedVolumeInUnits =
                    Math.Max(0, requestedVolumeInUnits);
                MaxRiskAmount =
                    Math.Max(0, maxRiskAmount);
            }
        }
}
