// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
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
