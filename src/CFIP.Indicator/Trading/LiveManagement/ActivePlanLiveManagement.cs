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
        private void UpdateActivePlanLiveManagement(
                                    int closedM5,
                                    double market)
                                {
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
                                }
    }
}
