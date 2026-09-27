// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89LifecycleState
        {
            Flat = 0,
            SignalDetected = 1,
            PlanReady = 2,
            ExecutionReady = 3,
            PendingOrder = 4,
            LivePosition = 5,
            ExitRequested = 6,
            RecoveryRequired = 7,
            Closed = 8,
            Rejected = 9,
            Error = 10
        }
}
