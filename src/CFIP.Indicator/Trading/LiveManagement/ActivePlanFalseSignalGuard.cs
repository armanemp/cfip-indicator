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
        private void ProcessActivePlanFalseSignalRisk(
                                    int closedM5,
                                    double currentMove)
                                {
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
