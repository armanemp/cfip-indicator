using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RunRuntimeSupervisorTimer()
        {
            if (_runtimeSupervisorBusy)
                return;

            _runtimeSupervisorBusy = true;

            try
            {
                DateTime reference =
                    TimeInUtc;

                RemoveExpiredPopup();

                if (_m5Bars == null ||
                    _m5Bars.Count < 32)
                {
                    _status =
                        "BUILDING DATA";

                    RenderPanel(true);
                    return;
                }

                MtfClosedContext mtf =
                    BuildMtfClosedContext(
                        reference);

                _lastMtfClosedContext =
                    mtf;

                int closedM5 =
                    mtf.M5;

                bool canAdvance =
                    mtf.HasPrimaryDecisionHistory &&
                    closedM5 >= 30 &&
                    closedM5 != _lastSupervisorProcessedM5;

                if (canAdvance &&
                    closedM5 == _lastSupervisorFailedM5 &&
                    (reference - _lastSupervisorFailureUtc)
                        .TotalSeconds < 2)
                    canAdvance = false;

                if (canAdvance)
                {
                    int hostIndex =
                        Bars == null || Bars.Count == 0
                            ? 0
                            : Bars.Count - 1;

                    bool processed =
                        ProcessNewClosedBar(
                            hostIndex,
                            closedM5,
                            reference,
                            mtf);

                    if (processed)
                        _lastSupervisorProcessedM5 =
                            closedM5;
                    else
                    {
                        _lastSupervisorFailedM5 =
                            closedM5;

                        _lastSupervisorFailureUtc =
                            reference;
                    }
                }

                if (mtf.HasPrimaryDecisionHistory)
                    _status =
                        closedM5 ==
                        _lastEvaluatedM5
                            ? "READY"
                            : "UPDATING MTF";

                RenderPanel(true);
            }
            catch (Exception ex)
            {
                _lastSupervisorFailedM5 =
                    _lastMtfClosedContext == null
                        ? -1
                        : _lastMtfClosedContext.M5;

                _lastSupervisorFailureUtc =
                    TimeInUtc;

                _status =
                    "MTF SUPERVISOR ERROR";

                Print(
                    "CFIP MTF supervisor fault: {0}",
                    ex.ToString());

                try
                {
                    RenderPanel(true);
                }
                catch
                {
                }
            }
            finally
            {
                _runtimeSupervisorBusy = false;
            }
        }
    }
}
