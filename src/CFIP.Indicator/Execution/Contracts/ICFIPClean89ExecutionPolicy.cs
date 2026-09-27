// Service contract migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public interface ICFIPClean89ExecutionPolicy
        {
            CFIPClean89ExecutionReadiness Evaluate(
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89EntrySnapshot entry,
                CFIPClean89TradePlan plan,
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89ConfigSnapshot configuration,
                double volumeInUnits,
                double riskAmount,
                double estimatedMargin);
        }
}
