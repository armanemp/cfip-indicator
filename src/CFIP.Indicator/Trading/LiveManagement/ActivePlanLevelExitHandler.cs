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
        private bool ProcessActivePlanLevelHits(
                                    int closedM5,
                                    double market)
                                {
                                    Position managedPosition =
                                        GetManagedLivePositionForPlan();

                                    if (_serverSideTakeProfitLadderActive &&
                                        managedPosition != null)
                                    {
                                        ObserveServerSidePartialTakeProfits(
                                            managedPosition,
                                            closedM5,
                                            market);
                                    }

                                    double liveStop =
                                        GetActiveBrokerStopPrice();
                        
                                    bool hitSl =
                                        IsFinitePositive(liveStop) &&
                                        (_plan.Direction == 1
                                            ? market <= liveStop
                                            : market >= liveStop);
                        
                                    bool hitTp1 =
                                        !_serverSideTakeProfitLadderActive &&
                                        _plan.Tp1 > 0 &&
                                        (_plan.Direction == 1
                                            ? market >= _plan.Tp1
                                            : market <= _plan.Tp1);
                        
                                    bool hitTp2 =
                                        !_serverSideTakeProfitLadderActive &&
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
                                        !_serverSideTakeProfitLadderActive &&
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
                                            return false;
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
                        
                                        return false;
                                    }
                        
                                    if (hitTp1 &&
                                        _tp1Hit == 0)
                                    {
                                        bool tp1Processed =
                                            ExecutePartialClose(
                                                PartialCloseTp1Percent,
                                                "TP1");
                        
                                        if (tp1Processed)
                                        {
                                            _tp1Hit = 1;

                                            if (UpdateUnhitTargets &&
                                                GetManagedLivePositionForPlan() != null)
                                            {
                                                UpdateUnhitTargetsLive(
                                                    closedM5,
                                                    market,
                                                    true);
                                            }
                                        }
                        
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
                                        {
                                            _tp2Hit = 1;

                                            if (UpdateUnhitTargets &&
                                                GetManagedLivePositionForPlan() != null)
                                            {
                                                UpdateUnhitTargetsLive(
                                                    closedM5,
                                                    market,
                                                    true);
                                            }
                                        }
                        
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
                        
                                        return false;
                                    }

                                    return true;
                                }
    }
}
