// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89ProtectionState
        {
            Unknown = 0,
            Unprotected = 1,
            PartiallyProtected = 2,
            FullyProtected = 3,
            RecoveryRequired = 4
        }
}
