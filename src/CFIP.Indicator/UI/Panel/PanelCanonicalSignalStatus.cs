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
                return "SIGNAL  •  WAIT / NO DIRECTION";

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
