// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89BrokerConstraints
        {
            public double MinVolumeInUnits { get; private set; }
            public double VolumeStepInUnits { get; private set; }
            public double MinStopDistancePips { get; private set; }
            public double MinTakeProfitDistancePips { get; private set; }
    
            public CFIPClean89BrokerConstraints(
                double minVolumeInUnits,
                double volumeStepInUnits,
                double minStopDistancePips,
                double minTakeProfitDistancePips)
            {
                MinVolumeInUnits = Math.Max(0, minVolumeInUnits);
                VolumeStepInUnits = Math.Max(0, volumeStepInUnits);
                MinStopDistancePips = Math.Max(0, minStopDistancePips);
                MinTakeProfitDistancePips =
                    Math.Max(0, minTakeProfitDistancePips);
            }
        }
}
