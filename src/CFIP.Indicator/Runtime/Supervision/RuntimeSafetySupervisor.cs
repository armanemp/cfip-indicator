using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RunRuntimeSafetySupervisor(
            DateTime nowUtc)
        {
            if (_calculationBusy ||
                _runtimeTimerBusy)
                return;

            int closedM5 =
                Math.Max(
                    1,
                    _lastEvaluatedM5);

            RunSafetySupervisorStep(
                () =>
                {
                    SynchronizeLiveBrokerState();
                },
                "BROKER RECONCILIATION");

            RunSafetySupervisorStep(
                () =>
                {
                    RecoverManagedLivePlan(
                        closedM5);
                },
                "LIFECYCLE RECOVERY");

            RunSafetySupervisorStep(
                () =>
                {
                    SynchronizeLiveBrokerState();
                },
                "BROKER RECONCILIATION POST-RECOVERY");

            RunSafetySupervisorStep(
                () =>
                {
                    if (_plan != null &&
                        _plan.IsLivePosition)
                    {
                        ProtectBrokerPositions(
                            closedM5);
                    }
                },
                "PROTECTION SUPERVISION");

            RunSafetySupervisorStep(
                () =>
                {
                    CheckEndOfDayAlert(
                        nowUtc);
                },
                "EOD SUPERVISION");

            RunSafetySupervisorStep(
                () =>
                {
                    SynchronizeLiveBrokerState();
                },
                "BROKER STATE FINALIZATION");
        }

        private void RunSafetySupervisorStep(
            Action action,
            string stage)
        {
            if (action == null)
                return;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP safety supervisor failed [{0}]: {1}",
                    stage,
                    ex.ToString());
            }
        }
    }
}
