// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89ExecutionEnvelope
        {
            public double MaxEntryChaseAtr { get; private set; }
            public double MaxBrokerSlippagePips { get; private set; }
            public double MaxBreakoutFillDeviationPips { get; private set; }
            public double MaxPlanRebaseDistancePips { get; private set; }
    
            public CFIPClean89ExecutionEnvelope(
                double maxEntryChaseAtr,
                double maxBrokerSlippagePips,
                double maxBreakoutFillDeviationPips,
                double maxPlanRebaseDistancePips)
            {
                MaxEntryChaseAtr =
                    Math.Max(0, maxEntryChaseAtr);
                MaxBrokerSlippagePips =
                    Math.Max(0, maxBrokerSlippagePips);
                MaxBreakoutFillDeviationPips =
                    Math.Max(0, maxBreakoutFillDeviationPips);
                MaxPlanRebaseDistancePips =
                    Math.Max(0, maxPlanRebaseDistancePips);
            }
        }
}
