// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89MtfSnapshot
        {
            public DateTime ServerUtc { get; private set; }
            public DateTime ReferenceUtc { get; private set; }
            public DateTime UserLocalTime { get; private set; }
            public TimeSpan ReferenceAge { get; private set; }
            public string ChartTimeframe { get; private set; }
    
            public CFIPClean89MtfBarSnapshot Chart { get; private set; }
            public CFIPClean89MtfBarSnapshot M1 { get; private set; }
            public CFIPClean89MtfBarSnapshot M5 { get; private set; }
            public CFIPClean89MtfBarSnapshot M15 { get; private set; }
            public CFIPClean89MtfBarSnapshot M30 { get; private set; }
            public CFIPClean89MtfBarSnapshot H1 { get; private set; }
            public CFIPClean89MtfBarSnapshot H4 { get; private set; }
            public CFIPClean89MtfBarSnapshot D1 { get; private set; }
            public CFIPClean89MtfBarSnapshot W1 { get; private set; }
    
            public CFIPClean89MtfDataStatus DataStatus { get; private set; }
    
            public bool IsReferenceValid
            {
                get
                {
                    return ReferenceUtc != DateTime.MinValue;
                }
            }
    
            public bool IsReferenceFresh
            {
                get
                {
                    return
                        IsReferenceValid &&
                        ReferenceAge >= TimeSpan.Zero &&
                        ReferenceAge <= TimeSpan.FromMinutes(10);
                }
            }
    
            public bool IsPrimaryDecisionReady
            {
                get
                {
                    return
                        IsReferenceValid &&
                        IsReferenceFresh &&
                        M5 != null &&
                        M15 != null &&
                        M30 != null &&
                        H1 != null &&
                        H4 != null &&
                        M5.IsAvailable &&
                        M15.IsAvailable &&
                        M30.IsAvailable &&
                        H1.IsAvailable &&
                        H4.IsAvailable &&
                        M5.IsFullyClosedAtReference &&
                        M15.IsFullyClosedAtReference &&
                        M30.IsFullyClosedAtReference &&
                        H1.IsFullyClosedAtReference &&
                        H4.IsFullyClosedAtReference &&
                        M5.HasMinimumHistory &&
                        M15.HasMinimumHistory &&
                        M30.HasMinimumHistory &&
                        H1.HasMinimumHistory &&
                        H4.HasMinimumHistory;
                }
            }
    
            public bool IsAllAvailableTimeframesClosed
            {
                get
                {
                    return
                        M1 != null &&
                        M5 != null &&
                        M15 != null &&
                        M30 != null &&
                        H1 != null &&
                        H4 != null &&
                        D1 != null &&
                        W1 != null &&
                        M1.IsAvailable &&
                        M5.IsAvailable &&
                        M15.IsAvailable &&
                        M30.IsAvailable &&
                        H1.IsAvailable &&
                        H4.IsAvailable &&
                        D1.IsAvailable &&
                        W1.IsAvailable &&
                        M1.IsFullyClosedAtReference &&
                        M5.IsFullyClosedAtReference &&
                        M15.IsFullyClosedAtReference &&
                        M30.IsFullyClosedAtReference &&
                        H1.IsFullyClosedAtReference &&
                        H4.IsFullyClosedAtReference &&
                        D1.IsFullyClosedAtReference &&
                        W1.IsFullyClosedAtReference;
                }
            }
    
            public CFIPClean89MtfSnapshot(
                DateTime serverUtc,
                DateTime referenceUtc,
                DateTime userLocalTime,
                TimeSpan referenceAge,
                string chartTimeframe,
                CFIPClean89MtfBarSnapshot chart,
                CFIPClean89MtfBarSnapshot m1,
                CFIPClean89MtfBarSnapshot m5,
                CFIPClean89MtfBarSnapshot m15,
                CFIPClean89MtfBarSnapshot m30,
                CFIPClean89MtfBarSnapshot h1,
                CFIPClean89MtfBarSnapshot h4,
                CFIPClean89MtfBarSnapshot d1,
                CFIPClean89MtfBarSnapshot w1,
                CFIPClean89MtfDataStatus dataStatus)
            {
                ServerUtc = serverUtc;
                ReferenceUtc = referenceUtc;
                UserLocalTime = userLocalTime;
                ReferenceAge =
                    referenceAge < TimeSpan.Zero
                        ? TimeSpan.Zero
                        : referenceAge;
                ChartTimeframe = chartTimeframe ?? string.Empty;
                Chart = chart ?? CFIPClean89MtfBarSnapshot.Missing("CHART");
                M1 = m1 ?? CFIPClean89MtfBarSnapshot.Missing("M1");
                M5 = m5 ?? CFIPClean89MtfBarSnapshot.Missing("M5");
                M15 = m15 ?? CFIPClean89MtfBarSnapshot.Missing("M15");
                M30 = m30 ?? CFIPClean89MtfBarSnapshot.Missing("M30");
                H1 = h1 ?? CFIPClean89MtfBarSnapshot.Missing("H1");
                H4 = h4 ?? CFIPClean89MtfBarSnapshot.Missing("H4");
                D1 = d1 ?? CFIPClean89MtfBarSnapshot.Missing("D1");
                W1 = w1 ?? CFIPClean89MtfBarSnapshot.Missing("W1");
                DataStatus = dataStatus;
            }
        }
}
