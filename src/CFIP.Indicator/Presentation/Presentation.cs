using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CFIP.Indicator
{
        public sealed class PresentationState
        {
            public DateTime GeneratedUtc { get; private set; }
            public Direction Direction { get; private set; }
            public LifecycleState LifecycleState { get; private set; }
            public string Readiness { get; private set; }
            public string ExecutionStatus { get; private set; }
            public string BrokerStatus { get; private set; }
            public string AlertStatus { get; private set; }
    
            public PresentationState(
                DateTime generatedUtc,
                Direction direction,
                LifecycleState lifecycleState,
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
    
        public interface IPresentationProjector
        {
            PresentationState Project(
                RuntimeSnapshot runtime,
                DecisionSnapshot decision,
                TradePlan plan,
                BrokerStateSnapshot broker,
                LifecycleState lifecycle);
        }
    
    
}
