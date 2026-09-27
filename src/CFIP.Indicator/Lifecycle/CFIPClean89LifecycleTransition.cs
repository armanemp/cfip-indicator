// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89LifecycleTransition
        {
            public CFIPClean89LifecycleState From { get; private set; }
            public CFIPClean89LifecycleState To { get; private set; }
            public DateTime TimeUtc { get; private set; }
            public string Reason { get; private set; }
    
            public CFIPClean89LifecycleTransition(
                CFIPClean89LifecycleState from,
                CFIPClean89LifecycleState to,
                DateTime timeUtc,
                string reason)
            {
                From = from;
                To = to;
                TimeUtc = timeUtc;
                Reason = reason ?? string.Empty;
            }
        }
}
