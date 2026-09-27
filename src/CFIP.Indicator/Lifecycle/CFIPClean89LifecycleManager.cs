// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89LifecycleManager
        {
            private CFIPClean89LifecycleState _state;
    
            public CFIPClean89LifecycleState State
            {
                get { return _state; }
            }
    
            public CFIPClean89LifecycleManager()
            {
                _state = CFIPClean89LifecycleState.Flat;
            }
    
            public bool TryTransition(
                CFIPClean89LifecycleState target,
                DateTime timeUtc,
                string reason)
            {
                if (!IsAllowed(_state, target))
                    return false;
    
                _state = target;
                return true;
            }
    
            private static bool IsAllowed(
                CFIPClean89LifecycleState from,
                CFIPClean89LifecycleState to)
            {
                if (from == to)
                    return true;
    
                switch (from)
                {
                    case CFIPClean89LifecycleState.Flat:
                        return
                            to == CFIPClean89LifecycleState.SignalDetected ||
                            to == CFIPClean89LifecycleState.PendingOrder ||
                            to == CFIPClean89LifecycleState.LivePosition ||
                            to == CFIPClean89LifecycleState.RecoveryRequired ||
                            to == CFIPClean89LifecycleState.Closed;
    
                    case CFIPClean89LifecycleState.SignalDetected:
                        return
                            to == CFIPClean89LifecycleState.PlanReady ||
                            to == CFIPClean89LifecycleState.Rejected ||
                            to == CFIPClean89LifecycleState.Flat;
    
                    case CFIPClean89LifecycleState.PlanReady:
                        return
                            to == CFIPClean89LifecycleState.ExecutionReady ||
                            to == CFIPClean89LifecycleState.Rejected ||
                            to == CFIPClean89LifecycleState.Flat;
    
                    case CFIPClean89LifecycleState.ExecutionReady:
                        return
                            to == CFIPClean89LifecycleState.PendingOrder ||
                            to == CFIPClean89LifecycleState.LivePosition ||
                            to == CFIPClean89LifecycleState.Rejected ||
                            to == CFIPClean89LifecycleState.Error;
    
                    case CFIPClean89LifecycleState.PendingOrder:
                        return
                            to == CFIPClean89LifecycleState.LivePosition ||
                            to == CFIPClean89LifecycleState.RecoveryRequired ||
                            to == CFIPClean89LifecycleState.Flat ||
                            to == CFIPClean89LifecycleState.Error;
    
                    case CFIPClean89LifecycleState.LivePosition:
                        return
                            to == CFIPClean89LifecycleState.ExitRequested ||
                            to == CFIPClean89LifecycleState.RecoveryRequired ||
                            to == CFIPClean89LifecycleState.Closed ||
                            to == CFIPClean89LifecycleState.Error;
    
                    case CFIPClean89LifecycleState.ExitRequested:
                        return
                            to == CFIPClean89LifecycleState.Closed ||
                            to == CFIPClean89LifecycleState.RecoveryRequired ||
                            to == CFIPClean89LifecycleState.Error;
    
                    case CFIPClean89LifecycleState.RecoveryRequired:
                        return
                            to == CFIPClean89LifecycleState.PendingOrder ||
                            to == CFIPClean89LifecycleState.LivePosition ||
                            to == CFIPClean89LifecycleState.Closed ||
                            to == CFIPClean89LifecycleState.Error ||
                            to == CFIPClean89LifecycleState.Flat;
    
                    case CFIPClean89LifecycleState.Closed:
                        return
                            to == CFIPClean89LifecycleState.SignalDetected ||
                            to == CFIPClean89LifecycleState.PendingOrder ||
                            to == CFIPClean89LifecycleState.LivePosition ||
                            to == CFIPClean89LifecycleState.RecoveryRequired ||
                            to == CFIPClean89LifecycleState.Flat;
    
                    case CFIPClean89LifecycleState.Rejected:
                        return
                            to == CFIPClean89LifecycleState.Flat ||
                            to == CFIPClean89LifecycleState.SignalDetected;
    
                    case CFIPClean89LifecycleState.Error:
                        return
                            to == CFIPClean89LifecycleState.RecoveryRequired ||
                            to == CFIPClean89LifecycleState.Flat;
    
                    default:
                        return false;
                }
            }
        }
}
