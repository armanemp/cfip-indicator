// Migrated from CFIP-PRO v89; now isolated from the cTrader host namespace.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator.Core
{
        public enum CFIPClean89FallbackKind
        {
            None = 0,
            Structural = 1,
            HigherTimeframe = 2,
            ExecutionFrame = 3,
            Atr = 4,
            Synthetic = 5
        }
}
