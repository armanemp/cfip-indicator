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
                
                            AlertEnvelope envelope =
                                BuildCanonicalAlertEnvelope(
                                    key,
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
                                    key,
                                    message);

                            // Restriction/blocked candidates may show the configured
                            // diagnostic popup, but never emit the normal signal sound
                            // or chart marker.
                            bool playSound =
                                EnableSoundAlerts &&
                                !blockedCandidateAlert;

                            SoundType soundType =
                                ResolveAlertSoundType(
                                    key,
                                    critical);

                            bool restrictionPopup =
                                restrictionAlert &&
                                ShowEntryRestrictionPopup;
                             bool importantPopup =
                                 IsImportantPopupAlertKey(
                                     key,
                                     critical);

                             bool showPopup =
                                 ShowPopupAlerts &&
                                 (restrictionPopup ||
                                  (!restrictionAlert &&
                                   (PopupCriticalOnly
                                       ? critical
                                       : importantPopup)));

                            if (playSound ||
                                showPopup)
                            {
                                if (!_alertDeliveryQueue.Enqueue(
                                        new AlertDelivery(
                                            envelope,
                                            playSound,
                                            soundType.ToString(),
                                            SoundFilePath,
                                            showPopup)))
                                {
                                    Print(
                                        "CFIP alert delivery queue rejected [{0}] revision={1}",
                                        envelope.AlertId,
                                        envelope.Identity.Revision);
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
                

                        }

        private AlertEnvelope BuildCanonicalAlertEnvelope(
                            string key,
                            string message,
                            int direction,
                            bool critical,
                            DateTime now)
                        {
                            int closedM5 =
                                ExtractVisualAlertM5(
                                    key,
                                    _lastEvaluatedM5);

                            if (closedM5 < 0 &&
                                _m5Bars != null &&
                                _m5Bars.Count > 1)
                                closedM5 =
                                    _m5Bars.Count - 2;

                            if (direction == 0)
                                direction =
                                    GetAuthoritativeDirection();

                            OpportunityLane lane =
                                ResolveProviderLane();

                            string signalId =
                                ResolveProviderSignalId(
                                    closedM5);

                            string scenarioId =
                                ResolveProviderScenarioId(
                                    signalId,
                                    lane,
                                    direction);

                            string planId =
                                ResolveProviderPlanId(
                                    signalId);

                            TradeOpportunityCandidate scenario = null;
                            _tradePlanRegistry.TryGetCandidate(
                                scenarioId,
                                out scenario);

                            string sourceTimeframe =
                                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                                    scenario,
                                    ProviderScenarioIdentityRule.CanonicalM5);

                            DateTime createdUtc =
                                _m5Bars != null &&
                                closedM5 >= 0 &&
                                closedM5 < _m5Bars.Count
                                    ? _m5Bars.OpenTimes[closedM5]
                                    : now;

                            long revision =
                                Math.Max(
                                    1,
                                    _cfipProviderRevision);

                            bool blockedCandidate =
                                IsBlockedCandidateAlert(
                                    key,
                                    message);

                            SignalStage stage =
                                ResolveAlertSignalStage(
                                    key,
                                    blockedCandidate);

                            ContractIdentity identity =
                                new ContractIdentity(
                                    ContractVersion.Current,
                                    signalId,
                                    scenarioId,
                                    planId,
                                    SymbolName ?? "",
                                    ResolveContractDirection(
                                        direction),
                                    ResolveContractLane(
                                        lane),
                                    sourceTimeframe,
                                    createdUtc,
                                    Math.Max(0, closedM5),
                                    null,
                                    revision,
                                    signalId,
                                    "CFIP-ALERT|" +
                                    key +
                                    "|" +
                                    revision.ToString(
                                        CultureInfo.InvariantCulture));

                            string alertId =
                                "CFIP-ALERT|" +
                                signalId +
                                "|" +
                                scenarioId +
                                "|" +
                                planId +
                                "|" +
                                revision.ToString(
                                    CultureInfo.InvariantCulture) +
                                "|" +
                                key;

                            return new AlertEnvelope(
                                identity,
                                stage,
                                alertId,
                                key ?? "",
                                message ?? "",
                                critical,
                                !blockedCandidate &&
                                IsVisualSignalAlertKey(key),
                                now);
                        }

        private bool IsBlockedCandidateAlert(
                            string key,
                            string message)
                        {
                            return
                                (key ?? "").StartsWith(
                                    "RESTRICT|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                (message ?? "").StartsWith(
                                    "CFIP ENTRY BLOCKED",
                                    StringComparison.OrdinalIgnoreCase);
                        }

        private SignalStage ResolveAlertSignalStage(
                            string key,
                            bool blockedCandidate)
                        {
                            if (blockedCandidate)
                                return SignalStage.Blocked;

                            if (string.IsNullOrWhiteSpace(key))
                                return SignalStage.Watch;

                            if (key.StartsWith(
                                    "ACTION|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "HIGH|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "SMART|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "PENDING-",
                                    StringComparison.OrdinalIgnoreCase))
                                return SignalStage.Confirmed;

                            if (key.StartsWith(
                                    "EARLY|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "WATCH|",
                                    StringComparison.OrdinalIgnoreCase))
                                return SignalStage.Watch;

                            if (key.StartsWith(
                                    "REACTION|",
                                    StringComparison.OrdinalIgnoreCase))
                                return SignalStage.Prediction;

                            if (key.StartsWith(
                                    "REVERSAL|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "TP",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "SL|",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "PROTECTION",
                                    StringComparison.OrdinalIgnoreCase) ||
                                key.StartsWith(
                                    "POSITION-OPEN|",
                                    StringComparison.OrdinalIgnoreCase))
                                return SignalStage.Active;

                            return SignalStage.Confirmed;
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
