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
                        
                                    _executionModel = null;
                        
                                    SetLifecycleState(
                                        LifecycleState.PlanReady,
                                        "PLAN READY");
                        
                                    ClearWatchObjects();
                        
                                    string message =
                                        "CFIP " +
                                        (plan.Direction == 1
                                            ? "BUY"
                                            : "SELL") +
                                        " | CONF " +
                                        (_decision == null
                                            ? 0
                                            : _decision.Confidence) +
                                        " | SMART " +
                                        (_decision == null
                                            ? 0
                                            : _decision.SmartQuality) +
                                        " | MODE " +
                                        ExecutionModeText(plan.EntryMode) +
                                        " | ENTRY " +
                                        Price(plan.Entry) +
                                        " | TRIGGER " +
                                        Price(plan.EntryTrigger) +
                                        " | IDEAL " +
                                        Price(plan.IdealEntry) +
                                        " | ENTRY Q " +
                                        plan.EntryQuality +
                                        " | HTF TP " +
                                        plan.HtfTargetCount +
                                        " | SL " +
                                        Price(plan.Stop) +
                                        " | TP1 " +
                                        Price(plan.Tp1) +
                                        " | RR " +
                                        plan.Tp1RR.ToString("F2");
                        
                                    if (AlertOnConfirmedSignal)
                                    {
                                        SendUnifiedAlert(
                                            "SIGNAL|" +
                                            plan.CreatedM5,
                                            message,
                                            plan.Direction,
                                            true);
                                    }
                                }
    }
}
