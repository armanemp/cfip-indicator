// ============================================================================
// CFIP Indicator — PlanActivation.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        // ============================================================
                        
                                private void ReconcilePreTradePlanDirection(int closedM5)
                                {
                                    if (_plan == null ||
                                        _plan.IsLivePosition ||
                                        _decision == null ||
                                        _decision.Direction == 0 ||
                                        _decision.Direction == _plan.Direction)
                                        return;
                        
                                    RemovePlanObjects();
                                    _plan = null;
                                    _executionModel = null;
                                    _lastVisualDirection = _decision.Direction;
                        
                                    SetAutoTradingState(
                                        "ARMED",
                                        "PRE-TRADE PLAN SUPERSEDED");
                                }
        
        private void ActivatePlan(
                                    Plan plan)
                                {
                                    _plan = plan;
                                    _lastSignalM5 = plan.CreatedM5;
                                    _lastConfirmedM5 = plan.CreatedM5;
                                    _lastConfirmedDirection = plan.Direction;
                                    _peakPrice = plan.Entry;
                        
                                    _lastMarket =
                                        plan.Direction == 1
                                            ? Symbol.Bid
                                            : Symbol.Ask;
                        
                                    _tp1Hit = 0;
                                    _tp2Hit = 0;
                                    _tp3Hit = 0;
                                    _tp4Hit = 0;
                                    _slHit = false;
                                    _outcomeRegistered = false;
                                    _outcomeTelemetryTimedOut = false;
                                    _brokerProtectionRecoveryRequired = false;
                                    _lastStructuralStopUpdateM5 =
                                        plan.CreatedM5;
                                    _lastTargetRepriceM5 =
                                        -1;
                                    _lastLiveStructuralPulseUtc =
                                        DateTime.MinValue;
                        
                                    _executionModel = null;
                        
                                    SetLifecycleState(
                                        LifecycleState.PlanReady,
                                        "PLAN READY");
                        
                                    ClearWatchObjects();
                        
                                    // Entry alerts are emitted only by the canonical
                                    // actionability gate after the plan is synchronized.
                                    // Plan creation itself is not an entry signal.
                                }
    }
}
