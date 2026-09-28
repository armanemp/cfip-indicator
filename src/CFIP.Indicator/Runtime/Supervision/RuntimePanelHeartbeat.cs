using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void HandleRuntimeHeartbeat()
        {
            if (_runtimeTimerBusy)
                return;

            _runtimeTimerBusy = true;

            try
            {
                DateTime now =
                    Server.TimeInUtc;

                if (_m5Bars != null &&
                    _m5Bars.Count >= 2)
                {
                    _lastMtfClosedContext =
                        BuildMtfClosedContext(
                            now);

                    RefreshClosedM1Frame(
                        _lastMtfClosedContext.M1);
                }

                _lastPanelHeartbeatUtc =
                    now;

                // Display heartbeat only. No decision, plan or broker work.
                RenderPanel();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP panel heartbeat failed: {0}",
                    ex.ToString());
            }
            finally
            {
                _runtimeTimerBusy = false;
            }
        }
    }
}
