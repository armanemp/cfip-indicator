// ============================================================================
// CFIP Indicator — ActivePlanEvaluation.cs
// ============================================================================

using System;
using CFIP.Contracts;
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
        private bool ValidateActivePlanRuntimeIntegrity(
                                    int closedM5)
                                {
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
                        
                                            ManagementCommandRequestStatus closeStatus =
                                                TryClosePosition(
                                                    integrityPosition,
                                                    "PLAN INTEGRITY FAILURE");
                                            if (!closeStatus.IsAccepted())
                                            {
                                                SetLifecycleState(
                                                    LifecycleState.RecoveryRequired,
                                                    "PLAN INTEGRITY EXIT REJECTED");
                        
                                                _autoExecutionBlockReason =
                                                    "PLAN INTEGRITY EXIT REJECTED";
                                            }
                        
                                            return false;
                                        }
                        
                                        SetLifecycleState(
                                            LifecycleState.Closed,
                                            "PLAN INVALID • NO BROKER POSITION");
                        
                                        _plan = null;
                                        RemovePlanObjects();
                                        return false;
                                    }

                                    return true;
                                }
    }
}
