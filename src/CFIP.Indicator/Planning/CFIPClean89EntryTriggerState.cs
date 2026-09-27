// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89EntryTriggerState
        {
            Blocked = 0,
            WaitingRetest = 1,
            WaitingBreakout = 2,
            TriggerReached = 3,
            Ready = 4,
            Invalidated = 5,
            Expired = 6
        }
}
