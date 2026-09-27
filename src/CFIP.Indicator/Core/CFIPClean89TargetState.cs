// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89TargetState
        {
            Proposed = 0,
            Active = 1,
            Hit = 2,
            Invalidated = 3,
            Consumed = 4
        }
}
