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
            if (_panelHeaderTitle == null)
                return;

            UpdatePanelHeaderLiveState();
            return;

            string m5 =
                _m5Bars == null
                    ? "-"
                    : _m5Bars.Count.ToString(CultureInfo.InvariantCulture);
            string m15 =
                _m15Bars == null
                    ? "-"
                    : _m15Bars.Count.ToString(CultureInfo.InvariantCulture);
            string m30 =
                _m30Bars == null
                    ? "-"
                    : _m30Bars.Count.ToString(CultureInfo.InvariantCulture);
            string h1 =
                _h1Bars == null
                    ? "-"
                    : _h1Bars.Count.ToString(CultureInfo.InvariantCulture);
            string h4 =
                _h4Bars == null
                    ? "-"
                    : _h4Bars.Count.ToString(CultureInfo.InvariantCulture);

            _panelHeaderTitle.Text =
                "CFIP SMART  •  " +
                _status +
                "  •  M5 " +
                m5 +
                "  M15 " +
                m15 +
                "  M30 " +
                m30 +
                "  H1 " +
                h1 +
                "  H4 " +
                h4;
        }
    }
}
