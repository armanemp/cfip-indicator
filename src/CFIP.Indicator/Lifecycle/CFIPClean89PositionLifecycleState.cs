// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89PositionLifecycleState
        {
            Unknown = 0,
            Adopted = 1,
            Protected = 2,
            ProtectionRecoveryRequired = 3,
            ExitRequested = 4,
            Closed = 5,
            Reconciled = 6,
            Orphan = 7,
            RecoveryRequired = 8
        }
}
