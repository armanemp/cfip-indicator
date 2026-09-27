// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public enum CFIPClean89DecisionPolicyMode
        {
            Confirmed = 0,
            Soft = 1,
            Aggressive = 2,
            Pending = 3
        }
}
