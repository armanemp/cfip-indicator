using System;
using System.Globalization;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RequestOptionalBars(
            TimeFrame timeFrame,
            Action<Bars> assign)
        {
            try
            {
                MarketData.GetBarsAsync(
                    timeFrame,
                    bars =>
                    {
                        try
                        {
                            if (bars != null && assign != null)
                            {
                                assign(bars);

                                if (_initializationReady)
                                    RegisterNative(bars);
                            }

                            _mtfClosedContextCache.Invalidate();
                        }
                        catch (Exception ex)
                        {
                            Print(
                                "CFIP optional market data callback failed [{0}]: {1}",
                                timeFrame,
                                ex.Message);
                        }
                    });
            }
            catch (Exception ex)
            {
                Print(
                    "CFIP optional market data request failed [{0}]: {1}",
                    timeFrame,
                    ex.Message);
            }
        }

        private void UpdateInitializationPanelStatus()
        {
            UpdatePanelHeaderLiveState();
        }
    }
}
