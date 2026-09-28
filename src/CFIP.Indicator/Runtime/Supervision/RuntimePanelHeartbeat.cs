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
                UpdatePanelHeartbeatRows(
                    now);
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
                _panelMtfAlignmentRow < 0 ||
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
                _panelMtfAlignmentRow,
                MtfAlignmentText(),
                PanelSectionColor,
                false,
                width);

            SetPanelRow(
                _panelClockRow,
                SymbolName +
                "  •  " +
                Bars.TimeFrame +
                "  •  " +
                now.ToString(
                    "HH:mm:ss") +
                " UTC",
                PanelMutedTextColor,
                false,
                width);
        }

    }
}
