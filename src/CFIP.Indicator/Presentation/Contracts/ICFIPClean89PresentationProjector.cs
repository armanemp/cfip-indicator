// Service contract migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public interface ICFIPClean89PresentationProjector
        {
            CFIPClean89PresentationState Project(
                CFIPClean89RuntimeSnapshot runtime,
                CFIPClean89DecisionSnapshot decision,
                CFIPClean89TradePlan plan,
                CFIPClean89BrokerStateSnapshot broker,
                CFIPClean89LifecycleState lifecycle);
        }
}
