// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89Regime
        {
            Unknown = 0,
            Trend = 1,
            Expansion = 2,
            Compression = 3,
            Range = 4,
            Transition = 5
        }
}
