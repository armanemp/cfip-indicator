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
                                    if (_plan == null ||
                                        !_plan.IsLivePosition ||
                                        !IsFinitePositive(_plan.Risk))
                                        return;

                                    int barsSincePlan =
                                        Math.Max(
                                            0,
                                            closedM5 -
                                            _plan.CreatedM5);

                                    double protectedStop =
                                        GetActiveBrokerStopPrice();

                                    if (!IsFinitePositive(protectedStop))
                                        protectedStop =
                                            _plan.Stop;

                                    double softAdverseR;
                                    double hardAdverseR;
                                    double protectedStopR;

                                    if (!FalseSignalAdverseRRule.TryResolveThresholds(
                                            _plan.Direction,
                                            _plan.Entry,
                                            _plan.Risk,
                                            FalseSignalAdverseR,
                                            protectedStop,
                                            out softAdverseR,
                                            out hardAdverseR,
                                            out protectedStopR))
                                        return;

                                    double adverseR =
                                        Math.Abs(currentMove) /
                                        _plan.Risk;

                                    if (double.IsNaN(adverseR) ||
                                        double.IsInfinity(adverseR))
                                        return;

                                    if (UseFalseSignalGuard &&
                                        EnableSoftAdverseRInvalidation &&
                                        currentMove < 0 &&
                                        adverseR >= softAdverseR &&
                                        barsSincePlan <=
                                        Math.Max(
                                            1,
                                            FalseSignalWatchBars))
                                    {
                                        if (AlertOnFalseSignalRisk &&
                                            _lastInvalidationAlertM5 !=
                                            closedM5)
                                        {
                                            _lastInvalidationAlertM5 =
                                                closedM5;

                                            string stopText =
                                                protectedStopR > 0
                                                    ? protectedStopR.ToString("F2")
                                                    : "N/A";

                                            SendUnifiedAlert(
                                                "INVALID-RISK|" +
                                                closedM5,
                                                "CFIP FALSE SIGNAL RISK | " +
                                                (_plan.Direction == 1
                                                    ? "BUY"
                                                    : "SELL") +
                                                " | ADVERSE " +
                                                adverseR.ToString("F2") +
                                                "R | EFFECTIVE " +
                                                softAdverseR.ToString("F2") +
                                                "R | BROKER SL " +
                                                stopText +
                                                "R",
                                                0,
                                                true);
                                        }

                                        if (InvalidateOnFalseSignal &&
                                            adverseR >= hardAdverseR)
                                        {
                                            RequestLivePlanExit(
                                                closedM5,
                                                "FALSE SIGNAL INVALIDATION");

                                            return;
                                        }
                                    }

                                    if (currentMove < 0 &&
                                        adverseR >= hardAdverseR &&
                                        AlertOnInvalidated &&
                                        _lastInvalidationAlertM5 !=
                                        closedM5)
                                    {
                                        _lastInvalidationAlertM5 =
                                            closedM5;

                                        SendUnifiedAlert(
                                            "INVALID|" +
                                            closedM5,
                                            "CFIP SETUP UNDER PRESSURE | " +
                                            (_plan.Direction == 1
                                                ? "BUY"
                                                : "SELL") +
                                            " | " +
                                            adverseR.ToString("F2") +
                                            "R / SL " +
                                            (protectedStopR > 0
                                                ? protectedStopR.ToString("F2")
                                                : "N/A") +
                                            "R",
                                            0,
                                            true);
                                    }
                                }
    }
}
