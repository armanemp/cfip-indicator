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
    }
}
