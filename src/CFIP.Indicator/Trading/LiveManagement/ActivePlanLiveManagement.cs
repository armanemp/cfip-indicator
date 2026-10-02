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
            if (_plan == null ||
                !_plan.IsLivePosition)
                return;

            double previousStop =
                _plan.Stop;

            double previousTp1 =
                _plan.Tp1;

            double previousTp2 =
                _plan.Tp2;

            // A protected-stop candidate is ephemeral. The broker-confirmed
            // stop remains the authoritative plan protection state.
            _pendingProtectedStopCandidate =
                0;

            double favorable =
                _plan.Direction == 1
                    ? _peakPrice - _plan.Entry
                    : _plan.Entry - _peakPrice;

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

                bool structuralPulse =
                    ShouldRunLiveStructuralPulse(
                        TimeInUtc);

                bool structuralUpdate =
                    structuralBarChanged ||
                    structuralPulse;

                double protectedStop =
                    CalculateProtectedStop(
                        market,
                        peakRR,
                        closedM5,
                        structuralUpdate);

                if (ProtectionProgressionRule.ShouldAdvanceStop(
                        _plan.Direction,
                        _plan.Stop,
                        protectedStop))
                {
                    _pendingProtectedStopCandidate =
                        NormalizePrice(
                            protectedStop);
                }

                if (UpdateUnhitTargets &&
                    peakRR >=
                    TargetUpdateTriggerRR &&
                    (!StructuralTargetUpdatesOnly ||
                     structuralUpdate))
                {
                    UpdateUnhitTargetsLive(
                        closedM5,
                        market,
                        structuralUpdate);
                }

                if (structuralBarChanged)
                    _lastStructuralStopUpdateM5 =
                        closedM5;
            }

            // Server-side TP partials are broker events. After evidence is
            // observed, the follow-up ladder mutation is attempted through the
            // same bounded closed-bar identity and the broker remains authoritative.
            if (_serverSideTakeProfitLadderActive)
            {
                Position managedPositionAfterUpdates =
                    GetManagedLivePositionForPlan();

                if (managedPositionAfterUpdates != null)
                {
                    if (_tp1Hit != 0 &&
                        _tp2Hit == 0)
                    {
                        TryAdvanceServerSideTakeProfitLadderAfterTp1(
                            managedPositionAfterUpdates,
                            closedM5,
                            market);
                    }

                    if (_tp2Hit != 0)
                    {
                        TryCollapseServerSideTakeProfitLadderToFinal(
                            managedPositionAfterUpdates,
                            closedM5);
                    }
                }
            }

            double updateAtr =
                _m5Frame != null &&
                _m5Frame.Index == closedM5 &&
                _m5Frame.Atr > 0
                    ? _m5Frame.Atr
                    : Atr(
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
