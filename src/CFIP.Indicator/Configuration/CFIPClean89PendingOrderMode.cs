// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89PendingOrderMode
        {
            Adaptive = 0,
            ContinuationStop = 1,
            ReversalLimit = 2,
            Both = 3
        }
}
