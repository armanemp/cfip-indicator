// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89PendingOrderLifecycleState
        {
            Unknown = 0,
            Pending = 1,
            ProtectionRecoveryRequired = 2,
            Filled = 3,
            Cancelled = 4,
            Reconciled = 5
        }
}
