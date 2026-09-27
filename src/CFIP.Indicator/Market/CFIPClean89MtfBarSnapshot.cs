// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89MtfBarSnapshot
        {
            public string Timeframe { get; private set; }
            public int ClosedIndex { get; private set; }
            public DateTime BarOpenUtc { get; private set; }
            public DateTime NextBarOpenUtc { get; private set; }
            public bool IsAvailable { get; private set; }
            public bool IsFullyClosedAtReference { get; private set; }
            public bool HasMinimumHistory { get; private set; }
    
            public CFIPClean89MtfBarSnapshot(
                string timeframe,
                int closedIndex,
                DateTime barOpenUtc,
                DateTime nextBarOpenUtc,
                bool isAvailable,
                bool isFullyClosedAtReference,
                bool hasMinimumHistory)
            {
                Timeframe = timeframe ?? string.Empty;
                ClosedIndex = closedIndex;
                BarOpenUtc = barOpenUtc;
                NextBarOpenUtc = nextBarOpenUtc;
                IsAvailable = isAvailable;
                IsFullyClosedAtReference = isFullyClosedAtReference;
                HasMinimumHistory = hasMinimumHistory;
            }
    
            public static CFIPClean89MtfBarSnapshot Missing(
                string timeframe)
            {
                return new CFIPClean89MtfBarSnapshot(
                    timeframe,
                    -1,
                    DateTime.MinValue,
                    DateTime.MinValue,
                    false,
                    false,
                    false);
            }
        }
}
