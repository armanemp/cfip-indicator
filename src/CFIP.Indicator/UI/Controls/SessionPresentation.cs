// CFIP Indicator — SessionPresentation.cs
 // Single-responsibility execution UI module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

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
                
                            int now =
                                utc.Hour * 60 +
                                utc.Minute;
                
                            int startMin =
                                start * 60;
                
                            int endMin =
                                end * 60;
                
                            bool open =
                                startMin <= endMin
                                    ? now >= startMin &&
                                      now < endMin
                                    : now >= startMin ||
                                      now < endMin;
                
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
                            DateTime utc =
                                TimeInUtc;
                
                            int now =
                                utc.Hour * 60 +
                                utc.Minute;
                
                            int start =
                                ClampInt(
                                    SessionStartUtc,
                                    0,
                                    23) * 60;
                
                            int end =
                                ClampInt(
                                    SessionEndUtc,
                                    0,
                                    23) * 60;
                
                            bool open =
                                start <= end
                                    ? now >= start &&
                                      now < end
                                    : now >= start ||
                                      now < end;
                
                            return open
                                ? TpLineColor
                                : PanelWarningColor;
                        }
    }
}
