// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89LivePositionActionKind
        {
            None = 0,
            ProtectionUpdate = 1,
            PartialClose = 2,
            Close = 3
        }
}
