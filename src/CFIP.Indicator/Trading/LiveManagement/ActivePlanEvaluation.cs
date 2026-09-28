// ============================================================================
// CFIP Indicator — ActivePlanEvaluation.cs
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
        private void EvaluateActivePlan(
                                    int closedM5)
                                {
                                    if (_plan == null ||
                                        !_plan.IsLivePosition)
                                        return;
                        
                                    double market =
                                        _plan.Direction == 1
                                            ? Symbol.Bid
                                            : Symbol.Ask;
                        
                                    _lastMarket = market;
                        
                                    if (!IsFinitePositive(market) ||
                                        _plan.Risk <= 0)
                                        return;
                        
                                    if (_plan.Direction == 1)
                                        _peakPrice =
                                            Math.Max(
                                                _peakPrice,
                                                market);
                                    else
                                        _peakPrice =
                                            Math.Min(
                                                _peakPrice,
                                                market);
                        
                                    double favorable =
                                        _plan.Direction == 1
                                            ? _peakPrice - _plan.Entry
                                            : _plan.Entry - _peakPrice;
                        
                                    double currentMove =
                                        _plan.Direction == 1
                                            ? market - _plan.Entry
                                            : _plan.Entry - market;
                        
                                    double previousStop =
                                        _plan.Stop;
                        
                                    double previousTp1 =
                                        _plan.Tp1;
                        
                                    double previousTp2 =
                                        _plan.Tp2;
                        
                                    double peakRR =
                                        favorable /
                                        Math.Max(
                                            Symbol.PipSize,
                                            _plan.Risk);
                        
                                    if (EnableLiveExitManagement)
                                    {
                                        bool structuralBarChanged =
                                            closedM5 !=
                                            _lastStructuralStopUpdateM5;
                        
                                        double protectedStop =
                                            CalculateProtectedStop(
                                                market,
                                                peakRR,
                                                closedM5,
                                                structuralBarChanged);
                        
                                        if (BetterStop(
                                                _plan.Direction,
                                                protectedStop,
                                                _plan.Stop))
                                        {
                                            _plan.Stop =
                                                NormalizePrice(
                                                    protectedStop);
                        
                                            RecalculatePlanRR();
                                        }
                        
                                        if (UpdateUnhitTargets &&
                                            peakRR >=
                                            TargetUpdateTriggerRR &&
                                            (!StructuralTargetUpdatesOnly ||
                                             closedM5 !=
                                             _lastTargetRepriceM5))
                                        {
                                            UpdateUnhitTargetsLive(
                                                closedM5,
                                                market);
                                        }
                        
                                        if (structuralBarChanged)
                                            _lastStructuralStopUpdateM5 =
                                                closedM5;
                                    }
                        
                                    double updateAtr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    bool changed =
                                        Math.Abs(
                                            previousStop -
                                            _plan.Stop) >=
                                            Math.Max(
                                                Symbol.PipSize,
                                                updateAtr *
                                                Math.Max(
                                                    0.01,
                                                    SlRepriceStepAtr)) ||
                                        Math.Abs(
                                            previousTp1 -
                                            _plan.Tp1) >=
                                            Symbol.PipSize ||
                                        Math.Abs(
                                            previousTp2 -
                                            _plan.Tp2) >=
                                            Symbol.PipSize;
                        
                                    if (changed &&
                                        AlertOnExitPlanUpdate)
                                    {
                                        SendUnifiedAlert(
                                            "PLANUPDATE|" +
                                            closedM5 +
                                            "|" +
                                            Price(_plan.Stop) +
                                            "|" +
                                            Price(_plan.Tp1),
                                            "CFIP SMART PLAN UPDATE | SL " +
                                            Price(_plan.Stop) +
                                            " | TP1 " +
                                            Price(_plan.Tp1) +
                                            " | EXIT " +
                                            GetSmartExitMode(),
                                            _plan.Direction,
                                            false);
                                    }
                        
                                    if (RequirePlanIntegrity &&
                                        !ValidatePlanIntegrity(
                                            _plan,
                                            _plan.Direction,
                                            _plan.Entry,
                                            Math.Max(
                                                Symbol.PipSize,
                                                Atr(
                                                    _m5Bars,
                                                    closedM5)),
                                            false))
                                    {
                                        SendUnifiedAlert(
                                            "INVALIDPLAN|" +
                                            _plan.CreatedM5,
                                            "CFIP PLAN INVALIDATED | STRUCTURE / RR / SPREAD GUARD",
                                            _plan.Direction,
                                            true);
                        
                                        _lastExitM5 =
                                            closedM5;
                        
                                        Position integrityPosition =
                                            GetManagedPositionById(
                                                _plan.PositionId);
                        
                                        if (integrityPosition != null)
                                        {
                                            SetLifecycleState(
                                                LifecycleState.ExitRequested,
                                                "PLAN INTEGRITY FAILURE");
                        
                                            if (!TryClosePosition(
                                                    integrityPosition,
                                                    "PLAN INTEGRITY FAILURE"))
                                            {
                                                SetLifecycleState(
                                                    LifecycleState.RecoveryRequired,
                                                    "PLAN INTEGRITY EXIT REJECTED");
                        
                                                _autoExecutionBlockReason =
                                                    "PLAN INTEGRITY EXIT REJECTED";
                                            }
                        
                                            return;
                                        }
                        
                                        SetLifecycleState(
                                            LifecycleState.Closed,
                                            "PLAN INVALID • NO BROKER POSITION");
                        
                                        _plan = null;
                                        RemovePlanObjects();
                                        return;
                                    }
                        
                                    double liveStop =
                                        GetActiveBrokerStopPrice();
                        
                                    bool hitSl =
                                        IsFinitePositive(liveStop) &&
                                        (_plan.Direction == 1
                                            ? market <= liveStop
                                            : market >= liveStop);
                        
                                    bool hitTp1 =
                                        _plan.Tp1 > 0 &&
                                        (_plan.Direction == 1
                                            ? market >= _plan.Tp1
                                            : market <= _plan.Tp1);
                        
                                    bool hitTp2 =
                                        _plan.Tp2 > 0 &&
                                        (_plan.Direction == 1
                                            ? market >= _plan.Tp2
                                            : market <= _plan.Tp2);
                        
                                    bool hitTp3 =
                                        _plan.Tp3 > 0 &&
                                        (_plan.Direction == 1
                                            ? market >= _plan.Tp3
                                            : market <= _plan.Tp3);
                        
                                    bool hitTp4 =
                                        _plan.Tp4 > 0 &&
                                        (_plan.Direction == 1
                                            ? market >= _plan.Tp4
                                            : market <= _plan.Tp4);
                        
                                    if (hitSl &&
                                        !_slHit)
                                    {
                                        if (GetManagedLivePositionForPlan() == null)
                                        {
                                            SetLifecycleState(
                                                LifecycleState.Closed,
                                                "SL LEVEL • POSITION ALREADY CLOSED");
                                            return;
                                        }
                        
                                        if (RequestLivePlanExit(
                                                closedM5,
                                                "SL LEVEL HIT"))
                                        {
                                            _slHit = true;
                        
                                            if (EnableLevelHitAlerts &&
                                                AlertOnLevelHit &&
                                                AlertOnSl)
                                            {
                                                SendUnifiedAlert(
                                                    "SL|" +
                                                    _plan.CreatedM5,
                                                    "CFIP SL HIT • EXIT REQUESTED | " +
                                                    Price(_plan.Stop),
                                                    -1,
                                                    true);
                                            }
                        
                                            DrawOutcomeMarker(
                                                "SL HIT",
                                                _plan.Stop,
                                                false);
                                        }
                        
                                        return;
                                    }
                        
                                    if (hitTp1 &&
                                        _tp1Hit == 0)
                                    {
                                        bool tp1Processed =
                                            ExecutePartialClose(
                                                PartialCloseTp1Percent,
                                                "TP1");
                        
                                        if (tp1Processed)
                                            _tp1Hit = 1;
                        
                                        if (tp1Processed &&
                                            EnableLevelHitAlerts &&
                                            AlertOnLevelHit &&
                                            AlertOnTp1)
                                        {
                                            SendUnifiedAlert(
                                                "TP1|" +
                                                _plan.CreatedM5,
                                                "CFIP TP1 HIT | " +
                                                Price(_plan.Tp1),
                                                _plan.Direction,
                                                true);
                                        }
                                    }
                        
                                    if (hitTp2 &&
                                        _tp2Hit == 0)
                                    {
                                        bool tp2Processed =
                                            ExecutePartialClose(
                                                PartialCloseTp2Percent,
                                                "TP2");
                        
                                        if (tp2Processed)
                                            _tp2Hit = 1;
                        
                                        if (tp2Processed &&
                                            EnableLevelHitAlerts &&
                                            AlertOnLevelHit &&
                                            AlertOnTp2)
                                        {
                                            SendUnifiedAlert(
                                                "TP2|" +
                                                _plan.CreatedM5,
                                                "CFIP TP2 HIT | " +
                                                Price(_plan.Tp2),
                                                _plan.Direction,
                                                false);
                                        }
                                    }
                        
                                    if (hitTp3 &&
                                        _tp3Hit == 0)
                                    {
                                        _tp3Hit = 1;
                        
                                        if (EnableLevelHitAlerts &&
                                            AlertOnLevelHit &&
                                            AlertOnTp3)
                                        {
                                            SendUnifiedAlert(
                                                "TP3|" +
                                                _plan.CreatedM5,
                                                "CFIP TP3 HIT | " +
                                                Price(_plan.Tp3),
                                                _plan.Direction,
                                                false);
                                        }
                                    }
                        
                                    if (hitTp4 &&
                                        _tp4Hit == 0)
                                    {
                                        if (RequestLivePlanExit(
                                                closedM5,
                                                "TP4 LEVEL HIT"))
                                        {
                                            _tp4Hit = 1;
                        
                                            if (EnableLevelHitAlerts &&
                                                AlertOnLevelHit &&
                                                AlertOnTp4)
                                            {
                                                SendUnifiedAlert(
                                                    "TP4|" +
                                                    _plan.CreatedM5,
                                                    "CFIP TP4 HIT • EXIT REQUESTED | " +
                                                    Price(_plan.Tp4),
                                                    _plan.Direction,
                                                    true);
                                            }
                        
                                            DrawOutcomeMarker(
                                                "TP4 HIT",
                                                _plan.Tp4,
                                                true);
                                        }
                        
                                        return;
                                    }
                        
                                    if (CheckLiveReversalAgainstPlan(
                                            closedM5))
                                        return;
                        
                                    if (CheckProfitExhaustionExit(
                                            closedM5,
                                            market,
                                            peakRR))
                                        return;
                        
                                    if (CheckStructuralSetupInvalidation(
                                            closedM5,
                                            market))
                                        return;
                        
                                    int barsSincePlan =
                                        Math.Max(
                                            0,
                                            closedM5 -
                                            _plan.CreatedM5);
                        
                                    if (UseFalseSignalGuard &&
                                        EnableSetupInvalidation &&
                                        currentMove < 0 &&
                                        barsSincePlan <=
                                        Math.Max(
                                            1,
                                            FalseSignalWatchBars) &&
                                        Math.Abs(
                                            currentMove) >=
                                        _plan.Risk *
                                        Math.Max(
                                            0.25,
                                            FalseSignalAdverseR))
                                    {
                                        if (AlertOnFalseSignalRisk &&
                                            _lastInvalidationAlertM5 !=
                                            closedM5)
                                        {
                                            _lastInvalidationAlertM5 =
                                                closedM5;
                        
                                            SendUnifiedAlert(
                                                "INVALID-RISK|" +
                                                closedM5,
                                                "CFIP FALSE SIGNAL RISK | " +
                                                (_plan.Direction == 1
                                                    ? "BUY"
                                                    : "SELL"),
                                                0,
                                                true);
                                        }
                        
                                        if (InvalidateOnFalseSignal &&
                                            Math.Abs(
                                                currentMove) >=
                                            _plan.Risk *
                                            Math.Max(
                                                1.0,
                                                FalseSignalAdverseR))
                                        {
                                            RequestLivePlanExit(
                                                closedM5,
                                                "FALSE SIGNAL INVALIDATION");
                        
                                            return;
                                        }
                                    }
                        
                                    if (currentMove < 0 &&
                                        Math.Abs(
                                            currentMove) >=
                                        _plan.Risk *
                                        Math.Max(
                                            0.25,
                                            FalseSignalAdverseR) &&
                                        AlertOnInvalidated &&
                                        _lastInvalidationAlertM5 !=
                                        closedM5)
                                    {
                                        SendUnifiedAlert(
                                            "INVALID|" +
                                            closedM5,
                                            "CFIP SETUP UNDER PRESSURE | " +
                                            (_plan.Direction == 1
                                                ? "BUY"
                                                : "SELL") +
                                            " | " +
                                            (Math.Abs(
                                                 currentMove) /
                                             _plan.Risk).ToString("F2") +
                                            "R",
                                            0,
                                            true);
                        
                                        _lastInvalidationAlertM5 =
                                            closedM5;
                                    }
                                }
    }
}
