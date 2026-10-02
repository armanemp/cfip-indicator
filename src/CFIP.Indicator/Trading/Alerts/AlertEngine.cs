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

                            // A blocked candidate does not create a trade/signal
                            // side effect. An explicitly configured restriction alert
                            // is a user-facing diagnostic event and remains deliverable.
                            if (message.StartsWith(
                                    "CFIP ENTRY BLOCKED",
                                    StringComparison.OrdinalIgnoreCase) &&
                                !key.StartsWith(
                                    "RESTRICT|",
                                    StringComparison.OrdinalIgnoreCase))
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

                            RememberVisualSignalAlert(
                                key,
                                direction,
                                now);
                
                            bool playSound =
                                EnableSoundAlerts;

                            SoundType soundType =
                                ResolveAlertSoundType(
                                    key,
                                    critical);

                            bool restrictionPopup =
                                restrictionAlert &&
                                ShowEntryRestrictionPopup;

                            bool showPopup =
                                ShowPopupAlerts &&
                                (restrictionPopup ||
                                 (!restrictionAlert &&
                                  (!PopupCriticalOnly ||
                                   critical)));

                            if (playSound ||
                                showPopup)
                            {
                                _alertDeliveryQueue.Enqueue(
                                    new AlertDelivery(
                                        key,
                                        message,
                                        critical,
                                        now,
                                        playSound,
                                        soundType.ToString(),
                                        SoundFilePath,
                                        showPopup));
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
                

                        }

        private bool IsImportantPopupAlertKey(
                            string key,
                            bool critical)
                        {
                            if (critical)
                                return true;

                            if (string.IsNullOrWhiteSpace(key))
                                return false;

                            string[] prefixes =
                            {
                                "ACTION|",
                                "HIGH|",
                                "SMART|",
                                "EARLY|",
                                "REACTION|",
                                "REVERSAL|",
                                "TP",
                                "SL|",
                                "INVALID",
                                "PROTECTION",
                                "POSITION-OPEN|",
                                "PENDING-",
                                "FILL-MISMATCH|",
                                "STRUCT-INVALID",
                                "EXHAUSTION-CLOSE|",
                                "OUTCOME-TIMEOUT|",
                                "DAILYLOSS|",
                                "RESTRICT|",
                            };

                            foreach (string prefix in prefixes)
                            {
                                if (key.StartsWith(
                                        prefix,
                                        StringComparison.OrdinalIgnoreCase))
                                    return true;
                            }

                            return false;
                        }

        private bool IsVisualSignalAlertKey(
                            string key)
                        {
                            if (string.IsNullOrWhiteSpace(key))
                                return false;

                            return
                                key.StartsWith("ACTION|", StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith("HIGH|", StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith("SMART|", StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith("EARLY|", StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith("REACTION|", StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith("REVERSAL|", StringComparison.OrdinalIgnoreCase);
                        }

        private void RememberVisualSignalAlert(
                            string key,
                            int direction,
                            DateTime now)
                        {
                            if (!IsVisualSignalAlertKey(key) ||
                                direction == 0)
                                return;

                            int alertM5 =
                                ExtractVisualAlertM5(
                                    key,
                                    _lastEvaluatedM5);

                            if (alertM5 < 0)
                                return;

                            int separator =
                                key.IndexOf('|');

                            _lastVisualAlertKind =
                                separator > 0
                                    ? key.Substring(
                                        0,
                                        separator)
                                    : "SIGNAL";

                            _lastVisualAlertM5 =
                                alertM5;
                            _lastVisualAlertDirection =
                                direction;
                            _lastVisualAlertUtc =
                                now;
                        }

        private int ExtractVisualAlertM5(
                            string key,
                            int fallback)
                        {
                            if (string.IsNullOrWhiteSpace(key))
                                return fallback;

                            string[] parts =
                                key.Split('|');

                            for (int i = 1;
                                 i < parts.Length;
                                 i++)
                            {
                                int value;

                                if (!int.TryParse(
                                        parts[i],
                                        NumberStyles.Integer,
                                        CultureInfo.InvariantCulture,
                                        out value))
                                    continue;

                                if (value < 0)
                                    continue;

                                if (_m5Bars == null ||
                                    value < _m5Bars.Count)
                                    return value;
                            }

                            return fallback;
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
                                    "ACTION|",
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
