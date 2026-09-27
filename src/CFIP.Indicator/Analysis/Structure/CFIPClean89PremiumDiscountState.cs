// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PremiumDiscountState
        {
            public bool Available { get; private set; }
            public double RangeLow { get; private set; }
            public double RangeHigh { get; private set; }
            public double Midpoint { get { return Available ? (RangeLow + RangeHigh) / 2.0 : 0; } }
            public double ValueRatio { get; private set; }
            public bool IsDiscount { get { return Available && ValueRatio < 0.5; } }
            public bool IsPremium { get { return Available && ValueRatio > 0.5; } }
    
            public CFIPClean89PremiumDiscountState(
                bool available, double rangeLow, double rangeHigh, double valueRatio)
            {
                Available = available; RangeLow = rangeLow; RangeHigh = rangeHigh;
                ValueRatio = Math.Max(0, Math.Min(1, valueRatio));
            }
        }
}
