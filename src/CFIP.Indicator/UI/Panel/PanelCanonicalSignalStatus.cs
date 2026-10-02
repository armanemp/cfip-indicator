using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private string GetCanonicalSignalPanelStatus()
        {
            if (_decision == null ||
                _decision.Direction == 0)
            {
                string primary =
                    GetPrimaryTimeframeSignalPanelStatus();

                return string.IsNullOrWhiteSpace(primary)
                    ? "SIGNAL  •  WAIT / NO DIRECTION"
                    : primary;
            }

            string direction =
                _decision.Direction == 1
                    ? "BUY"
                    : "SELL";

            if (!_decision.EntryAllowed)
                return "SIGNAL  •  " + direction + "  •  BLOCKED";

            if (!_decision.TriggerReady)
                return "SIGNAL  •  " + direction + "  •  WATCH";

            if (_decision.ActionableNow)
                return "SIGNAL  •  " + direction + "  •  ACTIONABLE";

            return "SIGNAL  •  " + direction + "  •  CONFIRMED";
        }

        private string GetPrimaryTimeframeSignalPanelStatus()
        {
            int m15 =
                _m15Frame == null
                    ? 0
                    : _m15Frame.Direction;

            int h1 =
                _h1Frame == null
                    ? 0
                    : _h1Frame.Direction;

            if (m15 == 0 && h1 == 0)
                return string.Empty;

            string m15Text =
                m15 == 1
                    ? "BUY"
                    : m15 == -1
                        ? "SELL"
                        : "WAIT";

            string h1Text =
                h1 == 1
                    ? "BUY"
                    : h1 == -1
                        ? "SELL"
                        : "WAIT";

            if (m15 != 0 &&
                h1 != 0 &&
                m15 == h1)
            {
                return
                    "PRIMARY M15/H1  •  " +
                    (m15 == 1 ? "BUY" : "SELL") +
                    "  •  M5/M1 TUNING";
            }

            if (m15 != 0 &&
                h1 != 0 &&
                m15 != h1)
            {
                return
                    "PRIMARY M15/H1  •  CONFLICT  •  " +
                    m15Text +
                    "/" +
                    h1Text;
            }

            return
                "PRIMARY M15/H1  •  " +
                (m15 != 0 ? "M15 " + m15Text : "M15 WAIT") +
                "  •  " +
                (h1 != 0 ? "H1 " + h1Text : "H1 WAIT") +
                "  •  M5/M1 TUNING";
        }

        private Color GetCanonicalSignalPanelStatusColor()
        {
            if (_decision == null ||
                _decision.Direction == 0)
                return PanelMutedTextColor;

            if (!_decision.EntryAllowed)
                return PanelWarningColor;

            return _decision.ActionableNow
                ? TpLineColor
                : _decision.TriggerReady
                    ? PanelAccentColor
                    : PanelSecondaryTextColor;
        }
    }
}
