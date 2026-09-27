// CFIP Indicator — DailyLossGuard.cs
// Single-responsibility risk module.

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
private bool DailyLossLimitHit(
                            DateTime nowUtc)
                        {
                            if (!EnableDailyLossLimit)
                                return false;
                
                            if (_dailyLossBaselineDate.Date !=
                                nowUtc.Date)
                            {
                                _dailyLossBaselineDate =
                                    nowUtc.Date;
                
                                _dailyStartEquity =
                                    Account.Equity;
                
                                _dailyLossLimitAlerted =
                                    false;
                            }
                
                            if (_dailyStartEquity <= 0)
                                return false;
                
                            double lossPercent =
                                (_dailyStartEquity -
                                 Account.Equity) /
                                _dailyStartEquity *
                                100.0;
                
                            if (lossPercent <
                                Math.Max(
                                    0.5,
                                    MaximumDailyLossPercent))
                                return false;
                
                            if (!_dailyLossLimitAlerted)
                            {
                                _dailyLossLimitAlerted =
                                    true;
                
                                SendUnifiedAlert(
                                    "DAILYLOSS|" +
                                    nowUtc.Date.ToString(
                                        "yyyyMMdd"),
                                    "Daily loss limit reached (" +
                                    lossPercent.ToString("F2") +
                                    "% >= " +
                                    MaximumDailyLossPercent.ToString(
                                        "F2") +
                                    "%) - new auto-trade entries are blocked for the " +
                                    "rest of the day. Open positions are left untouched.",
                                    0,
                                    true);
                            }
                
                            return true;
                        }
    }
}
