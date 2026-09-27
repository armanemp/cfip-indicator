// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public enum CFIPClean89TargetState
        {
            Proposed = 0,
            Active = 1,
            Hit = 2,
            Invalidated = 3,
            Consumed = 4
        }
}
