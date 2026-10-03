using System;
using System.Globalization;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void UpdatePanelHeaderLiveState()
        {
            if (_panelHeaderTitle == null)
                return;

            string text;

            if (!_initializationReady)
            {
                string m5 =
                    _m5Bars == null
                        ? "-"
                        : _m5Bars.Count.ToString(CultureInfo.InvariantCulture);
                string m15 =
                    _m15Bars == null
                        ? "-"
                        : _m15Bars.Count.ToString(CultureInfo.InvariantCulture);
                string h1 =
                    _h1Bars == null
                        ? "-"
                        : _h1Bars.Count.ToString(CultureInfo.InvariantCulture);

                text =
                    "CFIP SMART  •  " +
                    (_status ?? "STARTING") +
                    "  •  M5 " +
                    m5 +
                    "  M15 " +
                    m15 +
                    "  H1 " +
                    h1;
            }
            else
            {
                PanelTimeframePresentationState m15State =
                    ResolvePanelTimeframeState(_m15Frame);
                PanelTimeframePresentationState h1State =
                    ResolvePanelTimeframeState(_h1Frame);

                string m15Text =
                    m15State.DirectionLabel;

                string h1Text =
                    h1State.DirectionLabel;

                string cbot =
                    ResolvePanelHeaderCbotState();

                text =
                    "CFIP SMART  •  " +
                    GetCanonicalSignalPanelStatus() +
                    "  •  M15 " +
                    m15Text +
                    "  H1 " +
                    h1Text +
                    "  •  " +
                    cbot +
                    "  •  " +
                    Server.TimeInUtc.ToString(
                        "HH:mm:ss",
                        CultureInfo.InvariantCulture);
            }

            string key =
                text +
                "|" +
                GetCanonicalSignalPanelStatus() +
                "|" +
                ResolvePanelHeaderCbotState();

            if (string.Equals(
                    key,
                    _lastPanelHeaderLiveKey,
                    StringComparison.Ordinal))
                return;

            _panelHeaderTitle.Text = text;
            _panelHeaderTitle.ForegroundColor =
                _initializationReady
                    ? GetCanonicalSignalPanelStatusColor()
                    : PanelSecondaryTextColor;

            _lastPanelHeaderLiveKey = key;
        }
    }
}
