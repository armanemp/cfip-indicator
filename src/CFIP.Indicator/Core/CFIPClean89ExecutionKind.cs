// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public enum CFIPClean89ExecutionKind
        {
            None = 0,
            Market = 1,
            Stop = 2,
            Limit = 3
        }
}
