// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89MtfDataStatus
        {
            Ready = 0,
            PrimaryHistoryInsufficient = 1,
            MissingTimeframeData = 2,
            InvalidReference = 3,
            StaleReference = 4
        }
}
