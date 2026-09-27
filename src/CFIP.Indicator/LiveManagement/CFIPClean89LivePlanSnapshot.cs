// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        internal sealed class CFIPClean89LivePlanSnapshot
        {
            public string PlanId;
            public CFIPClean89Direction Direction;
            public double EntryPrice;
            public double StructuralStopPrice;
            public double InvalidationPrice;
            public double? Tp1Price;
            public double? Tp2Price;
            public double? Tp3Price;
            public double? Tp4Price;
        }
}
