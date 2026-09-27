using System;
using System.Collections.Generic;
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
                        _plan.Direction == 1
                            ? market <= liveStop
                            : market >= liveStop;
        
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
        
                        private bool CheckProfitExhaustionExit(
                    int closedM5,
                    double market,
                    double peakRR)
                {
                    if (!EnableProfitExhaustionProtection ||
                        _plan == null ||
                        !_plan.IsLivePosition ||
                        peakRR <
                        Math.Max(
                            0.8,
                            ExhaustionMinimumPeakRR) ||
                        _plan.Risk <= 0)
                        return false;
        
                    double favorable =
                        Math.Max(
                            Symbol.PipSize,
                            _plan.Risk) *
                        peakRR;
        
                    if (favorable <= 0)
                        return false;
        
                    double retracement =
                        _plan.Direction == 1
                            ? _peakPrice - market
                            : market - _peakPrice;
        
                    if (retracement <= 0)
                        return false;
        
                    double retracementPercent =
                        retracement /
                        favorable *
                        100.0;
        
                    if (retracementPercent <
                        Math.Max(
                            15,
                            ExhaustionRetracementPercent))
                        return false;
        
                    double currentRR =
                        _plan.Direction == 1
                            ? (market - _plan.Entry) /
                              Math.Max(
                                  Symbol.PipSize,
                                  _plan.Risk)
                            : (_plan.Entry - market) /
                              Math.Max(
                                  Symbol.PipSize,
                                  _plan.Risk);
        
                    int pressure =
                        CalculateSmartExitPressure(
                            market,
                            currentRR);
        
                    if (pressure <
                        Math.Max(
                            55,
                            ExhaustionPressureThreshold))
                        return false;
        
                    int opposite =
                        _plan.Direction * -1;
        
                    bool oppositeStructure =
                        _m5Frame != null &&
                        (opposite == 1
                            ? (_m5Frame.StructureBull ||
                               _m5Frame.MssBull ||
                               _m5Frame.ChochBull)
                            : (_m5Frame.StructureBear ||
                               _m5Frame.MssBear ||
                               _m5Frame.ChochBear));
        
                    bool oppositeReaction =
                        _reaction != null &&
                        _reaction.Direction == opposite &&
                        _reaction.Confidence >=
                        Math.Max(
                            LiveReactionThreshold,
                            LiveReversalMinimumConfidence) &&
                        _reaction.IndependentEvidence >=
                        Math.Max(
                            1,
                            ExhaustionMinimumOppositeEvidence);
        
                    if (!oppositeStructure &&
                        !oppositeReaction)
                        return false;
        
                    Position position =
                        GetManagedLivePositionForPlan();
        
                    if (position == null ||
                        position.NetProfit <= 0)
                        return false;
        
                    double protectedProfit =
                        position.NetProfit;
        
                    if (!RequestLivePlanExit(
                            closedM5,
                            "PROFIT EXHAUSTION"))
                        return false;
        
                    SendUnifiedAlert(
                        "EXHAUSTION-CLOSE|" +
                        position.Id,
                        "CFIP EXHAUSTION EXIT REQUESTED | #" +
                        position.Id +
                        " | +" +
                        protectedProfit.ToString("F2") +
                        " | PEAK RR " +
                        peakRR.ToString("F2") +
                        " | RETRACE " +
                        retracementPercent.ToString("F0") +
                        "% | PRESSURE " +
                        pressure,
                        _plan.Direction,
                        true);
        
                    SetAutoTradingState(
                        "EXECUTED",
                        "EXHAUSTION EXIT REQUESTED #" +
                        position.Id);
        
                    return true;
                }
        
                        private bool CheckStructuralSetupInvalidation(
                    int closedM5,
                    double market)
                {
                    if (!EnableSetupInvalidation ||
                        _plan == null ||
                        !_plan.IsLivePosition ||
                        _m5Bars == null ||
                        closedM5 < 30 ||
                        !IsFinitePositive(market))
                        return false;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (!IsFinitePositive(atr))
                        return false;
        
                    int swingLookback =
                        Math.Max(
                            10,
                            Math.Min(
                                StructureLookback,
                                closedM5 - 1));
        
                    int swingStart =
                        Math.Max(
                            1,
                            closedM5 -
                            swingLookback);
        
                    double swingHigh =
                        _m5Bars.HighPrices[swingStart];
        
                    double swingLow =
                        _m5Bars.LowPrices[swingStart];
        
                    for (int i = swingStart + 1;
                         i < closedM5;
                         i++)
                    {
                        swingHigh =
                            Math.Max(
                                swingHigh,
                                _m5Bars.HighPrices[i]);
        
                        swingLow =
                            Math.Min(
                                swingLow,
                                _m5Bars.LowPrices[i]);
                    }
        
                    double structureBuffer =
                        atr *
                        Math.Max(
                            0.02,
                            InvalidationStructureAtr);
        
                    bool structureFailure =
                        _plan.Direction == 1
                            ? swingLow > 0 &&
                              market <
                              swingLow -
                              structureBuffer
                            : swingHigh > 0 &&
                              market >
                              swingHigh +
                              structureBuffer;
        
                    double risk =
                        Math.Max(
                            Symbol.PipSize,
                            _plan.Risk);
        
                    double adverseR =
                        _plan.Direction == 1
                            ? (_plan.Entry - market) /
                              risk
                            : (market - _plan.Entry) /
                              risk;
        
                    double maxAdverseR =
                        Math.Max(
                            0.30,
                            InvalidationMaxAdverseR);
        
                    if (adverseR >= maxAdverseR)
                        structureFailure = true;
        
                    bool mtfFlip = false;
        
                    if (_m5Frame != null)
                    {
                        mtfFlip =
                            _plan.Direction == 1
                                ? _m5Frame.Direction == -1 &&
                                  (_m5Frame.MssBear ||
                                   _m5Frame.ChochBear)
                                : _m5Frame.Direction == 1 &&
                                  (_m5Frame.MssBull ||
                                   _m5Frame.ChochBull);
                    }
        
                    double zoneTolerance =
                        atr *
                        Math.Max(
                            0.02,
                            InvalidationZoneCloseAtr);
        
                    bool zoneFailure =
                        _plan.Direction == 1
                            ? market <
                              _plan.Stop -
                              zoneTolerance &&
                              (_m5Frame == null ||
                               _m5Frame.StructureBear ||
                               _m5Frame.MssBear ||
                               _m5Frame.ChochBear)
                            : market >
                              _plan.Stop +
                              zoneTolerance &&
                              (_m5Frame == null ||
                               _m5Frame.StructureBull ||
                               _m5Frame.MssBull ||
                               _m5Frame.ChochBull);
        
                    bool invalid =
                        (structureFailure ||
                         zoneFailure) &&
                        (!RequireMtfFlipForInvalidation ||
                         mtfFlip ||
                         adverseR >= maxAdverseR);
        
                    if (!invalid)
                        return false;
        
                    int score =
                        (structureFailure ? 40 : 0) +
                        (zoneFailure ? 30 : 0) +
                        (mtfFlip ? 30 : 0);
        
                    if (AlertOnInvalidated &&
                        _lastInvalidationAlertM5 !=
                        closedM5)
                    {
                        SendUnifiedAlert(
                            "STRUCT-INVALID|" +
                            closedM5,
                            "CFIP STRUCTURAL INVALIDATION | " +
                            (_plan.Direction == 1
                                ? "BUY"
                                : "SELL") +
                            " | SCORE " +
                            ClampInt(
                                score,
                                0,
                                100),
                            0,
                            true);
        
                        _lastInvalidationAlertM5 =
                            closedM5;
                    }
        
                    Position position =
                        GetManagedPositionById(
                            _plan.PositionId);
        
                    if (position == null)
                    {
                        SetLifecycleState(
                            LifecycleState.Closed,
                            "STRUCTURE INVALIDATED • POSITION ALREADY CLOSED");
                        return false;
                    }
        
                    SetLifecycleState(
                        LifecycleState.ExitRequested,
                        "STRUCTURAL INVALIDATION");
        
                    if (!TryClosePosition(
                            position,
                            "STRUCTURAL INVALIDATION"))
                    {
                        SetLifecycleState(
                            LifecycleState.RecoveryRequired,
                            "STRUCTURAL EXIT REJECTED");
        
                        _autoExecutionBlockReason =
                            "STRUCTURAL EXIT REJECTED";
        
                        SendUnifiedAlert(
                            "STRUCT-INVALID-EXIT-FAILED|" +
                            position.Id,
                            "CFIP STRUCTURAL INVALIDATION • BROKER EXIT REJECTED | #" +
                            position.Id,
                            _plan.Direction,
                            true);
                    }
        
                    _lastExitM5 = closedM5;
        
                    // _plan remains authoritative until OnPositionClosed confirms
                    // that the broker position is actually gone.
                    return true;
                }
        
                private int CalculateSmartExitPressure(
                    double market,
                    double currentRR)
                {
                    if (_plan == null)
                        return 0;
        
                    int pressure = 0;
                    int opposite = _plan.Direction * -1;
        
                    if (_reaction != null &&
                        _reaction.Direction == opposite)
                    {
                        pressure +=
                            Math.Min(
                                30,
                                Math.Max(
                                    0,
                                    _reaction.Confidence / 3));
        
                        if (_reaction.IndependentEvidence >=
                            MinimumLiveReactionEvidence)
                            pressure += 10;
                    }
        
                    if (_m5Frame != null)
                    {
                        bool structure =
                            opposite == 1
                                ? _m5Frame.StructureBull
                                : _m5Frame.StructureBear;
        
                        bool reversal =
                            opposite == 1
                                ? (_m5Frame.MssBull ||
                                   _m5Frame.ChochBull)
                                : (_m5Frame.MssBear ||
                                   _m5Frame.ChochBear);
        
                        bool force =
                            opposite == 1
                                ? (_m5Frame.DisplacementBull &&
                                   _m5Frame.LiquidityBull)
                                : (_m5Frame.DisplacementBear &&
                                   _m5Frame.LiquidityBear);
        
                        if (structure)
                            pressure += 15;
        
                        if (reversal)
                            pressure += 15;
        
                        if (force)
                            pressure += 20;
                    }
        
                    double atr =
                        Atr(
                            _m5Bars,
                            Math.Max(
                                1,
                                _m5Bars.Count - 2));
        
                    if (atr > 0)
                    {
                        Zone zone =
                            FindNearestOpposingZone(
                                _m5Bars,
                                Math.Max(
                                    1,
                                    _m5Bars.Count - 2),
                                _plan.Direction,
                                atr);
        
                        if (zone != null &&
                            DistanceToZone(
                                market,
                                zone) <=
                            atr *
                            Math.Max(
                                0.05,
                                ZoneProximityAtr))
                            pressure += 10;
                    }
        
                    if (currentRR < 0)
                        pressure += 10;
        
                    return ClampInt(
                        pressure,
                        0,
                        100);
                }
        
                private string GetSmartExitMode()
                {
                    if (_plan == null)
                        return "NO ACTIVE PLAN";
        
                    double risk =
                        Math.Max(
                            Symbol.PipSize,
                            _plan.Risk);
        
                    double currentRR =
                        _plan.Direction == 1
                            ? (_lastMarket - _plan.Entry) / risk
                            : (_plan.Entry - _lastMarket) / risk;
        
                    int pressure =
                        CalculateSmartExitPressure(
                            _lastMarket,
                            currentRR);
        
                    if (pressure >=
                        SmartExitPressureThreshold)
                        return "PROTECT";
        
                    if (pressure >=
                        LiveReactionWatchThreshold)
                        return "WATCH";
        
                    return "HOLD";
                }
        
                        private double CalculateProtectedStop(
                    double market,
                    double peakRR,
                    int closedM5,
                    bool structuralUpdate)
                {
                    if (_plan == null)
                        return 0;
        
                    double candidate =
                        _plan.Stop;
        
                    if (MoveSlToBreakEven &&
                        peakRR >=
                        BreakEvenTriggerRR)
                    {
                        double be =
                            _plan.Direction == 1
                                ? _plan.Entry +
                                  BreakEvenBufferPips *
                                  Symbol.PipSize
                                : _plan.Entry -
                                  BreakEvenBufferPips *
                                  Symbol.PipSize;
        
                        candidate =
                            _plan.Direction == 1
                                ? Math.Max(
                                    candidate,
                                    be)
                                : Math.Min(
                                    candidate,
                                    be);
                    }
        
                    if (UseSpreadAwareBreakEven &&
                        MoveSlToBreakEven &&
                        peakRR >= Math.Max(0.50, BreakEvenTriggerRR))
                    {
                        double spreadPips =
                            Math.Max(
                                0,
                                (Symbol.Ask - Symbol.Bid) /
                                Math.Max(Symbol.PipSize, 1e-9));
        
                        double lockPips =
                            Math.Max(
                                RiskFreeLockPips,
                                spreadPips + BreakEvenBufferPips);
        
                        double be =
                            _plan.Direction == 1
                                ? _plan.Entry + lockPips * Symbol.PipSize
                                : _plan.Entry - lockPips * Symbol.PipSize;
        
                        if (IsValidStop(
                                _plan.Direction,
                                _plan.Entry,
                                be))
                        {
                            candidate =
                                _plan.Direction == 1
                                    ? Math.Max(candidate, be)
                                    : Math.Min(candidate, be);
                        }
                    }
        
                    if ((EnableStructuralSlRepricing ||
                         EnableDynamicSlTrail) &&
                        (structuralUpdate ||
                         !StructuralStopManagementOnly) &&
                        peakRR >=
                        Math.Max(
                            SlRepriceStartRR,
                            SmartTrailMinimumRR) &&
                        UseSwingStructureInTrail)
                    {
                        double atr =
                            Atr(
                                _m5Bars,
                                closedM5);
        
                        if (atr > 0)
                        {
                            double structural =
                                _plan.Direction == 1
                                    ? FindSwingLowBelow(
                                        _m5Bars,
                                        closedM5,
                                        market)
                                    : FindSwingHighAbove(
                                        _m5Bars,
                                        closedM5,
                                        market);
        
                            if (IsFinitePositive(structural))
                            {
                                double room =
                                    atr *
                                    Math.Max(
                                        0.10,
                                        TrailDistanceAtr);
        
                                if (_plan.Direction == 1 &&
                                    structural <=
                                    market - room)
                                    candidate =
                                        Math.Max(
                                            candidate,
                                            structural);
                                else if (_plan.Direction == -1 &&
                                         structural >=
                                         market + room)
                                    candidate =
                                        Math.Min(
                                            candidate,
                                            structural);
                            }
                        }
                    }
        
                    int pressure =
                        CalculateSmartExitPressure(
                            market,
                            peakRR);
        
                    if (peakRR >= Math.Max(1.0, SmartTrailTightenAtRR))
                    {
                        double trailAtr =
                            Atr(_m5Bars, closedM5);
        
                        bool momentumAligned =
                            _m5Frame != null &&
                            (_plan.Direction == 1
                                ? _m5Frame.MomentumBull && _m5Frame.StructureBull
                                : _m5Frame.MomentumBear && _m5Frame.StructureBear);
        
                        if (trailAtr > 0 &&
                            momentumAligned)
                        {
                            double room =
                                trailAtr *
                                Math.Max(
                                    0.08,
                                    TrailDistanceAtr -
                                    SmartTrailMomentumBonusAtr);
        
                            double tightened =
                                _plan.Direction == 1
                                    ? market - room
                                    : market + room;
        
                            if (IsValidStop(
                                    _plan.Direction,
                                    _plan.Entry,
                                    tightened))
                            {
                                candidate =
                                    _plan.Direction == 1
                                        ? Math.Max(candidate, tightened)
                                        : Math.Min(candidate, tightened);
                            }
                        }
                    }
        
                    if (!StructuralStopManagementOnly &&
                        pressure >=
                        SmartExitPressureThreshold &&
                        peakRR >=
                        SmartTrailMinimumRR)
                    {
                        double atr =
                            Atr(
                                _m5Bars,
                                closedM5);
        
                        if (atr > 0)
                        {
                            double tightRoom =
                                atr *
                                Math.Min(
                                    0.45,
                                    Math.Max(
                                        0.10,
                                        SlRepriceBreathingAtr));
        
                            double tightened =
                                _plan.Direction == 1
                                    ? market - tightRoom
                                    : market + tightRoom;
        
                            if (IsValidStop(
                                    _plan.Direction,
                                    _plan.Entry,
                                    tightened))
                            {
                                candidate =
                                    _plan.Direction == 1
                                        ? Math.Max(
                                            candidate,
                                            tightened)
                                        : Math.Min(
                                            candidate,
                                            tightened);
                            }
                        }
                    }
        
                    double minimumDistance =
                        Math.Max(
                            Symbol.PipSize * 2,
                            Symbol.Ask -
                            Symbol.Bid);
        
                    candidate =
                        _plan.Direction == 1
                            ? Math.Min(
                                candidate,
                                market -
                                minimumDistance)
                            : Math.Max(
                                candidate,
                                market +
                                minimumDistance);
        
                    candidate =
                        NormalizePrice(
                            candidate);
        
                    if (!IsValidStop(
                            _plan.Direction,
                            _plan.Entry,
                            candidate))
                        return _plan.Stop;
        
                    double atrValue =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    double step =
                        atrValue *
                        Math.Max(
                            0.01,
                            TrailStepAtr);
        
                    if (!BetterStop(
                            _plan.Direction,
                            candidate,
                            _plan.Stop) ||
                        Math.Abs(
                            candidate -
                            _plan.Stop) <
                        Math.Max(
                            Symbol.PipSize,
                            step))
                        return _plan.Stop;
        
                    return candidate;
                }
        
                private void UpdateUnhitTargetsLive(
                    int closedM5,
                    double market)
                {
                    if (_plan == null)
                        return;
        
                    if (StructuralTargetUpdatesOnly &&
                        _lastTargetRepriceM5 ==
                        closedM5)
                        return;
        
                    double atr =
                        Atr(
                            _m5Bars,
                            closedM5);
        
                    if (atr <= 0)
                        return;
        
                    List<Level> levels =
                        BuildTargetLevels(
                            closedM5,
                            _plan.Direction,
                            _plan.Entry,
                            atr);
        
                    double step =
                        atr *
                        Math.Max(
                            0.05,
                            TargetUpdateStepAtr);
        
                    double spacing =
                        atr *
                        Math.Max(
                            0.05,
                            MinimumTpSpacingAtr);
        
                    bool changed = false;
        
                    for (int stage = 0;
                         stage < 4;
                         stage++)
                    {
                        int hit =
                            stage == 0
                                ? _tp1Hit
                                : stage == 1
                                    ? _tp2Hit
                                    : stage == 2
                                        ? _tp3Hit
                                        : _tp4Hit;
        
                        if (hit != 0)
                            continue;
        
                        double current =
                            stage == 0
                                ? _plan.Tp1
                                : stage == 1
                                    ? _plan.Tp2
                                    : stage == 2
                                        ? _plan.Tp3
                                        : _plan.Tp4;
        
                        if (!IsFinitePositive(current))
                            continue;
        
                        bool requireHtf =
                            stage == 0
                                ? RequireHtfRewardForTp1
                                : RequireHtfRewardForTp2Plus;
        
                        double previousTarget =
                            stage == 0
                                ? _plan.Entry
                                : stage == 1
                                    ? _plan.Tp1
                                    : stage == 2
                                        ? _plan.Tp2
                                        : _plan.Tp3;
        
                        double nextTarget =
                            stage == 0
                                ? _plan.Tp2
                                : stage == 1
                                    ? _plan.Tp3
                                    : stage == 2
                                        ? _plan.Tp4
                                        : 0;
        
                        double best =
                            current;
        
                        double bestScore =
                            double.MinValue;
        
                        for (int i = 0;
                             i < levels.Count;
                             i++)
                        {
                            Level level =
                                levels[i];
        
                            if (level.Score <
                                SmartTargetQuality)
                                continue;
        
                            bool htf =
                                IsHtfTimeframe(
                                    level.Timeframe);
        
                            if (requireHtf &&
                                !htf)
                                continue;
        
                            if (htf &&
                                !UseHigherTfLiquidityTargets &&
                                level.Kind.IndexOf(
                                    "LIQUIDITY",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                continue;
        
                            double distance =
                                Math.Abs(
                                    level.Price -
                                    _plan.Entry);
        
                            double rr =
                                distance /
                                Math.Max(
                                    Symbol.PipSize,
                                    _plan.Risk);
        
                            if (rr >
                                Math.Max(
                                    0,
                                    MaximumRewardRR))
                                continue;
        
                            bool improves =
                                _plan.Direction == 1
                                    ? level.Price >
                                      best + step
                                    : level.Price <
                                      best - step;
        
                            if (!improves)
                                continue;
        
                            bool keepsPreviousSpacing =
                                _plan.Direction == 1
                                    ? level.Price >
                                      previousTarget +
                                      spacing
                                    : level.Price <
                                      previousTarget -
                                      spacing;
        
                            if (!keepsPreviousSpacing)
                                continue;
        
                            bool keepsNextSpacing =
                                nextTarget <= 0 ||
                                (_plan.Direction == 1
                                    ? level.Price <
                                      nextTarget -
                                      spacing
                                    : level.Price >
                                      nextTarget +
                                      spacing);
        
                            if (!keepsNextSpacing)
                                continue;
        
                            if (RejectTargetObstacle &&
                                HasTargetObstacle(
                                    _m5Bars,
                                    closedM5,
                                    _plan.Direction,
                                    _plan.Entry,
                                    level.Price,
                                    atr))
                                continue;
        
                            double score =
                                level.Score;
        
                            if (htf)
                                score +=
                                    HtfRewardBonus;
        
                            if (level.Kind.IndexOf(
                                    "LIQUIDITY",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                score +=
                                    LiquidityRewardBonus;
        
                            if (level.Kind.IndexOf(
                                    "FVG",
                                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                                level.Kind.IndexOf(
                                    "ORDER_BLOCK",
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                score +=
                                    ZoneRewardBonus;
        
                            score +=
                                Math.Min(
                                    20,
                                    Math.Max(
                                        1,
                                        level.Hits) * 2);
        
                            if (score > bestScore)
                            {
                                bestScore =
                                    score;
                                best =
                                    level.Price;
                            }
                        }
        
                        if (Math.Abs(
                                best -
                                current) <
                            Symbol.PipSize)
                            continue;
        
                        if (stage == 0)
                            _plan.Tp1 =
                                NormalizePrice(best);
                        else if (stage == 1)
                            _plan.Tp2 =
                                NormalizePrice(best);
                        else if (stage == 2)
                            _plan.Tp3 =
                                NormalizePrice(best);
                        else
                            _plan.Tp4 =
                                NormalizePrice(best);
        
                        changed = true;
                    }
        
                    _lastTargetRepriceM5 =
                        closedM5;
        
                    if (changed)
                    {
                        ApplyTargetMeta(
                            levels,
                            _plan.Tp1,
                            atr,
                            out _plan.Tp1Source,
                            out _plan.Tp1Quality);
        
                        ApplyTargetMeta(
                            levels,
                            _plan.Tp2,
                            atr,
                            out _plan.Tp2Source,
                            out _plan.Tp2Quality);
        
                        ApplyTargetMeta(
                            levels,
                            _plan.Tp3,
                            atr,
                            out _plan.Tp3Source,
                            out _plan.Tp3Quality);
        
                        ApplyTargetMeta(
                            levels,
                            _plan.Tp4,
                            atr,
                            out _plan.Tp4Source,
                            out _plan.Tp4Quality);
        
                        _plan.HtfTargetCount =
                            CountHtfTargetsInPlan(
                                _plan);
                    }
        
                    RecalculatePlanRR();
                }
        
                private void RecalculatePlanRR()
                {
                    if (_plan == null ||
                        _plan.Risk <= 0)
                        return;
        
                    _plan.Tp1RR =
                        _plan.Tp1 > 0
                            ? Math.Abs(
                                _plan.Tp1 -
                                _plan.Entry) /
                              _plan.Risk
                            : 0;
        
                    _plan.Tp2RR =
                        _plan.Tp2 > 0
                            ? Math.Abs(
                                _plan.Tp2 -
                                _plan.Entry) /
                              _plan.Risk
                            : 0;
        
                    _plan.Tp3RR =
                        _plan.Tp3 > 0
                            ? Math.Abs(
                                _plan.Tp3 -
                                _plan.Entry) /
                              _plan.Risk
                            : 0;
        
                    _plan.Tp4RR =
                        _plan.Tp4 > 0
                            ? Math.Abs(
                                _plan.Tp4 -
                                _plan.Entry) /
                              _plan.Risk
                            : 0;
                }
        
                private bool BetterStop(
                    int direction,
                    double proposed,
                    double current)
                {
                    if (!IsFinitePositive(proposed) ||
                        !IsFinitePositive(current))
                        return false;
        
                    return direction == 1
                        ? proposed > current
                        : proposed < current;
                }
        
                // ============================================================
    }
}
