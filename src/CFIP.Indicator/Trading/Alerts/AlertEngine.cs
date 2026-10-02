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
using CFIP.Contracts;

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
                                !string.Equals(
                                    key == null ? "" : key,
                                    "RESTRICT|" + _lastRestrictionMessage,
                                    StringComparison.OrdinalIgnoreCase) &&
                                !(key ?? "").StartsWith(
                                    "RESTRICT|",
                                    StringComparison.OrdinalIgnoreCase))
                                return;
                
                            DateTime now =
                                TimeInUtc;
                
                            if (direction == 0)
                                direction =
                                    GetAuthoritativeDirection();
                
                            string normalizedKey =
                                key ?? "";

                            bool restrictionAlert =
                                normalizedKey.StartsWith(
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
                                    (normalizedKey.StartsWith(
                                        "SMART|",
                                        StringComparison.OrdinalIgnoreCase) ||
                                     normalizedKey.StartsWith(
                                        "REACTION|",
                                        StringComparison.OrdinalIgnoreCase))
                                        ? SmartAlertCooldownSeconds
                                        : AlertCooldownSeconds;
                
                                if (_alertCooldowns.TryGetValue(
                                        normalizedKey,
                                        out DateTime lastSent) &&
                                    (now - lastSent).TotalSeconds <
                                    Math.Max(
                                        1,
                                        cooldownSeconds))
                                    return;
                            }
                
                            _alertCooldowns[normalizedKey] = now;
                
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
                
                            AlertEnvelope envelope =
                                BuildCanonicalAlertEnvelope(
                                    normalizedKey,
                                    message,
                                    direction,
                                    critical,
                                    now);

                            _lastAlertMessage =
                                message;
                
                            _lastAlertDirection =
                                direction;
                
                            _lastAlertCritical =
                                critical;
                
                            _lastAlertUtc =
                                now;
                
                            bool blockedCandidateAlert =
                                IsBlockedCandidateAlert(
                                    normalizedKey,
                                    message);

                            // Restriction/blocked candidates remain diagnostic panel messages
                            // and never emit the normal signal sound or chart marker.
                            bool playSound =
                                EnableSoundAlerts &&
                                !blockedCandidateAlert;

                            SoundType soundType =
                                ResolveAlertSoundType(
                                    normalizedKey,
                                    critical);

                            // Every eligible canonical alert is delivered to the same bounded transport.
                            // The panel rail is now the sole visual message surface; sound remains optional.
                            bool queued =
                                _alertDeliveryQueue.Enqueue(
                                    new AlertDelivery(
                                        envelope,
                                        direction,
                                        playSound,
                                        soundType.ToString(),
                                        SoundFilePath));

                            Print(
                                "CFIP ALERT QUEUED | id={0} | key={1} | stage={2} | critical={3} | sound={4} | soundType={5} | queue={6}",
                                envelope.AlertId,
                                envelope.AlertKey,
                                envelope.Stage,
                                envelope.Critical,
                                playSound,
                                soundType,
                                _alertDeliveryQueue.Count);

                            if (!queued)
                            {
                                Print(
                                    "CFIP ALERT QUEUE REJECTED | id={0} | revision={1}",
                                    envelope.AlertId,
                                    envelope.Identity.Revision);
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
