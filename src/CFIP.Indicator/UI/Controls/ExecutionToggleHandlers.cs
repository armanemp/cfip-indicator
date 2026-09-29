using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void WireExecutionToggleHandlers()
        {
            if (_autoTradingQuickStatus != null)
                _autoTradingQuickStatus.Click +=
                    args => ToggleAutoTradingFromPanel();

            if (_automaticOrdersQuickStatus != null)
                _automaticOrdersQuickStatus.Click +=
                    args => ToggleAutomaticOrdersFromPanel();
        }

        private void ToggleAutoTradingFromPanel()
        {
            try
            {
                EnsureExecutionRuntimeState();

                bool enabled =
                    !AutoTradingEnabled;

                SetAutoTradingRuntimeState(
                    enabled,
                    enabled
                        ? "PANEL ENABLED"
                        : "PANEL DISABLED");

                SetAutoTradingState(
                    enabled ? "ARMED" : "OFF",
                    enabled
                        ? "PANEL ENABLED"
                        : "PANEL DISABLED");

                RenderPanel();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP AUTO TRADE panel toggle failed: {0}",
                    ex.Message);
            }
        }

        private void ToggleAutomaticOrdersFromPanel()
        {
            try
            {
                EnsureExecutionRuntimeState();

                bool enabled =
                    !AutomaticOrdersEnabled;

                SetAutomaticOrdersRuntimeState(
                    enabled,
                    enabled
                        ? "PANEL ENABLED"
                        : "PANEL DISABLED");

                RenderPanel();
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP AUTO ORDERS panel toggle failed: {0}",
                    ex.Message);
            }
        }
    }
}
