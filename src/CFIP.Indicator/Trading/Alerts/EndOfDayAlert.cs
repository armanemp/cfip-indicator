// ============================================================================
// CFIP Indicator — AlertEngine.cs
// ============================================================================

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
        private void CheckEndOfDayAlert(
                            DateTime nowUtc)
                        {
                            DateTime dayClose =
                                new DateTime(
                                    nowUtc.Year,
                                    nowUtc.Month,
                                    nowUtc.Day,
                                    ClampInt(
                                        SessionEndUtc,
                                        0,
                                        23),
                                    0,
                                    0,
                                    DateTimeKind.Utc);
                
                            if (EnableEndOfDayAlert)
                            {
                                DateTime warnStart =
                                    dayClose.AddMinutes(
                                        -Math.Max(
                                            5,
                                            EndOfDayAlertMinutesBefore));
                
                                if (nowUtc >= warnStart &&
                                    nowUtc <= dayClose &&
                                    _lastEndOfDayAlertDate.Date !=
                                    nowUtc.Date &&
                                    HasManagedOpenPosition())
                                {
                                    _lastEndOfDayAlertDate =
                                        nowUtc.Date;
                
                                    int minutesLeft =
                                        Math.Max(
                                            0,
                                            (int)Math.Round(
                                                (dayClose -
                                                 nowUtc).TotalMinutes));
                
                                    SendUnifiedAlert(
                                        "DAYEND|" +
                                        nowUtc.Date.ToString(
                                            "yyyyMMdd"),
                                        "Day-trading session closes in ~" +
                                        minutesLeft +
                                        " min (" +
                                        ClampInt(
                                            SessionEndUtc,
                                            0,
                                            23).ToString("00") +
                                        ":00 UTC) - " +
                                        (EnableEndOfDayAutoClose
                                            ? "positions will be closed automatically at " +
                                              "the session close."
                                            : "review/close open positions manually. " +
                                              "This indicator never closes trades " +
                                              "automatically."),
                                        0,
                                        true);
                                }
                            }
                
                            // ENHANCEMENT: optional actual auto-close at the day's close,
                            // separate from the warning above and defaulting ON per
                            // request. Fires once per day, only once we've reached the
                            // configured close time, and only if a managed position is
                            // still open at that point.
                            if (EnableEndOfDayAutoClose &&
                                nowUtc >= dayClose &&
                                _lastEndOfDayCloseDate.Date !=
                                nowUtc.Date &&
                                HasManagedOpenPosition())
                            {
                                _lastEndOfDayCloseDate =
                                    nowUtc.Date;
                
                                CloseAllPositions();
                
                                SendUnifiedAlert(
                                    "DAYEND-CLOSED|" +
                                    nowUtc.Date.ToString(
                                        "yyyyMMdd"),
                                    "CFIP closed all managed positions at day-trading " +
                                    "session end (" +
                                    ClampInt(
                                        SessionEndUtc,
                                        0,
                                        23).ToString("00") +
                                    ":00 UTC).",
                                    0,
                                    true);
                            }
                        }
    }
}
