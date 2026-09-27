// Service contract migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public interface ICFIPClean89ExecutionPlanner
        {
            CFIPClean89ExecutionIntent CreateIntent(
                CFIPClean89TradePlan plan,
                CFIPClean89EntrySnapshot entry,
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89ConfigSnapshot configuration,
                CFIPClean89ExecutionReadiness readiness);
        }
}
