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
        // ============================================================
                
                        private void SendUnifiedAlert(
                            string key,
                            string message,
                            int direction,
                            bool critical)
                        {
                            if (string.IsNullOrWhiteSpace(
                                    message))
                                return;
                
                            DateTime now =
                                TimeInUtc;
                
                            if (direction == 0)
                                direction =
                                    GetAuthoritativeDirection();
                
                            bool restrictionAlert =
                                key.StartsWith(
                                    "RESTRICT|",
                                    StringComparison.OrdinalIgnoreCase);
                
                            if (restrictionAlert &&
                                !string.IsNullOrWhiteSpace(
                                    _lastRestrictionMessage) &&
                                message.IndexOf(
                                    _lastRestrictionMessage,
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                return;
                
                            if (SuppressDuplicateAlerts)
                            {
                                int cooldownSeconds =
                                    (key.StartsWith(
                                        "SMART|",
                                        StringComparison.OrdinalIgnoreCase) ||
                                     key.StartsWith(
                                        "REACTION|",
                                        StringComparison.OrdinalIgnoreCase))
                                        ? SmartAlertCooldownSeconds
                                        : AlertCooldownSeconds;
                
                                if (_alertCooldowns.TryGetValue(
                                        key,
                                        out DateTime lastSent) &&
                                    (now - lastSent).TotalSeconds <
                                    Math.Max(
                                        1,
                                        cooldownSeconds))
                                    return;
                            }
                
                            _alertCooldowns[key] = now;
                
                            // Light housekeeping so this dictionary can't grow forever over
                            // a long-running session — unique keys (per-plan TP/SL, daily
                            // markers) accumulate over days/weeks otherwise.
                            if (_alertCooldowns.Count > 500)
                            {
                                List<string> stale =
                                    new List<string>();
                
                                foreach (KeyValuePair<string, DateTime> entry in
                                         _alertCooldowns)
                                {
                                    if ((now - entry.Value).TotalHours > 24)
                                        stale.Add(entry.Key);
                                }
                
                                foreach (string staleKey in stale)
                                    _alertCooldowns.Remove(staleKey);
                            }
                
                            _lastAlertMessage =
                                message;
                
                            _lastAlertDirection =
                                direction;
                
                            _lastAlertCritical =
                                critical;
                
                            _lastAlertUtc =
                                now;
                
                            if (EnableSoundAlerts)
                            {
                                try
                                {
                                    if (!string.IsNullOrWhiteSpace(
                                            SoundFilePath))
                                    {
                                        Notifications.PlaySound(
                                            SoundFilePath);
                                    }
                                    else
                                    {
                                        Notifications.PlaySound(
                                            ResolveAlertSoundType(
                                                key,
                                                critical));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Print(
                                        "CFIP sound alert failed: {0}",
                                        ex.Message);
                                }
                            }
                
                            if (EnableEmailAlerts &&
                                !string.IsNullOrWhiteSpace(
                                    SenderEmail) &&
                                !string.IsNullOrWhiteSpace(
                                    ReceiverEmail))
                            {
                                try
                                {
                                    Notifications.SendEmail(
                                        SenderEmail,
                                        ReceiverEmail,
                                        "CFIP SMART  " +
                                        SymbolName,
                                        message);
                                }
                                catch (Exception ex)
                                {
                                    Print(
                                        "CFIP email failed: {0}",
                                        ex.Message);
                                }
                            }
                
                            bool restrictionPopup =
                                restrictionAlert &&
                                ShowEntryRestrictionPopup;
                
                            if (ShowPopupAlerts &&
                                (restrictionPopup ||
                                 (!restrictionAlert &&
                                  (!PopupCriticalOnly ||
                                   critical))))
                            {
                                ShowPopup(
                                    message);
                            }
                        }
        
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
        
        private SoundType ResolveAlertSoundType(
                            string key,
                            bool critical)
                        {
                            if (!UseSemanticAlertSounds)
                                return AlertSoundType;
                
                            if (key.StartsWith(
                                    "SL|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "INVALID",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "RESTRICT|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "REVERSAL|",
                                    StringComparison.OrdinalIgnoreCase))
                                return SoundType.NegativeNotification;
                
                            if (key.StartsWith(
                                    "TP",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "AUTO",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "SIGNAL|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "HIGH|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "SMART|",
                                    StringComparison.OrdinalIgnoreCase))
                                return SoundType.PositiveNotification;
                
                            return critical
                                ? SoundType.Confirmation
                                : SoundType.Announcement;
                        }
        
        private void EmitContextAlerts(
                            int closedM5)
                        {
                            if (_lastContextM5 ==
                                closedM5 ||
                                _m5Frame == null)
                                return;
                
                            _lastContextM5 =
                                closedM5;
                
                            if (AlertOnBos &&
                                (_m5Frame.StructureBull ||
                                 _m5Frame.StructureBear))
                            {
                                if (ShowContextEventMarker)
                                {
                                    int bar =
                                        MapM5ToChart(
                                            closedM5,
                                            Bars.Count - 1);
                
                                    DrawIcon(
                                        P + "BOS_MARKER",
                                        _m5Frame.StructureBull
                                            ? ChartIconType.UpArrow
                                            : ChartIconType.DownArrow,
                                        bar,
                                        _m5Frame.StructureBull
                                            ? Bars.LowPrices[bar]
                                            : Bars.HighPrices[bar],
                                        _m5Frame.StructureBull
                                            ? BuyArrowColor
                                            : SellArrowColor);
                                }
                
                                int direction =
                                    _m5Frame.StructureBull
                                        ? 1
                                        : -1;
                
                                SendUnifiedAlert(
                                    "BOS|" +
                                    closedM5 +
                                    "|" +
                                    direction,
                                    "CFIP BOS | " +
                                    (direction == 1
                                        ? "BUY"
                                        : "SELL"),
                                    direction,
                                    false);
                            }
                
                            if (AlertOnMssChoch &&
                                (_m5Frame.MssBull ||
                                 _m5Frame.MssBear ||
                                 _m5Frame.ChochBull ||
                                 _m5Frame.ChochBear))
                            {
                                int direction =
                                    _m5Frame.MssBull ||
                                    _m5Frame.ChochBull
                                        ? 1
                                        : -1;
                
                                SendUnifiedAlert(
                                    "MSS|" +
                                    closedM5 +
                                    "|" +
                                    direction,
                                    "CFIP MSS/CHOCH | " +
                                    (direction == 1
                                        ? "BUY"
                                        : "SELL"),
                                    direction,
                                    false);
                            }
                
                            if (AlertOnLiquiditySweep &&
                                (_m5Frame.LiquidityBull ||
                                 _m5Frame.LiquidityBear))
                            {
                                int direction =
                                    _m5Frame.LiquidityBull
                                        ? 1
                                        : -1;
                
                                SendUnifiedAlert(
                                    "SWEEP|" +
                                    closedM5 +
                                    "|" +
                                    direction,
                                    "CFIP LIQUIDITY SWEEP | " +
                                    (direction == 1
                                        ? "BUY"
                                        : "SELL"),
                                    direction,
                                    false);
                            }
                
                            if (AlertOnEarlySetup &&
                                _prediction != null &&
                                _prediction.Confidence >=
                                MinimumEarlyConfidence &&
                                _prediction.Confidence <
                                MinimumConfidence &&
                                _lastEarlyAlertM5 !=
                                closedM5)
                            {
                                SendUnifiedAlert(
                                    "EARLY|" +
                                    closedM5 +
                                    "|" +
                                    _prediction.Direction,
                                    _prediction.Reason,
                                    _prediction.Direction,
                                    false);
                
                                _lastEarlyAlertM5 =
                                    closedM5;
                            }
                        }
    }
}
