// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
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
