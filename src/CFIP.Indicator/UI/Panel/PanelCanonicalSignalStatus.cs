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

            if (_decision.ActionableNow)
                return "SIGNAL  •  " + direction + "  •  ACTIONABLE";

            if (!_decision.TriggerReady)
                return "SIGNAL  •  " + direction + "  •  WATCH";

            return "SIGNAL  •  " + direction + "  •  CONFIRMED";
        }

        private string GetPrimaryTimeframeSignalPanelStatus()
        {
            PanelTimeframePresentationState m15State =
                ResolvePanelTimeframeState(_m15Frame);

            PanelTimeframePresentationState h1State =
                ResolvePanelTimeframeState(_h1Frame);

            int m15 =
                m15State.Direction;

            int h1 =
                h1State.Direction;

            if (m15 == 0 && h1 == 0)
                return string.Empty;

            string m15Text =
                m15State.DirectionLabel;

            string h1Text =
                h1State.DirectionLabel;

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
                return PanelDirectionColor(
                    GetMarketBiasDirection());

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