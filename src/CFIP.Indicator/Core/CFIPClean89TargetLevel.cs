// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89TargetLevel
        {
            public CFIPClean89TargetStage Stage { get; private set; }
            public CFIPClean89PriceLevel Level { get; private set; }
            public CFIPClean89TargetState State { get; private set; }
            public int Quality { get; private set; }
    
            public CFIPClean89TargetLevel(
                CFIPClean89TargetStage stage,
                CFIPClean89PriceLevel level,
                int quality,
                CFIPClean89TargetState state)
            {
                if ((int)stage < (int)CFIPClean89TargetStage.TP1 ||
                    (int)stage > (int)CFIPClean89TargetStage.TP4)
                    throw new ArgumentOutOfRangeException("stage");
    
                if (level == null)
                    throw new ArgumentNullException("level");
    
                Stage = stage;
                Level = level;
                Quality = Math.Max(0, Math.Min(100, quality));
                State = state;
            }
        }
}
