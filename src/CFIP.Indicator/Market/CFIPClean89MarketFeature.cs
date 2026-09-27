// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89MarketFeature
        {
            Trend = 0,
            Momentum = 1,
            Rsi = 2,
            Dmi = 3,
            EmaSlope = 4,
            Rejection = 5,
            VolumeExpansion = 6,
            MacdBias = 7,
            VwapBias = 8,
            HealthyVolatility = 9
        }
}
