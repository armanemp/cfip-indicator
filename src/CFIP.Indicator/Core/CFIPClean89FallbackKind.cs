// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89FallbackKind
        {
            None = 0,
            Structural = 1,
            HigherTimeframe = 2,
            ExecutionFrame = 3,
            Atr = 4,
            Synthetic = 5
        }
}
