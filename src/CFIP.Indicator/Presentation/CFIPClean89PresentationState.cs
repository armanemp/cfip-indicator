// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89PresentationState
        {
            public DateTime GeneratedUtc { get; private set; }
            public CFIPClean89Direction Direction { get; private set; }
            public CFIPClean89LifecycleState LifecycleState { get; private set; }
            public string Readiness { get; private set; }
            public string ExecutionStatus { get; private set; }
            public string BrokerStatus { get; private set; }
            public string AlertStatus { get; private set; }
    
            public CFIPClean89PresentationState(
                DateTime generatedUtc,
                CFIPClean89Direction direction,
                CFIPClean89LifecycleState lifecycleState,
                string readiness,
                string executionStatus,
                string brokerStatus,
                string alertStatus)
            {
                GeneratedUtc = generatedUtc;
                Direction = direction;
                LifecycleState = lifecycleState;
                Readiness = readiness ?? string.Empty;
                ExecutionStatus = executionStatus ?? string.Empty;
                BrokerStatus = brokerStatus ?? string.Empty;
                AlertStatus = alertStatus ?? string.Empty;
            }
        }
}
