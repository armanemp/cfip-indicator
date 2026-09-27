// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public enum CFIPClean89LiquidityKind
        {
            EqualHigh, EqualLow, SwingHigh, SwingLow,
            PriorDayHigh, PriorDayLow, PriorWeekHigh, PriorWeekLow,
            SessionHigh, SessionLow, DailyPivot, DailyR1, DailyR2, DailyS1, DailyS2,
            Forecast
        }
}
