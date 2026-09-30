// CFIP Indicator — SessionPresentation.cs
// Single-responsibility execution UI module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private string GetSessionPanelText()
        {
            DateTime utc =
                TimeInUtc;

            DateTime local =
                TimeInUtc +
                Application.UserTimeOffset;

            int start =
                ClampInt(
                    SessionStartUtc,
                    0,
                    23);

            int end =
                ClampInt(
                    SessionEndUtc,
                    0,
                    23);

            bool open =
                SessionWindowRule.IsInside(
                    utc,
                    start,
                    end);

            return
                "SESSION  " +
                (open ? "OPEN" : "CLOSED") +
                "  •  UTC " +
                utc.ToString("HH:mm") +
                "  •  LOCAL " +
                local.ToString("HH:mm") +
                "  •  " +
                start.ToString("00") +
                ":00–" +
                end.ToString("00") +
                ":00 UTC";
        }

        private Color GetSessionPanelColor()
        {
            bool open =
                SessionWindowRule.IsInside(
                    TimeInUtc,
                    SessionStartUtc,
                    SessionEndUtc);

            return open
                ? TpLineColor
                : PanelWarningColor;
        }
    }
}
