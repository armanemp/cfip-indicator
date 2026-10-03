using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private const int PanelContentRefreshMilliseconds = 500;

        private bool ShouldRefreshPanelContent(
            DateTime nowUtc)
        {
            return
                _lastPanelContentRefreshUtc == DateTime.MinValue ||
                (nowUtc - _lastPanelContentRefreshUtc).TotalMilliseconds >=
                PanelContentRefreshMilliseconds;
        }

        private void RefreshPanelContentIfDue(
            DateTime nowUtc)
        {
            if (!ShowUnifiedPanel ||
                _panel == null ||
                _panelHidden ||
                _panelRowsStack == null ||
                !ShouldRefreshPanelContent(nowUtc))
                return;

            bool ownsVisualSnapshot =
                _renderSignalVisualSnapshot == null;

            try
            {
                // Pull the freshest cBot heartbeat before building the one panel
                // snapshot used by every live row in this refresh.
                RefreshCbotExecutionStateIfDue();
                UpdatePanelHeaderLiveState();

                if (ownsVisualSnapshot)
                {
                    _renderSignalVisualSnapshot =
                        BuildSignalVisualSnapshot(
                            Math.Max(
                                1,
                                _lastEvaluatedM5));
                }

                RenderPanelRows(
                    EffectivePanelContentWidth());

                _panelContentRefreshSequence++;

                _lastPanelContentRefreshUtc =
                    nowUtc;
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP panel content refresh failed: {0}",
                    ex.ToString());
            }
            finally
            {
                if (ownsVisualSnapshot)
                    _renderSignalVisualSnapshot = null;
            }
        }
    }
}
