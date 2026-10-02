using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ShouldRunSafetySupervisor(DateTime nowUtc)
        {
            if (_lastSafetySupervisorUtc == DateTime.MinValue ||
                (nowUtc - _lastSafetySupervisorUtc).TotalMilliseconds >= 1000)
            {
                _lastSafetySupervisorUtc = nowUtc;
                return true;
            }

            return false;
        }

        private void HandleRuntimeHeartbeat()
        {
            if (_runtimeTimerBusy)
                return;

            _runtimeTimerBusy = true;

            try
            {
                DateTime now =
                    Server.TimeInUtc;

                // The heartbeat deliberately avoids full analysis. It supervises
                // broker/live safety state on its independent cadence and keeps
                // the complete panel view responsive without recalculating analysis.
                _lastPanelHeartbeatUtc =
                    now;

                // Economic calendar refresh runs on the timer, not on every
                // market tick, so external feed latency cannot stall decisions.
                if (EnableEconomicNewsCalendar)
                    RefreshEconomicNewsIfNeeded(
                        now);

                // Cross-instance DailyLoss storage reload is a timer concern.
                // Calculate consumes the already-loaded value without disk I/O.
                if (EnableDailyLossLimit)
                    ReloadSharedDailyLossLockFromStorage(
                        now);

                if (ShouldRunSafetySupervisor(now))
                    RunRuntimeSafetySupervisor(
                        now);

                UpdateProcessingHeartbeatLamp();

                RefreshPanelContentIfDue(
                    now);

                UpdatePanelHeartbeatRows(
                    now);

                UpdatePanelHeartbeatLiveRows();

                FlushBufferedPersistence(
                    now,
                    false);
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
        private void UpdatePanelHeartbeatRows(
            DateTime now)
        {
            if (!ShowUnifiedPanel ||
                _panel == null ||
                _panelRows.Count == 0 ||
                _panelClockRow < 0)
                return;

            int width =
                Math.Max(
                    200,
                    PanelWidth -
                    2 * Math.Max(
                        0,
                        PanelPadding) -
                    2 * Math.Max(
                        0,
                        PanelBorderThickness));

            SetPanelRow(
                _panelClockRow,
                SymbolName +
                "  •  EXEC M15  •  " +
                now.ToString(
                    "HH:mm:ss") +
                " UTC",
                PanelMutedTextColor,
                false,
                width);
        }

    }
}
